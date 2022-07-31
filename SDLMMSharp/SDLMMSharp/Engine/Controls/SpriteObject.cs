using SDLMMSharp.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDLMMSharp.Engine.Controls
{
    public class SpriteObject : DraggableTarget
    {
        public event EventHandler Load;
        public event EventHandler Click;
        public event MouseEventHandler MouseClick;
        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MouseDoubleClick;
        public IRendererDelegates.OnKeyboardAction KeyAction;
        
        public bool CanClick = true;
        
        public void ApplyChildLocation(DraggableTarget target)
        {
            Point pt = target.Location;
            pt.Offset(this.Location);
            target.Location = pt;
        }
        public override void Dispose()
        {
            if (this.Controls != null)
            {
                for (LinkedListNode<IDraggableTarget> targetNode=this.Controls.First; targetNode != null; targetNode=targetNode.Next)
                {
                    try
                    {
                        IDraggableTarget target = targetNode.Value;
                        if (target == null)
                        {
                            continue;
                        }
                        target.Dispose();
                    }
                    catch(Exception ee)
                    {

                    }
                }
            }
            base.Dispose();
            if (this.Controls != null)
            {
                this.Controls.Clear();
            }
        }
        public virtual void AddControl(DraggableTarget target)
        {
            ApplyChildLocation(target);
            target.ImageLayerHandle = this.Controls.AddLast(target);
        }
        protected virtual bool GetStyle(ControlStyles selectable)
        {
            return true;
        }

        protected virtual void SetStyle(ControlStyles selectable, bool value)
        {
            
        }
        public AutoScaleMode AutoScaleMode;

        public bool AutoSize;

        public bool UseVisualStyleBackColor;

        public int TabIndex;

        public ResizerClazz Resizer;

        public AnchorStyles Anchor;

        public string Name;

        public override void Paint(IRenderer gc)
        {
            if (!loaded)
            {
                loaded = true;
                OnLoad();
            }
            base.Paint(gc);
        }

        protected virtual bool mouseDoubleClickHandler(DraggableTarget sender, int x, int y)
        {
            if (MouseDoubleClick != null)
            {
                MouseEventArgs args = new MouseEventArgs(MouseButtons.Left, 2, x, y, 0);
                MouseDoubleClick(sender, args);
            }
            return true;
        }
        
        protected virtual bool mouseActionHandler(DraggableTarget sender, bool on, int x, int y)
        {
            if (on)
            {
                MouseEventArgs args = new MouseEventArgs(MouseButtons.Left, 1, x, y, 0);
                if (MouseDown != null)
                {
                    MouseDown(this, args);
                    return true;
                }               
                
                return false;
            }
            else
            {
                MouseEventArgs args = new MouseEventArgs(MouseButtons.Left, 1, x, y, 0);
                if (MouseClick != null)
                {
                    MouseClick(this, args);
                    return true;
                }
                else if (Click != null)
                {
                    Click(this, args);
                    return true;
                }
            }
            return on;
        }

        public virtual void InitializeComponent()
        {
            
        }
        bool loaded = false;

        public bool DoubleBuffered { get; set; }
        public ImageLayout BackgroundImageLayout
        {
            get
            {
                return shape.ImageLayout;
            }
            set
            {
                shape.ImageLayout = value;
            }
        }

        public virtual void OnLoad()
        {
            if (Load != null)
            {
                Load(this, EventArgs.Empty);
            }
        }


        public SpriteObject(bool runInit=true,bool CanResize=false)
        {
            if (CanResize)
            {
                Resizer = new ResizerClazz(this);
            }
            //this.shape.StrokeWidth = 2;
            //this.shape.ForeColor = Color.White;
            supportEnterItem = true;
            
            this.mouseActionDelegate += mouseActionHandler;
            this.mouseDoubleClickDelegate += mouseDoubleClickHandler;
            if (runInit)
            {
                InitializeComponent();
            }
        }

        
        public IDraggableTarget AdditionalHitTest(int x, int y)
        {
            if (this.Resizer != null)
            {
                if (this.Resizer.WidthAdjuster != null && this.Resizer.WidthAdjuster.IsHit(x, y))
                {
                    return this.Resizer.WidthAdjuster;
                }
                if (this.Resizer.HeightAdjuster != null && this.Resizer.HeightAdjuster.IsHit(x, y))
                {
                    return this.Resizer.HeightAdjuster;
                }
                if (this.Resizer.BothAdjuster != null && this.Resizer.BothAdjuster.IsHit(x, y))
                {
                    return this.Resizer.BothAdjuster;
                }
            }

            return null;
        }
        public override SpriteObject GetSpriteObject()
        {
            return this;
        }

        public void Invalidate()
        {

        }

        public virtual void SuspendLayout()
        {

        }
        public virtual void PerformLayout()
        {

        }

        public virtual void ResumeLayout(bool v = true)
        {
            
        }
    }
}
