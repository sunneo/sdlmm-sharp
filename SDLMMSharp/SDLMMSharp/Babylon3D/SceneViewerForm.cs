using SDLMMSharp.Engine;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SDLMMSharp.Babylon3D
{
    /// <summary>
    /// Scene viewer form for loading and viewing JSON-based 3D scenes.
    /// This is a port of the scene_viewer.c example.
    /// </summary>
    public class SceneViewerForm : Form
    {
        private IRenderer renderer;
        private BaseEngine engine;
        private Babylon3DScene scene;
        private Timer animationTimer;
        private bool isRunning = false;
        private Label fpsLabel;
        private Label infoLabel;
        private int frameCount = 0;
        private DateTime lastFpsUpdate = DateTime.Now;
        private bool autoRotateCamera = true;
        private float cameraAngle = 0f;

        public SceneViewerForm()
        {
            InitializeComponent();
        }

        public SceneViewerForm(string sceneFile) : this()
        {
            if (!string.IsNullOrEmpty(sceneFile))
            {
                LoadScene(sceneFile);
            }
        }

        private void InitializeComponent()
        {
            this.Text = "SDLMM+Babylon3D JSON Scene Viewer";
            this.Size = new Size(816, 639);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.AllowDrop = true;

            // Create renderer control
            renderer = new SharpDXControl(800, 600);
            if (renderer is Control ctrl)
            {
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl);
            }

            // Create info label
            infoLabel = new Label
            {
                Text = "Drag & drop a scene JSON file or press O to open",
                Location = new Point(10, 30),
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Regular)
            };

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
                rendererCtrl.Controls.Add(infoLabel);
            }

            // Create engine
            engine = new BaseEngine(renderer);

            // Create default scene with a cube
            CreateDefaultScene();

            // Setup animation timer (~60 FPS)
            animationTimer = new Timer();
            animationTimer.Interval = 16;
            animationTimer.Tick += AnimationTimer_Tick;

            this.Load += SceneViewerForm_Load;
            this.FormClosing += SceneViewerForm_FormClosing;
            this.KeyDown += SceneViewerForm_KeyDown;
            this.DragEnter += SceneViewerForm_DragEnter;
            this.DragDrop += SceneViewerForm_DragDrop;
        }

        private void CreateDefaultScene()
        {
            scene = new Babylon3DScene(engine, 800, 600);
            scene.AutoRotate = true;
            scene.RotationSpeed = 0.01f;

            // Create a simple cube
            Mesh cube = Mesh.CreateCube("DefaultCube");
            cube.Position = new Vector3(0, 0, 10);
            scene.AddMesh(cube);

            // Setup camera
            scene.Camera.Position = new Vector3(0, 0, -10);
            scene.Camera.Target = Vector3.Zero;

            engine.Renderer.SetRootScene(scene);
            scene.Start();

            infoLabel.Text = "Default cube scene - Press O to open a JSON scene";
        }

        public void LoadScene(string filename)
        {
            if (!File.Exists(filename))
            {
                MessageBox.Show($"File not found: {filename}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Check scene format
                SceneFormat format = SceneLoader.GetSceneFormat(filename);

                if (format == SceneFormat.Scene3D)
                {
                    // Clean up old scene
                    if (scene != null)
                    {
                        scene.End();
                        scene = null;
                    }

                    // Load new scene
                    scene = SceneLoader.LoadSceneFromFile(engine, filename);
                    if (scene != null)
                    {
                        scene.AutoRotate = false;
                        autoRotateCamera = true;
                        cameraAngle = 0;

                        engine.Renderer.SetRootScene(scene);
                        scene.Start();

                        string name = Path.GetFileName(filename);
                        this.Text = $"Scene Viewer - {name}";
                        infoLabel.Text = $"Loaded: {name} ({scene.Meshes.Count} models)";
                    }
                    else
                    {
                        MessageBox.Show("Failed to load 3D scene", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Only 3D scenes (format=1) are supported", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading scene: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SceneViewerForm_Load(object sender, EventArgs e)
        {
            isRunning = true;
            animationTimer.Start();
            engine.Start();
        }

        private void SceneViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
            animationTimer.Stop();
            engine.End();
            scene?.End();
            renderer?.Dispose();
        }

        private void SceneViewerForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.O:
                    OpenSceneFile();
                    break;
                case Keys.R:
                    autoRotateCamera = !autoRotateCamera;
                    break;
                case Keys.Space:
                    if (scene != null)
                        scene.AutoRotate = !scene.AutoRotate;
                    break;
                case Keys.Escape:
                    this.Close();
                    break;
            }
        }

        private void OpenSceneFile()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "JSON Scene Files (*.json)|*.json|All Files (*.*)|*.*";
                dialog.Title = "Open Scene File";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadScene(dialog.FileName);
                }
            }
        }

        private void SceneViewerForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void SceneViewerForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                LoadScene(files[0]);
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (!isRunning || scene == null) return;

            // Update scene (rotates meshes if AutoRotate is enabled)
            scene.Update(0.016f);

            // Rotate camera around the scene
            if (autoRotateCamera)
            {
                cameraAngle += 0.01f;
                float radius = 10.0f;
                scene.Camera.Position = new Vector3(
                    radius * (float)Math.Cos(cameraAngle),
                    scene.Camera.Position.Y,
                    radius * (float)Math.Sin(cameraAngle)
                );
            }

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
        /// Run the scene viewer as a standalone application.
        /// </summary>
        public static void Run(string[] args = null)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SceneViewerForm form;
            if (args != null && args.Length > 0 && File.Exists(args[0]))
            {
                form = new SceneViewerForm(args[0]);
            }
            else
            {
                form = new SceneViewerForm();
            }

            Application.Run(form);
        }
    }
}
