using SDLMMSharp.Base;
using SDLMMSharp.Engine.Controls;
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
    public class EngineMouseHandler
    {
        protected BaseEngine owner;
        public EngineMouseHandler me;
        volatile bool mouseIsDown = false;
        volatile bool enabled = false;
        int MouseClickTimeMs = 400;
        public IDraggableTarget dragItem;

        public class MouseEvent
        {
            public int btn;
            public int x;
            public int y;
            public bool isDown;
            public int clickCnt = 0;
            public DateTime clickTime;
        }
        MouseEvent mouseEvt = new MouseEvent();
        public bool IsMouseDown()
        {
            return mouseIsDown;
        }
        public virtual void Start()
        {
            owner.MouseAction += Ctrl_MouseAction;
            owner.MouseMove += Ctrl_MouseMove;
            owner.MouseWheel += Ctrl_MouseWheel;
            
            this.enabled = true;
        }
        public virtual void End()
        {
            owner.MouseAction -= Ctrl_MouseAction;
            owner.MouseMove -= Ctrl_MouseMove;
            owner.MouseWheel -= Ctrl_MouseWheel;
            this.enabled = false;
        }

        private void Ctrl_MouseWheel(int x, int y, int scrollAmount)
        {
            if (!enabled) return;
            OnMouseWheel(x, y, scrollAmount);
        }

        private void Ctrl_MouseMove(int x, int y, int btn, bool ison)
        {
            if (!enabled) return;
            OnMouseMove(x, y, btn, ison);
        }


        private void Ctrl_MouseAction(int x, int y, int btn, bool ison)
        {
            if (!enabled) return;
            if (ison)
            {
                mouseEvt.clickTime = DateTime.Now;
                mouseEvt.x = x;
                mouseEvt.y = y;
                mouseEvt.btn = btn;
                mouseEvt.isDown = true;
                OnMouseDown(x, y, btn);
            }
            else
            {
                if(mouseEvt.isDown && btn == mouseEvt.btn)
                {
                    DateTime dt = DateTime.Now;
                    if (dt.Subtract(mouseEvt.clickTime).TotalMilliseconds <= MouseClickTimeMs)
                    {
                        mouseEvt.clickCnt += 1;
                    }
                }
                mouseEvt.clickTime = DateTime.Now;
                mouseEvt.x = x;
                mouseEvt.y = y;
                mouseEvt.btn = btn;
                mouseEvt.isDown = false;
                if(mouseEvt.clickCnt >= 2)
                {
                    mouseEvt.clickCnt = 0;
                    OnMouseDoubleClick(x, y, btn);
                    return;
                }
                OnMouseUp(x, y, btn);
            }

        }
        protected virtual DraggableTarget CreateWorldDraggable()
        {
            DraggableTarget worldDraggable = new DraggableTarget();
            worldDraggable.mouseMovedDelegate = (bool on, int x, int y) =>
            {
                IScene scene = owner.GetCurrentScene();
                if (on && scene.IsWorldDraggable())
                {
                    Point mouseDown = worldDraggable.mouseDownPosition;
                    if (scene.HandleWorldDrag(mouseDown, x, y))
                    {
                        return true;
                    }
                    Point newOffset = scene.GetOffset();
                    newOffset.X += x - mouseDown.X;
                    newOffset.Y += y - mouseDown.Y;
                    scene.SetOffset(newOffset.X, newOffset.Y);
                    return true;
                }
                return false;
            };
            
            return worldDraggable;
        }
        DraggableTarget mWorldDraggable;
        DraggableTarget worldDraggable
        {
            get
            {
                if(mWorldDraggable == null)
                {
                    mWorldDraggable = CreateWorldDraggable();
                }
                return mWorldDraggable;
            }
        }
        protected virtual void OnMouseWheel(int x, int y, int cnt)
        {

        }
        protected virtual void OnMouseMove(int x, int y, int btn, bool ison)
        {

        }
        protected virtual void OnMouseDown(int x,int y, int btn)
        {

        }
        
  
        public void MouseUpDropItem(int x, int y, IDraggableTarget e)
        {
            MouseUpDropItem(x, y, e, true);
        }
        volatile bool handlingMouseUpDrop = false;
        public void MouseUpDropItem(int x, int y, IDraggableTarget e, bool triggerGroupAction)
        {

            EngineMouseHandler pthis = me;
            if (pthis.handlingMouseUpDrop) return;
            pthis.handlingMouseUpDrop = true;
            try
            {
                pthis.MouseUpDropItemNoRecursive(x, y, e, triggerGroupAction);
            }
            catch (Exception ee)
            {

            }
            pthis.handlingMouseUpDrop = false;

        }

        private void MouseUpDropItemNoRecursive(int x, int y, IDraggableTarget e, bool triggerGroupAction)
        {
            
        }

        protected virtual void OnMouseUp(int x, int y, int btn)
        {
            mouseIsDown = false;
            owner.GetCurrentScene().CancelDrag();
            EngineMouseHandler pthis = me;
            if (btn != 0)
            {
                return;
            }

            owner.InvalidateRenderer();
            pthis.mouseIsDown = false;
            if (owner.Renderer.IsAnimating())
            {
                /// suppress any mouse action when animating
                return;
            }
            if (pthis.dragItem != null)
            {
                MouseUpDropItem(x, y, pthis.dragItem);
            }
            pthis.dragItem = null;
        }
        public static IEnumerable<Object> newDoubleClickAnimation(IScene scene, RectangleShape shape, Action action)
        {
            //IScene scene = Parent.getRenderer().scene.get();
            Point sceneOffset = scene.GetOffset();
            IRenderer canvas = scene.GetCanvas();


            DateTime clickTime = DateTime.Now;
            int upperbound = 25;
            int index = 0;
            
            for(int i=0; i<index; ++i)
            {
                DateTime now = DateTime.Now;
                if (now.Subtract(clickTime).TotalMilliseconds >= 500)
                {
                    index = upperbound;
                    break;
                }
            }
            if (action != null)
            {
                action.Invoke();
            }


            double alpha = 1.0 - (1.0 / upperbound * index);
            double sizeRatio = 1.0 + (1.0 / upperbound * index);
            Rectangle origRect = Rectangle.Empty;

            if (shape != null)
            {
                origRect = shape.Rectangle;
                Rectangle newRect = new Rectangle(origRect.X, origRect.Y, (int)(origRect.Width * sizeRatio),
                        (int)(origRect.Height * sizeRatio));
                newRect.X -= (newRect.Width - origRect.Width) / 2; /// align to center
                newRect.Y -= (newRect.Height - origRect.Height) / 2; /// align to center
                if (shape.BackgroundImage != null)
                {
                    canvas.drawImage(shape.BackgroundImage, newRect.X, newRect.Y, newRect.Width, newRect.Height, (int)(255 * alpha));
                }
                else
                {
                    canvas.drawRect(newRect.X, newRect.Y, newRect.Width, newRect.Height, shape.ForeColor.ToArgb(), false, 1);
                }
                
            }
            yield break;
	}
    protected virtual void OnMouseDoubleClick(int x,int y,int btn)
        {
            
            EngineMouseHandler pthis = me;
            if (btn != 0)
            {
                return;
            }
            owner.InvalidateRenderer();
            if (owner.Renderer.IsAnimating())
            {
                /// suppress any mouse action when animating
                owner.Renderer.CancelAnimate();
                return;
            }

            Point origPoint = new Point(x, y);
            Point refTransPoint = origPoint; // 

            IScene scene = owner.Renderer.GetCurrentScene();
            

            IDraggableTarget item = GetDraggableTarget(refTransPoint.X, refTransPoint.Y);

            if (item != null && item.CanEnter())
            {
                RectangleShape shape = null;
                if (item.GetSpriteObject() != null)
                {
                    shape = item.GetSpriteObject().shape;
                }
                else
                {
                    if (item is RectangleShape) {
                        shape = ((RectangleShape)item);
                    }
                }
                IEnumerable<Object> animate = newDoubleClickAnimation(scene, shape, ()=> {
                    owner.Renderer.GetCurrentScene().EnterItem(item);
                });
                owner.Renderer.Animate(owner.Renderer.CreateSnapshotAdditionEffect(animate), true);
            }
        }
        public EngineMouseHandler(BaseEngine owner)
        {
            this.owner = owner;
            me = this;
        }
        public virtual void ResetCursor()
        {
            this.owner.ResetCursor();
        }

        public EngineMouseHandler()
        {
            me = this;
        }
        public virtual void CancelDrag()
        {
        
        }
        public virtual void CancelMouseDown()
        {
            mouseIsDown = false;
        
        }

        public IDraggableTarget GetDraggableTarget(int x, int y)
        {
            return GetDraggableTarget(x, y, false);
        }
        public IDraggableTarget GetDraggableTarget(int x, int y, bool selectOverlay)
        {
            
            IEnumerable<DraggableTarget> target = owner.GetCurrentScene().GetClickableObjects();
            if (selectOverlay)
            {
                target = owner.GetCurrentScene().GetOverlayToolObjects();
            }
           

            foreach (DraggableTarget entry in target)
            {
                try
                {
                    if (entry == null) continue;
                    if (!(entry is SpriteObject)) continue;


                    SpriteObject attr = entry as SpriteObject;
                    if (attr == null || attr.isDisposed())
                    {
                        continue;
                    }

                    if (!attr.Enabled)
                    {
                        continue;
                    }
                    
                    if (attr != null)
                    {
                        IDraggableTarget additionalTest = attr.AdditionalHitTest(x, y);
                        if (additionalTest != null)
                        {
                            return additionalTest;
                        }
                        if (attr.IsHit(x, y))
                        {
                            if (attr.Resizer != null)
                            {
                                if (attr.Resizer.WidthAdjuster.IsHit(x, y))
                                {
                                    return attr.Resizer.WidthAdjuster;
                                }
                                else if (attr.Resizer.HeightAdjuster.IsHit(x, y))
                                {
                                    return attr.Resizer.HeightAdjuster;
                                }
                                else if (attr.Resizer.BothAdjuster.IsHit(x, y))
                                {
                                    return attr.Resizer.BothAdjuster;
                                }
                            }
                            return attr;
                        }
                    }
                }
                catch (Exception ee)
                {
                    
                }

            }
            return null;
        }
    }
}
