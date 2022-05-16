using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp.Base
{
    public class LabelShape : RectangleShape
    {
        public class DrawLabelParam
        {
            public bool wordwrap;
            public bool alignCenter;
            public ContentAlignment hAlign;
            public bool vCenter;
            public bool invertBold;
            public Font font;
            public int color;
            public int bgcolor;
            public String toString()
            {
                return String.Format("DrawLabelParam {wordwrap={0},alignCenter={1}(v={2},h={3}),color={4,XXXXXXXX},bgclolor={5,XXXXXXXX} }"
                            , wordwrap
                            , alignCenter
                            , vCenter
                            , hAlign
                            , color
                            , bgcolor);
            }
        }

        public String Text = "";
        public Font Font;
        public bool vCenter;
        
        public ContentAlignment hAlign = ContentAlignment.TopLeft;
        /**
         * true to draw a invert font onto (x,y-2)
         */
        public bool invertBold = false;
        public bool circledBackground = false;
        public CircleShape circleShape;
        /**
         * get circle shape of this label
         * it will only be available when {@link #circledBackground} is set true
         * @return
         */
        public CircleShape getCircleShape()
        {
            return circleShape;
        }
        public static Point GetInvertBoldOffset()
        {
            return new Point(0, -1);
        }

        public bool alignCenter = false;

        public bool wordwrap = false;

        public LabelShape(String txt) : base()
        {
            this.StrokeWidth = 0;
            if (String.IsNullOrEmpty(txt)) return;
            if (this.Text == null) return;
            this.Text = txt.Trim();
            
        }
        public DrawLabelParam GetDrawLabelParam()
        {
            DrawLabelParam parm = new DrawLabelParam();
            parm.wordwrap = this.wordwrap;
            parm.alignCenter = this.alignCenter;
            parm.hAlign = this.hAlign;
            parm.vCenter = this.vCenter;
            parm.invertBold = this.invertBold;
            parm.font = this.Font;
            parm.color = this.ForeColor.ToArgb();
            parm.bgcolor = this.BackColor.ToArgb();           
            return parm;
        }
        override public void Paint(IRenderer gc)
        {
            if (!Visible) return;
            if (this.circleShape != null)
            {
                this.circleShape.Paint(gc);
            }
            else
            {
                base.Paint(gc);
            }
            Rectangle rect = this.rect;
            DrawLabelParam parm = GetDrawLabelParam();
            
           // LabelShape.DrawLabel(gc, Text, rect.X, rect.Y, rect.Width, rect.Height, parm.color, parm.bgcolor,0);               
            LabelShape.DrawLabel(gc, Text, rect, parm);
        }

      
        public override bool IsHit(int x, int y)
        {
            if (this.circleShape != null)
            {
                return this.circleShape.IsHit(x, y);
            }
            return base.IsHit(x, y);
        }

        public static Rectangle DrawLabel(IRenderer gc, String text, Rectangle rect, DrawLabelParam parms)
        {
            if (gc == null || gc.IsDisposed()) return new Rectangle(0, 0, 1, 1);
            Size size = Size.Empty;
            Point pos = rect.Location;
            if (text == null) return new Rectangle(0, 0, 1, 1);
            if (parms.wordwrap)
            {
                size = gc.MeasureString(text, parms.font, rect.Width).ToSize();
            }
            else
            {
                size = gc.MeasureString(text, parms.font).ToSize();
            }

            ContentAlignment hCenterValue = parms.hAlign;
            bool vCenterValue = parms.vCenter;
            if (parms.alignCenter)
            {
                hCenterValue = ContentAlignment.MiddleCenter;
                vCenterValue = true;
            }
            if (hCenterValue == ContentAlignment.MiddleCenter ||
                hCenterValue == ContentAlignment.TopCenter ||
                hCenterValue == ContentAlignment.BottomCenter)
            {
                pos.X = rect.X + rect.Width / 2 - size.Width / 2;
            }
            if (vCenterValue)
            {
                pos.Y = rect.Y + rect.Height / 2 - size.Height / 2;
            }

            gc.fillRect(pos.X, pos.Y, (int)size.Width, (int)size.Height, parms.bgcolor);
            int colorInt = unchecked((int)(~(parms.color) | 0xff000000));
            if (!parms.wordwrap)
            {
                if (parms.invertBold)
                {
                    gc.drawString(text, pos.X + GetInvertBoldOffset().X, pos.Y + GetInvertBoldOffset().Y, colorInt, parms.font);
                }
                gc.drawString(text, pos.X, pos.Y, parms.color, parms.font);
            }
            else
            {
                if (parms.invertBold)
                {
                    gc.drawString(text, Rectangle.FromLTRB( pos.X + GetInvertBoldOffset().X, pos.Y + GetInvertBoldOffset().Y, size.Width, size.Height), colorInt, parms.font);
                }

                gc.drawString(text, Rectangle.FromLTRB(pos.X, pos.Y, Math.Max(size.Width, rect.Width - (pos.X - rect.X)), Math.Max(size.Height, rect.Height - (pos.Y - rect.Y))), parms.color, parms.font);
            }
            return new Rectangle(pos.X, pos.Y, (int)size.Width, (int)size.Height);
        }
        public static void DrawLabel(IRenderer gc, String text, int x, int y, int w, int h, int color, int backcolor, int borderWidth)
        {
            DrawLabel(gc, text, x, y, w, h, color, backcolor, borderWidth, 0, 0);
        }
        public static void DrawLabel(IRenderer gc, String text, int x, int y, int w, int h, int color, int backcolor, int borderWidth, int horizonPad, int verticalPad)
        {

            if (w < 0)
            {
                w = gc.MeasureString(text).ToSize().Width;
            }
            if (h < 0)
            {
                h = gc.MeasureString(text).ToSize().Height;
            }
            RectangleShape.DrawRectangle(gc, x - horizonPad, y - verticalPad, w + horizonPad * 2, h + verticalPad * 2, borderWidth, color, backcolor);
            gc.drawString(text, x, y, color);
        }

        public override String ToString()
        {
            if (this.disposed)
            {
                return "LabelShape(*DISPOSED*)";
            }
            return "LabelShape" + String.Format("(rect={0},text={1})"
                        , this.rect
                        , this.Text
                        );

        }
    }

}
