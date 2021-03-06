﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp
{
    public class IRendererDelegates
    {
        public delegate void OnMouseButtonAction(int x, int y, int btn, bool ison);
        public delegate void OnMouseMoveAction(int x, int y, int btn, bool ison);
        public delegate void OnMouseWhellAction(int x, int y, int scrollAmount);
        public delegate void OnKeyboardAction(int keycode, bool ctrl, bool ison);
    }
    
    public interface IRenderer:IDisposable
    {
        IGraphics GetGraphics();
        System.Drawing.Drawing2D.CompositingMode CompositingMode { get; set; }
        System.Drawing.Drawing2D.CompositingQuality CompositingQuality { get; set; }
        System.Drawing.Drawing2D.InterpolationMode InterpolationMode { get; set; }
        System.Drawing.Drawing2D.SmoothingMode SmoothingMode { get; set; }
        System.Drawing.Text.TextRenderingHint TextRenderingHint { get; set; }
        IRendererDelegates.OnMouseButtonAction onMouseClickHandler { get; set; }
        IRendererDelegates.OnMouseMoveAction onMouseMoveHandler { get; set; }
        IRendererDelegates.OnKeyboardAction onKeyboard { get; set; }
        IRendererDelegates.OnMouseWhellAction onMouseWhell { get; set; }
        bool Selectable { get; set; }
        bool IsValid { get; }
        void setUseAlpha(bool buse);
        void Clear(int color);
        void drawPixels(int[] pixels, int x, int y, int w, int h);
        void drawImage(System.Drawing.Image bmp, int x, int y, int w, int h, float alpha);
        void drawImage(System.Drawing.Image bmp, int x, int y, int w, int h);

        void SetClipping(Rectangle rect);
        void UnsetClipping();

        IDisposable SetRectangleClipping(int x, int y, int w, int h);
        IDisposable SetCircleClipping(System.Drawing.Point center, int radius);
        void DisposeObject(IDisposable obj);

        void drawString(String str, int x, int y, int color, System.Drawing.Font font = null);
        void drawString(String str, System.Drawing.Rectangle rect, int color, System.Drawing.Font font = null);
        void drawLine(int x0, int y0, int x1, int y1, int color, int width);
        void setArgument(String key, Object val);
        void unsetArgument(String key);
        void drawEllipse(int x, int y, int w, int h, int color, int strokeWidth);

        void drawEllipse(int x, int y, int w, int h, int color);
        void fillEllipse(int x, int y, int w, int h, int color);
        void fillEllipse(int x, int y, int w, int h, System.Drawing.Brush gdibrush);

        void drawCircle(int n_cx, int n_cy, int radius, int pixel);
        void fillCircle(int n_cx, int n_cy, int radius, int pixel);
        void fillCircle(int n_cx, int n_cy, int radius, System.Drawing.Brush gdibrush);

        void drawPixel(int x, int y, int color);

        void drawRect(int x, int y, int w, int h, int color, bool dashed = false, int width = 1);
        void fillRect(int x, int y, int w, int h, int color);
        void fillRect(System.Drawing.Rectangle r, int color);
        void fillRect(System.Drawing.Point position, System.Drawing.Size size, int color);
        void fillRect(System.Drawing.Rectangle r, System.Drawing.Brush linearGradient);
        void fillRect(System.Drawing.Point position, System.Drawing.Size size, System.Drawing.Brush gdibrush);
        void fillRect(int x, int y, int w, int h, System.Drawing.Brush gdibrush);

        void drawRoundRect(int x, int y, int w, int h, float rad, int color, int penWidth = 1);
        void fillRoundRect(int x, int y, int w, int h, float radx, float rady, int color);
        void fillRoundRect(int x, int y, int w, int h, float rad, int color);
        void fillRoundRect(int x, int y, int w, int h, float radx, float rady, System.Drawing.Brush gdibrush);
        void fillRoundRect(int x, int y, int w, int h, float rad, System.Drawing.Brush gdibrush);
        void fillRoundRect(System.Drawing.Rectangle r, float rad, int color);
        void fillRoundRect(System.Drawing.Rectangle r, float rad, System.Drawing.Brush brush);
        void fillRoundRect(System.Drawing.Point position, System.Drawing.Size size, float rad, int color);
        void fillRoundRect(System.Drawing.Point position, System.Drawing.Size size, float rad, System.Drawing.Brush brush);

        void fillPolygon(System.Drawing.Point[] points, int color, int offsetX = 0, int offsetY = 0);
        void fillPolygon(System.Drawing.PointF[] points, int color, int offsetX = 0, int offsetY = 0);
        void fillPolygon(System.Drawing.Point[] points, System.Drawing.Brush gdibrush, int offsetX = 0, int offsetY = 0);
        void fillPolygon(System.Drawing.PointF[] points, System.Drawing.Brush gdibrush, int offsetX = 0, int offsetY = 0);
        void drawPolygon(System.Drawing.Point[] points, int color, int width, bool dashed = false, int offsetX = 0, int offsetY = 0);
        void drawPolygon(System.Drawing.PointF[] points, int color, int width, bool dashed = false, int offsetX = 0, int offsetY = 0);
 

        System.Drawing.Bitmap flushToBMP(int left, int top, int w, int h);
        System.Drawing.Bitmap flushToBMP();
        void disposeImage(Image image);
        System.Drawing.SizeF MeasureString(String s, System.Drawing.Font font = null, int maxsize = -1);
        void flush();


    }
}
