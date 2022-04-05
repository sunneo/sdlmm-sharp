﻿using SDLMMSharp.Base;
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
        public RectangleShape ClickableRange;
        public Func<DraggableTarget, bool> CanEnterDelegate;
        public Func<bool, int, int, bool> mouseActionDelegate;
        public Func<bool, int, int, bool> mouseMovedDelegate;
        public Func<int, int, bool> mouseDoubleClickDelegate;
        volatile bool mouseActionDelegateRunning = false;
        volatile bool mouseMovedDelegateRunning = false;
        volatile bool mouseDoubleDelegateRunning = false;
        public RectangleShape shape;
        
        bool mDisposed = false;
        bool mEnabled = true;
        bool supportEnterItem;
        bool mselected = false;
        public LabelShape label;

        public virtual void SetSelected(bool value)
        {
            mselected = value;
        }

        public virtual bool SupportDrag()
        {
            return false;
        }

       
        public virtual bool isDisposed()
        {
            return mDisposed;
        }
        public void Dispose()
        {
            if (isDisposed()) return;
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
                    if (label != null)
                    {
                        label.ImageAlpha = 128;
                        label.ForeColor = Color.FromArgb(128,Color.Black);
                    }
                }
                else
                {
                    shape.ImageAlpha = 255;
                    if (label != null)
                    {
                        label.ImageAlpha = 255;
                        label.ForeColor = Color.FromArgb(255, Color.Black);
                    }
                }
            }
        }



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
            return shape.IsHit(x, y);
        }

        public virtual void Paint(IRenderer gc)
        {

        }

        public virtual void SetPosition(int x, int y)
        {
            shape.SetLocation(x, y);
        }

        public virtual void SetRectangle(Rectangle rect)
        {
            shape.SetRectangle(rect);
        }

        public virtual void SetSize(int width, int height)
        {
            shape.SetSize(width, height);
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
                        ret = mouseMovedDelegate(on, x, y);
                    }
                    catch (Exception ee)
                    {

                    }
                    mouseMovedDelegateRunning = false;
                }
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
            if (Enabled)
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
                        ret = mouseActionDelegate(on, x, y);
                    }
                    catch (Exception ee)
                    {

                    }
                    mouseActionDelegateRunning = false;
                }
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
                        ret = mouseDoubleClickDelegate(x, y);
                    }
                    catch (Exception ee)
                    {

                    }
                    mouseDoubleDelegateRunning = false;
                }
            }
            return ret;
        }

        public SpriteObject GetSpriteObject()
        {
            return null;
        }
    }
}
