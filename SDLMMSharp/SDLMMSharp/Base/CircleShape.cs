﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp.Base
{
    public class CircleShape:RectangleShape
    {
        public CircleShape()
        {

        }
        public CircleShape(Rectangle rect):base(rect,1,Color.Black)
        {
            
        }
      
        public int Radius
        {
            get
            {
                return Math.Min(rect.Width / 2, rect.Height/2);
            }
            set
            {
                Point center = Center;
                int x = center.X - value;
                int y = center.Y - value;
                int width = value * 2;
                int height = value * 2;
                this.Rectangle = new Rectangle(x, y, width, height);
            }
        }

        protected override void DrawBackground(IRenderer gc)
        {
            if (BackColor.A != 0)
            {
                gc.fillEllipse(rect.X,rect.Y,rect.Width,rect.Height, BackColor.ToArgb());
            }
            if (BackgroundImage == null) return;
            if (cachedImage == null ||cachedImage.IsDisposed)
            {
                if (BackgroundImage != null && BackgroundImage.image != null && !BackgroundImage.IsDisposed)
                {
                    cachedImage = BitmapWrap.FromBitmap(new Bitmap(BackgroundImage, rect.Width, rect.Height));
                }
            }
            else
            {
                if (cachedImage.Width != rect.Width || cachedImage.Height != rect.Height)
                {
                    if (cachedImage.image != BackgroundImage.image)
                    {
                        cachedImage.DoDispose();
                    }
                    if (BackgroundImage != null && BackgroundImage.image != null && !BackgroundImage.IsDisposed)
                    {
                        cachedImage = BitmapWrap.FromBitmap(new Bitmap(BackgroundImage, rect.Width, rect.Height));
                    }
                }
            }
            if (cachedImage != null && !cachedImage.IsDisposed)
            {
                gc.drawImage(cachedImage, rect.X, rect.Y, rect.Width, rect.Height);
            }
        }
        protected override void DrawForeGround(IRenderer gc)
        {
            if (ForeColor.A == 0 || StrokeWidth == 0) return;           
            gc.drawEllipse(rect.X, rect.Y, rect.Width, rect.Height, ForeColor.ToArgb(), StrokeWidth);
        }
        protected override void SetClip(IRenderer gc)
        {
            //clipObject = gc.SetCircleClipping(this.Center, this.Radius);
        }
        protected override void UnsetClip(IRenderer gc)
        {
           // base.UnsetClip(gc);
        }
    }
}
