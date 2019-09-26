using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace SDLMMSharp.CustomControl
{
    public partial class SliderCheckbox : UserControl
    {
        [Browsable(true)]
        public virtual String ToolTipText
        {
            get
            {
                return ToolTip.GetToolTip(this);
            }
            set
            {
                ToolTip.SetToolTip(this, value);
                ToolTip.SetToolTip(this.sdlmmControl1, value);
                if (String.IsNullOrEmpty(value))
                {
                    ToolTip.RemoveAll();
                }
            }
        }

        [Browsable(true)]
        public Color TextOnSliderTrueColor
        {
            get { return mTextOnSliderTrueColor; }
            set { mTextOnSliderTrueColor = value; }
        }
        private Color mTextOnSliderTrueColor = Color.White;

        [Browsable(true)]
        public Color TextOnSliderFalseColor
        {
            get { return mTextOnSliderFalseColor; }
            set { mTextOnSliderFalseColor = value; }
        }
        private Color mTextOnSliderFalseColor = Color.White;

        [Browsable(true)]
        public bool DrawTextOnSlider
        {
            get { return mDrawTextOnSlider; }
            set { mDrawTextOnSlider = value; }
        }
        private bool mDrawTextOnSlider = true;

        [Browsable(true)]
        public String TextOnSliderTrue
        {
            get { return mTextOnSliderTrue; }
            set { mTextOnSliderTrue = value; }
        }
        private String mTextOnSliderTrue = "ON";

        [Browsable(true)]
        public String TextOnSliderFalse
        {
            get { return mTextOnSliderFalse; }
            set { mTextOnSliderFalse = value; }
        }
        private String mTextOnSliderFalse = "OFF";


        [DefaultValue(false)]
        [Browsable(true)]
        public bool FlattenStyle { get; set; }


        [Browsable(true)]
        public Color FlattenBorderColorFalse
        {
            get { return mFlattenBorderColorFalse; }
            set { mFlattenBorderColorFalse = value; }
        }

        [Browsable(true)]
        public int FlattenBorderThickness
        {
            get
            {
                return mFlattenBorderThickness;
            }
            set
            {
                mFlattenBorderThickness = value;
            }
        }

        private int mFlattenBorderThickness = 2;

        [Browsable(true)]
        public Color FlattenBorderColorTrue
        {
            get { return mFlattenBorderColorTrue; }
            set { mFlattenBorderColorTrue = value; }
        }

        Color mFlattenBorderColorFalse = Color.FromArgb(unchecked((int)0xff808080));
        Color mFlattenBorderColorTrue = Color.FromArgb(unchecked((int)0xffffffff));


        public delegate void CheckedChangeHandler(object sender, bool ischecked);
        public event CheckedChangeHandler OnCheckedChanged;
        public Double PadPercentageX = 0.1;
        public Double PadPercentageY = 0.125;
        public Double BackgroundHeightRatio = 0.75;
        private int BackgroundHeight = 0;
        private int BackgroundHeightOffset = 0;
        [DefaultValue(false)]
        [Browsable(true)]
        public bool UsePaintBackground { get; set; }

        public int TransitionMillis = 200; // 200ms
        public int MillisPerStep = 16;


        private bool mUseGradient = true;

        [DefaultValue(true)]
        [Browsable(true)]
        public bool UseGradient
        {
            get
            {
                return mUseGradient;
            }
            set
            {
                mUseGradient = value;
            }
        }

        [DefaultValue(true)]
        [Browsable(true)]
        public bool HasShadow
        {
            get
            {
                return mHasShadow;
            }
            set
            {
                mHasShadow = value;
            }
        }
        private bool mHasShadow = true;

        [DefaultValue(1)]
        [Browsable(true)]
        public int ShadowOffset
        {
            get { return mShadowOffset; }
            set { mShadowOffset = value; }
        }
        private int mShadowOffset = 0;


        [DefaultValue(1)]
        [Browsable(true)]
        public int ShadowAdditionRadius { get; set; }


        [Browsable(true)]
        public int ShadowColor
        {
            get { return mShadowColor; }
            set { mShadowColor = value; }
        }

        private int mShadowColor = unchecked((int)0x20444444);


        public Bitmap SliderBitmapFalse;
        public Bitmap SliderBitmapTrue;
        public Bitmap ThumbBitmap;
        private bool BoundsSet = false;
        Rectangle sliderbound = Rectangle.Empty;
        private bool mDefaultThumbColor = true;
        private bool mUseDefaultBackColor = true;

        [Browsable(true)]
        [DefaultValue(true)]
        public bool DefaultThumbColor
        {
            get
            {
                return mDefaultThumbColor;
            }
            set
            {
                mDefaultThumbColor = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(true)]
        public bool UseDefaultBackColor
        {
            get { return mUseDefaultBackColor; }

            set { mUseDefaultBackColor = value; }
        }

        public new Rectangle Bounds
        {
            get
            {
                if (!BoundsSet) return base.Bounds;
                return sliderbound;
            }
            set
            {
                if (value == null || value.Equals(Rectangle.Empty))
                {
                    BoundsSet = false;
                }
                else
                {
                    sliderbound = value;
                    BoundsSet = true;
                    RefreshSliderRect();
                }
            }
        }
        //==== 如果沒有指定背景圖片就會用以下的設定 ===
        // False的顏色 
        private Color _FalseColor = Color.FromArgb(unchecked((int)0xff5f5f5f));
        // True的背景顏色
        private Color _TrueColor = Color.FromArgb(unchecked((int)0xff00ff00));

        // False的背景顏色 
        public Color FalseColor
        {
            get
            {
                return _FalseColor;
            }
            set
            {
                _FalseColor = value;
                RefreshFalseColorLinearGradient();
            }
        }

        // True的背景顏色
        public Color TrueColor
        {
            get
            {
                return _TrueColor;
            }
            set
            {
                _TrueColor = value;
                RefreshTrueColorLinearGradient();
            }
        }

        // 上面那顆的顏色
        public Color _ThumbColorTrue = Color.FromArgb(unchecked((int)0xff008800));
        public Color _ThumbColor = Color.FromArgb(unchecked((int)0xffb5b5b5));
        public Color ThumbColorTrue
        {
            get
            {
                return _ThumbColorTrue;
            }
            set
            {
                _ThumbColorTrue = value;
            }
        }
        public Color ThumbColor
        {
            get
            {
                return _ThumbColor;
            }
            set
            {
                _ThumbColor = value;
            }
        }
        public LinearGradientBrush _FalseColorLinearGradient;
        public LinearGradientBrush _TrueColorLinearGradient;
        public void SetDrawingTarget(Graphics g, Rectangle r)
        {
            //CurrentRenderTarget.SetOverrideDrawingTarget(g, r);
        }
        private LinearGradientBrush RefreshFalseColorLinearGradientDefault(uint alpha = 255, bool replace = true)
        {
            LinearGradientBrush ret;
            SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder br1 = new SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder(new Point(0, SliderRect.Top), new Point(0, SliderRect.Bottom), FalseColor, FalseColor);
            uint alphaMask = 0x01000000 * alpha;
            UInt32 shadowColor = 0xff000000 | 0xff888888 & (UInt32)FalseColor.ToArgb();
            br1.AddPosition(0).AddColor(0x7c7c7c | alphaMask)
                .AddPosition(0.40).AddColor(0xa0a0a0 | alphaMask)
                .AddPosition(1).AddColor(0xa0a0a0 | alphaMask);
            ret = br1.Build();
            if (replace)
            {
                if (_FalseColorLinearGradient != null)
                {
                    _FalseColorLinearGradient.Dispose();
                    _FalseColorLinearGradient = null;
                }
                _FalseColorLinearGradient = ret;
            }
            return ret;
        }
        private LinearGradientBrush RefreshFalseColorLinearGradient(uint alpha = 255, bool replace = true)
        {
            if (UseDefaultBackColor)
            {
                return RefreshFalseColorLinearGradientDefault(alpha, replace);
            }
            LinearGradientBrush ret;
            uint alphaMask = 0x01000000 * alpha;
            SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder br1 = new SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder(new Point(0, SliderRect.Top), new Point(0, SliderRect.Bottom), FalseColor, FalseColor);
            UInt32 shadowColor = alphaMask | (0x888888 & (UInt32)FalseColor.ToArgb());
            Color myFalseColor = Color.FromArgb((int)alpha, FalseColor);
            br1.AddPosition(0).AddColor(myFalseColor)
                .AddPosition(0.40).AddColor(myFalseColor)
                .AddPosition(1).AddColor(shadowColor);
            ret = br1.Build();
            if (replace)
            {
                if (_FalseColorLinearGradient != null)
                {
                    _FalseColorLinearGradient.Dispose();
                    _FalseColorLinearGradient = null;
                }
                _FalseColorLinearGradient = ret;
            }
            return ret;
        }
        private LinearGradientBrush RefreshTrueColorLinearGradientDefault(uint alpha = 255, bool replace = true)
        {
            LinearGradientBrush ret;
            uint alphaMask = 0x01000000 * alpha;
            SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder br1 = new SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder(new Point(0, SliderRect.Top), new Point(0, SliderRect.Bottom), TrueColor, TrueColor);
            UInt32 shadowColor = alphaMask | (0x888888 & (UInt32)TrueColor.ToArgb());
            br1.AddPosition(0).AddColor(0x008000 | alphaMask)
                .AddPosition(0.40).AddColor(0x00bf00 | alphaMask)
                .AddPosition(1).AddColor(0x00bf00 | alphaMask);
            ret = br1.Build();
            if (replace)
            {
                if (_TrueColorLinearGradient != null)
                {
                    _TrueColorLinearGradient.Dispose();
                    _TrueColorLinearGradient = null;
                }
                _TrueColorLinearGradient = ret;
            }
            return ret;
        }
        private LinearGradientBrush RefreshTrueColorLinearGradient(uint alpha = 255, bool replace = true)
        {
            if (UseDefaultBackColor)
            {
                return RefreshTrueColorLinearGradientDefault(alpha, replace);
            }
            LinearGradientBrush ret;
            uint alphaMask = 0x01000000 * alpha;
            SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder br1 = new SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder(new Point(0, SliderRect.Top), new Point(0, SliderRect.Bottom), TrueColor, TrueColor);
            Color myTrueColor = Color.FromArgb((int)alpha, TrueColor);
            UInt32 shadowColor = alphaMask | (0x888888 & (UInt32)TrueColor.ToArgb());
            br1.AddPosition(0).AddColor(myTrueColor)
                .AddPosition(0.40).AddColor(myTrueColor)
                .AddPosition(1).AddColor(shadowColor);
            ret = br1.Build();
            if (replace)
            {
                if (_TrueColorLinearGradient != null)
                {
                    _TrueColorLinearGradient.Dispose();
                    _TrueColorLinearGradient = null;
                }
                _TrueColorLinearGradient = ret;
            }
            return ret;
        }
        private LinearGradientBrush ThumbColorLinearGradient(int x, int y, int r, int alpha = 255)
        {
            if (DefaultThumbColor)
            {
                return ThumbColorLinearGradientDefault(x, y, r, alpha);
            }
            UInt32 shadowColor = 0xff000000 | 0xffcccccc & (UInt32)ThumbColor.ToArgb();
            Color alphaColor = Color.FromArgb(alpha, ThumbColor);
            SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder br1 = new SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder(new Point(x - r, y - r), new Point(x + r, y + r), alphaColor, alphaColor);
            br1.AddPosition(0).AddColor(alphaColor)
                .AddPosition(1).AddColor(shadowColor);
            return br1.Build();
        }
        private LinearGradientBrush ThumbColorLinearGradientDefault(int x, int y, int r, int alpha = 255)
        {
            UInt32 shadowColor = 0xff000000 | 0xffcccccc & (UInt32)ThumbColor.ToArgb();
            Color alphaColor = Color.FromArgb(alpha, ThumbColor);
            uint alphaMask = 0x01000000 * (uint)alpha;
            SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder br1 = new SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder(new Point(x, y - r), new Point(x, y + r), alphaColor, alphaColor);
            br1.AddPosition(0).AddColor(0xfcfff4 | (alphaMask))
                .AddPosition(0.4).AddColor(0xdfe5d7 | (alphaMask))
                .AddPosition(1).AddColor(0xb3bead | (alphaMask));
            return br1.Build();
        }
        private LinearGradientBrush ThumbColorLinearGradientTrue(int x, int y, int r, int alpha = 255)
        {
            if (DefaultThumbColor)
            {
                return ThumbColorLinearGradientTrueDefault(x, y, r, alpha);
            }
            UInt32 shadowColor = 0xff000000 | 0xff000000 & (UInt32)ThumbColorTrue.ToArgb();
            Color alphaColor = Color.FromArgb(alpha, ThumbColorTrue);
            SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder br1 = new SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder(new Point(x - r, y - r), new Point(x + r, y + r), alphaColor, alphaColor);

            br1.AddPosition(0).AddColor(alphaColor)
                .AddPosition(1).AddColor(shadowColor);
            return br1.Build();
        }
        private LinearGradientBrush ThumbColorLinearGradientTrueDefault(int x, int y, int r, int alpha = 255)
        {
            UInt32 shadowColor = 0xff000000 | 0xff000000 & (UInt32)ThumbColorTrue.ToArgb();
            Color alphaColor = Color.FromArgb(alpha, ThumbColorTrue);
            uint alphaMask = 0x01000000 * (uint)alpha;
            SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder br1 = new SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder(new Point(x, y - r), new Point(x, y + r), alphaColor, alphaColor);
            br1.AddPosition(0).AddColor(0xb3ffb3 | (alphaMask))
                .AddPosition(0.40).AddColor(0x80ff80 | (alphaMask))
                .AddPosition(1).AddColor(0x00bf00 | (alphaMask));
            return br1.Build();
        }
        public LinearGradientBrush FalseColorLinearGradient
        {
            get
            {
                if (_FalseColorLinearGradient == null)
                {
                    RefreshFalseColorLinearGradient();
                }
                return _FalseColorLinearGradient;
            }
        }
        public LinearGradientBrush TrueColorLinearGradient
        {
            get
            {
                if (_TrueColorLinearGradient == null)
                {
                    RefreshTrueColorLinearGradient();
                }
                return _TrueColorLinearGradient;
            }
        }
        public void ManualSet(bool bChecked)
        {
            _Checked = bChecked;
            if (this.Visible && !IsDisposed && IsHandleCreated)
                this.Invalidate();
        }
        private bool _Checked;
        public bool Checked
        {
            get
            {
                return _Checked;
            }
            set
            {
                bool origVal = _Checked;
                _Checked = value;
                if (origVal != _Checked)
                {
                    if (this.Visible && !IsDisposed && IsHandleCreated)
                    {
                        try
                        {
                            Invalidate();

                            if (OnCheckedChanged != null)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    OnCheckedChanged(this, _Checked);
                                }));
                            }
                        }
                        catch (Exception ee)
                        {
                            Console.WriteLine(ee.ToString());
                        }
                    }
                }
            }
        }
        volatile bool Transforming = false;
        private Rectangle SliderRect;
        private double Progress = 0;

        public SliderCheckbox()
        {
            InitializeComponent();
            CurrentRenderTarget.setUseAlpha(true);
            CurrentRenderTarget.onMouseClickHandler += (this.UponMouseClick);
            CurrentRenderTarget.SmoothMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            CurrentRenderTarget.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            CurrentRenderTarget.onMouseMoveHandler += (this.onMouseMove);
            sdlmmControl1.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            sdlmmControl1.SmoothMode = SmoothingMode.HighQuality;
            sdlmmControl1.InterpolationMode = InterpolationMode.Bicubic;
            RefreshSliderRect();
        }
        
        private void onMouseMove(int x, int y, int btn, bool on)
        {
            if (!Visible) return;
            if (this.Clickable)
            {
                if (Rectangle.FromLTRB(x, y, x, y).IntersectsWith(SliderRect))
                {
                    Cursor = Cursors.Hand;
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }
        }
        private void DrawThumbWithBitmap()
        {
            int x = SliderRect.Left;
            int x2 = SliderRect.Left + SliderRect.Width - ThumbBitmap.Width;
            int y = SliderRect.Top;
            if (Transforming)
            {
                int offset = 0;
                if (!Checked)
                {
                    offset = (int)(Progress * (-SliderRect.Width));
                    CurrentRenderTarget.drawImage(this.ThumbBitmap, x2 + offset, y, ThumbBitmap.Width, ThumbBitmap.Height);
                }
                else
                {
                    offset = (int)(Progress * SliderRect.Width);
                    CurrentRenderTarget.drawImage(this.ThumbBitmap, x + offset, y, ThumbBitmap.Width, ThumbBitmap.Height);
                }
            }
            else
            {
                if (!Checked)
                {
                    CurrentRenderTarget.drawImage(this.ThumbBitmap, x, y, ThumbBitmap.Width, ThumbBitmap.Height);
                }
                else
                {
                    CurrentRenderTarget.drawImage(this.ThumbBitmap, x2, y, ThumbBitmap.Width, ThumbBitmap.Height);
                }
            }
        }
        private void DrawThumbWithoutBitmap()
        {
            int r = SliderRect.Height / 2;
            int x = SliderRect.Left + r;
            int x2 = SliderRect.Left + SliderRect.Width - r;
            int y = SliderRect.Top + r;
            if (Transforming)
            {
                int offset = 0;
                int x3 = 0;
                if (Checked)
                {
                    offset = (int)(Progress * (-SliderRect.Width));
                    x3 = x2 + offset;
                    if (x3 < x)
                    {
                        x3 = x;
                    }
                    if (HasShadow)
                    {
                        CurrentRenderTarget.fillCircle(x3 + ShadowOffset, y + ShadowOffset, r + ShadowAdditionRadius, ShadowColor);
                    }
                    if (UseGradient)
                    {
                        using (LinearGradientBrush brush = ThumbColorLinearGradientTrue(x3, y, r + 1))
                        {
                            CurrentRenderTarget.fillCircle(x3, y, r, brush);
                        }
                        using (LinearGradientBrush brush = ThumbColorLinearGradient(x3, y, r + 1, (int)(Progress * 255)))
                        {
                            CurrentRenderTarget.fillCircle(x3, y, r, brush);
                        }

                    }
                    else
                    {

                        CurrentRenderTarget.fillCircle(x3, y, r, ThumbColorTrue.ToArgb());
                        CurrentRenderTarget.fillCircle(x3, y, r, Color.FromArgb((int)(Progress * 255), ThumbColor).ToArgb());
                    }
                }
                else
                {
                    offset = (int)(Progress * SliderRect.Width);
                    x3 = x + offset;
                    if (x3 > x2)
                    {
                        x3 = x2;
                    }
                    if (HasShadow)
                    {
                        CurrentRenderTarget.fillCircle(x3 + ShadowOffset, y + ShadowOffset, r + ShadowAdditionRadius, ShadowColor);
                    }
                    if (UseGradient)
                    {
                        using (LinearGradientBrush brush = ThumbColorLinearGradient(x3, y, r + 1))
                        {
                            CurrentRenderTarget.fillCircle(x3, y, r, brush);
                        }
                        using (LinearGradientBrush brush = ThumbColorLinearGradientTrue(x3, y, r + 1, (int)(Progress * 255)))
                        {
                            CurrentRenderTarget.fillCircle(x3, y, r, brush);
                        }
                    }
                    else
                    {
                        CurrentRenderTarget.fillCircle(x3, y, r, ThumbColor.ToArgb());
                        CurrentRenderTarget.fillCircle(x3, y, r, Color.FromArgb((int)(Progress * 255), ThumbColorTrue).ToArgb());
                    }
                }

            }
            else
            {
                if (!Checked)
                {
                    if (HasShadow)
                    {
                        CurrentRenderTarget.fillCircle(x + ShadowOffset, y + ShadowOffset, r + ShadowAdditionRadius, ShadowColor);
                    }
                    if (UseGradient)
                    {
                        using (LinearGradientBrush brush = ThumbColorLinearGradient(x, y, r + 1))
                        {
                            CurrentRenderTarget.fillCircle(x, y, r, brush);
                        }
                    }
                    else
                    {
                        CurrentRenderTarget.fillCircle(x, y, r, ThumbColor.ToArgb());
                    }

                }
                else
                {
                    if (HasShadow)
                    {
                        CurrentRenderTarget.fillCircle(x2 + ShadowOffset, y + ShadowOffset, r + ShadowAdditionRadius, ShadowColor);
                    }
                    if (UseGradient)
                    {
                        using (LinearGradientBrush brush = ThumbColorLinearGradientTrue(x2, y, r + 1))
                        {
                            CurrentRenderTarget.fillCircle(x2, y, r, brush);
                        }
                    }
                    else
                    {
                        CurrentRenderTarget.fillCircle(x2, y, r, ThumbColorTrue.ToArgb());
                    }

                }
            }
        }
        private void DrawThumb()
        {
            if (ThumbBitmap != null)
            {
                DrawThumbWithBitmap();
            }
            else
            {
                DrawThumbWithoutBitmap();
            }

        }
        private void DrawBackgroundWithBitmap()
        {
            if (Transforming)
            {
                if (!Checked)
                {
                    CurrentRenderTarget.drawImage(SliderBitmapFalse, SliderRect.X, SliderRect.Y, SliderRect.Width, SliderRect.Height);
                    CurrentRenderTarget.drawImage(SliderBitmapTrue, SliderRect.X, SliderRect.Y, SliderRect.Width, SliderRect.Height, (float)Progress);
                }
                else
                {
                    CurrentRenderTarget.drawImage(SliderBitmapTrue, SliderRect.X, SliderRect.Y, SliderRect.Width, SliderRect.Height);
                    CurrentRenderTarget.drawImage(SliderBitmapFalse, SliderRect.X, SliderRect.Y, SliderRect.Width, SliderRect.Height, (float)Progress);
                }
            }
            else
            {
                if (!Checked)
                {
                    CurrentRenderTarget.drawImage(SliderBitmapTrue, SliderRect.X, SliderRect.Y, SliderRect.Width, SliderRect.Height);
                }
                else
                {
                    CurrentRenderTarget.drawImage(SliderBitmapFalse, SliderRect.X, SliderRect.Y, SliderRect.Width, SliderRect.Height);
                }
            }
        }
        private void DrawBackgroundOnNoBitmap()
        {
            if (Transforming)
            {
                if (!Checked)
                {

                    if (UseGradient)
                    {
                        using (LinearGradientBrush br2 = RefreshTrueColorLinearGradient(Math.Min(255, (uint)(255 * Progress)), false))
                        {
                            CurrentRenderTarget.fillRoundRect(SliderRect.X, SliderRect.Y + (BackgroundHeightOffset / 2), SliderRect.Width, BackgroundHeight, BackgroundHeight / 2, FalseColorLinearGradient);
                            CurrentRenderTarget.fillRoundRect(SliderRect.X, SliderRect.Y + (BackgroundHeightOffset / 2), SliderRect.Width, BackgroundHeight, BackgroundHeight / 2, br2);
                        }
                    }
                    else
                    {
                        CurrentRenderTarget.fillRoundRect(SliderRect.X, SliderRect.Y + (BackgroundHeightOffset / 2), SliderRect.Width, BackgroundHeight, BackgroundHeight / 2, FalseColor.ToArgb());
                        CurrentRenderTarget.fillRoundRect(SliderRect.X, SliderRect.Y + (BackgroundHeightOffset / 2), SliderRect.Width, BackgroundHeight, BackgroundHeight / 2, Color.FromArgb((int)(255 * Progress) & 0xff, TrueColor).ToArgb());
                    }
                }
                else
                {
                    if (UseGradient)
                    {
                        using (LinearGradientBrush br2 = RefreshFalseColorLinearGradient(Math.Min(255, (uint)(255 * Progress)), false))
                        {
                            CurrentRenderTarget.fillRoundRect(SliderRect.X, SliderRect.Y + (BackgroundHeightOffset / 2), SliderRect.Width, BackgroundHeight, BackgroundHeight / 2, TrueColorLinearGradient);
                            CurrentRenderTarget.fillRoundRect(SliderRect.X, SliderRect.Y + (BackgroundHeightOffset / 2), SliderRect.Width, BackgroundHeight, BackgroundHeight / 2, br2);
                        }
                    }
                    else
                    {
                        CurrentRenderTarget.fillRoundRect(SliderRect.X, SliderRect.Y + (BackgroundHeightOffset / 2), SliderRect.Width, BackgroundHeight, BackgroundHeight / 2, TrueColor.ToArgb());
                        CurrentRenderTarget.fillRoundRect(SliderRect.X, SliderRect.Y + (BackgroundHeightOffset / 2), SliderRect.Width, BackgroundHeight, BackgroundHeight / 2, Color.FromArgb((int)(255 * Progress) & 0xff, FalseColor).ToArgb());
                    }
                }
            }
            else
            {
                if (!Checked)
                {
                    if (UseGradient)
                    {
                        CurrentRenderTarget.fillRoundRect(SliderRect.X, SliderRect.Y + (BackgroundHeightOffset / 2), SliderRect.Width, BackgroundHeight, BackgroundHeight / 2, FalseColorLinearGradient);
                    }
                    else
                    {
                        CurrentRenderTarget.fillRoundRect(SliderRect.X, SliderRect.Y + (BackgroundHeightOffset / 2), SliderRect.Width, BackgroundHeight, BackgroundHeight / 2, FalseColor.ToArgb());
                    }

                }
                else
                {
                    if (UseGradient)
                    {
                        CurrentRenderTarget.fillRoundRect(SliderRect.X, SliderRect.Y + (BackgroundHeightOffset / 2), SliderRect.Width, BackgroundHeight, BackgroundHeight / 2, TrueColorLinearGradient);
                    }
                    else
                    {
                        CurrentRenderTarget.fillRoundRect(SliderRect.X, SliderRect.Y + (BackgroundHeightOffset / 2), SliderRect.Width, BackgroundHeight, BackgroundHeight / 2, TrueColor.ToArgb());
                    }

                }
            }
        }
        private void RefreshSliderRect()
        {
            int width = (int)(this.Width * (1.0 - 2 * PadPercentageX));
            int height = (int)(this.Height * (1.0 - 2 * PadPercentageY));
            if (BoundsSet)
            {
                SliderRect = this.Bounds;
                this.BackgroundHeight = (int)(this.Bounds.Height * BackgroundHeightRatio);
                BackgroundHeightOffset = (int)(this.Bounds.Height * (1.0 - BackgroundHeightRatio));
            }
            else
            {
                SliderRect = new Rectangle((int)(this.Width * PadPercentageX), (int)(this.Height * PadPercentageY), width, height);
                this.BackgroundHeight = (int)(this.SliderRect.Height * BackgroundHeightRatio);
                BackgroundHeightOffset = (int)(this.SliderRect.Height * (1.0 - BackgroundHeightRatio));
            }


        }
        private void DrawBackgroundFlatten()
        {
            if (mFlattenBorderThickness <= 0) return;
            int height = Math.Max(1, SliderRect.Height - FlattenBorderThickness + 1);
            if (Transforming)
            {
                if (!Checked)
                {
                    Color color2 = Color.FromArgb((int)(255 * Progress), FlattenBorderColorTrue);
                    CurrentRenderTarget.drawRoundRect(
                        SliderRect.X,
                        SliderRect.Y,
                        SliderRect.Width,
                        height,
                        height / 2.0f, FlattenBorderColorFalse.ToArgb(), FlattenBorderThickness);
                    CurrentRenderTarget.drawRoundRect(
                        SliderRect.X,
                        SliderRect.Y,
                        SliderRect.Width,
                        height,
                        height / 2.0f, color2.ToArgb(), FlattenBorderThickness);
                }
                else
                {
                    Color color2 = Color.FromArgb((int)(255 * Progress), FlattenBorderColorFalse);
                    CurrentRenderTarget.drawRoundRect(
                        SliderRect.X,
                        SliderRect.Y,
                        SliderRect.Width,
                        height,
                        height / 2.0f, FlattenBorderColorTrue.ToArgb(), FlattenBorderThickness);
                    CurrentRenderTarget.drawRoundRect(
                        SliderRect.X,
                        SliderRect.Y,
                        SliderRect.Width,
                        height,
                        height / 2.0f, color2.ToArgb(), FlattenBorderThickness);
                }
            }
            else
            {
                if (!Checked)
                {
                    CurrentRenderTarget.drawRoundRect(
                        SliderRect.X,
                        SliderRect.Y,
                        SliderRect.Width,
                        height,
                        height / 2.0f, FlattenBorderColorFalse.ToArgb(), FlattenBorderThickness);
                }
                else
                {
                    CurrentRenderTarget.drawRoundRect(
                        SliderRect.X,
                        SliderRect.Y,
                        SliderRect.Width,
                        height,
                        height / 2.0f, FlattenBorderColorTrue.ToArgb(), FlattenBorderThickness);
                }
            }

        }
        private void DrawBackground()
        {
            if (SliderBitmapFalse != null && SliderBitmapTrue != null)
            {
                DrawBackgroundWithBitmap();
            }
            else
            {
                if (FlattenStyle)
                {
                    DrawBackgroundFlatten();
                }
                else
                {
                    DrawBackgroundOnNoBitmap();
                }
            }
        }
        private void DrawText()
        {
            if (!DrawTextOnSlider) return;
            int centerX = SliderRect.Left + SliderRect.Width / 2;
            int centerXLeft = centerX - SliderRect.Width / 4;
            int centerXRight = centerX + SliderRect.Width / 4;
            if (Transforming)
            {
                int alpha = 0;
                alpha = (int)(255 * (1 - Progress));
                if (!Checked)
                {
                    SizeF txtSize = CurrentRenderTarget.MeasureString(TextOnSliderFalse, this.Font);
                    CurrentRenderTarget.drawString(TextOnSliderFalse,
                        centerXRight - (int)txtSize.Width / 2,
                        SliderRect.Y + SliderRect.Height / 2 - (int)txtSize.Height / 2,
                        Color.FromArgb(alpha, TextOnSliderFalseColor).ToArgb(), this.Font
                        );
                }
                else
                {
                    SizeF txtSize = CurrentRenderTarget.MeasureString(TextOnSliderTrue, this.Font);
                    CurrentRenderTarget.drawString(TextOnSliderTrue,
                        centerXLeft - (int)txtSize.Width / 2,
                        SliderRect.Y + SliderRect.Height / 2 - (int)txtSize.Height / 2,
                        Color.FromArgb(alpha, TextOnSliderFalseColor).ToArgb(), this.Font
                        );
                }
            }
            else
            {
                if (!Checked)
                {
                    SizeF txtSize = CurrentRenderTarget.MeasureString(TextOnSliderFalse, this.Font);
                    CurrentRenderTarget.drawString(TextOnSliderFalse,
                        centerXRight - (int)txtSize.Width / 2,
                        SliderRect.Y + SliderRect.Height / 2 - (int)txtSize.Height / 2,
                        TextOnSliderFalseColor.ToArgb(), this.Font
                        );
                }
                else
                {
                    SizeF txtSize = CurrentRenderTarget.MeasureString(TextOnSliderTrue, this.Font);
                    CurrentRenderTarget.drawString(TextOnSliderTrue,
                        centerXLeft - (int)txtSize.Width / 2,
                        SliderRect.Y + SliderRect.Height / 2 - (int)txtSize.Height / 2,
                        TextOnSliderTrueColor.ToArgb(), this.Font
                        );
                }
            }
        }
        private void OnDraw()
        {

            DrawBackground();
            DrawThumb();
            DrawText();
        }
        SharpDXControl externalTarget = null;
        public SharpDXControl CurrentRenderTarget
        {
            get
            {
                if (externalTarget == null)
                {
                    return sdlmmControl1;
                }
                return externalTarget;
            }
        }
        public void SetRenderTarget(SharpDXControl target)
        {
            externalTarget = target;
        }
        volatile bool mEnabled = true;

        public bool Clickable
        {
            get
            {
                return mEnabled;
            }
            set
            {
                mEnabled = value;
            }
        }
        
        private void UponMouseClick(int x, int y, int btn, bool on)
        {
            if (!this.mEnabled) return;
            if (Transforming) return;
            if (!Rectangle.FromLTRB(x, y, x + Width, y + Height).IntersectsWith(SliderRect)) return;
            if (on)
            {
                Progress = 0;
                if (MillisPerStep == 0) MillisPerStep = 1;
                int steps = (int)((double)TransitionMillis / (double)MillisPerStep);
                double progressPerStep = 1.0 / (double)steps;
                if (progressPerStep <= 0)
                {
                    progressPerStep = 1.0;
                }
                Timer timer = new Timer() { Interval = MillisPerStep };
                timer.Tick += (o, e) =>
                {
                    if (Progress < 1.0)
                    {
                        OnDraw();
                        Invalidate();
                        if (Progress + progressPerStep < 1)
                        {
                            Progress += progressPerStep;
                        }
                        else
                        {
                            Progress = 1;
                        }
                    }
                    else
                    {
                        AfterTransformed(timer);
                    }
                };
                Transforming = true;
                timer.Start();

                this.Invalidate();
            }
        }
        private void AfterTransformed(Timer timer)
        {
            if (!this.Visible || !this.mEnabled) return;
            Progress = 1.0;
            Transforming = false;
            OnDraw();
            Invalidate();
            timer.Stop();
            Checked = !Checked;
        }
        public void PerformClick()
        {
            UponMouseClick(SliderRect.Left, SliderRect.Top, 0, true);
        }
        public volatile bool ClearBeforeRender = true;
        private void PaintBody()
        {
            if (!Visible) return;
            if (externalTarget == null)
            {
                if (this.BackColor != Color.Transparent)
                    CurrentRenderTarget.Clear(this.BackColor.ToArgb());
            }

            OnDraw();
            if (externalTarget == null)
                CurrentRenderTarget.flush();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!Visible) return;
            if (this.BackColor != Color.Transparent)
                e.Graphics.Clear(this.BackColor);
            
            PaintBody();
            sdlmmControl1.flush();
            e.Graphics.Flush();
            //this.Update();
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (!Visible) return;

            
            if (!UsePaintBackground) return;
            PaintBody();
            sdlmmControl1.flush();
            sdlmmControl1.Update();
            e.Graphics.Flush();
           // this.Update();
        }
        public override void Refresh()
        {
            base.Refresh();
        }
        public static Form Test(bool setBound = false)
        {
            Form ret = new Form();
            ret.Size = new Size(300, 200);
#if false
            ListView listView1 = new ListView();
            listView1.Items.Add(new ListViewItem("A"));
            listView1.Items.Add(new ListViewItem("B"));
            listView1.Items.Add(new ListViewItem("C"));
            listView1.OwnerDraw = true;
            Dictionary<int, object> objs = new Dictionary<int,object>();
            ret.Controls.Add(listView1);
            listView1.Dock = DockStyle.Fill;
            listView1.MouseClick += (o, e) => {
                ListViewItem item = listView1.GetItemAt(e.X, e.Y);
                if (item != null)
                {
                    item.Checked = !item.Checked;
                }
            };
            listView1.DrawItem += new DrawListViewItemEventHandler((sender, e) =>
            {
                ListView listView = (ListView)sender;

                // Check if e.Item is selected and the ListView has a focus.
                if (!listView.Focused && e.Item.Selected)
                {
                    Rectangle rowBounds = e.Bounds;
                    int leftMargin = e.Item.GetBounds(ItemBoundsPortion.Label).Left;
                    Rectangle bounds = new Rectangle(leftMargin, rowBounds.Top, rowBounds.Width - leftMargin, rowBounds.Height);
                    e.Graphics.FillRectangle(SystemBrushes.Highlight, bounds);
                }
                else
                {
                    SliderCheckbox slider = null;
                    if (!objs.ContainsKey(e.ItemIndex))
                    {
                        slider = new SliderCheckbox();
                        objs[e.ItemIndex] = slider;
                    }
                    else
                    {
                        slider = (SliderCheckbox)objs[e.ItemIndex];
                    }
                    slider.Checked = e.Item.Checked;
                    slider.Width = e.Bounds.Width;
                    slider.Height = e.Bounds.Height;
                    slider.Location = new Point(e.Bounds.Left,e.Bounds.Top);
                    slider.SetDrawingTarget(e.Graphics, e.Bounds);
                    //slider.PaintBody();
                    Bitmap bmp = new Bitmap(e.Bounds.Width,e.Bounds.Height);
                    slider.DrawToBitmap(bmp, e.Bounds);
                    e.Graphics.DrawImage(bmp, new Point(0, 0));
                    e.Graphics.Flush();
                    
                }
            });
#endif
            SliderCheckbox slider1 = new SliderCheckbox();
            ret.Controls.Add(slider1);
            if (setBound)
            {
                slider1.Bounds = new Rectangle(10, 10, 50, 20);
            }
            slider1.UseGradient = true;
            slider1.Dock = DockStyle.Fill;
            //slider1.ThumbColor = Color.FromArgb(unchecked((int)0xff00ff00));
            ret.ShowDialog();
            return ret;
        }

        private void SliderCheckbox_SizeChanged(object sender, EventArgs e)
        {
            if (!Visible) return;
            RefreshSliderRect();
            if (UseGradient)
            {
                RefreshFalseColorLinearGradient();
                RefreshTrueColorLinearGradient();
            }
        }

        private void SliderCheckbox_VisibleChanged(object sender, EventArgs e)
        {
            if (!Visible) return;
            RefreshSliderRect();
            if (UseGradient)
            {
                RefreshFalseColorLinearGradient();
                RefreshTrueColorLinearGradient();
            }
            if (this.IsHandleCreated)
            {
                this.Invalidate();
            }
        }
    }
}
