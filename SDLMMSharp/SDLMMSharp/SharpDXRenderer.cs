using System;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.DXGI.Factory;
using SharpDX.Mathematics.Interop;
using Resource = SharpDX.Direct3D11.Resource;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SDLMMSharp
{

    public class SharpDXControl : UserControl, IRenderer
    {
        public class SpecialArguments
        {
            public const String TrimOutOfBound = "TrimOutOfBounds";
        }
        Device device;
        SwapChain swapChain;
        Surface surface;
        SharpDX.Direct2D1.RenderTarget d2dRenderTarget;
        RawRectangleF clientArea;
        public IRendererDelegates.OnMouseButtonAction onMouseClickHandler { get; set; }
        public IRendererDelegates.OnMouseMoveAction onMouseMoveHandler { get; set; }
        public IRendererDelegates.OnKeyboardAction onKeyboard { get; set; }
        public IRendererDelegates.OnMouseWhellAction onMouseWhell { get; set; }
        public static readonly int MOUSE_LEFT = 0;
        public static readonly int MOUSE_MIDDLE = 1;
        public static readonly int MOUSE_RIGHT = 2;
        System.Drawing.Drawing2D.SmoothingMode mSmoothMode = System.Drawing.Drawing2D.SmoothingMode.Default;
        System.Drawing.Text.TextRenderingHint mTextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
        System.Drawing.Drawing2D.InterpolationMode mInterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
        bool useAlpha = true;
        public SharpDXControl()
        {
            ///this.DoubleBuffered = true;
        }
        public SharpDXControl(int width,int height)
        {
            ///this.DoubleBuffered = true;
            this.Size = new System.Drawing.Size(width, height);
        }
        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);
            bool redraw = false;
            lock (_drawLock)
            {
                if (DrawRequestFlushed)
                {
                    this.swapChain.Present(0, PresentFlags.DoNotWait);
                }
                else
                {
                    redraw = true;
                }
            }
            if (redraw)
            {
                this.Draw();
            }

        }
        public void setUseAlpha(bool buse)
        {
            useAlpha = buse;
        }
        private int mouseIdx(MouseButtons btn)
        {
            switch (btn)
            {
                default:
                case System.Windows.Forms.MouseButtons.Left:
                    return MOUSE_LEFT;
                case System.Windows.Forms.MouseButtons.Middle:
                    return MOUSE_MIDDLE;
                case System.Windows.Forms.MouseButtons.Right:
                    return MOUSE_RIGHT;
            }
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (onMouseWhell != null)
            {
                onMouseWhell(e.X, e.Y, e.Delta * SystemInformation.MouseWheelScrollLines / 120);
            }
            base.OnMouseWheel(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (onMouseClickHandler != null)
            {
                onMouseClickHandler(e.X, e.Y, mouseIdx(e.Button), true);
            }
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (onMouseClickHandler != null)
            {
                onMouseClickHandler(e.X, e.Y, mouseIdx(e.Button), false);
            }
            base.OnMouseDown(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (onMouseMoveHandler != null)
            {
                onMouseMoveHandler(e.X, e.Y, mouseIdx(e.Button), e.Button != System.Windows.Forms.MouseButtons.None);
            }
            base.OnMouseMove(e);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (onKeyboard != null)
            {
                onKeyboard((int)e.KeyData, e.Control, true);
            }
            base.OnKeyDown(e);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (onKeyboard != null)
            {
                onKeyboard((int)e.KeyData, e.Control, false);
            }
            base.OnKeyUp(e);
        }
        public bool Selectable
        {
            get
            {
                return GetStyle(ControlStyles.Selectable);
            }
            set
            {
                SetStyle(ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor, value);
            }
        }
        private readonly Object _drawLock = new Object();
        private readonly Object _drawReqLock = new Object();
        public string RendererName
        {
            get
            {
                return "Direct3D";
            }
        }

        public bool IsValid
        {
            get
            {
                return d2dRenderTarget != null;
            }
        }
        public class DXGI2DContext : IDisposable
        {
            int width;
            int height;

            SharpDX.WIC.ImagingFactory wicFactory;
            SharpDX.Direct2D1.Factory d2dFactory;
            SharpDX.WIC.Bitmap wicBitmap;
            RenderTargetProperties renderTargetProperties;
            WicRenderTarget d2dRenderTarget;
            SharpDX.WIC.WICStream stream;
            System.IO.MemoryStream memStream;

            public SharpDX.Direct2D1.RenderTarget RenderTarget
            {
                get
                {
                    return d2dRenderTarget;
                }
            }
            public DXGI2DContext(int width, int height)
            {
                this.width = width;
                this.height = height;
                wicFactory = new SharpDX.WIC.ImagingFactory();
                d2dFactory = new SharpDX.Direct2D1.Factory();
                wicBitmap = new SharpDX.WIC.Bitmap(wicFactory, width, height, SharpDX.WIC.PixelFormat.Format32bppBGR, SharpDX.WIC.BitmapCreateCacheOption.CacheOnLoad);
                renderTargetProperties = new RenderTargetProperties(RenderTargetType.Default, new PixelFormat(Format.Unknown, AlphaMode.Unknown), 0, 0, RenderTargetUsage.None, SharpDX.Direct2D1.FeatureLevel.Level_DEFAULT);
                d2dRenderTarget = new WicRenderTarget(d2dFactory, wicBitmap, renderTargetProperties);

            }

            public void Dispose()
            {
                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }
                if (d2dRenderTarget != null)
                {
                    d2dRenderTarget.Dispose();
                    d2dRenderTarget = null;
                }
                if (wicBitmap != null)
                {
                    wicBitmap.Dispose();
                    wicBitmap = null;
                }
                if (d2dFactory != null)
                {
                    d2dFactory.Dispose();
                    d2dFactory = null;
                }
                if (wicFactory != null)
                {
                    wicFactory.Dispose();
                    wicFactory = null;
                }
                if (memStream != null)
                {
                    memStream.Dispose();
                }
            }
            private void InitDXGIRendering()
            {


            }
            public System.Drawing.Bitmap FlushToBitmap()
            {
                byte[] bits = new byte[width * height * 4];
                memStream = new System.IO.MemoryStream(bits);
                var stream = new SharpDX.WIC.WICStream(wicFactory, memStream);
                // Initialize a Jpeg encoder with this stream
                var encoder = new SharpDX.WIC.BitmapEncoder(wicFactory, SharpDX.WIC.ContainerFormatGuids.Bmp);
                encoder.Initialize(stream);

                // Create a Frame encoder
                var bitmapFrameEncode = new SharpDX.WIC.BitmapFrameEncode(encoder);
                bitmapFrameEncode.Initialize();
                bitmapFrameEncode.SetSize(width, height);
                var pixelFormatGuid = SharpDX.WIC.PixelFormat.FormatDontCare;
                bitmapFrameEncode.SetPixelFormat(ref pixelFormatGuid);
                bitmapFrameEncode.WriteSource(wicBitmap);

                bitmapFrameEncode.Commit();
                encoder.Commit();

                bitmapFrameEncode.Dispose();
                encoder.Dispose();
                stream.Dispose();
                return (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memStream);
            }
        }

        public void InitRendering()
        {
            try
            {
                lock (_drawLock)
                {
                    m_Ready = false;
                    ResizeRedraw = true;
                    var desc = new SwapChainDescription
                    {
                        BufferCount = 2,
                        ModeDescription = new ModeDescription(ClientSize.Width, ClientSize.Height, new Rational(60, 1),
                            Format.R8G8B8A8_UNorm),
                        IsWindowed = true,
                        OutputHandle = Handle,
                        SampleDescription = new SampleDescription(1, 0),
                        SwapEffect = SwapEffect.Discard,
                        Usage = Usage.RenderTargetOutput | Usage.Shared
                    };

                    Device.CreateWithSwapChain(DriverType.Hardware,
                        DeviceCreationFlags.BgraSupport,
                        new[] { SharpDX.Direct3D.FeatureLevel.Level_9_3 },
                        desc,
                        out device,
                        out swapChain);

                    var d2dFactory = new SharpDX.Direct2D1.Factory();

                    Factory factory = swapChain.GetParent<Factory>();
                    factory.MakeWindowAssociation(Handle, WindowAssociationFlags.IgnoreAll);

                    Texture2D backBuffer = Resource.FromSwapChain<Texture2D>(swapChain, 0);

                    surface = backBuffer.QueryInterface<Surface>();

                    d2dRenderTarget = new SharpDX.Direct2D1.RenderTarget(d2dFactory, surface,
                        new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));


                    var bitmapProperties = new BitmapProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore));

                    clientArea = new RawRectangleF
                    {
                        Left = 0,
                        Top = 0,
                        Bottom = ClientSize.Height,
                        Right = ClientSize.Width
                    };

                    factory.Dispose();
                    backBuffer.Dispose();
                    m_Ready = true;
                }
            }
            catch (Exception ee)
            {

            }
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitRendering();
        }

        public void EndRendering()
        {
            DisposeDirect3D();
        }

        private void DisposeDirect3D()
        {
            lock (_drawLock)
            {
                if (surface != null)
                    surface.Dispose();
                if (d2dRenderTarget != null)
                    d2dRenderTarget.Dispose();
                if (swapChain != null)
                    swapChain.Dispose();
                if (device != null)
                    device.Dispose();
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeDirect3D();
            }
            base.Dispose(disposing);
        }
        protected override void OnResize(EventArgs e)
        {
            try
            {
                DisposeDirect3D();
                InitRendering();
                base.OnResize(e);
            }
            catch
            {
                // This is pretty stupid, but Mono will send a resize event to this component
                // even when it's not added to a frame, so this will fail horribly
                // during the renderer self-test procedure, which detects this type of failure...
                // on different thread.
            }
        }
        SharpDX.DirectWrite.TextFormat m_TextFormat;
        SharpDX.DirectWrite.Factory FactoryDWrite = new SharpDX.DirectWrite.Factory();
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (m_TextFormat != null)
                m_TextFormat.Dispose();
            m_TextFormat = new SharpDX.DirectWrite.TextFormat(FactoryDWrite, this.Font.FontFamily.Name, this.Font.Size);
        }
        static RawColor4 ToRawColor4(System.Drawing.Color c)
        {
            RawColor4 ret = default(RawColor4);
            ret.A = c.A / 255.0f;
            ret.B = c.B / 255.0f;
            ret.R = c.R / 255.0f;
            ret.G = c.G / 255.0f;
            return ret;
        }
        volatile bool m_Ready = false;
        void Draw()
        {
			if(InvokeRequired){
				this.Invoke(new Action(Draw));
				return;
			}
            lock (_drawLock)
            {
                if (!m_Ready) return;
                lock (_drawReqLock)
                {
                    if (DrawRequests.Count > 0)
                    {
						d2dRenderTarget.BeginDraw();
                        foreach (Action<RenderTarget> req in DrawRequests)
                        {
                            req(d2dRenderTarget);
                        }
                        DrawRequestFlushed = true;
						this.d2dRenderTarget.Flush();
                        d2dRenderTarget.EndDraw();
                    }
                }
				swapChain.Present(0, PresentFlags.None);
            }
			
        }

        List<Action<RenderTarget>> DrawRequests = new List<Action<RenderTarget>>();
        private RawColor4 coveredColor(int color)
        {
            if (!useAlpha)
            {
                color = unchecked((int)(color | 0xff000000));
            }
            return ToRawColor4(System.Drawing.Color.FromArgb(color));
        }
        volatile bool DrawRequestFlushed = false;
        private void AddDrawRequest(Action<RenderTarget> action)
        {
            lock (_drawReqLock)
            {
                if (DrawRequestFlushed)
                {
                    DrawRequestFlushed = false;
                    DrawRequests.Clear();
                    foreach (var parms in BrushParams)
                    {
                        parms.Dispose();
                    }
                    BrushParams.Clear();
                }

                DrawRequests.Add(action);
            }
        }
        public void Clear(int color)
        {
            if ((color & 0xff000000) != 0)
            {
                lock (_drawReqLock)
                {
                    DrawRequests.Clear();
                }
            }
            AddDrawRequest((target) =>
            {
                target.Clear(coveredColor(color));
            });


        }
        public System.Drawing.SizeF MeasureString(String s, System.Drawing.Font font = null, int maxsize = -1)
        {
            if (font == null) font = this.Font;
            using (SharpDX.DirectWrite.TextFormat format = new SharpDX.DirectWrite.TextFormat(FactoryDWrite, font.FontFamily.Name, WeightFromFontStyle(font.Style), StyleFromFontStyle(font.Style), font.Size))
            {
                if (maxsize == -1)
                {
                    maxsize = this.Width;
                }
                format.WordWrapping = SharpDX.DirectWrite.WordWrapping.Wrap;

                using (SharpDX.DirectWrite.TextLayout layout =
            new SharpDX.DirectWrite.TextLayout(FactoryDWrite, s, format, maxsize, font.Height))
                {
                    return new System.Drawing.SizeF(layout.Metrics.Width, layout.Metrics.Height);
                }

            }

        }

        public void drawRoundRect(int x, int y, int w, int h, float rad, int color, int penWidth = 1)
        {
            AddDrawRequest((target) =>
            {
                using (var brush = GetBrushFromColor(target, color))
                {
                    RoundedRectangle rect = new RoundedRectangle();
                    rect.Rect = new RawRectangleF(x, y, x + w, y + h);
                    rect.RadiusX = rad;
                    rect.RadiusY = rad;
                    target.DrawRoundedRectangle(rect, brush, penWidth);
                }
            });
        }
        public void fillRoundRect(int x, int y, int w, int h, float radx, float rady, int color)
        {
            AddDrawRequest((target) =>
            {
                using (var brush = GetBrushFromColor(target, color))
                {
                    dxFillRoundRect(target, x, y, w, h, radx, rady, brush);
                }
            });

        }
        public void fillRoundRect(int x, int y, int w, int h, float rad, int color)
        {
            fillRoundRect(x, y, w, h, rad, rad, color);
        }
        private void dxFillRoundRect(RenderTarget target, int x, int y, int w, int h, float radx, float rady, Brush brush)
        {
            RoundedRectangle rect = new RoundedRectangle();
            rect.Rect = new RawRectangleF(x, y, x + w, y + h);
            rect.RadiusX = radx;
            rect.RadiusY = rady;
            target.FillRoundedRectangle(rect, brush);
        }
        public void fillRoundRect(int x, int y, int w, int h, float radx, float rady, System.Drawing.Brush gdibrush)
        {
            var param = BrushParamFromGDIBrush(gdibrush);
            lock (_drawReqLock)
            {
                BrushParams.Add(param);
            }
            AddDrawRequest((target) =>
            {
                var brush = param.GetBrush(target);
                if (brush != null)
                {
                    dxFillRoundRect(target, x, y, w, h, radx, rady, brush);
                    brush.Dispose();
                }
            });

        }
        public void fillRoundRect(int x, int y, int w, int h, float rad, System.Drawing.Brush gdibrush)
        {
            fillRoundRect(x, y, w, h, rad, rad, gdibrush);
        }
        public void fillRoundRect(System.Drawing.Rectangle r, float rad, int color)
        {
            fillRoundRect(r.X, r.Y, r.Width, r.Height, rad, rad, color);
        }
        public void fillRoundRect(System.Drawing.Rectangle r, float rad, System.Drawing.Brush brush)
        {
            fillRoundRect(r.X, r.Y, r.Width, r.Height, rad, rad, brush);
        }
        public void fillRoundRect(System.Drawing.Point position, System.Drawing.Size size, float rad, int color)
        {
            fillRoundRect(position.X, position.Y, size.Width, size.Height, rad, rad, color);
        }
        public void fillRoundRect(System.Drawing.Point position, System.Drawing.Size size, float rad, System.Drawing.Brush brush)
        {
            fillRoundRect(position.X, position.Y, size.Width, size.Height, rad, rad, brush);
        }
        public void drawImage(System.Drawing.Image _bmp, int x, int y, int w, int h, float alpha)
        {
            AddDrawRequest((target) =>
            {
                System.Drawing.Bitmap bmp = null;
                bool needDispose = false;
                if (_bmp is System.Drawing.Bitmap)
                {
                    bmp = _bmp as System.Drawing.Bitmap;
                }
                else
                {
                    bmp = new System.Drawing.Bitmap(_bmp);
                    needDispose = true;
                }
                if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppPArgb && bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    needDispose = true;
                    System.Drawing.Bitmap newBmp = new System.Drawing.Bitmap(_bmp.Width,_bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBmp))
                    {
                        g.DrawImage(_bmp, 0, 0, newBmp.Width, newBmp.Height);
                        g.Flush();
                    }
                    bmp = newBmp;
                }
                var mem = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                
                var bitmapProperties = new BitmapProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore));
                using (Bitmap gameBitmap = new Bitmap(d2dRenderTarget, new Size2(bmp.Width, bmp.Height), bitmapProperties))
                {
                    gameBitmap.CopyFromMemory(mem.Scan0, mem.Stride);
                    d2dRenderTarget.DrawBitmap(gameBitmap, new RawRectangleF(x, y, x + w, y + h), alpha, BitmapInterpolationMode.NearestNeighbor);
                }
                bmp.UnlockBits(mem);
                if (needDispose)
                {
                    bmp.Dispose();
                }
            });

        }
        public void fillPolygon(System.Drawing.Point[] points, int color, int offsetX = 0, int offsetY = 0)
        {
            AddDrawRequest((target) =>
            {
                var brush = GetBrushFromColor(target, color);
                if (brush != null)
                {
                    dxFillPolygon(target, points, brush, offsetX, offsetY);
                    brush.Dispose();
                }
            });
        }
        public void fillPolygon(System.Drawing.PointF[] points, int color, int offsetX = 0, int offsetY = 0)
        {
            AddDrawRequest((target) =>
            {
                var brush = GetBrushFromColor(target, color);
                if (brush != null)
                {
                    dxFillPolygon(target, points, brush, offsetX, offsetY);
                    brush.Dispose();
                }
            });
        }
        static RawVector2 Vector2FromGDIPoint(System.Drawing.Point pt, int offsetx = 0, int offsety = 0)
        {
            return new RawVector2(pt.X+offsetx, pt.Y+offsety);
        }
        static RawVector2 Vector2FromGDIPoint(System.Drawing.PointF pt,int offsetx=0,int offsety=0)
        {
            return new RawVector2(pt.X + offsetx, pt.Y + offsety);
        }
        static RawVector2[] Vector2sFromGDIPoints(System.Drawing.Point[] pts, int startIdx=0, int offsetx = 0, int offsety = 0)
        {
            RawVector2[] ret = new RawVector2[pts.Length];
            for (int i = startIdx; i < pts.Length; ++i)
            {
                ret[i] = Vector2FromGDIPoint(pts[i],offsetx,offsety);
            }
            return ret;
        }
        static RawVector2[] Vector2sFromGDIPoints(System.Drawing.PointF[] pts, int startIdx = 0, int offsetx = 0, int offsety = 0)
        {
            RawVector2[] ret = new RawVector2[pts.Length];
            for (int i = startIdx; i < pts.Length; ++i)
            {
                ret[i] = Vector2FromGDIPoint(pts[i], offsetx, offsety);
            }
            return ret;
        }
        void dxFillPolygon(RenderTarget target, System.Drawing.PointF[] points, Brush brush, int offsetX = 0, int offsetY = 0)
        {
            PathGeometry geo = new PathGeometry(target.Factory);
            RawVector2[] path = Vector2sFromGDIPoints(points, 1, offsetX, offsetY);
            var sink1 = geo.Open();
            sink1.BeginFigure(Vector2FromGDIPoint(points[0]), new FigureBegin());
            sink1.AddLines(path);
            sink1.EndFigure(new FigureEnd());
            sink1.Close();
            target.FillGeometry(geo, brush);
        }
        void dxFillPolygon(RenderTarget target, System.Drawing.Point[] points, Brush brush, int offsetX = 0, int offsetY = 0)
        {
            PathGeometry geo = new PathGeometry(target.Factory);
            RawVector2[] path = Vector2sFromGDIPoints(points, 1, offsetX, offsetY);
            var sink1 = geo.Open();
            sink1.BeginFigure(Vector2FromGDIPoint(points[0]), new FigureBegin());
            sink1.AddLines(path);
            sink1.EndFigure(new FigureEnd());
            sink1.Close();
            target.FillGeometry(geo, brush);
        }
        public void fillPolygon(System.Drawing.Point[] points, System.Drawing.Brush gdibrush, int offsetX = 0, int offsetY = 0)
        {
            var param = BrushParamFromGDIBrush(gdibrush);
            lock (_drawReqLock)
            {
                BrushParams.Add(param);
            }
            AddDrawRequest((target) =>
            {
                var brush = param.GetBrush(target);
                if (brush != null)
                {
                    dxFillPolygon(target, points, brush, offsetX, offsetY);
                    brush.Dispose();
                }
            });
          
        }
        public void fillPolygon(System.Drawing.PointF[] points, System.Drawing.Brush gdibrush, int offsetX = 0, int offsetY = 0)
        {
            var param = BrushParamFromGDIBrush(gdibrush);
            lock (_drawReqLock)
            {
                BrushParams.Add(param);
            }
            AddDrawRequest((target) =>
            {
                var brush = param.GetBrush(target);
                if (brush != null)
                {
                    dxFillPolygon(target, points, brush, offsetX, offsetY);
                    brush.Dispose();
                }
            });

        }
        public void drawPolygon(System.Drawing.Point[] points, int color, int width, bool dashed = false, int offsetX = 0, int offsetY = 0)
        {
            RawVector2[] path = Vector2sFromGDIPoints(points, 1, offsetX, offsetY);
            
            AddDrawRequest((target) =>
            {
                PathGeometry geo = new PathGeometry(target.Factory);
                var sink1 = geo.Open();
                sink1.BeginFigure(Vector2FromGDIPoint(points[0]), new FigureBegin());
                sink1.AddLines(path);
                sink1.EndFigure(new FigureEnd());
                sink1.Close();
                if (dashed)
                {
                    target.DrawGeometry(geo, this.GetBrushFromColor(target, color), width, GetDashedStyle(target));
                }
                else
                {
                    target.DrawGeometry(geo, this.GetBrushFromColor(target, color), width);
                }
                
            });
        }
        public void drawPolygon(System.Drawing.PointF[] points, int color, int width, bool dashed = false, int offsetX = 0, int offsetY = 0)
        {
            RawVector2[] path = Vector2sFromGDIPoints(points, 1, offsetX, offsetY);

            AddDrawRequest((target) =>
            {
                PathGeometry geo = new PathGeometry(target.Factory);
                var sink1 = geo.Open();
                sink1.BeginFigure(Vector2FromGDIPoint(points[0]), new FigureBegin());
                sink1.AddLines(path);
                sink1.EndFigure(new FigureEnd());
                sink1.Close();
                if (dashed)
                {
                    target.DrawGeometry(geo, this.GetBrushFromColor(target, color), width, GetDashedStyle(target));
                }
                else
                {
                    target.DrawGeometry(geo, this.GetBrushFromColor(target, color), width);
                }

            });
        }
        public void drawImage(System.Drawing.Image bmp, int x, int y, int w, int h)
        {
            drawImage(bmp, x, y, w, h, 1.0f);
        }
        private SharpDX.DirectWrite.FontStyle StyleFromFontStyle(System.Drawing.FontStyle style)
        {
            SharpDX.DirectWrite.FontStyle ret = SharpDX.DirectWrite.FontStyle.Normal;
            if (style.HasFlag(System.Drawing.FontStyle.Italic))
            {
                ret |= SharpDX.DirectWrite.FontStyle.Italic;
            }
            return ret;
        }
        private SharpDX.DirectWrite.FontWeight WeightFromFontStyle(System.Drawing.FontStyle style)
        {
            SharpDX.DirectWrite.FontWeight ret = SharpDX.DirectWrite.FontWeight.Normal;
            if (style.HasFlag(System.Drawing.FontStyle.Bold))
            {
                ret = SharpDX.DirectWrite.FontWeight.Bold;
            }
            return ret;
        }
        private SharpDX.Direct2D1.Brush GetBrushFromColor(RenderTarget target, int color)
        {
            SharpDX.Direct2D1.SolidColorBrush brush = new SolidColorBrush(target, coveredColor(color));
            return brush;
        }
        public void drawString(String str, System.Drawing.Rectangle rect, int color, System.Drawing.Font font = null)
        {


            if (font == null)
            {
                font = this.Font;
            }

            AddDrawRequest((target) =>
            {
                HandleDrawString(target,str,rect,color,font);
            });


        }
        private void HandleDrawString(RenderTarget target,String str, System.Drawing.Rectangle rect, int color, System.Drawing.Font font = null)
        {
            if (String.IsNullOrEmpty(str)) return;
            
            using (Brush brush = GetBrushFromColor(target, color))
            {
                SharpDX.DirectWrite.FontWeight weight = WeightFromFontStyle(font.Style);
                SharpDX.DirectWrite.FontStyle style = StyleFromFontStyle(font.Style);
                SharpDX.DirectWrite.TextFormat stringFormat = new SharpDX.DirectWrite.TextFormat(this.FactoryDWrite, font.FontFamily.Name, weight, style, font.Size);
                stringFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading;
                stringFormat.WordWrapping = SharpDX.DirectWrite.WordWrapping.Wrap;

                if (font == null) font = this.Font;
                int maxsize = this.Width;

                if (pushedArgument.ContainsKey("TrimOutOfBounds") && ((bool)pushedArgument["TrimOutOfBounds"]))
                {
                    using (SharpDX.DirectWrite.TextLayout layout = new SharpDX.DirectWrite.TextLayout(FactoryDWrite, str, stringFormat, maxsize, font.Height))
                    {
                        var lines = layout.GetLineMetrics();
                        StringBuilder strb = new StringBuilder();
                        var hittests = layout.HitTestTextRange(0, str.Length, rect.X, rect.Y);

                        int trimPos = -1;
                        for (int i = 0; i < hittests.Length; ++i)
                        {
                            if (hittests[i].Top + hittests[i].Height > rect.Bottom)
                            {
                                trimPos = hittests[i].TextPosition;
                                break;
                            }
                        }
                        String trimmedStr = str;
                        if (trimPos > -1)
                        {
                            trimmedStr = str.Substring(0, trimPos);
                        }
                        target.PushAxisAlignedClip(new RectangleF(rect.X, rect.Y, rect.Right, rect.Bottom), AntialiasMode.Aliased);
                        target.DrawText(trimmedStr, stringFormat, new RawRectangleF(rect.X, rect.Y, rect.Right, rect.Bottom), brush, DrawTextOptions.EnableColorFont | DrawTextOptions.Clip);
                        target.PopAxisAlignedClip();
                    }
                }
                else
                {
                    target.PushAxisAlignedClip(new RectangleF(rect.X, rect.Y, rect.Right, rect.Bottom), AntialiasMode.Aliased);
                    target.DrawText(str, stringFormat, new RawRectangleF(rect.X, rect.Y, rect.Right, rect.Bottom), brush, DrawTextOptions.EnableColorFont | DrawTextOptions.Clip);
                    target.PopAxisAlignedClip();
                }

               
                stringFormat.Dispose();
            }
            
        }
        public void disposeImage(System.Drawing.Image image)
        {
            AddDrawRequest((target) =>
            {
                try
                {
                    if (image != null)
                    {
                        image.Dispose();
                    }
                }
                catch (Exception ee)
                {

                }
            });
        }
        public void drawLine(int x0, int y0, int x1, int y1, int color, int width)
        {
            AddDrawRequest((target) =>
            {
                using (var brush = GetBrushFromColor(target, color))
                {
                    target.DrawLine(new RawVector2(x0, y0), new RawVector2(x1, y1), brush, width);
                }
            });
        }
        public void drawEllipse(int x, int y, int w, int h, int color)
        {
            AddDrawRequest((target) =>
            {
                using (var brush = GetBrushFromColor(target, color))
                {
                    Ellipse ell = new Ellipse(new RawVector2(x + w / 2, y + h / 2), w, h);
                    target.DrawEllipse(ell, brush);
                }
            });
        }
        public class BrushParam : IDisposable
        {
            public SharpDXControl Parent;
            public virtual Brush GetBrush(RenderTarget target)
            {
                return null;
            }

            public virtual void Dispose()
            {

            }
        }
        public class SolidBrushParam : BrushParam
        {
            public int Color;
            public override Brush GetBrush(RenderTarget target)
            {
                return Parent.GetBrushFromColor(target, Color);
            }
        }
        public class LinearGradientBrushParam : BrushParam
        {
            public LinearGradientBrushProperties prop = new LinearGradientBrushProperties();
            public GradientStop[] stops = null;
            public GradientStopCollection stopCollection;
            Brush brush;
            RenderTarget target;
            public override Brush GetBrush(RenderTarget target)
            {
                if (this.target != null && target != this.target)
                {
                    if (stopCollection != null)
                    {
                        stopCollection.Dispose();
                        stopCollection = null;
                    }
                }
                if (stops == null)
                {
                    return null;
                }
                if (stopCollection == null)
                {
                    stopCollection = new GradientStopCollection(target, stops, ExtendMode.Clamp);
                }

                this.brush = new LinearGradientBrush(target, prop, stopCollection);
                this.target = target;
                return brush;
            }
            public override void Dispose()
            {
                base.Dispose();

                if (stopCollection != null)
                {
                    stops = null;
                    stopCollection.Dispose();
                }
                stopCollection = null;
            }
        }
        public List<BrushParam> BrushParams = new List<BrushParam>();
        private BrushParam BrushParamFromGDIBrush(System.Drawing.Brush brush)
        {
            if (brush is System.Drawing.Drawing2D.LinearGradientBrush)
            {
                LinearGradientBrushParam parm = new LinearGradientBrushParam();
                System.Drawing.Drawing2D.LinearGradientBrush linear = (System.Drawing.Drawing2D.LinearGradientBrush)brush;

                parm.prop.StartPoint = new RawVector2(linear.Rectangle.Left, linear.Rectangle.Top);
                parm.prop.EndPoint = new RawVector2(linear.Rectangle.Right, linear.Rectangle.Bottom);
                parm.stops = new GradientStop[linear.InterpolationColors.Colors.Length];
                for (int i = 0; i < parm.stops.Length; ++i)
                {
                    parm.stops[i].Color = ToRawColor4(linear.InterpolationColors.Colors[i]);
                    parm.stops[i].Position = linear.InterpolationColors.Positions[i];
                }

                parm.Parent = this;
                return parm;
            }
            else if (brush is System.Drawing.SolidBrush)
            {
                System.Drawing.SolidBrush solid = (System.Drawing.SolidBrush)brush;
                SolidBrushParam solidParm = new SolidBrushParam();
                solidParm.Parent = this;
                solidParm.Color = solid.Color.ToArgb();
                return solidParm;
            }
            else
            {
                SolidBrushParam solidParm = new SolidBrushParam();
                solidParm.Parent = this;
                solidParm.Color = 0;
                return solidParm;
            }
        }
        public void fillEllipse(int x, int y, int w, int h, int color)
        {
            if ((color & 0xff000000) != 0)
            {
                if (x <= -3 && y <= -3 && w >= this.Width+6 && h >= this.Height+6)
                {
                    lock (_drawReqLock)
                    {
                        DrawRequests.Clear();
                    }
                }
            }
            AddDrawRequest((target) =>
            {
                using (var brush = GetBrushFromColor(target, color))
                {
                    dxFillEllipse(target, x, y, w, h, brush);
                }
            });
        }
        public void fillEllipse(int x, int y, int w, int h, System.Drawing.Brush gdibrush)
        {
            var param = BrushParamFromGDIBrush(gdibrush);
            lock (_drawReqLock)
            {
                BrushParams.Add(param);
            }
            AddDrawRequest((target) =>
            {
                Brush brush = param.GetBrush(target);
                if (brush != null)
                {
                    dxFillEllipse(target, x, y, w, h, brush);
                    brush.Dispose();
                }
            });
        }
        private void dxFillEllipse(SharpDX.Direct2D1.RenderTarget target, int x, int y, int w, int h, Brush brush)
        {
            Ellipse ell = new Ellipse(new RawVector2(x + w / 2, y + h / 2), w, h);
            target.FillEllipse(ell, brush);
        }
        public void drawCircle(int n_cx, int n_cy, int radius, int pixel)
        {
            int halfRadius = radius / 2;
            drawEllipse(n_cx - halfRadius, n_cy - halfRadius, radius / 2, radius / 2, pixel);
        }
        public void fillCircle(int n_cx, int n_cy, int radius, int pixel)
        {
            int halfRadius = radius / 2;
            fillEllipse(n_cx - halfRadius, n_cy - halfRadius, radius, radius, pixel);
        }
        public void fillCircle(int n_cx, int n_cy, int radius, System.Drawing.Brush gdibrush)
        {
            int halfRadius = radius / 2;
            fillEllipse(n_cx - halfRadius, n_cy - halfRadius, radius, radius, gdibrush);
        }
        public void drawPixel(int x, int y, int color)
        {
            fillRect(x, y, 1, 1, color);
        }
        StrokeStyle mDashedStyle;
        StrokeStyle GetDashedStyle(RenderTarget target)
        {
            if (mDashedStyle == null)
            {
                StrokeStyleProperties prop = new StrokeStyleProperties();
                prop.DashStyle = DashStyle.DashDotDot;
                prop.DashOffset = 2.0f;
                mDashedStyle = new StrokeStyle(target.Factory, prop);
            }
            return mDashedStyle;
        }
        public void drawRect(int x, int y, int w, int h, int color, bool dashed = false, int width = 1)
        {
            AddDrawRequest((target) =>
            {
                using (Brush brush = GetBrushFromColor(target, color))
                {
                    if (dashed)
                    {
                        target.DrawRectangle(new RawRectangleF(x, y, x + w, y + h), brush, width, GetDashedStyle(target));
                    }
                    else
                    {
                        target.DrawRectangle(new RawRectangleF(x, y, x + w, y + h), brush, width);
                    }
                }
            });

        }
        public void fillRect(int x, int y, int w, int h, int color)
        {
            
            if ((color & 0xff000000) != 0)
            {
                if (x <= 0 && y <= 0 && w >= this.Width && h >= this.Height)
                {
                    lock (_drawReqLock)
                    {
                        DrawRequests.Clear();
                    }     
                }
            }
            
            AddDrawRequest((target) =>
            {
                using (var brush = GetBrushFromColor(target, color))
                {
                    dxFillRect(target, x, y, w, h, brush);
                }
            });

        }
        public void fillRect(System.Drawing.Rectangle r, int color)
        {
            fillRect(r.X, r.Y, r.Width, r.Height, color);
        }
        public void fillRect(System.Drawing.Point position, System.Drawing.Size size, int color)
        {
            fillRect(position.X, position.Y, size.Width, size.Height, color);
        }
        public void fillRect(System.Drawing.Rectangle r, System.Drawing.Brush linearGradient)
        {
            fillRect(r.X, r.Y, r.Width, r.Height, ((System.Drawing.SolidBrush)linearGradient).Color.ToArgb());
        }
        private void dxFillRect(SharpDX.Direct2D1.RenderTarget target, int x, int y, int w, int h, Brush brush)
        {
            target.FillRectangle(new RawRectangleF(x, y, x + w, y + h), brush);
        }
        public void fillRect(System.Drawing.Point position, System.Drawing.Size size, System.Drawing.Brush gdibrush)
        {
            fillRect(position.X, position.Y, size.Width, size.Height, gdibrush);
        }
        public void fillRect(int x, int y, int w, int h, System.Drawing.Brush gdibrush)
        {
            var param = BrushParamFromGDIBrush(gdibrush);
            lock (_drawReqLock)
            {
                BrushParams.Add(param);
            }
            AddDrawRequest((target) =>
            {
                Brush brush = param.GetBrush(target);
                if (brush != null)
                {
                    dxFillRect(target, x, y, w, h, brush);
                    brush.Dispose();
                }
            });

        }
        public void drawString(String str, int x, int y, int color, System.Drawing.Font font = null)
        {
            drawString(str, new System.Drawing.Rectangle(x, y, this.Width, this.Height), color, font);
        }
        public void drawPixels(int[] pixels, int x, int y, int w, int h)
        {
            var bitmapProperties = new BitmapProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore));
            using (Bitmap gameBitmap = new Bitmap(d2dRenderTarget, new Size2(w, h), bitmapProperties))
            {
                gameBitmap.CopyFromMemory(pixels, w * 4);
                d2dRenderTarget.DrawBitmap(gameBitmap, clientArea, 1.0f, BitmapInterpolationMode.NearestNeighbor);
            }

        }
        public System.Drawing.Bitmap flushToBMP(int width,int height)
        {
            using (var dxgi = new DXGI2DContext(width, height))
            {
                RenderTarget current = this.d2dRenderTarget;
                this.d2dRenderTarget = dxgi.RenderTarget;
                Draw();
                this.d2dRenderTarget = current;
                return dxgi.FlushToBitmap();
            }
        }
        public System.Drawing.Bitmap flushToBMP()
        {
            return flushToBMP(this.Width, this.Height);
        }
        public System.Drawing.Bitmap flushToBMP(int left, int top, int w, int h)
        {
            using (var bmp = flushToBMP(w,h))
            {
                return bmp.Clone(new System.Drawing.Rectangle(left, top, w, h), bmp.PixelFormat);
            }
        }
        public void flush()
        {
            this.Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            bool redraw = false;
            lock (_drawLock)
            {
                if (this.DrawRequestFlushed)
                {
                    swapChain.Present(0, PresentFlags.None);
                }
                else
                {
                    redraw = true;
                }
            }
            if (redraw)
            {
                Draw();
            }
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

        }
        public IGraphics GetGraphics()
        {
            return new GraphicsWrapper(this);
        }
        public System.Drawing.Drawing2D.CompositingMode CompositingMode
        {
            get;
            set;
        }

        public System.Drawing.Drawing2D.CompositingQuality CompositingQuality
        {
            get;
            set;
        }

        public System.Drawing.Drawing2D.InterpolationMode InterpolationMode
        {
            get
            {
                return mInterpolationMode;
            }
            set
            {
                mInterpolationMode = value;
            }
        }

        public System.Drawing.Drawing2D.SmoothingMode SmoothingMode
        {
            get
            {
                return mSmoothMode;
            }
            set
            {
                mSmoothMode = value;
            }
        }
        public System.Drawing.Drawing2D.SmoothingMode SmoothMode
        {
            get
            {
                return mSmoothMode;
            }
            set
            {
                mSmoothMode = value;
            }
        }
        public System.Drawing.Text.TextRenderingHint TextRenderingHint
        {
            get
            {
                return mTextRenderingHint;
            }
            set
            {
                mTextRenderingHint = value;
            }
        }


        public void SetClipping(System.Drawing.Rectangle rect)
        {
            AddDrawRequest((target) =>
               {
                   target.PushAxisAlignedClip(new RectangleF(rect.X, rect.Y, rect.Right, rect.Bottom), AntialiasMode.Aliased);
               });
        }

        public void UnsetClipping()
        {
            AddDrawRequest((target) =>
            {
                target.PopAxisAlignedClip();
            });
        }

        Dictionary<String, Object> pushedArgument = new Dictionary<string, object>();
        public void setArgument(string key, object val)
        {
            AddDrawRequest((target) =>
            {
                pushedArgument[key] = val;
            });
            
        }

        public void unsetArgument(string key)
        {
            AddDrawRequest((target) =>
            {
                pushedArgument.Remove(key);
            });
            
        }
    }
}