using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Threading;
using SDLMMSharp.Base;

namespace SDLMMSharp.Engine.Controls
{
    public class GifRendererSpriteObject:SpriteObject
    {
        private object locker = new object();
        volatile int mDuration = -1;
        volatile bool mKeepRatio = false;
        FrameDimension imageDimension = null;
        volatile Image mImage;
        public List<Image> imgs = new List<Image>();
        public List<int> durations = new List<int>();
        IEnumerator<int> PlayingIterator;
        Thread ThreadPlayer = null;
        object threadLocker = new object();
        List<int> SelectedIndexes = new List<int>();
        volatile bool mDoRepeat = false;
        volatile bool mAutoStart = false;
        volatile bool mPreGenerateFrame = true;

        public event EventHandler<int> FrameIndexChanged;

        public bool PreGenerateFrame
        {
            get
            {
                return mPreGenerateFrame;
            }
            set
            {
                mPreGenerateFrame = value;
                if (!value)
                {
                    lock (locker)
                    {
                        imgs.Clear();
                    }
                }
            }
        }


        [Browsable(true)]
        public bool AutoStart
        {
            get
            {
                return mAutoStart;
            }
            set
            {
                bool origVal = mAutoStart;
                mAutoStart = value;
                if (origVal != mAutoStart)
                {

                    if (Visible && mAutoStart)
                    {
                        if (durations.Count == 0)
                        {
                            _LoadImage(this.mImage);
                        }

                        Start();
                    }
                    else
                    {
                        Stop();
                    }
                }
            }
        }
        volatile bool IsRunning;
        public void Stop()
        {
            IsRunning = false;
            ThreadPlayer = null;
        }

        [Browsable(true)]
        public bool DoRepeat
        {
            get
            {
                return mDoRepeat;
            }
            set
            {
                mDoRepeat = value;
            }
        }
        [Browsable(true)]
        public bool KeepRatio
        {
            get
            {
                return mKeepRatio;
            }
            set
            {
                bool origVal = mKeepRatio;
                mKeepRatio = value;
                if (origVal != mKeepRatio)
                {
                    if (mImage != null)
                    {
                        _LoadImage(mImage);
                    }
                }
            }
        }
        public void LoadImage(Image img)
        {
            _LoadImage(img);
            if (mAutoStart && Visible)
            {
                this.Start();
            }
        }


        public event EventHandler ImageChanged;

        [Browsable(true)]
        public event EventHandler<Image> OnImageUpdated;



        [Browsable(true)]
        public Image Image
        {
            get
            {

                {
                    return mImage;
                }
            }
            set
            {

                {
                    Image origImage = mImage;
                    mImage = value;
                    if (mImage != null)
                    {
                        _LoadImage(mImage);
                    }
                    if (ImageChanged != null)
                    {
                        ImageChanged(this, EventArgs.Empty);
                    }
                }
            }
        }
        public class GifWrapper
        {
            Image mImage;
            volatile bool mPreGenerateFrame = true;
            FrameDimension imageDimension = null;
            public List<int> durations = new List<int>();
            public List<Image> imgs = new List<Image>();
            private void _LoadImage(Image img)
            {
                if (img == null)
                {
                    return;
                }
                if (mPreGenerateFrame)
                {
                    imgs.Clear();
                    if (img.FrameDimensionsList.Length > 0)
                    {
                        this.imageDimension = new FrameDimension(img.FrameDimensionsList[0]);
                        int count = img.GetFrameCount(this.imageDimension);

                        try
                        {
                            int outputWidth = img.Width;
                            int outputHeight = img.Height;
                            for (int i = 0; i < count; ++i)
                            {
                                img.SelectActiveFrame(this.imageDimension, i);
                                Bitmap bmp = null;
                                bmp = new Bitmap(outputWidth, outputHeight);
                                //extract next frame
                                using (Graphics graphics = Graphics.FromImage(bmp))
                                {
                                    graphics.DrawImage(img, 0, 0, outputWidth, outputHeight);
                                    PropertyItem item = img.GetPropertyItem(0x5100);
                                    int delay = (item.Value[0] + item.Value[1] * 256) * 10;
                                    durations.Add(delay);

                                    imgs.Add(bmp);
                                }
                            }
                        }
                        catch (Exception ee)
                        {
                            Console.WriteLine(ee.ToString());
                        }

                    }
                }
            }
            public System.Drawing.Image Image
            {
                get
                {
                    return mImage;
                }
                set
                {
                    mImage = value;
                    _LoadImage(this.mImage);
                }
            }
            [Browsable(true), ReadOnly(true)]
            public int FrameCount
            {
                get
                {
                    if (mImage == null)
                    {
                        return 0;
                    }
                    else
                    {
                        if (mPreGenerateFrame)
                        {
                            return imgs.Count;
                        }
                        else
                        {
                            Image img = mImage;
                            if (img.FrameDimensionsList.Length > 0)
                            {
                                FrameDimension dimension = new FrameDimension(img.FrameDimensionsList[0]);
                                int count = img.GetFrameCount(dimension);
                                return count;
                            }
                            return 0;
                        }
                    }
                }
            }
            private Image GetFrame(int idx)
            {
                if (mImage == null) return null;

                if (mPreGenerateFrame)
                {
                    return imgs[idx];
                }
                else
                {
                    this.mImage.SelectActiveFrame(this.imageDimension, idx);
                    return this.mImage;
                }
            }
        }


