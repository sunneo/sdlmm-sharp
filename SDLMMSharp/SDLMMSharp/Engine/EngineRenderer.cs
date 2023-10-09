using SDLMMSharp.Engine.Scenes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDLMMSharp.Engine
{
    public class EngineRenderer
    {
        BaseEngine owner;
        IRenderer renderer;
        Timer timer;
        int fps;
        LinkedList<IScene> SceneStack = new LinkedList<IScene>();
        IScene RootScene;
        IScene CurrentScene;
        bool animating = false;
        internal volatile bool updated = false;
        AutoTunedTimerDispatcher timerDispatcher;
        volatile bool animationCancelled = false;
        public int backgroundColor = unchecked((int)0xffffffff);
        public IRenderer GetCanvas()
        {
            return renderer;
        }
        public void CancelAnimate()
        {
            animationCancelled = true;
        }
        public bool IsUpdateRequested()
        {
            return updated;
        }
        public BaseEngine Parent
        {
            get
            {
                return owner;
            }
        }
        public virtual void Invalidate()
        {
            updated = true;
        }
        public bool IsAnimating()
        {
            return animating;
        }
        public void SetRootScene(IScene scene)
        {
            RootScene = scene;
            if(SceneStack.Count == 0)
            {
                this.PushScene(scene);
            }
        }
        public EngineRenderer(BaseEngine owner) :this(owner,60)
        {
        }

        public EngineRenderer(BaseEngine owner,int fps)
        {
            this.owner = owner;
            this.renderer = owner.GetCanvas();
            timer = new Timer();
            if (fps < 1) fps = 60;
            this.fps = fps;
            timerDispatcher = new AutoTunedTimerDispatcher(this);
            timer.Interval = 1000/fps;
            timer.Tick += Timer_Tick;    
        }

        
        private bool PaintAnimationPreEffect()
        {
            try
            {
                if (this.animationIteratorPreEffects != null && this.animationIteratorPreEffects.Count > 0)
                {
                    if (animationCancelled)
                    {
                        animationIteratorPreEffects.Clear();
                        return false;
                    }
                    else
                    {
                        
                        LinkedListNode<IEnumerator<Object>> node = animationIteratorPreEffects.First;
                        while(node != null)
                        {
                            LinkedListNode<IEnumerator<Object>> next = node.Next;
                            if(!node.Value.MoveNext())
                            {
                                animationIteratorPreEffects.Remove(node);
                            }
                            node = next;
                        }
                    }
                }
            }
            catch (Exception ee)
            {
             
            }
            return false;
        }
        private bool PaintAnimationAfterEffect()
        {
            try
            {
                if (this.animationIteratorAfterEffects != null && this.animationIteratorAfterEffects.Count > 0)
                {
                    if (animationCancelled)
                    {
                        animationIteratorAfterEffects.Clear();
                        return false;
                    }
                    else
                    {

                        LinkedListNode<IEnumerator<Object>> node = animationIteratorAfterEffects.First;
                        while (node != null)
                        {
                            LinkedListNode<IEnumerator<Object>> next = node.Next;
                            if (!node.Value.MoveNext())
                            {
                                animationIteratorAfterEffects.Remove(node);
                            }
                            node = next;
                        }
                    }
                }
            }
            catch (Exception ee)
            {

            }
            return false;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!timer.Enabled) return;
            timerDispatcher.run();
        }
        public virtual void PushScene(IScene scene)
        {
            CurrentScene = scene;
            SceneStack.AddLast(scene);
        }
        public virtual IScene PopScene()
        {
            if (CurrentScene != null)
            {
                CurrentScene.End();
            }
            if (this.SceneStack.Count > 0)
            {
                this.SceneStack.RemoveLast();
            }
            IScene ret = null;
            if(this.SceneStack.Count > 0)
            {
                ret = this.SceneStack.Last.Value;
            }
            else
            {
                ret = RootScene;
            }
            
            CurrentScene = ret;
            return ret;

        }

        public IScene GetCurrentScene()
        {
            return CurrentScene;
        }

        public virtual void Clear()
        {
            CurrentScene = null;
            SceneStack.Clear();
        }

        public virtual void Paint()
        {
            IScene scene = CurrentScene;
            if (scene == null) return;
            try
            {
                //PaintAnimationPreEffect();
                scene.Paint(this.renderer);
                //PaintAnimationAfterEffect();
                this.renderer.flush();
            }
            catch(Exception ee)
            {

            }
        }

        /// <summary>
        /// start a timer to run looper
        /// </summary>
        public void Start()
        {
            timer.Enabled = true;
            timer.Start();
        }
        public void End()
        {
            timer.Enabled = false;
            timer.Stop();
        }

        public virtual void Animate(IEnumerable<Object> iter, bool isPreAffect)
        {
            if (isPreAffect)
            {
                this.animationIteratorPreEffects.AddLast(iter.GetEnumerator());
            }
            else
            {
                this.animationIteratorAfterEffects.AddLast(iter.GetEnumerator());
            }
        }
        LinkedList<IEnumerator<Object>> animationIteratorPreEffects = new LinkedList<IEnumerator<object>>();
        LinkedList<IEnumerator<Object>> animationIteratorAfterEffects = new LinkedList<IEnumerator<object>>();
        internal IEnumerable<object> CreateSnapshotAdditionEffect(IEnumerable<object> animate)
        {
            IRenderer renderer = this.GetCanvas();
            if(renderer == null)
            {
                yield break;
            }
            Bitmap img = renderer.flushToBMP();
            Size size = img.Size;
            IEnumerator<object> iter = animate.GetEnumerator();
            while(true)
            {
                try
                {
                    renderer.drawImage(img, 0, 0, size.Width, size.Height);
                    if (!iter.MoveNext())
                    {
                        break;
                    }
                }
                catch(Exception ee)
                {
                    break;
                }
                yield return iter.Current;
            };
            img.Dispose();
            yield break;
        }
    }


    class AutoTunedTimerDispatcher
    {

        EngineRenderer Parent;
        volatile bool drawing = false;
        int mIdleTime = 16;
        int mBusyFreq = 60;
        DateTime previousRendererTime = DateTime.Now;

        public AutoTunedTimerDispatcher(EngineRenderer Parent)
        {
            this.Parent = Parent;
        }

        public AutoTunedTimerDispatcher SetIdleFreq(int freq)
        {
            this.mIdleTime = freq;
            return this;
        }
        public AutoTunedTimerDispatcher SetBusyFreq(int freq)
        {
            this.mBusyFreq = freq;
            return this;
        }
        public void run()
        {
            if (Parent.Parent.IsDisposed())
            {
                return;
            }
            DateTime now = DateTime.Now;
            if (drawing) return;
            // animating should be as fast as it can.
            double elapsed = now.Subtract(previousRendererTime).TotalMilliseconds;
            if (!Parent.IsAnimating() && !Parent.IsUpdateRequested())
            {
                if (elapsed < mIdleTime)
                {
                    return;
                }
            }

            if (!Parent.IsAnimating() && !Parent.Parent.MouseHandler.IsMouseDown())
            {
                Parent.updated = false;
            }
            previousRendererTime = now;
            drawing = true;
            Parent.Paint();
            drawing = false;
        }
    }

}
