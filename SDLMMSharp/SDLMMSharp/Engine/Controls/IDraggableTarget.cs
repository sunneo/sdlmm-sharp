using SDLMMSharp.Base;
using SDLMMSharp.Engine.Scenes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp.Engine.Controls
{
    public interface IDraggableTarget:IShape,IMouseHandler
    {
        void SetPosition(int x, int y);
        Point GetPosition();
        void SetSize(int width, int height);
        Size GetSize();

        Rectangle GetRectangle();
        void SetRectangle(Rectangle rect);

        bool IsBeloneScene(IScene scene);
        bool CanEnter();
        SpriteObject GetSpriteObject();
    }
}
