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
        protected Point offset;
        protected double zoom = 1.0;
        protected Point constraintPosition;
        protected BaseEngine owner;
        protected LinkedList<IDraggableTarget> draggables = new LinkedList<IDraggableTarget>();
        protected LinkedList<IDraggableTarget> overlayTools = new LinkedList<IDraggableTarget>();
        IDraggableTarget selection;
        public Image BackgroundImage;
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
            Parent.KeyboardAction -= OnKeyboardAction;
        }
        public IDraggableTarget AddDraggableObject(IDraggableTarget obj)
        {
            obj.ImageLayerHandle = draggables.AddFirst(obj);
            return obj;
        }
        public IDraggableTarget AddOverlayToolObject(IDraggableTarget obj)
        {
            obj.ImageLayerHandle = overlayTools.AddFirst(obj);
            return obj;
        }
        public virtual void InitializeComponent()
        {
            draggables.Clear();
            overlayTools.Clear();
        }

        public virtual IEnumerable<IDraggableTarget> GetClickableObjects()
        {
            return draggables;
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

        public virtual IEnumerable<IDraggableTarget> GetOverlayToolObjects()
        {
            return overlayTools;
        }

        public virtual IDraggableTarget GetSelection()
        {
            return selection;
        }

        public double GetZoom()
        {
            return zoom;
        }

        public virtual void Paint(IRenderer gc)
        {
            gc.Clear(unchecked((int)(0xffffffff)));
            if (this.BackgroundImage != null) 
            {
                Rectangle rect = gc.GetClientArea();
                gc.drawImage(BackgroundImage, 0, 0, rect.Width, rect.Height);
            }
            for(LinkedListNode<IDraggableTarget> obj = this.draggables.First; obj != null; obj = obj.Next)
            {
                if (obj.Value == null) continue;
                if (!(obj.Value is DraggableTarget)) continue;
                DraggableTarget draggable = (DraggableTarget)obj.Value;
                draggable.Paint(gc);
            }
            for (LinkedListNode<IDraggableTarget> obj = this.overlayTools.First; obj != null; obj = obj.Next)
            {
                if (obj.Value == null) continue;
                if (!(obj.Value is DraggableTarget)) continue;
                DraggableTarget draggable = (DraggableTarget)obj.Value;
                draggable.Paint(gc);
            }
        }

        public virtual void Refresh()
        {
            InitializeComponent();
        }

        public void SetOffset(int x, int y)
        {
            offset.X = x;
            offset.Y = y;
        }

        public virtual void SetSelection(IDraggableTarget draggable)
        {
            if (selection == draggable) return;
            SpriteObject obj = null;
            do {
                if (selection == null) break;
                obj = selection.GetSpriteObject();
                if (obj == null) break;
                obj.SetSelected(false);
            }while (false) ;
            selection = draggable;
            do
            {
                if (selection == null) break;
                obj = selection.GetSpriteObject();
                if (obj == null) break;
                obj.SetSelected(true);
            } while (false);
        }

        public void SetZoom(double zoom)
        {
            this.zoom = zoom;
        }

        public virtual void Start()
        {
            Parent.KeyboardAction += OnKeyboardAction;
            InitializeComponent();
        }

        protected virtual void OnKeyboardAction(int keycode, bool ctrl, bool ison)
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
