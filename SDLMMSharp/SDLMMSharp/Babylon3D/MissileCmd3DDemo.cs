using SDLMMSharp.Engine;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SDLMMSharp.Babylon3D
{
    /// <summary>
    /// Missile Command 3D demo - based on the ref-sdlmm missilecmd.c implementation.
    /// Classic missile defense game reimagined.
    /// </summary>
    public class MissileCmd3DDemo : Form
    {
        private const int SCREEN_WIDTH = 800;
        private const int SCREEN_HEIGHT = 600;
        private const int MAX_BUILD = 5;
        private const int MAX_RADIUS = 32;
        private const int MAX_MISSILE = 16;
        private const int MAX_ENEMY = 20;
        private const int MAX_ENEMY_SPEED = 2;

        private IRenderer renderer;
        private BaseEngine engine;
        private MissileCmd3DScene scene;
        private Timer animationTimer;
        private bool isRunning = false;
        private Label scoreLabel;

        public MissileCmd3DDemo()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Missile Command 3D Demo";
            this.Size = new Size(816, 639);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Create renderer control
            SharpDXControl sharpDx = new SharpDXControl(SCREEN_WIDTH, SCREEN_HEIGHT);
            sharpDx.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            sharpDx.Dock = DockStyle.Fill;
            sharpDx.BackColor = Color.Black;
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
                ctrl.MouseMove += Ctrl_MouseMove;
                ctrl.MouseClick += Ctrl_MouseClick;
            }

            // Create score label
            scoreLabel = new Label
            {
                Text = "Score: 0",
                Location = new Point(10, 10),
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            if (renderer is Control rendererCtrl)
            {
                rendererCtrl.Controls.Add(scoreLabel);
            }

            // Create engine and scene
            engine = new BaseEngine(renderer);
            scene = new MissileCmd3DScene(engine, SCREEN_WIDTH, SCREEN_HEIGHT);

            // Set the scene as current
            engine.Renderer.SetRootScene(scene);
            scene.Start();

            // Setup animation timer (~60 FPS)
            animationTimer = new Timer();
            animationTimer.Interval = 16;
            animationTimer.Tick += AnimationTimer_Tick;

            this.Load += MissileCmd3DDemo_Load;
            this.FormClosing += MissileCmd3DDemo_FormClosing;
        }

        private void Ctrl_MouseMove(object sender, MouseEventArgs e)
        {
            scene.SetMousePosition(e.X, e.Y);
        }

        private void Ctrl_MouseClick(object sender, MouseEventArgs e)
        {
            scene.LaunchMissile(e.X, e.Y);
        }

        private void MissileCmd3DDemo_Load(object sender, EventArgs e)
        {
            isRunning = true;
            animationTimer.Start();
            engine.Start();
        }

        private void MissileCmd3DDemo_FormClosing(object sender, FormClosingEventArgs e)
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

            // Update scene
            scene.Update(0.016f);

            // Update score label
            scoreLabel.Text = $"Score: {scene.Score:D4}  Missiles: {scene.RemainMissile:D4}  Enemies: {scene.RemainEnemy:D3}/{scene.RemainGenEnemy:D3}";

            // Trigger redraw
            engine.InvalidateRenderer();
        }

        /// <summary>
        /// Run the demo as a standalone application.
        /// </summary>
        public static void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MissileCmd3DDemo());
        }
    }

    /// <summary>
    /// Missile Command 3D Scene that handles game logic and rendering.
    /// </summary>
    internal class MissileCmd3DScene : Babylon3DScene
    {
        private const int MAX_BUILD = 5;
        private const int MAX_RADIUS = 32;
        private const int MAX_MISSILE = 16;
        private const int MAX_ENEMY = 20;
        private const int MAX_ENEMY_SPEED = 2;
        private const int BUILD_WIDTH = 64;
        private const int BUILD_HEIGHT = 64;

        private class Missile
        {
            public float FX, FY, TX, TY;
            public float X, Y, DX, DY;
            public bool Alive, Expl;
            public int R, TargetBuild;
            public bool IsHit;
        }

        private class OurLaunchedMissile
        {
            public int TX, TY, R;
            public bool Active, Expl;
            public float X, Y, DX, DY;
        }

        private class Build
        {
            public int Left, Top, Right, Bottom;
            public bool Alive, IsBuild;
        }

        private Build[] builds;
        private Missile[] enemies;
        private OurLaunchedMissile[] launchedMissiles;
        private Random random = new Random();
        private int mouseX, mouseY;

        public int Score { get; private set; } = 0;
        public int RemainMissile { get; private set; } = 45;
        public int RemainGenEnemy { get; private set; } = 40;
        public int RemainEnemy { get; private set; } = 40;
        private int maxEnemyMissile = 15;

        public MissileCmd3DScene(BaseEngine owner, int width, int height) : base(owner, width, height)
        {
            InitializeBuildings();
            InitializeGame();
        }

        private void InitializeBuildings()
        {
            builds = new Build[MAX_BUILD];
            int padding = 10;
            int buildTop = device.WorkingHeight - BUILD_HEIGHT;

            for (int i = 0; i < MAX_BUILD; i++)
            {
                builds[i] = new Build
                {
                    Left = (padding + BUILD_WIDTH) * i,
                    Top = buildTop,
                    Right = (padding + BUILD_WIDTH) * i + BUILD_WIDTH,
                    Bottom = device.WorkingHeight,
                    Alive = true,
                    IsBuild = true
                };
            }

            // Middle building is the launcher
            builds[MAX_BUILD / 2].IsBuild = false;
            builds[MAX_BUILD / 2].Top = device.WorkingHeight - BUILD_HEIGHT;
        }

        private void InitializeGame()
        {
            enemies = new Missile[MAX_ENEMY];
            for (int i = 0; i < MAX_ENEMY; i++)
            {
                enemies[i] = new Missile();
            }

            launchedMissiles = new OurLaunchedMissile[MAX_MISSILE];
            for (int i = 0; i < MAX_MISSILE; i++)
            {
                launchedMissiles[i] = new OurLaunchedMissile();
            }
        }

        public void SetMousePosition(int x, int y)
        {
            mouseX = x;
            mouseY = y;
        }

        public void LaunchMissile(int tx, int ty)
        {
            if (RemainMissile <= 0) return;

            for (int i = 0; i < MAX_MISSILE; i++)
            {
                if (!launchedMissiles[i].Active)
                {
                    int launcherIdx = MAX_BUILD / 2;
                    int sx = builds[launcherIdx].Left + 32;
                    int sy = builds[launcherIdx].Top;
                    float dx = (tx - sx) / 50.0f;
                    float dy = (ty - sy) / 50.0f;

                    launchedMissiles[i].Active = true;
                    launchedMissiles[i].X = sx;
                    launchedMissiles[i].Y = sy;
                    launchedMissiles[i].R = 3;
                    launchedMissiles[i].TX = tx;
                    launchedMissiles[i].TY = ty;
                    launchedMissiles[i].DX = dx;
                    launchedMissiles[i].DY = dy;
                    launchedMissiles[i].Expl = false;
                    RemainMissile--;
                    break;
                }
            }
        }

        private void GenerateEnemy()
        {
            if (RemainGenEnemy <= 0) return;

            int currentAlive = 0;
            for (int i = 0; i < MAX_ENEMY; i++)
            {
                if (currentAlive >= maxEnemyMissile) break;

                if (enemies[i].Alive)
                {
                    currentAlive++;
                    continue;
                }

                int targetIdx = random.Next(MAX_BUILD);
                int sx = random.Next(device.WorkingWidth);
                int sy = 0;
                int tx = (builds[targetIdx].Left + builds[targetIdx].Right) / 2;
                int ty = builds[targetIdx].Top;
                int enemySpeed = random.Next(MAX_ENEMY_SPEED);
                if (enemySpeed == 0) enemySpeed = 1;

                float dx = (tx - sx) / (1024.0f / enemySpeed);
                float dy = (ty - sy) / (1024.0f / enemySpeed);

                enemies[i].X = enemies[i].FX = sx;
                enemies[i].Y = enemies[i].FY = sy;
                enemies[i].DX = dx;
                enemies[i].DY = dy;
                enemies[i].TX = tx;
                enemies[i].TY = ty;
                enemies[i].Expl = false;
                enemies[i].Alive = true;
                enemies[i].R = 2;
                enemies[i].TargetBuild = targetIdx;
                enemies[i].IsHit = false;

                currentAlive++;
                RemainGenEnemy--;
                if (RemainGenEnemy == 0) return;
            }
        }

        private void UpdateEnemies()
        {
            for (int i = 0; i < MAX_ENEMY; i++)
            {
                if (!enemies[i].Alive) continue;

                if (enemies[i].Expl)
                {
                    if (enemies[i].R >= MAX_RADIUS * 2)
                    {
                        enemies[i].Alive = false;
                        enemies[i].IsHit = false;
                        enemies[i].Expl = false;
                        if (RemainEnemy - 1 >= 0)
                            RemainEnemy--;
                    }
                    enemies[i].R += 2;
                }
                else
                {
                    // Check collision with our missiles
                    for (int j = 0; j < MAX_MISSILE; j++)
                    {
                        if (!launchedMissiles[j].Active || !launchedMissiles[j].Expl) continue;

                        float distX = enemies[i].X - launchedMissiles[j].X;
                        float distY = enemies[i].Y - launchedMissiles[j].Y;
                        if (distX * distX + distY * distY < launchedMissiles[j].R * launchedMissiles[j].R)
                        {
                            enemies[i].Expl = true;
                            enemies[i].IsHit = true;
                            Score += 100;
                            break;
                        }
                    }

                    // Check collision with other explosions
                    for (int j = 0; j < MAX_ENEMY; j++)
                    {
                        if (j == i || !enemies[j].Alive || !enemies[j].Expl || !enemies[j].IsHit) continue;

                        float distX = enemies[i].X - enemies[j].X;
                        float distY = enemies[i].Y - enemies[j].Y;
                        if (distX * distX + distY * distY < enemies[j].R * enemies[j].R)
                        {
                            enemies[i].Expl = true;
                            enemies[i].IsHit = true;
                            Score += 100;
                            break;
                        }
                    }

                    enemies[i].X += enemies[i].DX;
                    enemies[i].Y += enemies[i].DY;

                    // Check if reached target
                    if (enemies[i].X >= enemies[i].TX - 1 && enemies[i].X <= enemies[i].TX + 1 &&
                        enemies[i].Y >= enemies[i].TY - 1 && enemies[i].Y <= enemies[i].TY + 1)
                    {
                        enemies[i].Expl = true;
                        builds[enemies[i].TargetBuild].Alive = false;
                    }
                }
            }
        }

        private void UpdateMissiles()
        {
            for (int i = 0; i < MAX_MISSILE; i++)
            {
                if (!launchedMissiles[i].Active) continue;

                if (!launchedMissiles[i].Expl)
                {
                    if ((int)(launchedMissiles[i].TX - launchedMissiles[i].X) == 0 &&
                        0 == (int)(launchedMissiles[i].TY - launchedMissiles[i].Y))
                    {
                        launchedMissiles[i].Expl = true;
                    }
                    launchedMissiles[i].X += launchedMissiles[i].DX;
                    launchedMissiles[i].Y += launchedMissiles[i].DY;
                }
                else
                {
                    if (launchedMissiles[i].R < MAX_RADIUS)
                    {
                        launchedMissiles[i].R++;
                    }
                    else
                    {
                        launchedMissiles[i].Active = false;
                        launchedMissiles[i].Expl = false;
                    }
                }
            }
        }

        private void Reinit()
        {
            if (RemainEnemy <= 0 && RemainGenEnemy <= 0)
            {
                InitializeBuildings();
                RemainEnemy = 40;
                RemainGenEnemy = 40;
                RemainMissile = 45;
            }
        }

        public override void Update(float deltaTime = 0.016f)
        {
            Reinit();
            UpdateMissiles();
            UpdateEnemies();

            if (random.Next(100) < 20)
            {
                GenerateEnemy();
            }
        }

        public override void Paint(IRenderer gc)
        {
            // Clear background
            gc.Clear(unchecked((int)0xff000000));

            // Draw buildings
            for (int i = 0; i < MAX_BUILD; i++)
            {
                int color = builds[i].Alive ? unchecked((int)0xff00ff00) : unchecked((int)0xff808080);
                if (builds[i].IsBuild)
                {
                    gc.fillRect(builds[i].Left, builds[i].Top, BUILD_WIDTH, BUILD_HEIGHT, color);
                    gc.drawRect(builds[i].Left, builds[i].Top, BUILD_WIDTH, BUILD_HEIGHT, unchecked((int)0xffffffff));
                }
                else
                {
                    // Draw launcher
                    gc.fillRect(builds[i].Left + 16, builds[i].Top, 32, BUILD_HEIGHT, unchecked((int)0xff0000ff));
                    gc.drawRect(builds[i].Left + 16, builds[i].Top, 32, BUILD_HEIGHT, unchecked((int)0xffffffff));
                }
            }

            // Draw our missiles
            for (int i = 0; i < MAX_MISSILE; i++)
            {
                if (!launchedMissiles[i].Active) continue;

                if (!launchedMissiles[i].Expl)
                {
                    // Draw missile trail
                    float dx = launchedMissiles[i].DX;
                    float dy = launchedMissiles[i].DY;
                    for (int j = 0; j < 8; j++)
                    {
                        int trailColor = unchecked((int)(0xffffff - 0x101010 * (j + 1)));
                        gc.fillCircle((int)(launchedMissiles[i].X - j * dx), 
                                     (int)(launchedMissiles[i].Y - j * dy), 
                                     launchedMissiles[i].R + j, trailColor);
                    }
                    gc.fillCircle((int)launchedMissiles[i].X, (int)launchedMissiles[i].Y, 
                                 launchedMissiles[i].R, unchecked((int)0xffffff00));
                }
                else
                {
                    // Draw explosion - use bright yellow/orange colors
                    int r = random.Next(200, 256);
                    int g = random.Next(100, 200);
                    int b = random.Next(0, 100);
                    int explColor = unchecked((int)(0xff000000 | (r << 16) | (g << 8) | b));
                    gc.fillCircle((int)launchedMissiles[i].X, (int)launchedMissiles[i].Y, 
                                 launchedMissiles[i].R, explColor);
                }
            }

            // Draw enemy missiles
            for (int i = 0; i < MAX_ENEMY; i++)
            {
                if (!enemies[i].Alive) continue;

                if (enemies[i].Expl)
                {
                    // Generate random explosion color in yellow/orange/red range
                    int r = random.Next(200, 256);
                    int g = random.Next(100, 200);
                    int b = 0;
                    int explColor = unchecked((int)(0xff000000 | (r << 16) | (g << 8) | b));
                    gc.fillCircle((int)enemies[i].X, (int)enemies[i].Y, enemies[i].R, explColor);
                }
                else
                {
                    gc.drawLine((int)enemies[i].FX, (int)enemies[i].FY, 
                               (int)enemies[i].X, (int)enemies[i].Y, unchecked((int)0xff0000ff));
                    gc.drawLine((int)enemies[i].FX - 1, (int)enemies[i].FY, 
                               (int)enemies[i].X, (int)enemies[i].Y, unchecked((int)0xff0000bb));
                    gc.drawLine((int)enemies[i].FX + 1, (int)enemies[i].FY, 
                               (int)enemies[i].X, (int)enemies[i].Y, unchecked((int)0xff0000aa));
                    gc.fillCircle((int)enemies[i].X, (int)enemies[i].Y, enemies[i].R + 1, unchecked((int)0xffffff00));
                    gc.drawCircle((int)enemies[i].X, (int)enemies[i].Y, enemies[i].R, unchecked((int)0xffff0000));
                }
            }

            // Draw 2D overlay elements (if any)
            base.Paint(gc);
        }
    }
}
