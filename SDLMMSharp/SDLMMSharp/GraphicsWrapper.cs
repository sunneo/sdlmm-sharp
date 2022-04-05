using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp
{
    internal class GraphicsWrapper:IGraphics
    {
        IRenderer instance;
        internal GraphicsWrapper(IRenderer renderer)
        {
            this.instance = renderer;
        }
        public System.Drawing.Drawing2D.CompositingMode CompositingMode
        {
            get
            {
                return instance.CompositingMode;
            }
            set
            {
                instance.CompositingMode =value;
            }
        }

        public System.Drawing.Drawing2D.CompositingQuality CompositingQuality
        {
            get
            {
                return instance.CompositingQuality;
            }
            set
            {
                instance.CompositingQuality = value;
            }
        }

        public System.Drawing.Drawing2D.InterpolationMode InterpolationMode
        {
            get
            {
                return instance.InterpolationMode;
            }
            set
            {
                instance.InterpolationMode = value;
            }
        }

        public System.Drawing.Drawing2D.SmoothingMode SmoothingMode
        {
            get
            {
                return instance.SmoothingMode;
            }
            set
            {
                instance.SmoothingMode = value;
            }
        }

        public System.Drawing.Text.TextRenderingHint TextRenderingHint
        {
            get
            {
                return instance.TextRenderingHint;
            }
            set
            {
                instance.TextRenderingHint = value;
            }
        }

        public void DrawImage(System.Drawing.Image image, int x, int y, int width, int height)
        {
            instance.drawImage(image, x, y, width, height);
        }

        public void DrawImage(System.Drawing.Image image, System.Drawing.Point point)
        {
            instance.drawImage(image, point.X, point.Y, image.Width, image.Height);
        }

     
        public void DrawImage(System.Drawing.Image image, System.Drawing.PointF point)
        {
            instance.drawImage(image, (int)point.X, (int)point.Y, image.Width, image.Height);
        }

      
        public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle rect)
        {
            instance.drawImage(image, (int)rect.X, (int)rect.Y, rect.Width, rect.Height);
        }

        public void DrawImage(System.Drawing.Image image, System.Drawing.RectangleF rect)
        {
            instance.drawImage(image, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }

        public void DrawImage(System.Drawing.Image image, float x, float y)
        {
            instance.drawImage(image, (int)x, (int)y, image.Width, image.Height);
        }

        public void DrawImage(System.Drawing.Image image, int x, int y)
        {
            instance.drawImage(image, x, y,image.Width,image.Height);
        }

        public void DrawLine(System.Drawing.Pen pen, System.Drawing.Point pt1, System.Drawing.Point pt2)
        {
            instance.drawLine(pt1.X, pt1.Y, pt2.X, pt2.Y, pen.Color.ToArgb(), (int)pen.Width);
        }

        public void DrawLine(System.Drawing.Pen pen, System.Drawing.PointF pt1, System.Drawing.PointF pt2)
        {
            instance.drawLine((int)pt1.X, (int)pt1.Y, (int)pt2.X, (int)pt2.Y, pen.Color.ToArgb(), (int)pen.Width);
        }

        public void DrawLine(System.Drawing.Pen pen, float x1, float y1, float x2, float y2)
        {
            instance.drawLine((int)x1, (int)y1, (int)x2, (int)y2, pen.Color.ToArgb(), (int)pen.Width);
        }

        public void DrawLine(System.Drawing.Pen pen, int x1, int y1, int x2, int y2)
        {
            instance.drawLine((int)x1, (int)y1, (int)x2, (int)y2, pen.Color.ToArgb(), (int)pen.Width);
        }

        public void DrawLines(System.Drawing.Pen pen, System.Drawing.Point[] points)
        {
            if (points.Length < 2)
            {
                return;
            }
            int idx1 = 0;
            int idx2 = 1;
            for (int i = 1; i < points.Length; ++i)
            {
                idx2 = i;
                var pt1 = points[idx1];
                var pt2 = points[idx2];
                DrawLine(pen, pt1, pt2);
                idx1 = idx2;
            }
        }

        public void DrawLines(System.Drawing.Pen pen, System.Drawing.PointF[] points)
        {
            if (points.Length < 2)
            {
                return;
            }
            int idx1 = 0;
            int idx2 = 1;
            for (int i = 1; i < points.Length; ++i)
            {
                idx2 = i;
                var pt1 = points[idx1];
                var pt2 = points[idx2];
                DrawLine(pen, pt1, pt2);
                idx1 = idx2;
            }
        }

       
        public void DrawPolygon(System.Drawing.Pen pen, System.Drawing.Point[] points)
        {
            instance.drawPolygon(points, pen.Color.ToArgb(), (int)pen.Width, (pen.DashStyle != System.Drawing.Drawing2D.DashStyle.Solid));
        }

        public void DrawPolygon(System.Drawing.Pen pen, System.Drawing.PointF[] points)
        {
            instance.drawPolygon(points, pen.Color.ToArgb(), (int)pen.Width, (pen.DashStyle != System.Drawing.Drawing2D.DashStyle.Solid));
        }

        public void DrawRectangle(System.Drawing.Pen pen, System.Drawing.Rectangle rect)
        {
            instance.drawRect(rect.X, rect.Y, rect.Width, rect.Height, pen.Color.ToArgb(), pen.DashStyle != System.Drawing.Drawing2D.DashStyle.Solid, (int)pen.Width);
        }

        public void DrawRectangle(System.Drawing.Pen pen, float x, float y, float width, float height)
        {
            instance.drawRect((int)x, (int)y, (int)width, (int)height, pen.Color.ToArgb(), pen.DashStyle != System.Drawing.Drawing2D.DashStyle.Solid, (int)pen.Width);
        }

        public void DrawRectangle(System.Drawing.Pen pen, int x, int y, int width, int height)
        {
            instance.drawRect((int)x, (int)y, (int)width, (int)height, pen.Color.ToArgb(), pen.DashStyle != System.Drawing.Drawing2D.DashStyle.Solid, (int)pen.Width);
        }

        public void DrawRectangles(System.Drawing.Pen pen, System.Drawing.Rectangle[] rects)
        {
            foreach (var rect in rects)
            {
                DrawRectangle(pen, rect);
            }
        }

        public void DrawRectangles(System.Drawing.Pen pen, System.Drawing.RectangleF[] rects)
        {
            foreach (var rect in rects)
            {
                DrawRectangle(pen, rect.X,rect.Y,rect.Width,rect.Height);
            }
        }

        int GetColorFromBrush(System.Drawing.Brush brush)
        {
            int color = System.Drawing.Color.Black.ToArgb();
            if (brush is System.Drawing.SolidBrush)
            {
                color = (brush as System.Drawing.SolidBrush).Color.ToArgb();
            }
            return color;
        }
        public void DrawString(string s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.PointF point)
        {
            instance.drawString(s, (int)point.X, (int)point.Y, GetColorFromBrush(brush), font);
        }

        public void DrawString(string s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.RectangleF layoutRectangle)
        {
            instance.drawString(s, new System.Drawing.Rectangle((int)layoutRectangle.X, (int)layoutRectangle.Y, (int)layoutRectangle.Width, (int)layoutRectangle.Height), GetColorFromBrush(brush), font);
        }

        public void DrawString(string s, System.Drawing.Font font, System.Drawing.Brush brush, float x, float y)
        {
            instance.drawString(s,(int)x,(int)y, GetColorFromBrush(brush), font);
        }

        public void FillEllipse(System.Drawing.Brush brush, System.Drawing.Rectangle rect)
        {
            instance.fillEllipse(rect.X, rect.Y, rect.Width, rect.Height, brush);
        }

        public void FillEllipse(System.Drawing.Brush brush, System.Drawing.RectangleF rect)
        {
            instance.fillEllipse((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height, brush);
        }

        public void FillEllipse(System.Drawing.Brush brush, float x, float y, float width, float height)
        {
            instance.fillEllipse((int)x, (int)y, (int)width, (int)height, brush);
        }

        public void FillEllipse(System.Drawing.Brush brush, int x, int y, int width, int height)
        {
            instance.fillEllipse((int)x, (int)y, (int)width, (int)height, brush);
        }

        
        public void FillPolygon(System.Drawing.Brush brush, System.Drawing.Point[] points)
        {
            instance.fillPolygon(points, brush);
        }

        public void FillPolygon(System.Drawing.Brush brush, System.Drawing.PointF[] points)
        {
            instance.fillPolygon(points, brush);
        }

        
        public void FillRectangle(System.Drawing.Brush brush, System.Drawing.Rectangle rect)
        {
            instance.fillRect(rect, brush);
        }

        public void FillRectangle(System.Drawing.Brush brush, System.Drawing.RectangleF rect)
        {
            instance.fillRect((int)rect.X,(int)rect.Y,(int)rect.Width,(int)rect.Height,brush);
        }

        public void FillRectangle(System.Drawing.Brush brush, float x, float y, float width, float height)
        {
            instance.fillRect((int)x, (int)y, (int)width, (int)height, brush);
        }

        public void FillRectangle(System.Drawing.Brush brush, int x, int y, int width, int height)
        {
            instance.fillRect((int)x, (int)y, (int)width, (int)height, brush);
        }

        public void FillRectangles(System.Drawing.Brush brush, System.Drawing.Rectangle[] rects)
        {
            foreach (var rect in rects)
            {
                FillRectangle(brush, rect);
            }
        }

        public void FillRectangles(System.Drawing.Brush brush, System.Drawing.RectangleF[] rects)
        {
            foreach (var rect in rects)
            {
                FillRectangle(brush, rect);
            }
        }


        public void Flush()
        {
            instance.flush();
        }

        public System.Drawing.Rectangle ClientRectangle
        {
            get
            {
                return instance.GetClientArea();
            }
        }

        public System.Drawing.Bitmap Bitmap
        {
            get
            {
                return instance.flushToBMP();
            }
        }
    }
}
