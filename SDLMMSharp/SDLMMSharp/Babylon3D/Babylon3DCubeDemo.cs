using SDLMMSharp.Engine;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SDLMMSharp.Babylon3D
{
    /// <summary>
    /// Demo form showing a rotating 3D cube using the Babylon3D software renderer.
    /// This is a port of the babylon3D_cube.c example.
    /// </summary>
    public class Babylon3DCubeDemo : Form
    {
        private IRenderer renderer;
        private BaseEngine engine;
        private Babylon3DScene scene;
        private Timer animationTimer;
        private bool isRunning = false;
        private Label fpsLabel;
        private int frameCount = 0;
        private DateTime lastFpsUpdate = DateTime.Now;

        public Babylon3DCubeDemo()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Babylon 3D - Rotating Cube Demo";
            this.Size = new Size(816, 639);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Create renderer control
            renderer = new SharpDXControl(800, 600);
            if (renderer is Control ctrl)
            {
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl);
            }

            // Create FPS label
            fpsLabel = new Label
            {
                Text = "FPS: 0",
                Location = new Point(10, 10),
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            if (renderer is Control rendererCtrl)
            {
                rendererCtrl.Controls.Add(fpsLabel);
            }

            // Create engine and scene
            engine = new BaseEngine(renderer);
            scene = new Babylon3DScene(engine, 800, 600);
            scene.AutoRotate = true;
            scene.RotationSpeed = 0.02f;

            // Create the cube mesh
            Mesh cube = Mesh.CreateCube("Cube");
            cube.Position = new Vector3(0, 0, 10);

            // Try to load texture (optional)
            try
            {
                Texture texture = Texture.Load("texture.png");
                if (texture != null)
                {
                    cube.Texture = texture;
                }
            }
            catch
            {
                // No texture available, cube will render with solid colors
            }

            scene.AddMesh(cube);

            // Setup camera
            scene.Camera.Position = new Vector3(0, 0, -10);
            scene.Camera.Target = Vector3.Zero;

            // Set the scene as current
            engine.Renderer.SetRootScene(scene);
            scene.Start();

            // Setup animation timer (~60 FPS)
            animationTimer = new Timer();
            animationTimer.Interval = 16;
            animationTimer.Tick += AnimationTimer_Tick;

            this.Load += Babylon3DCubeDemo_Load;
            this.FormClosing += Babylon3DCubeDemo_FormClosing;
        }

        private void Babylon3DCubeDemo_Load(object sender, EventArgs e)
        {
            isRunning = true;
            animationTimer.Start();
            engine.Start();
        }

        private void Babylon3DCubeDemo_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
            animationTimer.Stop();
            engine.End();
            scene?.End();
            renderer?.Dispose();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (!isRunning) return;

            // Update scene (rotates the cube)
            scene.Update(0.016f);

            // Trigger redraw
            engine.InvalidateRenderer();

            // Update FPS counter
            frameCount++;
            var now = DateTime.Now;
            var elapsed = (now - lastFpsUpdate).TotalSeconds;
            if (elapsed >= 1.0)
            {
                int fps = (int)(frameCount / elapsed);
                fpsLabel.Text = $"FPS: {fps}";
                frameCount = 0;
                lastFpsUpdate = now;
            }
        }

        /// <summary>
        /// Run the demo as a standalone application.
        /// </summary>
        public static void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Babylon3DCubeDemo());
        }
    }
}
