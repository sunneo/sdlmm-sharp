using SDLMMSharp.Engine;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SDLMMSharp.Babylon3D
{
    /// <summary>
    /// N-Body 3D gravitational simulation using Babylon3D engine.
    /// Based on ref-sdlmm/exams/nbody3d.c - NVIDIA CUDA nbody sample with softening parameter.
    /// 
    /// Physics: F = G * m_i * m_j * r / (r^2 + epsilon^2)^(3/2)
    /// 
    /// Controls:
    ///   0-3: Change display mode
    ///   C: Snap camera to cluster center
    ///   R: Toggle random simulation factor  
    ///   H: Toggle help
    ///   +/-: Zoom camera
    ///   Arrow keys/Mouse drag: Rotate camera
    /// </summary>
    public class NBody3DDemo : Form
    {
        private const int SCREEN_WIDTH = 1400;
        private const int SCREEN_HEIGHT = 800;
        private const int NUM_BODY = 3072;

        private IRenderer renderer;
        private BaseEngine engine;
        private NBody3DScene scene;
        private Timer animationTimer;
        private bool isRunning = false;

        public NBody3DDemo()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "N-Body 3D Simulation (Babylon3D)";
            this.Size = new Size(SCREEN_WIDTH + 16, SCREEN_HEIGHT + 39);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.KeyPreview = true;

            SharpDXControl sharpDx = new SharpDXControl(SCREEN_WIDTH, SCREEN_HEIGHT);
            sharpDx.Dock = DockStyle.Fill;
            sharpDx.BackColor = Color.Black;
            sharpDx.setUseAlpha(true);
            renderer = sharpDx;
            
            if (renderer is Control ctrl)
            {
                this.Controls.Add(ctrl);
                ctrl.MouseDown += Ctrl_MouseDown;
                ctrl.MouseMove += Ctrl_MouseMove;
                ctrl.MouseUp += Ctrl_MouseUp;
                ctrl.MouseWheel += Ctrl_MouseWheel;
            }

            engine = new BaseEngine(renderer);
            scene = new NBody3DScene(engine, SCREEN_WIDTH, SCREEN_HEIGHT);
            scene.InitializeBodies(NUM_BODY);

            engine.Renderer.SetRootScene(scene);
            scene.Start();

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
            scene.HandleKeyboard(e.KeyCode, e.Control, true);
        }

        private void Ctrl_MouseDown(object sender, MouseEventArgs e)
        {
            scene.HandleMouseButton(e.X, e.Y, true, e.Button);
        }

        private void Ctrl_MouseMove(object sender, MouseEventArgs e)
        {
            scene.HandleMouseMove(e.X, e.Y, e.Button != MouseButtons.None);
        }

        private void Ctrl_MouseUp(object sender, MouseEventArgs e)
        {
            scene.HandleMouseButton(e.X, e.Y, false, e.Button);
        }

        private void Ctrl_MouseWheel(object sender, MouseEventArgs e)
        {
            scene.HandleMouseWheel(e.Delta);
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (!isRunning) return;
            scene.Update(0.016f);
            engine.InvalidateRenderer();
        }

        public static void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new NBody3DDemo());
        }
    }

    internal class NBody3DScene : Babylon3DScene
    {
        private class Body
        {
            public float X, Y, Z, VX, VY, VZ, NewVX, NewVY, NewVZ, Mass, GravitationalPotential;
        }

        private const float MIN_X_AXIS = 0, MIN_Y_AXIS = 0, MIN_Z_AXIS = 0, MIN_VELOCITY = 1;
        private const float MAX_MASS = 300, MIN_MASS = 200;
        private float MAX_X_AXIS = 300.0f, MAX_Y_AXIS = 300.0f, MAX_Z_AXIS = 100.0f;
        private float MAX_VELOCITY = 10.0f, GRAVITY_COEF = 30.3f, SOFTENING = 100.001f, SOFTENING_SQUARED;

        private const float MOUSE_ROTATION_SENSITIVITY = 0.005f;
        private const float CAM_ANGLE_X_MAX = 1.5f, CAM_ANGLE_X_MIN = -1.5f;

        private static readonly int[] glowColors = {
            0xFFFF00, 0xFFFF40, 0xFFFF80, 0xFFFFC0, 0xFFFFFF,
            0xE0E000, 0xC0C000, 0xFFFF60, 0xFFFFA0, 0xFFFFE0
        };

        private Body[] bodies;
        private int bodyCount;
        private Random random = new Random();

        public int ShowMode { get; set; } = 3;
        public bool ShowHelp { get; set; } = true;
        public float SimulateTimeFactor { get; set; } = 0.02f;
        public bool RandomSimulateFactor { get; set; } = false;
        public float ParticleSize { get; set; } = 15.0f;

        private float camDist = 50.0f, camAngleX = 0.3f, camAngleY = 0.0f;
        private bool cameraTracking = true;
        private float targetCamAngleX = 0.3f, targetCamAngleY = 0.0f, targetCamDist = 50.0f;
        private const float camTransitionSpeed = 0.1f;

        private float snapshotCenterX, snapshotCenterY, snapshotCenterZ;
        private float currentCenterX, currentCenterY, currentCenterZ;

        private bool mouseDown = false;
        private int lastMouseX, lastMouseY;

        private Vector3[] particlePositions;
        private int[] particleColors;
        private Texture particleTexture;

        private int frameCount = 0;
        private DateTime lastFpsUpdate = DateTime.Now;
        private int currentFps = 0;

        public NBody3DScene(BaseEngine owner, int width, int height) : base(owner, width, height)
        {
            SOFTENING_SQUARED = SOFTENING * SOFTENING;
        }

        public void InitializeBodies(int count)
        {
            bodyCount = count;
            bodies = new Body[count];
            
            for (int i = 0; i < count; i++)
            {
                bodies[i] = new Body
                {
                    X = MIN_X_AXIS + (float)random.NextDouble() * (MAX_X_AXIS - MIN_X_AXIS),
                    Y = MIN_Y_AXIS + (float)random.NextDouble() * (MAX_Y_AXIS - MIN_Y_AXIS),
                    Z = MIN_Z_AXIS + (float)random.NextDouble() * (MAX_Z_AXIS - MIN_Z_AXIS),
                    VX = 0, VY = 0, VZ = 0, NewVX = 0, NewVY = 0, NewVZ = 0,
                    Mass = random.Next((int)MIN_MASS, (int)MAX_MASS)
                };
            }

            particlePositions = new Vector3[count];
            particleColors = new int[count];
            particleTexture = Texture.CreateGaussian(64);

            for (int i = 0; i < count; i++)
                particleColors[i] = glowColors[i % glowColors.Length];

            FindClusterCenter(ref snapshotCenterX, ref snapshotCenterY, ref snapshotCenterZ);
        }

        private float Clampf(float v, float minv, float maxv)
        {
            if (v > maxv) v = (v + maxv) / 2;
            if (v < minv) v = (v + minv) / 2;
            return v;
        }

        private void UpdateBody(int i)
        {
            float sumX = 0, sumY = 0, sumZ = 0, potential = 0.0f;

            for (int j = 0; j < bodyCount; j++)
            {
                if (j == i) continue;

                float xPos = bodies[j].X - bodies[i].X;
                float yPos = bodies[j].Y - bodies[i].Y;
                float zPos = bodies[j].Z - bodies[i].Z;

                float distSqr = xPos * xPos + yPos * yPos + zPos * zPos + SOFTENING_SQUARED;
                float invDist = 1.0f / (float)Math.Sqrt(distSqr);
                float invDistCube = invDist * invDist * invDist;
                float s = GRAVITY_COEF * bodies[j].Mass * invDistCube;

                sumX += s * xPos;
                sumY += s * yPos;
                sumZ += s * zPos;
                potential += bodies[j].Mass * invDist;
            }

            bodies[i].GravitationalPotential = potential;
            bodies[i].NewVX += sumX * SimulateTimeFactor;
            bodies[i].NewVY += sumY * SimulateTimeFactor;
            bodies[i].NewVZ += sumZ * SimulateTimeFactor;

            bodies[i].X += Clampf(bodies[i].NewVX, MIN_VELOCITY, MAX_VELOCITY) * SimulateTimeFactor;
            bodies[i].Y += Clampf(bodies[i].NewVY, MIN_VELOCITY, MAX_VELOCITY) * SimulateTimeFactor;
            bodies[i].Z += Clampf(bodies[i].NewVZ, MIN_VELOCITY, MAX_VELOCITY) * SimulateTimeFactor;

            bodies[i].VX = bodies[i].NewVX;
            bodies[i].VY = bodies[i].NewVY;
            bodies[i].VZ = bodies[i].NewVZ;
        }

        private void FindClusterCenter(ref float centerX, ref float centerY, ref float centerZ)
        {
            float maxPotential = -1e30f;
            int maxPotentialIdx = 0;

            for (int i = 0; i < bodyCount; i++)
                if (bodies[i].GravitationalPotential > maxPotential)
                {
                    maxPotential = bodies[i].GravitationalPotential;
                    maxPotentialIdx = i;
                }

            float sumX = 0, sumY = 0, sumZ = 0, totalWeight = 0, clusterRadius = 100.0f;

            for (int i = 0; i < bodyCount; i++)
            {
                float dx = bodies[i].X - bodies[maxPotentialIdx].X;
                float dy = bodies[i].Y - bodies[maxPotentialIdx].Y;
                float dz = bodies[i].Z - bodies[maxPotentialIdx].Z;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);

                if (dist < clusterRadius)
                {
                    float weight = bodies[i].Mass / (dist + 1.0f);
                    sumX += bodies[i].X * weight;
                    sumY += bodies[i].Y * weight;
                    sumZ += bodies[i].Z * weight;
                    totalWeight += weight;
                }
            }

            if (totalWeight > 0)
            {
                centerX = sumX / totalWeight;
                centerY = sumY / totalWeight;
                centerZ = sumZ / totalWeight;
            }
            else
            {
                centerX = bodies[maxPotentialIdx].X;
                centerY = bodies[maxPotentialIdx].Y;
                centerZ = bodies[maxPotentialIdx].Z;
            }
        }

        private void UpdateParticlePositions()
        {
            float scale = 0.1f;
            bool shouldCentralize = cameraTracking;

            for (int i = 0; i < bodyCount; i++)
            {
                if (shouldCentralize)
                    particlePositions[i] = new Vector3(
                        (bodies[i].X - snapshotCenterX) * scale,
                        (bodies[i].Y - snapshotCenterY) * scale,
                        (bodies[i].Z - snapshotCenterZ) * scale);
                else
                    particlePositions[i] = new Vector3(
                        (bodies[i].X - MAX_X_AXIS / 2) * scale,
                        (bodies[i].Y - MAX_Y_AXIS / 2) * scale,
                        (bodies[i].Z - MAX_Z_AXIS / 2) * scale);
            }
        }

        private void UpdateCamera()
        {
            if (cameraTracking)
            {
                camAngleX += (targetCamAngleX - camAngleX) * camTransitionSpeed;
                camAngleY += (targetCamAngleY - camAngleY) * camTransitionSpeed;
                camDist += (targetCamDist - camDist) * camTransitionSpeed;
            }

            if (camAngleX > CAM_ANGLE_X_MAX) camAngleX = CAM_ANGLE_X_MAX;
            if (camAngleX < CAM_ANGLE_X_MIN) camAngleX = CAM_ANGLE_X_MIN;

            float cosAngleX = (float)Math.Cos(camAngleX);
            float sinAngleX = (float)Math.Sin(camAngleX);
            float cosAngleY = (float)Math.Cos(camAngleY);
            float sinAngleY = (float)Math.Sin(camAngleY);

            camera.Position = new Vector3(
                camDist * cosAngleX * sinAngleY,
                camDist * sinAngleX,
                camDist * cosAngleX * cosAngleY);
            camera.Target = Vector3.Zero;
        }

        public override void Update(float deltaTime = 0.016f)
        {
            for (int i = 0; i < bodyCount; i++)
                UpdateBody(i);

            FindClusterCenter(ref currentCenterX, ref currentCenterY, ref currentCenterZ);
            UpdateCamera();
            UpdateParticlePositions();

            if (RandomSimulateFactor)
                SimulateTimeFactor = (float)random.NextDouble() * 2.0f;
        }

        public override void Paint(IRenderer gc)
        {
            device.Clear(unchecked((int)0xff000000));
            device.RenderParticles(camera, particlePositions, particleColors, ParticleSize, particleTexture, true);
            gc.drawPixels(device.BackBuffer, 0, 0, device.WorkingWidth, device.WorkingHeight);

            if (ShowHelp)
            {
                gc.drawString($"FPS: {currentFps}", 5, 5, unchecked((int)0xffffffff));
                gc.drawString($"simulate factor: {SimulateTimeFactor:F5}", 5, 25, unchecked((int)0xffffffff));
                gc.drawString($"particle size: {ParticleSize:F1}", 5, 65, unchecked((int)0xffffffff));
                gc.drawString($"cam dist:{camDist:F1} angle:({camAngleX:F2},{camAngleY:F2})", 5, 85, unchecked((int)0xffffffff));
                gc.drawString($"camera tracking: {(cameraTracking ? "on" : "off")} [C]", 5, 105, unchecked((int)0xffffffff));
                gc.drawString("[h]help [+/-]zoom [arrows/mouse]rotate [C]track [R]random", 5, 205, unchecked((int)0xffaaaaaa));
            }

            frameCount++;
            var now = DateTime.Now;
            var elapsed = (now - lastFpsUpdate).TotalSeconds;
            if (elapsed >= 1.0)
            {
                currentFps = (int)(frameCount / elapsed);
                frameCount = 0;
                lastFpsUpdate = now;
            }

            base.Paint(gc);
        }

        public void HandleKeyboard(Keys key, bool ctrl, bool isDown)
        {
            if (!isDown) return;

            switch (key)
            {
                case Keys.D0: case Keys.NumPad0: ShowMode = 0; break;
                case Keys.D1: case Keys.NumPad1: ShowMode = 1; break;
                case Keys.D2: case Keys.NumPad2: ShowMode = 2; break;
                case Keys.D3: case Keys.NumPad3: ShowMode = 3; break;
                case Keys.C:
                    cameraTracking = !cameraTracking;
                    if (cameraTracking)
                    {
                        snapshotCenterX = currentCenterX;
                        snapshotCenterY = currentCenterY;
                        snapshotCenterZ = currentCenterZ;
                    }
                    break;
                case Keys.R: RandomSimulateFactor = !RandomSimulateFactor; break;
                case Keys.H: ShowHelp = !ShowHelp; break;
                case Keys.Oemplus: case Keys.Add:
                    targetCamDist *= 0.9f;
                    if (targetCamDist < 10) targetCamDist = 10;
                    break;
                case Keys.OemMinus: case Keys.Subtract:
                    targetCamDist *= 1.1f;
                    if (targetCamDist > 200) targetCamDist = 200;
                    break;
                case Keys.Up:
                    targetCamAngleX += 0.1f;
                    if (targetCamAngleX > CAM_ANGLE_X_MAX) targetCamAngleX = CAM_ANGLE_X_MAX;
                    break;
                case Keys.Down:
                    targetCamAngleX -= 0.1f;
                    if (targetCamAngleX < CAM_ANGLE_X_MIN) targetCamAngleX = CAM_ANGLE_X_MIN;
                    break;
                case Keys.Left: targetCamAngleY += 0.1f; break;
                case Keys.Right: targetCamAngleY -= 0.1f; break;
            }
        }

        public void HandleMouseButton(int x, int y, bool isDown, MouseButtons button)
        {
            if (button == MouseButtons.Left)
            {
                mouseDown = isDown;
                lastMouseX = x;
                lastMouseY = y;
            }
        }

        public void HandleMouseMove(int x, int y, bool isButtonDown)
        {
            if (mouseDown && isButtonDown)
            {
                int dx = x - lastMouseX;
                int dy = y - lastMouseY;

                targetCamAngleY -= dx * MOUSE_ROTATION_SENSITIVITY;
                targetCamAngleX -= dy * MOUSE_ROTATION_SENSITIVITY;

                if (targetCamAngleX > CAM_ANGLE_X_MAX) targetCamAngleX = CAM_ANGLE_X_MAX;
                if (targetCamAngleX < CAM_ANGLE_X_MIN) targetCamAngleX = CAM_ANGLE_X_MIN;

                lastMouseX = x;
                lastMouseY = y;
            }
        }

        public void HandleMouseWheel(int delta)
        {
            if (delta > 0)
            {
                targetCamDist *= 0.9f;
                if (targetCamDist < 10) targetCamDist = 10;
            }
            else
            {
                targetCamDist *= 1.1f;
                if (targetCamDist > 200) targetCamDist = 200;
            }
        }

        public override void End()
        {
            particleTexture?.Dispose();
            base.End();
        }
    }
}
