using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDLMMSharp.Base
{
    public class RectangleShape : IShape
    {
        public Rectangle Rectangle
        {
            get
            {
                return rect;
            }
            set
            {
                rect = value;
                RectangleChanged(this, rect);
            }
        }
        
        protected Rectangle rect;
        public int StrokeWidth;
        public Color ForeColor;
        public Color BackColor;
        public Image BackgroundImage
        {
            get
            {
                return mBackgroundImage;
            }
            set
            {
                mBackgroundImage = value;
                backgroundChanged = true;
            }
        }
        Image mBackgroundImage;
        bool backgroundChanged = false;
        public int ImageAlpha;
        public bool Dashed = false;
        protected Image cachedImage;
        protected bool disposed = false;
        public event EventHandler<Rectangle> RectangleChanged;
        public event EventHandler<Action<IRenderer>> OverlayRequested;
        public Size Size
        {
            get
            {
                return rect.Size;
            }
            set
            {
                SetSize(value);
            }
        }
        public Point Location
        {
            get
            {
                return rect.Location;
            }
            set
            {
                SetLocation(value);
            }
        }
        public Point Center
        {
            get
            {
                return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            }
        }
        public int Left
        {
            get
            {
                return rect.Left;
            }
        }
        public int Top
        {
            get
            {
                return rect.Top;
            }
        }
        public int Right
        {
            get
            {
                return rect.Right;
            }
        }
        public int Bottom
        {
            get
            {
                return rect.Bottom;
            }
        }
        public RectangleShape()
        {
            this.rect = new Rectangle(0, 0, 100, 100);
            this.StrokeWidth = 1;
            this.ForeColor = Color.Black;
        }

        public bool Visible = true;

        public RectangleShape(Rectangle rect, int strokeWidth, Color color)
        {
            this.rect = rect;
            this.StrokeWidth = strokeWidth;
            this.ForeColor = color;
        }
        public RectangleShape(Rectangle rect, int strokeWidth, Color color, Color backColor)
        {
            this.rect = rect;
            this.StrokeWidth = strokeWidth;
            this.ForeColor = color;
            this.BackColor = backColor;
        }
        public void SetSize(int width, int height)
        {
            this.SetSize(new Size(width, height));
        }
        public void SetSize(Size size)
        {
            this.rect.Size = size;
            if (RectangleChanged != null)
            {
                RectangleChanged(this, this.rect);
            }
        }
        virtual public void SetRectangle(Rectangle rect)
        {
            this.rect = rect;
            if (RectangleChanged != null)
            {
                RectangleChanged(this, this.rect);
            }

        }
        virtual public void SetLocation(int x,int y)
        {
            this.SetLocation(new Point(x, y));
        }
        virtual public void SetLocation(Point pt)
        {
            this.rect.Location = pt;
            if (RectangleChanged != null)
            {
                RectangleChanged(this, this.rect);
            }
        }
        virtual public void Dispose()
        {
            if (disposed) return;
            if(cachedImage != null)
            {
                cachedImage.Dispose();
                cachedImage = null;
            }
            if(BackgroundImage != null)
            {
                BackgroundImage.Dispose();
                BackgroundImage = null;
            }
            foreach (Delegate del in this.OverlayRequested.GetInvocationList())
                Delegate.RemoveAll(del, del);
            foreach (Delegate del in this.RectangleChanged.GetInvocationList())
                Delegate.RemoveAll(del, del);
            disposed = true;
        }

        virtual public bool IsHit(int x, int y)
        {
            if (disposed) return false;
            return rect.IntersectsWith(new Rectangle(x, y, 1, 1));
        }
        public ImageLayout ImageLayout = ImageLayout.Stretch;
        virtual protected void DrawBackground(IRenderer gc)
        {
            if (BackColor.A != 0)
            {
                gc.fillRect(rect, BackColor.ToArgb());
            }
            if (BackgroundImage == null) return;
            Rectangle targetRect = rect;
            Point location = rect.Location;
            if (ImageLayout!= ImageLayout.Stretch)
            {
                Size size = BackgroundImage.Size;
                Size newSize = size;
                if(size.Width > targetRect.Width)
                {
                    // shrink by width
                    double ratioW = ((double)targetRect.Width) / size.Width;
                    int newHeight = (int)(targetRect.Height * ratioW);
                    newSize.Height = newHeight;
                }
                if(size.Height > targetRect.Height)
                {
                    // shrink by height
                    double ratioH = ((double)targetRect.Height) / size.Height;
                    int newWidth = (int)(targetRect.Width * ratioH);
                    newSize.Width = newWidth;
                }
                if(ImageLayout == ImageLayout.Center)
                {
                    location.X = rect.X + rect.Width / 2 - newSize.Width / 2;
                    location.Y = rect.Y + rect.Height / 2 - newSize.Height / 2;
                }
                targetRect.Size = newSize;
            }
            if (cachedImage == null || backgroundChanged)
            {
                backgroundChanged = false;
                cachedImage = new Bitmap(BackgroundImage, targetRect.Width, targetRect.Height);
            }
            else
            {
                if (cachedImage.Width != targetRect.Width || cachedImage.Height != targetRect.Height)
                {
                    cachedImage.Dispose();
                    cachedImage = new Bitmap(BackgroundImage, targetRect.Width, targetRect.Height);
                }
            }
            gc.drawImage(cachedImage, location.X, location.Y, targetRect.Width, targetRect.Height);
        }
        protected IDisposable clipObject;
        virtual protected void SetClip(IRenderer gc)
        {
            //clipObject = gc.SetRectangleClipping(rect.X, rect.Y, rect.Width, rect.Height);
        }
        virtual protected void UnsetClip(IRenderer gc)
        {
            if (clipObject == null) return;
           // gc.DisposeObject(clipObject);
            clipObject = null;
        }
        virtual protected void DrawForeGround(IRenderer gc)
        {
            if (ForeColor.A == 0 || StrokeWidth == 0) return;
            gc.drawRect(rect.X, rect.Y, rect.Width, rect.Height, ForeColor.ToArgb(), Dashed, StrokeWidth);
        }
        virtual public void Paint(IRenderer gc)
        {
            if (!Visible) return;
            if (disposed) return;
            if (rect.Width <= 0 || rect.Height <= 0) return;
            SetClip(gc);
            DrawBackground(gc);
            DrawForeGround(gc);
            UnsetClip(gc);
        }
        public static void DrawRectangle(IRenderer gc, int x, int y, int w, int h, int borderwidth, int bordercolor,
            int backcolor)
        {
            int alphaback = (int)(((backcolor & 0xff000000) >> 24) & 0xff);
            if (alphaback > 0)
            {
                gc.fillRect(x, y, w, h,backcolor);
            }
            if (borderwidth > 0)
            {
                gc.drawRect(x, y, w, h, bordercolor, false, 1);
            }
        }
    }
}
