using SDLMMSharp.Engine;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SDLMMSharp.Babylon3D
{
    /// <summary>
    /// N-Body 3D simulation demo - based on the ref-sdlmm nbody.c implementation.
    /// Simulates gravitational interactions between particles in 3D space.
    /// </summary>
    public class NBody3DDemo : Form
    {
        private const int SCREEN_WIDTH = 800;
        private const int SCREEN_HEIGHT = 600;
        private const int NUM_BODY = 2000;
        private const int LOOP_COUNT = 500;
        private const float MAX_X_AXIS = 1000;
        private const float MIN_X_AXIS = 0;
        private const float MAX_Y_AXIS = 1000;
        private const float MIN_Y_AXIS = 0;
        private const float MAX_VELOCITY = 200;
        private const float MIN_VELOCITY = -200;
        private const float MAX_MASS = 150;
        private const float MIN_MASS = 3;
        private const float GRAVITY_COEF = 3.3f;

        private IRenderer renderer;
        private BaseEngine engine;
        private NBody3DScene scene;
        private Timer animationTimer;
        private bool isRunning = false;
        private Label fpsLabel;
        private Label infoLabel;
        private int frameCount = 0;
        private DateTime lastFpsUpdate = DateTime.Now;

        public NBody3DDemo()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "N-Body 3D Simulation Demo";
            this.Size = new Size(816, 639);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Create renderer control
            SharpDXControl sharpDx = new SharpDXControl(SCREEN_WIDTH, SCREEN_HEIGHT);
            sharpDx.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            sharpDx.Dock = DockStyle.Fill;
            sharpDx.BackColor = Color.FromArgb(0x2f, 0x2f, 0x2f);
            sharpDx.setUseAlpha(true);
            sharpDx.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            sharpDx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            sharpDx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            sharpDx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            sharpDx.SmoothMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            renderer = sharpDx;
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

            // Create info label
            infoLabel = new Label
            {
                Text = "Press H for help",
                Location = new Point(10, 35),
                Size = new Size(400, 100),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Regular)
            };

            if (renderer is Control rendererCtrl)
            {
                rendererCtrl.Controls.Add(fpsLabel);
                rendererCtrl.Controls.Add(infoLabel);
            }

            // Create engine and scene
            engine = new BaseEngine(renderer);
            scene = new NBody3DScene(engine, SCREEN_WIDTH, SCREEN_HEIGHT);
            scene.InitializeBodies(NUM_BODY);

            // Set the scene as current
            engine.Renderer.SetRootScene(scene);
            scene.Start();

            // Setup animation timer (~60 FPS)
            animationTimer = new Timer();
            animationTimer.Interval = 16;
            animationTimer.Tick += AnimationTimer_Tick;

            this.Load += NBody3DDemo_Load;
            this.FormClosing += NBody3DDemo_FormClosing;
            this.KeyDown += NBody3DDemo_KeyDown;
        }

        private void NBody3DDemo_Load(object sender, EventArgs e)
        {
            isRunning = true;
            animationTimer.Start();
            engine.Start();
        }

        private void NBody3DDemo_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
            animationTimer.Stop();
            engine.End();
            scene?.End();
            renderer?.Dispose();
        }

        private void NBody3DDemo_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D0:
                case Keys.NumPad0:
                    scene.ShowMode = 0;
                    break;
                case Keys.D1:
                case Keys.NumPad1:
                    scene.ShowMode = 1;
                    break;
                case Keys.D2:
                case Keys.NumPad2:
                    scene.ShowMode = 2;
                    break;
                case Keys.D3:
                case Keys.NumPad3:
                    scene.ShowMode = 3;
                    break;
                case Keys.C:
                    scene.Centralize = !scene.Centralize;
                    break;
                case Keys.R:
                    scene.RandomSimulateFactor = !scene.RandomSimulateFactor;
                    break;
                case Keys.H:
                    scene.ShowHelp = !scene.ShowHelp;
                    break;
            }
            UpdateInfoLabel();
        }

        private void UpdateInfoLabel()
        {
            if (scene.ShowHelp)
            {
                infoLabel.Text = $"Show mode: {scene.ShowMode} [0-3]\n" +
                                $"Simulate factor: {scene.SimulateTimeFactor:F5}\n" +
                                $"Randomize factor: {(scene.RandomSimulateFactor ? "ON" : "OFF")} [R]\n" +
                                $"Centralize: {(scene.Centralize ? "ON" : "OFF")} [C]\n" +
                                $"Bodies: {NUM_BODY}";
            }
            else
            {
                infoLabel.Text = "Press H for help";
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (!isRunning) return;

            // Update scene (physics simulation)
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

            UpdateInfoLabel();
        }

        /// <summary>
        /// Run the demo as a standalone application.
        /// </summary>
        public static void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new NBody3DDemo());
        }
    }

    /// <summary>
    /// N-Body 3D Scene that handles the physics simulation and rendering.
    /// </summary>
    internal class NBody3DScene : Babylon3DScene
    {
        private class Body
        {
            public float X, Y, Z;
            public float VX, VY, VZ;
            public float Mass;
        }

        private Body[] bodies;
        private int bodyCount;
        private Random random = new Random();

        public int ShowMode { get; set; } = 3;
        public bool Centralize { get; set; } = false;
        public bool RandomSimulateFactor { get; set; } = true;
        public float SimulateTimeFactor { get; set; } = 0.01f;
        public bool ShowHelp { get; set; } = true;

        private const float MAX_X_AXIS = 1000;
        private const float MIN_X_AXIS = 0;
        private const float MAX_Y_AXIS = 1000;
        private const float MIN_Y_AXIS = 0;
        private const float MAX_Z_AXIS = 1000;
        private const float MIN_Z_AXIS = 0;
        private const float MAX_VELOCITY = 200;
        private const float MIN_VELOCITY = -200;
        private const float MAX_MASS = 150;
        private const float MIN_MASS = 3;
        private const float GRAVITY_COEF = 3.3f;

        public NBody3DScene(BaseEngine owner, int width, int height) : base(owner, width, height)
        {
        }

        public void InitializeBodies(int count)
        {
            bodyCount = count;
            bodies = new Body[count];
            for (int i = 0; i < count; i++)
            {
                bodies[i] = new Body
                {
                    X = (float)(random.NextDouble() * (MAX_X_AXIS - MIN_X_AXIS) + MIN_X_AXIS),
                    Y = (float)(random.NextDouble() * (MAX_Y_AXIS - MIN_Y_AXIS) + MIN_Y_AXIS),
                    Z = (float)(random.NextDouble() * (MAX_Z_AXIS - MIN_Z_AXIS) + MIN_Z_AXIS),
                    VX = 0,
                    VY = 0,
                    VZ = 0,
                    Mass = (float)(random.NextDouble() * (MAX_MASS - MIN_MASS) + MIN_MASS)
                };
            }
        }

        private float Clamp(float v, float minv, float maxv)
        {
            if (v > maxv) return maxv;
            if (v < minv) return minv;
            return v;
        }

        private void UpdateBody(int i, float simulateTime)
        {
            float sumX = 0, sumY = 0, sumZ = 0;

            for (int j = 0; j < bodyCount; j++)
            {
                if (j == i) continue;

                float xPos = bodies[j].X - bodies[i].X;
                float yPos = bodies[j].Y - bodies[i].Y;
                float zPos = bodies[j].Z - bodies[i].Z;
                float distance = (float)Math.Sqrt(xPos * xPos + yPos * yPos + zPos * zPos);

                if (distance == 0) continue;

                float force = GRAVITY_COEF * bodies[i].Mass / (distance * distance);
                sumX += force * xPos;
                sumY += force * yPos;
                sumZ += force * zPos;
            }

            bodies[i].VX += sumX * simulateTime;
            bodies[i].VY += sumY * simulateTime;
            bodies[i].VZ += sumZ * simulateTime;

            // Clamp velocities
            bodies[i].VX = Clamp(bodies[i].VX, MIN_VELOCITY, MAX_VELOCITY);
            bodies[i].VY = Clamp(bodies[i].VY, MIN_VELOCITY, MAX_VELOCITY);
            bodies[i].VZ = Clamp(bodies[i].VZ, MIN_VELOCITY, MAX_VELOCITY);

            // Update positions using clamped velocities
            bodies[i].X += bodies[i].VX * simulateTime;
            bodies[i].Y += bodies[i].VY * simulateTime;
            bodies[i].Z += bodies[i].VZ * simulateTime;
        }

        public override void Update(float deltaTime = 0.016f)
        {
            // Update all bodies
            for (int i = 0; i < bodyCount; i++)
            {
                UpdateBody(i, SimulateTimeFactor);
            }

            // Randomize simulate factor if enabled
            if (RandomSimulateFactor)
            {
                SimulateTimeFactor = (float)random.NextDouble();
            }
        }

        public override void Paint(IRenderer gc)
        {
            // Clear background
            gc.Clear(unchecked((int)0xff2f2f2f));

            // Calculate averages for centralization
            float avgX = 0, avgY = 0, avgZ = 0;
            for (int i = 0; i < bodyCount; i++)
            {
                avgX += bodies[i].X;
                avgY += bodies[i].Y;
                avgZ += bodies[i].Z;
            }
            avgX /= bodyCount;
            avgY /= bodyCount;
            avgZ /= bodyCount;

            // Draw all bodies
            for (int i = 0; i < bodyCount; i++)
            {
                int x, y, r;
                if (Centralize)
                {
                    x = (int)(device.WorkingWidth / 2 * (bodies[i].X / avgX));
                    y = (int)(device.WorkingHeight / 2 * (bodies[i].Y / avgY));
                }
                else
                {
                    x = (int)(avgX - bodies[i].X + device.WorkingWidth / 2);
                    y = (int)(avgY - bodies[i].Y + device.WorkingHeight / 2);
                }
                r = (int)(5 * (bodies[i].Z / avgZ));

                if (r < 0 || r > 255) continue;

                int color = unchecked((int)(0x00D0D0 | (r * 0xa0a000)));

                switch (ShowMode)
                {
                    case 0: // Circle outline
                        gc.drawCircle(x, y, Math.Max(1, r), color);
                        break;
                    case 1: // Pixel
                        gc.drawPixel(x, y, color);
                        break;
                    case 2: // Pixel + Circle
                        gc.drawPixel(x, y, unchecked((int)0xffffffff));
                        gc.drawCircle(x, y, Math.Max(1, r), color);
                        break;
                    case 3: // Filled circle
                        gc.fillCircle(x, y, Math.Max(1, r), color);
                        break;
                }
            }

            // Draw 2D overlay elements (if any)
            base.Paint(gc);
        }
    }
}
