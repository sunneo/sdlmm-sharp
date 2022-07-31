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
            SetBaseControl(ctrl);
            this.mouseHandler = new EngineMouseHandler(this);
            
        }
        public void SetBaseControl(IRenderer ctrl)
        {
            if(ctrl == this.BaseControl)
            {
                return;
            }
            bool migrateActions = false;
            if (this.BaseControl != null)
            {
                migrateActions = true;
            }
            this.BaseControl = ctrl;
            if (this.renderer == null)
            {
                this.renderer = new EngineRenderer(this);
            }
            BaseControl.onKeyboard += OnKeyAction;
            if (migrateActions)
            {
                foreach(IRendererDelegates.OnMouseButtonAction mouseact in mouseActions.Values)
                {
                    BaseControl.onMouseClickHandler += mouseact;
                }
                foreach (IRendererDelegates.OnMouseMoveAction mouseact in mouseMoveActions.Values)
                {
                    BaseControl.onMouseMoveHandler += mouseact;
                }
                foreach (IRendererDelegates.OnMouseWheelAction mouseact in mouseWheelActions.Values)
                {
                    BaseControl.onMouseWhell += mouseact;
                }
            }
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
        protected virtual void OnKeyAction(int keycode,bool ctrl,bool ison)
        {
            do
            {
                if (renderer == null) break;
                if (KeyboardAction == null) break;
                IScene scene = renderer.GetCurrentScene();
                if (scene == null) break;
                if(scene.OnKeyAction(keycode, ctrl, ison))
                {
                    return;
                }
            } while (false);
            if (KeyboardAction != null)
            {
                KeyboardAction(keycode, ctrl, ison);
            }
        }

        protected Dictionary<IRendererDelegates.OnMouseButtonAction, IRendererDelegates.OnMouseButtonAction> mouseActions = new Dictionary<IRendererDelegates.OnMouseButtonAction, IRendererDelegates.OnMouseButtonAction>();
        protected Dictionary<IRendererDelegates.OnMouseMoveAction, IRendererDelegates.OnMouseMoveAction> mouseMoveActions = new Dictionary<IRendererDelegates.OnMouseMoveAction, IRendererDelegates.OnMouseMoveAction>();
        protected Dictionary<IRendererDelegates.OnMouseWheelAction, IRendererDelegates.OnMouseWheelAction> mouseWheelActions = new Dictionary<IRendererDelegates.OnMouseWheelAction, IRendererDelegates.OnMouseWheelAction>();

        public event IRendererDelegates.OnKeyboardAction KeyboardAction;
        public event IRendererDelegates.OnMouseButtonAction MouseAction
        {
            add
            {
                BaseControl.onMouseClickHandler += value;
                this.mouseActions[value] = value;
            }
            remove
            {
                BaseControl.onMouseClickHandler -= value;
                this.mouseActions.Remove(value);
            }
        }
        public event IRendererDelegates.OnMouseMoveAction MouseMove
        {
            add
            {
                BaseControl.onMouseMoveHandler += value;
                this.mouseMoveActions[value] = value;
            }
            remove
            {
                BaseControl.onMouseMoveHandler -= value;
                this.mouseMoveActions.Remove(value);
            }
        }
        public event IRendererDelegates.OnMouseWheelAction MouseWheel
        {
            add
            {
                BaseControl.onMouseWhell += value;
                mouseWheelActions[value] = value;
            }
            remove
            {
                BaseControl.onMouseWhell -= value;
                mouseWheelActions.Remove(value);
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
