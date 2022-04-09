using SDLMMSharp.Base;
using SDLMMSharp.Engine;
using SDLMMSharp.Engine.Controls;
using SDLMMSharp.Engine.Scenes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDLMMSharp
{
    internal partial class SDLMMDemoForm : Form
    {
        public SDLMMDemoForm()
        {
            InitializeComponent();
            progressSlider1.SetValueRange(0.0, 100.0, 100);
        }

        private void buttonOpenTestScene_Click(object sender, EventArgs e)
        {
            FormTestScene frm = new FormTestScene();
            frm.Show(this);
        }
    }
    internal class FormTestScene:Form
    {
        IRenderer renderer;
        BaseEngine engine;
        private void InitializeComponent()
        {
            
            this.SuspendLayout();
            this.Size = new Size(500, 500);
            SharpDXControl control = new SharpDXControl(500,500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimizeBox = false;
            this.HelpButton = false;
            this.BackColor = Color.White;
            this.Controls.Add(control);
            control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            control.Dock = DockStyle.Fill;
            control.BackColor = Color.White;
            control.setUseAlpha(true);
            control.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            control.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            control.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            control.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            control.SmoothMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            renderer = control;
            this.ResumeLayout(true);
        }
        public FormTestScene()
        {
            InitializeComponent();
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            engine = new BaseEngine(renderer);
            engine.Start();
            engine.Renderer.PushScene(new TestScene(engine));
        }
        internal class TestScene:BaseScene
        {
            
            public TestScene(BaseEngine engine):base(engine)
            {

            }
            private SpriteObject createRectangle(int x,int y,int w,int h)
            {
                SpriteObject obj = new SpriteObject();
                obj.shape = new RectangleShape(new Rectangle(x,y,w,h),1,Color.Black, Color.White);
                return obj;
            }
            public override void Paint(IRenderer gc)
            {
                gc.Clear(unchecked((int)0xff000000));
                base.Paint(gc);
            }
            private SpriteObject createCircle(int x, int y, int r)
            {
                SpriteObject obj = new SpriteObject();
                obj.shape = new CircleShape(new Rectangle(x, y, r*2, r*2));
                obj.shape.StrokeWidth = 1;
                obj.shape.ForeColor = Color.Black;
                obj.shape.BackColor = Color.Yellow;
                
                return obj;
            }
            public override void Layout()
            {
                base.Layout();
                AddDraggableObject(createRectangle(10, 10, 50, 50));
                AddDraggableObject(createRectangle(30, 30, 50, 50));
                AddDraggableObject(createRectangle(100, 100, 150, 150));
                AddDraggableObject(createCircle(300, 300, 20));
                AddDraggableObject(createCircle(400, 200, 20));
            }
            public override void Start()
            {
                base.Start();
            }
        }
    }
}
