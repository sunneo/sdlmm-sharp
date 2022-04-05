using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp.Engine.Controls
{
    public class ResizerClazz : DraggableTarget
    {
        public SpriteObject Target;
        ResizerClazz me;
        public SizerClazz WidthAdjuster = new SizerClazz();
        public SizerClazz HeightAdjuster = new SizerClazz();
        public SizerClazz BothAdjuster = new SizerClazz();
        Rectangle origRect;
        public ResizerClazz(SpriteObject Target)
        {
            me = this;
            this.Target = Target;
            WidthAdjuster.setSelectedDelegate = WidthAdjusterSetSelected;
            HeightAdjuster.setSelectedDelegate = HeightAdjusterSetSelected;
            HeightAdjuster.setSelectedDelegate = BothAdjusterSetSelected;

            WidthAdjuster.mouseActionDelegate = WidthAdjusterMouseAction;
            HeightAdjuster.mouseActionDelegate = HeightAdjusterMouseAction;
            HeightAdjuster.mouseActionDelegate = BothAdjusterMouseAction;

            WidthAdjuster.mouseActionDelegate = WidthAdjusterMouseMoved;
            HeightAdjuster.mouseActionDelegate = HeightAdjusterMouseMoved;
            HeightAdjuster.mouseActionDelegate = BothAdjusterMouseMoved;
        }
        protected virtual bool WidthAdjusterMouseAction(bool on,int x,int y)
        {
            bool ret = base.mouseAction(on, x, y);
            origRect = Target.GetRectangle();
            return ret;
        }
        protected virtual bool WidthAdjusterMouseMoved(bool on,int x,int y)
        {
            if (!on)
            {
                return WidthAdjuster.BaseMouseMoved(on, x, y);
            }
            int width = x - WidthAdjuster.mouseDownPosition.X;

            Rectangle rect =origRect;
            rect.Width += width;
            if (rect.Width < 5) rect.Width = 5;
            Target.SetRectangle(rect);
            return true;
        }
        protected virtual void WidthAdjusterSetSelected(bool value)
        {
            this.SetSelected(value);
        }
        protected virtual bool HeightAdjusterMouseAction(bool on, int x, int y)
        {
            bool ret = base.mouseAction(on, x, y);
            origRect = Target.GetRectangle();
            return ret;
        }
        protected virtual bool HeightAdjusterMouseMoved(bool on, int x, int y)
        {
            if (!on)
            {
                return HeightAdjuster.BaseMouseMoved(on, x, y);
            }
            int height = y - HeightAdjuster.mouseDownPosition.Y;
            Rectangle rect = origRect;
            rect.Height += height;
            if (rect.Height < 5) rect.Height = 5;
            Target.SetRectangle(rect);
            return true;
        }
        protected virtual void HeightAdjusterSetSelected(bool value)
        {
            this.SetSelected(value);
        }
        protected virtual bool BothAdjusterMouseMoved(bool on, int x, int y)
        {
            if (!on)
            {
                return BothAdjuster.BaseMouseMoved(on, x, y);
            }
            if (Target.shape == null) return false;
            int width = x - BothAdjuster.mouseDownPosition.X;
            int height = y - BothAdjuster.mouseDownPosition.Y;

            Rectangle rect = origRect;
            rect.Width += width;
            rect.Height += height;
            if (rect.Width < 5) rect.Width = 5;
            if (rect.Height < 5) rect.Height = 5;
            Target.SetRectangle(rect);
            return true;
        }
        protected virtual bool BothAdjusterMouseAction(bool on, int x, int y)
        {
            bool ret = base.mouseAction(on, x, y);
            origRect = Target.GetRectangle();
            return ret;
        }
        protected virtual void BothAdjusterSetSelected(bool value)
        {
            this.SetSelected(value);
        }
    }
}
