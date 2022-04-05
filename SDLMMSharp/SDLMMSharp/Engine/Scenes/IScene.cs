using SDLMMSharp.Engine.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp.Engine.Scenes
{
    public interface IScene
    {
        Point GetOffset();
        void SetOffset(int x, int y);

        double GetZoom();

        void SetZoom(double zoom);

        void SetSelection(IDraggableTarget draggable);
        void GetSelection(IDraggableTarget draggable);
        void Start();
        void End();
        void Refresh();
        void Paint(IRenderer gc);

        Point GetConstraintPosition(int x, int y);

        IEnumerable<DraggableTarget> GetClickableObjects();
        IEnumerable<DraggableTarget> GetOverlayToolObjects();
        void CancelDrag();
        bool IsWorldDraggable();
        bool HandleWorldDrag(Point mouseDown, int x, int y);
        EngineRenderer GetRenderer();
        IRenderer GetCanvas();
        void EnterItem(IDraggableTarget spriteObject);
    }
}
