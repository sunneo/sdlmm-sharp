using SDLMMSharp.Engine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDLMMSharp.Engine
{
    public class BaseEngine
    {
        IRenderer BaseControl;
        EngineRenderer renderer;
        EngineMouseHandler mouseHandler;
        public IRenderer GetCanvas()
        {
            return BaseControl;
        }
        public bool IsDisposed()
        {
            return BaseControl.IsDisposed();
        }
        public EngineRenderer Renderer
        {
            get
            {
                return renderer;
            }
            set
            {
                renderer = value;
            }
        }
        public EngineMouseHandler MouseHandler
        {
            get
            {
                return mouseHandler;
            }
            set
            {
                mouseHandler = value;
            }
        }
        public virtual void SetCursor(Cursor cursor)
        {
            Cursor.Current = cursor;
        }
        public virtual void ResetCursor()
        {
            SetCursor(Cursors.Arrow);
        }
        public BaseEngine(IRenderer ctrl)
        {
            this.BaseControl = ctrl;
            this.renderer = new EngineRenderer(this);
            this.mouseHandler = new EngineMouseHandler(this);
        }
        public void Start()
        {
            this.renderer.Start();
            this.mouseHandler.Start();
        }
        public void End()
        {
            this.renderer.End();
            this.mouseHandler.End();
        }
        public event IRendererDelegates.OnKeyboardAction KeyboardAction
        {
            add
            {
                BaseControl.onKeyboard += value;
            }
            remove
            {
                BaseControl.onKeyboard -= value;
            }
        }
        public event IRendererDelegates.OnMouseButtonAction MouseAction
        {
            add
            {
                BaseControl.onMouseClickHandler += value;
            }
            remove
            {
                BaseControl.onMouseClickHandler -= value;
            }
        }
        public event IRendererDelegates.OnMouseMoveAction MouseMove
        {
            add
            {
                BaseControl.onMouseMoveHandler += value;
            }
            remove
            {
                BaseControl.onMouseMoveHandler -= value;
            }
        }
        public event IRendererDelegates.OnMouseWheelAction MouseWheel
        {
            add
            {
                BaseControl.onMouseWhell += value;
            }
            remove
            {
                BaseControl.onMouseWhell -= value;
            }
        }

        public IScene GetCurrentScene()
        {
            return this.renderer.GetCurrentScene();
        }

        public void InvalidateRenderer()
        {
            if (this.renderer == null) return;
            this.renderer.Invalidate();
        }
    }
}
