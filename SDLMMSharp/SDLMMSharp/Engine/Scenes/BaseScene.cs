using SDLMMSharp.Engine.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp.Engine.Scenes
{
    public class BaseScene : IScene
    {
        Point offset;
        double zoom = 1.0;
        protected Point constraintPosition;
        BaseEngine owner;
        public BaseEngine Parent
        {
            get
            {
                return this.owner;
            }
        }
        public BaseScene(BaseEngine owner)
        {
            this.owner = owner;
        }
        public virtual void End()
        {
            
        }

        public virtual IEnumerable<DraggableTarget> GetClickableObjects()
        {
            yield break;
        }
        
        public virtual Point GetConstraintPosition(int x, int y)
        {
            constraintPosition.X = x;
            constraintPosition.Y = y;
            return constraintPosition;
        }

        public Point GetOffset()
        {
            return offset;
        }

        public virtual IEnumerable<DraggableTarget> GetOverlayToolObjects()
        {
            yield break;
        }

        public void GetSelection(IDraggableTarget draggable)
        {
           
        }

        public double GetZoom()
        {
            return zoom;
        }

        public virtual void Paint(IRenderer gc)
        {
            
        }

        public virtual void Refresh()
        {
            
        }

        public void SetOffset(int x, int y)
        {
            offset.X = x;
            offset.Y = y;
        }

        public virtual void SetSelection(IDraggableTarget draggable)
        {
            
        }

        public void SetZoom(double zoom)
        {
            this.zoom = zoom;
        }

        public virtual void Start()
        {
            
        }

        public void CancelDrag()
        {
            
        }

        public virtual bool IsWorldDraggable()
        {
            return false;
        }
        public virtual bool HandleWorldDrag(Point mouseDown,int x,int y)
        {
            return false;
        }

        public EngineRenderer GetRenderer()
        {
            return Parent.Renderer;
        }

        public IRenderer GetCanvas()
        {
            EngineRenderer renderer = GetRenderer();
            if (renderer == null) return null;
            return renderer.GetCanvas();
        }

        public virtual void EnterItem(IDraggableTarget spriteObject)
        {
            
        }
    }
}
