using SDLMMSharp.Base;
using SDLMMSharp.Engine.Scenes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDLMMSharp.Engine.Controls
{
    public class DraggableTarget : IDraggableTarget, IDisposable
    {
        public static long StaticSeriesNo = 0;
        public long SeriesNo = StaticSeriesNo++;
        public event EventHandler SizeChanged;
        LinkedListNode<IDraggableTarget> m_ImageLayerHandler;
        public LinkedList<IDraggableTarget> Overlay = new LinkedList<IDraggableTarget>();
        public RectangleShape ClickableRange;
        public Dictionary<String, object> dataset = new Dictionary<string, object>();
        public Func<DraggableTarget, bool> CanEnterDelegate;
        public Func<DraggableTarget,bool, int, int, bool> mouseActionDelegate;
        public Func<DraggableTarget, bool, int, int, bool> mouseMovedDelegate;
        public Func<DraggableTarget, int, int, bool> mouseDoubleClickDelegate;
        volatile bool mouseActionDelegateRunning = false;
        volatile bool mouseMovedDelegateRunning = false;
        volatile bool mouseDoubleDelegateRunning = false;
        public RectangleShape shape = new RectangleShape() {  Visible=true, StrokeWidth=0};
        public bool IsDraggable = false;
        
        bool mDisposed = false;
        bool mEnabled = true;
        protected bool supportEnterItem;
        bool mselected = false;
        
        public Color BackColor
        {
            get
            {
                return shape.BackColor;
            }
            set
            {
                shape.BackColor = value;
            }
        }
        public bool NeedCache
        {
            get
            {
                return shape.NeedCache;
            }
            set
            {
                shape.NeedCache = value;
            }
        }
        public Color ForeColor
        {
            get
            {
                return shape.ForeColor;
            }
            set
            {
                shape.ForeColor = value;
            }
        }
        public int Left
        {
            get
            {
                return shape.Location.X;
            }
            set
            {
                Point pt = shape.Location;
                pt.X = value;
                shape.Location = pt;
            }
        }
        public int Right
        {
            get
            {
                return shape.Right;
            }
        }
        public int Bottom
        {
            get
            {
                return shape.Bottom;
            }
        }
        public int Top
        {
            get
            {
                return shape.Location.Y;
            }
            set
            {
                Point pt = shape.Location;
                pt.Y = value;
                shape.Location = pt;
            }
        }

        public virtual void BringToFront()
        {
            if (ImageLayerHandle == null) return;

            if (ImageLayerHandle.List == null) return;
            LinkedList<IDraggableTarget> list = ImageLayerHandle.List;
            list.Remove(ImageLayerHandle);
            this.ImageLayerHandle = list.AddLast(this);
            
        }
        public LinkedListNode<IDraggableTarget> ImageLayerHandle
        {
            get => m_ImageLayerHandler;
            set
            {
                bool removeFirst = false;
                if (value != null && value != m_ImageLayerHandler)
                {
                    removeFirst = true;
                }
                if(value == null)
                {
                    removeFirst = true;
                }
                if(removeFirst)
                {
                    if (m_ImageLayerHandler != null)
                    {
                        if (m_ImageLayerHandler.List != null)
                        {
                            m_ImageLayerHandler.List.Remove(m_ImageLayerHandler);
                        }
                    }
                }
                m_ImageLayerHandler = value;
            }
        }
        public LinkedList<IDraggableTarget> Controls
        {
            get
            {
                return Overlay;
            }
        }
        public int GetIndexOfControl(IDraggableTarget ctrl)
        {
            int pt = 0;
            for (LinkedListNode<IDraggableTarget> i = Controls.First; i != null; i = i.Next)
            {
                if (i.Value == ctrl)
                {
                    return pt;
                }
                ++pt;
            }
            return -1;
        }
        public IDraggableTarget GetControlAt(int idx)
        {
            int pt = 0;
            for(LinkedListNode<IDraggableTarget> i=Controls.First; i!=null; i=i.Next)
            {
                if(pt == idx)
                {
                    return i.Value;
                }
                ++pt;
            }
            return null;
        }
        public virtual void SetSelected(bool value)
        {
            mselected = value;
        }

        public virtual bool SupportDrag()
        {
            return IsDraggable;
        }

       
        public virtual bool isDisposed()
        {
            return mDisposed;
        }
        public virtual void Dispose()
        {
            if (isDisposed()) return;
            try
            {
                if (shape != null)
                {
                    shape.Dispose();
                    shape = null;
                }
                if (ClickableRange != null)
                {
                    ClickableRange.Dispose();
                    ClickableRange = null;
                }
                if (m_ImageLayerHandler != null && m_ImageLayerHandler.List != null)
                {
                    m_ImageLayerHandler.List.Remove(m_ImageLayerHandler);
                }
                m_ImageLayerHandler = null;
            }
            catch(Exception ee)
            {

            }
            mDisposed = true;
        }

        public bool Visible
        {
            get
            {
                if (shape == null) return false;
                return shape.Visible;
            }
            set
            {
                if (shape == null) return;
                shape.Visible = value;
                OnVisibleChanged();
            }
        }

        protected virtual void OnVisibleChanged()
        {

        }

        public virtual bool IsShapeIntersectsCanvas(IGraphics gc, Rectangle clientArea)
        {
            try
            {
                if (this.isDisposed() || this.shape == null)
                {
                    return false;
                }
                if (clientArea == null)
                {
                    clientArea = gc.ClientRectangle;
                }
                if (clientArea == null) return false;

                return clientArea.IntersectsWith(this.GetRectangle());
            }
            catch (Exception ee)
            {

            }
            return false;
        }

        
        public virtual bool Enabled
        {
            get
            {
                return mEnabled;
            }
            set
            {
                this.mEnabled = value;
                if (!value)
                {
                    shape.ImageAlpha = 128;
                }
                else
                {
                    shape.ImageAlpha = 255;
                }
            }
        }

        public BitmapWrap BackgroundImage
        {
            get
            {
                return shape.BackgroundImage;
            }
            set
            {
                shape.BackgroundImage = value;
            }
        }
        public Point Location
        {
            get
            {
                return shape.Location;
            }
            set
            {
                SetPosition(value.X, value.Y);
            }
        }
        public Size Size
        {
            get
            {
                return shape.Size;
            }
            set
            {               
                SetSize(value.Width,value.Height);
            }
        }
        public virtual void Refresh()
        {

        }
        protected virtual void OnSizeChanged()
        {
            if(SizeChanged!=null)
            {
                SizeChanged(this, EventArgs.Empty);
            }
        }

        public int Width
        {
            get
            {
                return shape.Rectangle.Width;
            }
            set
            {
                Rectangle rect = shape.Rectangle;
                rect.Width = value;
                SetRectangle(rect);
            }
        }
        public int Height
        {
            get
            {
                return shape.Rectangle.Height;
            }
            set
            {
                Rectangle rect = shape.Rectangle;
                rect.Height = value;
                SetRectangle(rect);
            }
        }
        public Rectangle Bounds
        {
            get
            {
                return this.shape.Rectangle;
            }
            set
            {
                this.shape.SetRectangle(value);
                OnSizeChanged();
            }
        }

        public object Tag;

        public virtual bool CanEnter()
        {
            if (!Enabled)
            {
                return false;
            }

            if (CanEnterDelegate != null)
            {
                try
                {
                    return CanEnterDelegate(this);
                }
                catch (Exception ee)
                {

                }
            }
            return supportEnterItem;
        }


        public virtual Point GetPosition()
        {
            return shape.Rectangle.Location;
        }

        public virtual Rectangle GetRectangle()
        {
            return shape.Rectangle;
        }

        public virtual Size GetSize()
        {
            return shape.Rectangle.Size;
        }

        public virtual bool IsBeloneScene(IScene scene)
        {
            return true;
        }

        public virtual bool IsHit(int x, int y)
        {
            if (shape == null) return false;
            return shape.IsHit(x, y);
        }

        public virtual void Paint(IRenderer gc)
        {
            if (!Visible) return;
            if (shape == null) return;

            shape.Paint(gc);
            foreach(IDraggableTarget overlay in Overlay)
            {
                if (!overlay.Visible) continue;
                overlay.Paint(gc);
            }
        }

        public virtual void SetPosition(int x, int y)
        {
            if (shape == null) return;
            shape.SetLocation(x, y);
            
        }
        public DraggableTarget()
        {
            shape.RectangleChanged += Shape_RectangleChanged;
        }

        private void Shape_RectangleChanged(object sender, KeyValuePair<Rectangle,Rectangle> e)
        {
            Point orig = e.Key.Location;
            foreach (IDraggableTarget overlay in Overlay)
            {
                Point pt = overlay.GetPosition();
                pt.Offset(e.Value.X - orig.X, e.Value.Y - orig.Y);
                overlay.SetPosition(pt.X, pt.Y);
            }
            OnSizeChanged();
        }

        public virtual void SetRectangle(Rectangle rect)
        {
            if (shape == null) return;
            shape.SetRectangle(rect);
            
        }

        public virtual void SetSize(int width, int height)
        {
            shape.SetSize(width, height);
            OnSizeChanged();
        }

        public Point mouseDownPosition;
        public virtual bool mouseMoved(bool on, int x, int y)
        {
            if (!Enabled)
            {
                return false;
            }
            bool ret = false;
            if (mouseMovedDelegate != null)
            {
                if (!mouseMovedDelegateRunning)
                {
                    mouseMovedDelegateRunning = true;
                    try
                    {
                        ret = mouseMovedDelegate(this,on, x, y);
                    }
                    catch (Exception ee)
                    {

                    }
                    mouseMovedDelegateRunning = false;
                }
            } 
            else
            {

            }
            return ret;
        }

        public virtual bool mouseAction(bool on, int x, int y)
        {
            if (on)
            {
                mouseDownPosition.X = x;
                mouseDownPosition.Y = y;
            }
            if (!Enabled)
            {
                return true;
            }
            bool ret = false;
            if (mouseActionDelegate != null)
            {
                if (!mouseActionDelegateRunning)
                {
                    mouseActionDelegateRunning = true;
                    try
                    {
                        ret = mouseActionDelegate(this, on, x, y);
                    }
                    catch (Exception ee)
                    {

                    }
                    mouseActionDelegateRunning = false;
                }
            }
            else
            {

            }
            return ret;
        }

        public virtual bool mouseDoubleClick(int x, int y)
        {
            if (Enabled)
            {
                return true;
            }
            bool ret = false;
            if (mouseDoubleClickDelegate != null)
            {
                if (!mouseDoubleDelegateRunning)
                {
                    mouseDoubleDelegateRunning = true;
                    try
                    {
                        ret = mouseDoubleClickDelegate(this, x, y);
                    }
                    catch (Exception ee)
                    {

                    }
                    mouseDoubleDelegateRunning = false;
                }
            }
            
            return ret;
        }

        public virtual SpriteObject GetSpriteObject()
        {
            return null;
        }
    }
}