        [Browsable(true)]
        public int Duration
        {
            get
            {
                return mDuration;
            }
            set
            {
                mDuration = value;
            }
        }

        public GifRendererSpriteObject()
        {
            DoRepeat = true;
        }
        



        protected override void OnVisibleChanged()
        {
            if (this.Visible)
            {
                if (mAutoStart)
                {
                    if (durations.Count == 0)
                    {
                        _LoadImage(this.mImage);
                    }
                    Start();
                }
            }
            base.OnVisibleChanged();
        }

        protected override void OnSizeChanged()
        {
            lock (locker)
            {
                if (mKeepRatio)
                {
                    _LoadImage(mImage);
                }
            }
            base.OnSizeChanged();
        }
        
        
        private Image GetFrame(int idx)
        {
            if (mImage == null) return null;

            if (mPreGenerateFrame)
            {
                return imgs[idx];
            }
            else
            {
                this.mImage.SelectActiveFrame(this.imageDimension, idx);
                return this.mImage;
            }
        }

        public static Bitmap GetResizedBitmap(Image bmp, Size size, Color backColor, bool keepRatio = false, bool bAlignCenter = true)
        {
            Bitmap ret = new Bitmap(size.Width, size.Height);
            using (Graphics ctx = Graphics.FromImage(ret))
            {
                ctx.Clear(backColor);
                ctx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                ctx.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                ctx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                ctx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                if (!keepRatio)
                {
                    ctx.DrawImage(bmp, 0, 0, size.Width, size.Height);
                }
                else
                {
                    double ratiox = ((double)size.Width) / bmp.Width;
                    double ratioy = ((double)size.Height) / bmp.Width;
                    double ratio = Math.Min(ratiox, ratioy);
                    int newWidth = (int)(ratio * bmp.Width);
                    int newHeight = (int)(ratio * bmp.Height);
                    if (newWidth <= 0)
                    {
                        newWidth = 1;
                    }
                    if (newHeight <= 0)
                    {
                        newHeight = 1;
                    }
                    if (bAlignCenter)
                        ctx.DrawImage(bmp, size.Width / 2 - newWidth / 2, size.Height / 2 - newHeight / 2, newWidth, newHeight);
                    else
                        ctx.DrawImage(bmp, 0, size.Height / 2 - newHeight / 2, newWidth, newHeight);
                }
            }
            return ret;
        }
        private void ThreadPlayerBody()
        {
            int delay = 0;
            int Idx = 0;
            if (PlayingIterator == null) return;
            IsRunning = true;
            while (IsRunning)
            {
                DateTime dtStart = DateTime.Now;
                lock (locker)
                {
                    if (!PlayingIterator.MoveNext())
                    {
                        if (!DoRepeat)
                        {
                            return;
                        }
                        else
                        {
                            PlayingIterator = SelectedIndexes.GetEnumerator();
                            PlayingIterator.MoveNext();
                        }
                    }
                    if (!IsRunning) return;
                    Idx = PlayingIterator.Current;
                    mFrameIndex = Idx;
                    if (FrameIndexChanged != null)
                    {
                        FrameIndexChanged(this, mFrameIndex);
                    }
                    if (mDuration == -1)
                    {
                        delay = durations[Idx];
                    }
                    else
                    {
                        delay = mDuration;
                    }
                    Bitmap img = imgs[Idx] as Bitmap;
                    if (img == null) continue;
                    if (!IsRunning) return;
                    this.shape.BackgroundImage = img;
                    if (!IsRunning) return;
                    if (OnImageUpdated != null)
                    {
                        OnImageUpdated(this, img);
                    }
                }
                DateTime dtEnd = DateTime.Now;
                if (!IsRunning) return;
                int elapsed = (int)dtEnd.Subtract(dtStart).TotalMilliseconds;
                if (delay > 0)
                {
                    if (elapsed > delay)
                    {
                        Thread.Sleep(1);
                    }
                    else
                    {
                        Thread.Sleep(delay - elapsed);
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
                if (!IsRunning) return;
            }
        }

        public void Start(int start = 0, int len = -1)
        {
            if (this.mImage == null) return;
            lock (threadLocker)
            {
                if (ThreadPlayer != null && ThreadPlayer.IsAlive)
                {
                    ThreadPlayer.Abort();
                    ThreadPlayer = null;
                }
            }
            lock (locker)
            {
                SelectedIndexes.Clear();
                int count = FrameCount;
                if (len <= 0) len = count;
                int end = start + len;
                if (end > count) end = count;
                for (int i = start; i < end; ++i)
                {
                    SelectedIndexes.Add(i);
                }
                PlayingIterator = SelectedIndexes.GetEnumerator();
                PlayingIterator.MoveNext();
            }
            ThreadPlayer = new Thread(ThreadPlayerBody);
            ThreadPlayer.Name = "GifRenderer";
            ThreadPlayer.IsBackground = true;
            ThreadPlayer.Start();
        }

        private void CloseEverything()
        {
            lock (threadLocker)
            {
                if (ThreadPlayer != null && ThreadPlayer.IsAlive)
                {
                    ThreadPlayer.Abort();
                    ThreadPlayer = null;
                }
            }
            lock (locker)
            {
                SelectedIndexes.Clear();
                foreach (Image imgInLst in imgs)
                {
                    imgInLst.Dispose();
                }
                imgs.Clear();
            }
            if (mImage != null)
            {
                mImage.Dispose();
                mImage = null;
            }
        }

        [Browsable(true), ReadOnly(true)]
        public int FrameCount
        {
            get
            {
                if (mImage == null)
                {
                    return 0;
                }
                else
                {
                    if (PreGenerateFrame)
                    {
                        return imgs.Count;
                    }
                    else
                    {
                        Image img = mImage;
                        if (img.FrameDimensionsList.Length > 0)
                        {
                            FrameDimension dimension = new FrameDimension(img.FrameDimensionsList[0]);
                            int count = img.GetFrameCount(dimension);
                            return count;
                        }
                        return 0;
                    }
                }
            }
        }
        public int FrameIndex
        {
            get
            {
                return mFrameIndex;
            }
        }
        volatile int mFrameIndex = 0;
        private void _LoadImage(Image img)
        {
            if (img == null)
            {
                return;
            }
            if (PreGenerateFrame)
            {
                imgs.Clear();
                if (img.FrameDimensionsList.Length > 0)
                {
                    this.imageDimension = new FrameDimension(img.FrameDimensionsList[0]);
                    int count = img.GetFrameCount(this.imageDimension);

                    try
                    {
                        double ratioX = 1;
                        double ratioY = 1;
                        int outputWidth = img.Width;
                        int outputHeight = img.Height;
                        if (img.Height > img.Width)
                        {
                            ratioX = ((double)img.Width) / Width;
                            ratioY = ratioX;
                            outputWidth = (int)(img.Width / ratioX);
                            outputHeight = (int)(img.Height / ratioY);
                        }
                        else
                        {
                            ratioY = ((double)img.Height) / Height;
                            ratioX = ratioY;
                            outputWidth = (int)(img.Width / ratioX);
                            outputHeight = (int)(img.Height / ratioY);
                        }
                        for (int i = 0; i < count; ++i)
                        {
                            img.SelectActiveFrame(this.imageDimension, i);
                            Bitmap bmp = null;
                            if (mKeepRatio)
                            {
                                bmp = new Bitmap(outputWidth, outputHeight);
                            }
                            else
                            {
                                bmp = new Bitmap(outputWidth, outputHeight);
                            }
                            //extract next frame
                            using (Graphics graphics = Graphics.FromImage(bmp))
                            {
                                graphics.DrawImage(img, 0, 0, outputWidth, outputHeight);
                                PropertyItem item = img.GetPropertyItem(0x5100);
                                int delay = (item.Value[0] + item.Value[1] * 256) * 10;
                                durations.Add(delay);

                                imgs.Add(bmp);
                            }
                        }
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine(ee.ToString());
                    }

                }
            }
        }
        public void LoadFile(String filename)
        {
            CloseEverything();
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            {
                this.mImage = Image.FromStream(stream);
                _LoadImage(this.mImage);
                if (mAutoStart && Visible)
                {
                    this.Start();
                }
            }
        }
        public void Close()
        {
            CloseEverything();
        }
        volatile bool mIsDisposed = false;
        public new bool IsDisposed
        {
            get
            {
                return mIsDisposed;
            }
        }
        public virtual void Dispose()
        {
            if (!mIsDisposed)
            {
                CloseEverything();
            }
            mIsDisposed = true;
        }
        ~GifRendererSpriteObject()
        {
            Dispose();
        }
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        
    }
}
