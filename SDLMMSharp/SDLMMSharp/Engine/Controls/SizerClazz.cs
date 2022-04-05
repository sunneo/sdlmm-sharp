using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp.Engine.Controls
{
    public class SizerClazz : DraggableTarget
    {

        public Rectangle rect;


        public Action<bool> setSelectedDelegate;

        public virtual bool BaseMouseAction(bool on, int x, int y)
        {
            return base.mouseAction(on, x, y);
        }
        public virtual bool BaseMouseMoved(bool on, int x, int y)
        {
            return base.mouseMoved(on, x, y);
        }
        public override bool IsHit(int x, int y)
        {
            if (rect == null) return false;
            return rect.Contains(x, y);
        }
        public override void SetSelected(bool value)
        {
            if (setSelectedDelegate != null)
            {
                setSelectedDelegate(value);
            }
        }
        public override bool mouseAction(bool on, int x, int y)
        {
            return base.mouseAction(on, x, y);
        }
        public override bool mouseMoved(bool on, int x, int y)
        {
            return base.mouseMoved(on, x, y);
        }
        public SizerClazz()
        {

        }
    }

}
