using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp
{
    public interface IGraphics
    {
        CompositingMode CompositingMode { get; set; }
        CompositingQuality CompositingQuality { get; set; }
        InterpolationMode InterpolationMode { get; set; }
        SmoothingMode SmoothingMode { get; set; }
        TextRenderingHint TextRenderingHint { get; set; }
        void DrawImage(Image image, int x, int y, int width, int height);
        void DrawImage(Image image, Point point);
        void DrawImage(Image image, PointF point);
        void DrawImage(Image image, Rectangle rect);
        void DrawImage(Image image, RectangleF rect);
        void DrawImage(Image image, float x, float y);
        void DrawImage(Image image, int x, int y);
        void DrawLine(Pen pen, Point pt1, Point pt2);
        void DrawLine(Pen pen, PointF pt1, PointF pt2);
        void DrawLine(Pen pen, float x1, float y1, float x2, float y2);
        void DrawLine(Pen pen, int x1, int y1, int x2, int y2);
        void DrawLines(Pen pen, Point[] points);
        void DrawLines(Pen pen, PointF[] points);
        void DrawPolygon(Pen pen, Point[] points);
        void DrawPolygon(Pen pen, PointF[] points);
        void DrawRectangle(Pen pen, Rectangle rect);
        void DrawRectangle(Pen pen, float x, float y, float width, float height);
        void DrawRectangle(Pen pen, int x, int y, int width, int height);
        void DrawRectangles(Pen pen, Rectangle[] rects);
        void DrawRectangles(Pen pen, RectangleF[] rects);
        void DrawString(string s, Font font, Brush brush, PointF point);
        void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle);
        void DrawString(string s, Font font, Brush brush, float x, float y);
        void FillEllipse(Brush brush, Rectangle rect);
        void FillEllipse(Brush brush, RectangleF rect);
        void FillEllipse(Brush brush, float x, float y, float width, float height);
        void FillEllipse(Brush brush, int x, int y, int width, int height);
        void FillPolygon(Brush brush, Point[] points);
        void FillPolygon(Brush brush, PointF[] points);
        void FillRectangle(Brush brush, Rectangle rect);
        void FillRectangle(Brush brush, RectangleF rect);
        void FillRectangle(Brush brush, float x, float y, float width, float height);
        void FillRectangle(Brush brush, int x, int y, int width, int height);
        void FillRectangles(Brush brush, Rectangle[] rects);
        void FillRectangles(Brush brush, RectangleF[] rects);
        void Flush();
        System.Drawing.Bitmap Bitmap {   get;   }
    }
}
