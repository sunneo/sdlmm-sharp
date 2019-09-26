using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDLMMSharp
{
    public class CursorIconManager
    {
        static Object locker = new Object();
        struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);
        [DllImport("user32.dll")]
        static extern IntPtr CreateIconIndirect(ref IconInfo icon);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool DestroyIcon(IntPtr handle);
        /// <summary>
        /// Create a cursor from a bitmap without resizing and with the specified
        /// hot spot
        /// </summary>
        static Cursor CreateCursorNoResize(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            IntPtr ptr = bmp.GetHicon();
            IconInfo tmp = new IconInfo();
            GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            DestroyIcon(ptr);
            ptr = CreateIconIndirect(ref tmp);
            Cursor ret = new Cursor(ptr);
            DestroyIcon(ptr);
            if (CursorGenerated != null)
            {
                CursorGenerated.Dispose();
                CursorGenerated = null;
            }
            CursorGenerated = ret;
            return ret;
        }

        private static IntPtr CursorIconHandler = IntPtr.Zero;
        /// <summary>
        /// Create a 32x32 cursor from a bitmap, with the hot spot in the middle
        /// </summary>
        public static Cursor CreateCursor(Bitmap bmp, int x, int y)
        {
            int xHotSpot = x;
            int yHotSpot = y;
            if (!CursorIconHandler.Equals(IntPtr.Zero))
            {
                DestroyIcon(CursorIconHandler);
                CursorIconHandler = IntPtr.Zero;
            }
            CursorIconHandler = bmp.GetHicon();
           
            IconInfo tmp = new IconInfo();
            GetIconInfo(CursorIconHandler, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            DestroyIcon(CursorIconHandler);
            CursorIconHandler = CreateIconIndirect(ref tmp);
            Cursor ret = new Cursor(CursorIconHandler);
            if (CursorGenerated != null)
            {
                CursorGenerated.Dispose();
                CursorGenerated = null;
            }
            CursorGenerated = ret;
            return ret;
        }
        static Bitmap cursorBMP = null;
        static Cursor CursorGenerated = null;
        public static void ReleaseCursorBMP(bool locked = false)
        {
            if (!locked)
            {
                lock (locker)
                {
                    if (cursorBMP != null)
                    {
                        cursorBMP.Dispose();
                        cursorBMP = null;
                        if (!CursorIconHandler.Equals(IntPtr.Zero))
                        {
                            DestroyIcon(CursorIconHandler);
                            CursorIconHandler = IntPtr.Zero;
                        }
                    }
                }
            }
            else
            {
                if (cursorBMP != null)
                {
                    cursorBMP.Dispose();
                    cursorBMP = null;
                    if (!CursorIconHandler.Equals(IntPtr.Zero))
                    {
                        DestroyIcon(CursorIconHandler);
                        CursorIconHandler = IntPtr.Zero;
                    }
                }
            }
        }
        public static Cursor CreateCursorWithBitmap(Bitmap bmp, Cursor referenceCursor = null, bool isStatic=false)
        {
            if (referenceCursor == null)
            {
                referenceCursor = Cursor.Current;
                if (referenceCursor == null)
                {
                    referenceCursor = Cursors.Default;
                }
            }
            lock (locker)
            {
                ReleaseCursorBMP(true);
               

                if (referenceCursor.Size.Width > 0 && referenceCursor.Size.Height > 0 && bmp.Height > 0)
                {
                    try
                    {
                        cursorBMP = new Bitmap(referenceCursor.Size.Width + 6 + (int)(bmp.Width), Math.Max(referenceCursor.Size.Height, (int)(bmp.Height)));         
                        using (Graphics g = Graphics.FromImage(cursorBMP))
                        {
                            Rectangle rect = new Rectangle(referenceCursor.HotSpot.X + referenceCursor.Size.Width, referenceCursor.HotSpot.Y, (int)bmp.Width, (int)bmp.Height);
                            g.DrawImage(bmp, rect);
                            referenceCursor.Draw(g, new Rectangle(0, 0, referenceCursor.Size.Width, referenceCursor.Size.Height));
                            g.Flush();
                        }
                        Cursor ret = CreateCursor(cursorBMP, referenceCursor.HotSpot.X, referenceCursor.HotSpot.Y);
                        if (isStatic)
                        {
                            cursorBMP = null;
                            CursorIconHandler = IntPtr.Zero;
                            CursorGenerated = null;
                        }
                        //ReleaseCursorBMP(true);
                        return ret;
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine(ee.ToString());
                    }
                }
                return Cursors.Default;
            }
        }
        public static Cursor CreateCursorWithText(String txt, Cursor referenceCursor = null, Font font = null,bool withBG=false,bool withBorder=false)
        {
            if (referenceCursor == null)
            {
                referenceCursor = Cursor.Current;
                if (referenceCursor == null)
                {
                    referenceCursor = Cursors.Default;
                }
            }
            if(font == null)
            {
                font = SystemFonts.DefaultFont;
            }
            lock (locker)
            {
                ReleaseCursorBMP(true);
                
                SizeF txtSize = TextRenderer.MeasureText(txt, font);
                
                cursorBMP = new Bitmap(referenceCursor.Size.Width + 6 + (int)(txtSize.Width),
                        Math.Max(referenceCursor.Size.Height, (int)(txtSize.Height)));

                if (referenceCursor.Size.Width > 0 && referenceCursor.Size.Height > 0 && txtSize.Height > 0 && cursorBMP != null)
                {
                    try
                    {
                        using (Graphics g = Graphics.FromImage(cursorBMP))
                        {
                            Rectangle rect = new Rectangle(referenceCursor.HotSpot.X + referenceCursor.Size.Width, referenceCursor.HotSpot.Y, (int)txtSize.Width, (int)txtSize.Height);
                            if(withBG)
                                g.FillRectangle(new SolidBrush(Color.White), rect);
                            if(withBorder)
                                g.DrawRectangle(new Pen(Color.Black), rect);
                            g.DrawString(txt, font, new SolidBrush(Color.Black), referenceCursor.HotSpot.X + referenceCursor.Size.Width, referenceCursor.HotSpot.Y);
                            referenceCursor.Draw(g, new Rectangle(0, 0, referenceCursor.Size.Width, referenceCursor.Size.Height));
                            g.Flush();
                        }
                        Cursor ret = CreateCursor(cursorBMP, referenceCursor.HotSpot.X, referenceCursor.HotSpot.Y);
                        //ReleaseCursorBMP(true);
                        return ret;
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine(ee.ToString());
                    }
                }
                return Cursors.Default;
            }
        }
    }
}
