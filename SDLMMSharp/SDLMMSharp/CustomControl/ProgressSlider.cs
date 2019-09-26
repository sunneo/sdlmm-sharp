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
using System.Runtime.InteropServices;

namespace SDLMMSharp.CustomControl
{
    public partial class ProgressSlider : UserControl
    {
        private volatile bool _UsePlainValue = false;
        public bool UsePlainValue
        {
            get
            {
                return _UsePlainValue;
            }
            set
            {
                _UsePlainValue = value;
            }
        }
        public decimal SelectedPlainValue = 0;
        private decimal PlainValueMin = 0;
        private decimal PlainValueMax = 100;
        private decimal PlainValueLen = 100;
        private decimal PlainValueStep = 1;
        private decimal PlainValuePartPerStep = 1;
        private void _SetValueRange(decimal min, decimal max, decimal step)
        {
            PlainValueMin = min;
            PlainValueMax = max;
            UsePlainValue = true;
            PlainValueStep = step;
            PlainValueLen = (PlainValueMax - PlainValueMin);
            PlainValuePartPerStep =  PlainValueLen/ PlainValueStep;
        } 

        /// <summary>
        /// Set Progress Slider to Plain Value Style
        /// 
        /// </summary>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        /// <param name="step">number of steps</param>
        public void SetValueRange(int min, int max, int step)
        {
            _SetValueRange(new decimal(min), new decimal(max), new decimal(step));
        }
        public void SetValueRange(double min,double max,int step)
        {
            _SetValueRange(new decimal(min), new decimal(max), new decimal(step));
        }
        public bool UseGradient = true;
        Color _SliderBackColor = Color.FromArgb(unchecked((int)0xff888888));
        Color _SliderForeColor = Color.FromArgb(unchecked((int)0xff00aaff));
        public double TextPercentageX = 0.05;
        public double TextPercentageY = 0.45;
        public Double PadPercentageX = 0.1;
        public Double PadPercentageY = 0.1;
        public List<ListViewItem> Items = new List<ListViewItem>();
        public bool AutoWrap = false;
        public double RoundRectangeRadiusRatio = 0.5;
        private bool BoundsSet = false;
        Rectangle sliderbound = Rectangle.Empty;
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
                }
            }
        }
        public String SelectedText
        {
            get
            {
                if (this.SelectedIndex < 0 || this.SelectedIndex >= Items.Count)
                {
                    return null;
                }
                return Items[SelectedIndex].Text;
            }
        }

        public Color SliderBackColor
        {
            get
            {
                return _SliderBackColor;
            }
            set
            {
                _SliderBackColor = value;
                RefreshBackGradient();
            }
        }
        public Color SliderForeColor
        {
            get
            {
                return _SliderForeColor;
            }
            set
            {
                _SliderForeColor = value;
                RefreshForeGraident();
            }
        }
        private void RefreshBackGradient()
        {
            SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder builder = new SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder(new Point(this.Left, this.progressStartY), new Point(this.Left, this.progressStartY + this.progressLengthY), this._SliderBackColor, this._SliderBackColor);
            uint colorArgb = unchecked((uint)_SliderBackColor.ToArgb());
            builder.AddColor(0xff404040 & colorArgb)
                .AddPosition(0)
                .AddColor(0xff888888 & colorArgb)
                .AddPosition(0.1)
                .AddColor(_SliderBackColor)
                .AddPosition(0.9)
                .AddColor(0xff888888 & colorArgb)
                .AddPosition(0.98)
                .AddColor(0xff404040 & colorArgb)
                .AddPosition(1);
            BackLinearGradient = builder.Build();
        }
        private void RefreshForeGraident()
        {
            SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder builder = new SDLMMSharp.SDLMMControl.LinearGradientBrushBuilder(new Point(this.Left, this.progressStartY), new Point(this.Left, this.progressStartY+this.progressLengthY), this._SliderForeColor, this._SliderForeColor);
            uint colorArgb = unchecked((uint)_SliderForeColor.ToArgb());
            builder.AddColor(0xff404040 | colorArgb)
                .AddPosition(0)
                .AddColor(0xff808080 | colorArgb)
                .AddPosition(0.1)
                .AddColor(_SliderForeColor)
                .AddPosition(0.9)
                .AddColor(0xff808080 | colorArgb)
                .AddPosition(0.98)
                .AddColor(0xff404040 | colorArgb)
                .AddPosition(1);
            ForeLinearGradient = builder.Build();
        }
        public Color TextColor = Color.FromArgb(unchecked((int)0xffffffff));
        public Font TextFont = SystemFonts.DefaultFont;
        int _SelectedIndex = -1;
        public int SelectedIndex
        {
            get
            {
                return _SelectedIndex;
            }
            set
            {
                SetSelectedIndex(value);
            }
        }
        public double Progress
        {
            get
            {
                if (UsePlainValue)
                {
                    return (double)((SelectedPlainValue - PlainValueMin+1) / PlainValueLen);
                }
                else
                {
                    if (SelectedIndex < 0) return 0;
                    if (Items.Count == 0) return 0;
                    return ((double)SelectedIndex + 1) / (double)Items.Count;
                }
            }
        }
        private double RulerGridGap = 0;
        private LinearGradientBrush BackLinearGradient;
        private LinearGradientBrush ForeLinearGradient;
        public Color BorderColor = Color.FromArgb(unchecked((int)0xffdddddd));
        Font myFont;
        private void DrawText()
        {
            int x = (int)(this.TextPercentageX * this.progressLength)+this.progressStart;
            int y = (int)(this.TextPercentageY * this.progressLengthY)+this.progressStartY;
            String txt = "";
            if (UsePlainValue)
            {
                txt = this.SelectedPlainValue.ToString();
            }
            else
            {
                txt = this.SelectedText;
            }
           
            int widthTxt = (int)sdlmmControl1.MeasureString(txt,myFont).Width;
            int remainSpace = this.progressLength - widthTxt;
            if (remainSpace < 0)
            {
                int overflowLength = (int)(-remainSpace / myFont.Height);
                int inProgressLength = txt.Length - overflowLength;
                txt = txt.Substring(0, inProgressLength);

            }
            sdlmmControl1.drawString(txt, x + 1, y + 1, ((~this.TextColor.ToArgb()) / 2) | unchecked((int)0x20000000), myFont);
            sdlmmControl1.drawString(txt, x, y, this.TextColor.ToArgb(), myFont);
        }

        private void DrawBorder()
        {
            sdlmmControl1.drawRoundRect(progressStart, progressStartY, progressLength, progressLengthY, (int)(progressLengthY * RoundRectangeRadiusRatio), BorderColor.ToArgb());
        }

        private void DrawFore()
        {
            if (!UsePlainValue)
            {
                if (this.Items.Count != 0)
                {
                    RulerGridGap = (double)this.progressLength / this.Items.Count;
                }
                if (_SelectedIndex < 0) return;
            }
            else
            {
                RulerGridGap = (double)(this.progressLength / (double)this.PlainValueStep);
            }
            double progress = this.Progress;

            if (progress >= 0)
            {
                if (!UseGradient)
                    sdlmmControl1.fillRect(progressStart, progressStartY, (int)(progressLength * progress), progressLengthY, this.SliderForeColor.ToArgb());
                else
                    sdlmmControl1.fillRect(progressStart, progressStartY, (int)(progressLength * progress), progressLengthY, ForeLinearGradient);
            }
        }

        private void DrawBack()
        {
            if (!UseGradient)
                sdlmmControl1.fillRect(progressStart, progressStartY, progressLength, progressLengthY, SliderBackColor.ToArgb());
            else
                sdlmmControl1.fillRect(progressStart, progressStartY, progressLength, progressLengthY, BackLinearGradient);
        }
        private void prepareStartLength()
        {
            if (!BoundsSet)
            {
                progressStart = (int)(this.Width * PadPercentageX);
                progressLength = (int)(this.Width * (1.0 - 2 * PadPercentageX));
                progressStartY = (int)(this.Height * PadPercentageY);
                progressLengthY = (int)(this.Height * (1.0 - 2 * PadPercentageY));
            }
            else
            {
                progressStart = Bounds.X;
                progressLength = Bounds.Width;
                progressStartY = Bounds.Y;
                progressLengthY = Bounds.Height;
            }
        }
        int progressLength;
        int progressStart;
        int progressStartY;
        int progressLengthY;
        private void OnDraw()
        {
            sdlmmControl1.Clear(Color.FromArgb(255,this.BackColor).ToArgb());
            prepareStartLength();
            DrawBack();
            DrawFore();
            //DrawBorder();
            DrawText();
            sdlmmControl1.flush();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (!Visible || !Enabled) return;
            base.OnPaint(e);
            OnDraw();
            e.Graphics.Flush();
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (!Visible || !Enabled) return;
            base.OnPaintBackground(e);
            OnDraw();
            e.Graphics.Flush();
        }
        public delegate void SelectedIndexChangeHandler(object sender, int idx);
        public event SelectedIndexChangeHandler SelectedIndexChange;
        private volatile bool HasClick = false;
        private int GetMouseClickPosition(int x)
        {
            if (!UsePlainValue)
            {
                if (Items.Count <= 1) return 0;
                if (x < 0) return 0;
                if (x >= Width) return Items.Count - 1;
                int offset = x - progressStart;
                int idx = 0;

                idx = (int)( offset / RulerGridGap);
                if (idx < 0) return 0;
                if (idx >= Items.Count) return Items.Count - 1;
                return idx;
            }
            else
            {
                int offset = x - progressStart;
                int idx = (int)(offset / RulerGridGap);
                if (idx < 0) return 0;
                if (idx >= PlainValueStep) idx = (int)( PlainValueStep);
                return idx;
            }
        }
        private void UponMouseClick(int x, int y, int btn, bool on)
        {
            if (!Visible || !Enabled) return;
            if (!Rectangle.FromLTRB(progressStart, progressStartY, progressStart + progressLength, progressStartY+progressLengthY).Contains(new Point(x, y)))
            {
                return;
            }
            if (!this.Focus())
            {
                this.sdlmmControl1.Focus();
            }
            HasClick = on;
            if (on)
            {
                SetSelectedIndex(GetMouseClickPosition(x),true);
                this.Invalidate();
            }
        }
 
        Point prevPos;

        private void UponMouseMove(int x, int y, int btn, bool on)
        {
            lock (this)
            {
                if (!Visible || !Enabled) return;
                if (Rectangle.FromLTRB(progressStart, progressStartY, progressStart + progressLength, progressStartY + progressLengthY).Contains(new Point(x, y)))
                {
#if true
                    if (!HasClick)
                    {
                        Cursor cur2 = Cursors.Hand;
                        Point currentPos = new Point(x, y);
                        if (!prevPos.Equals(currentPos))
                        {
                            prevPos = currentPos;
                            String txt = "";
                            if (!UsePlainValue)
                            {
                                int mousePos = GetMouseClickPosition(x);
                                txt = Items[mousePos].Text;
                            }
                            else
                            {
                                int mousePos = GetMouseClickPosition(x);
                                txt = (PlainValueMin + mousePos * PlainValuePartPerStep).ToString();
                            }
                            Cursor = CursorIconManager.CreateCursorWithText(txt, cur2, sdlmmControl1.Font);
                            CursorIconManager.ReleaseCursorBMP();
                        }
                    }
                    else
                    {
                        // clicked
                        CursorIconManager.ReleaseCursorBMP();
                        Cursor = Cursors.Hand;
                    }
#endif
                }
                else
                {
                    // out of range
                    CursorIconManager.ReleaseCursorBMP();
                    Cursor = Cursors.Default;
#if false
                toolTip1.Hide(this);
                this.Invalidate();
#endif
                }
                if (on && HasClick)
                {
                    // drag
                    SetSelectedIndex(GetMouseClickPosition(x), true);
                    this.Invalidate();
                }
                else
                {
                    HasClick = false;
                }
            }
        }
        protected override void OnLostFocus(EventArgs e)
        {
            Cursor = Cursors.Default;

            CursorIconManager.ReleaseCursorBMP();
            base.OnLostFocus(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (!Visible)
            {
                CursorIconManager.ReleaseCursorBMP();
            }
            base.OnVisibleChanged(e);
        }

        private void SetSelectedIndex(int idx,bool fromUI=false)
        {
            if (!UsePlainValue)
            {
                if (idx < 0)
                {
                    idx = 0;
                }
                else if (idx >= Items.Count)
                {
                    idx = Items.Count - 1;
                }
                int origIdx = _SelectedIndex;
                _SelectedIndex = idx;
                if (origIdx != _SelectedIndex)
                {
                    if (SelectedIndexChange != null)
                    {
                        SelectedIndexChange(this, _SelectedIndex);
                    }
                }
            }
            else
            {
                int origIdx = _SelectedIndex;
                _SelectedIndex = idx;
                decimal origVal = SelectedPlainValue;
                SelectedPlainValue = PlainValueMin + idx * PlainValuePartPerStep;
                if (origIdx != _SelectedIndex)
                {
                    if (SelectedIndexChange != null)
                    {
                        SelectedIndexChange(this, _SelectedIndex);
                    }
                }
            }
            if (!fromUI)
            {
                Invalidate();
            }
        }
        private void UponMouseWheel(int x, int y, int delta)
        {
            if (!Visible || !Enabled) return;
            if (delta < 0)
            {
                SetSelectedIndex(SelectedIndex - 1,true);
                this.Invalidate();
            }
            else
            {
                SetSelectedIndex(SelectedIndex + 1,true);
                this.Invalidate();
            }
        }
        public delegate void OnKeyHandler(object sender,int keycode,bool ctrl, bool on);
        public event OnKeyHandler OnKey;
        private void UponKeyboard(int keycode, bool ctrl, bool on)
        {
            if (!Visible || !Enabled) return;
            if ((sdlmmControl1.Focused || Focused) && OnKey != null)
            {
                OnKey(this,keycode, ctrl, on);
            }
            switch (keycode)
            {
                case 37: // left
                case 40: // down
                    SetSelectedIndex(SelectedIndex - 1,true);

                    this.Invalidate();
                    break;
                case 39: // right
                case 38: // up
                    SetSelectedIndex(SelectedIndex + 1,true);

                    this.Invalidate();
                    break;
            }
        }
        public ProgressSlider()
        {
            InitializeComponent();
            sdlmmControl1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            sdlmmControl1.SmoothMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            sdlmmControl1.onMouseMoveHandler =(UponMouseMove);
            sdlmmControl1.onMouseClickHandler = (UponMouseClick);
            sdlmmControl1.onMouseWhell =(UponMouseWheel);
            sdlmmControl1.onKeyboard = (UponKeyboard);
            sdlmmControl1.setUseAlpha(true);
            sdlmmControl1.Selectable = true;
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.ContainerControl, true);
            RefreshBackGradient();
            RefreshForeGraident();
            RefreshFont();
        }
        public static Form Test(bool setBound=false)
        {
            Form ret = new Form();
            ret.Size = new Size(300, 200);
            {
                ProgressSlider slider1 = new ProgressSlider();
               
                //slider1.Dock = DockStyle.Fill;
                slider1.UseGradient = true;
                slider1.Items.Add(new ListViewItem("Arrange"));
                slider1.Items.Add(new ListViewItem("B"));
                slider1.Items.Add(new ListViewItem("房子"));
                slider1.Items.Add(new ListViewItem("螢火"));
                slider1.Items.Add(new ListViewItem("這裡可以填幾個中文字許功蓋飯"));
                slider1.OnKey += new ProgressSlider.OnKeyHandler((sender, keycode, ctrl, on) =>
                {
                    if (on && keycode == (int)Keys.Enter)
                    {
                        MessageBox.Show("Current Value:" + (sender as ProgressSlider).SelectedText);
                    }
                    Console.WriteLine("KeyPress:{0}", keycode);

                });
            
                slider1.Bounds = new Rectangle(0, 0, 150, 20);
                slider1.Size = new Size(150, 40);
                slider1.Location = new Point(10, 10);
                slider1.SelectedIndex = 0;
                slider1.Visible = true;
                ret.Controls.Add(slider1);
            }
            
            {
                ProgressSlider slider2 = new ProgressSlider();
                
                slider2.UseGradient = true;
                slider2.Items.Add(new ListViewItem("9600"));
                slider2.Items.Add(new ListViewItem("12800"));
                slider2.Items.Add(new ListViewItem("25600"));
                slider2.Items.Add(new ListViewItem("115200"));
                slider2.Items.Add(new ListViewItem("假的"));
                slider2.SelectedIndexChange += (sender, idx) =>
                {
                    Console.WriteLine("Selected {0}", idx);
                };
                slider2.OnKey += new ProgressSlider.OnKeyHandler((sender, keycode, ctrl, on) =>
                {
                    if (on && keycode == (int)Keys.Enter)
                    {
                        MessageBox.Show("Current Value:" + (sender as ProgressSlider).SelectedText);
                    }
                    Console.WriteLine("KeyPress:{0}", keycode);

                });
                slider2.SelectedIndex = 0;

                slider2.Bounds = new Rectangle(0, 0, 150, 20);
                slider2.Size = new Size(150, 40);
                slider2.Location = new Point(10, ret.Height - 20);
                slider2.Visible = true;
                ret.Controls.Add(slider2);
            }

            ret.ShowDialog();
            return ret;
        }
        private void RefreshFont()
        {
            int fontHeight= Math.Max(1,this.Bounds.Height/2);
            fontHeight = Math.Min(fontHeight, 72);
            myFont = new Font(this.Font.FontFamily,fontHeight);
        }
        private void ProgressSlider_SizeChanged(object sender, EventArgs e)
        {
            if (!Visible || !Enabled) return;
            prepareStartLength();
            RefreshBackGradient();
            RefreshForeGraident();
            RefreshFont();
        }

        private void ProgressSlider_VisibleChanged(object sender, EventArgs e)
        {
            if (!Visible || !Enabled) return;
            prepareStartLength();
            RefreshBackGradient();
            RefreshForeGraident();
            RefreshFont();
        }

    }
}
