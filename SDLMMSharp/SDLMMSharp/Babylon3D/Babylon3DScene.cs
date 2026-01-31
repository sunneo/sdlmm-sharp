using SDLMMSharp.Engine;
using SDLMMSharp.Engine.Controls;
using SDLMMSharp.Engine.Scenes;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SDLMMSharp.Babylon3D
{
    /// <summary>
    /// 3D Scene that integrates Babylon3D rendering with the SDLMMSharp engine.
    /// </summary>
    public class Babylon3DScene : BaseScene
    {
        protected Device device;
        protected Camera camera;
        protected List<Mesh> meshes = new List<Mesh>();
        protected Vector3 lightPosition = new Vector3(0, 10, 10);
        protected bool autoRotate = false;
        protected float rotationSpeed = 0.01f;

        public Device Device => device;
        public Camera Camera => camera;
        public List<Mesh> Meshes => meshes;
        public Vector3 LightPosition
        {
            get => lightPosition;
            set => lightPosition = value;
        }
        public bool AutoRotate
        {
            get => autoRotate;
            set => autoRotate = value;
        }
        public float RotationSpeed
        {
            get => rotationSpeed;
            set => rotationSpeed = value;
        }

        public Babylon3DScene(BaseEngine owner, int width, int height) : base(owner)
        {
            device = new Device(width, height);
            camera = new Camera(new Vector3(0, 0, -10), Vector3.Zero);
        }

        public override void InitializeComponent()
        {
            base.InitializeComponent();
        }

        /// <summary>
        /// Add a mesh to the scene.
        /// </summary>
        public void AddMesh(Mesh mesh)
        {
            if (mesh != null)
                meshes.Add(mesh);
        }

        /// <summary>
        /// Remove a mesh from the scene.
        /// </summary>
        public void RemoveMesh(Mesh mesh)
        {
            meshes.Remove(mesh);
        }

        /// <summary>
        /// Clear all meshes from the scene.
        /// </summary>
        public void ClearMeshes()
        {
            foreach (var mesh in meshes)
            {
                mesh?.Dispose();
            }
            meshes.Clear();
        }

        /// <summary>
        /// Update scene (call each frame for animations).
        /// </summary>
        public virtual void Update(float deltaTime = 0.016f)
        {
            if (autoRotate)
            {
                foreach (var mesh in meshes)
                {
                    if (mesh != null)
                    {
                        mesh.Rotation.X += rotationSpeed;
                        mesh.Rotation.Y += rotationSpeed;
                    }
                }
            }
        }

        public override void Paint(IRenderer gc)
        {
            // Clear 3D device
            device.Clear(mBgColor);

            // Render all meshes
            if (meshes.Count > 0)
            {
                device.Render(camera, meshes.ToArray(), lightPosition);
            }

            // Copy device backbuffer to renderer
            gc.drawPixels(device.BackBuffer, 0, 0, device.WorkingWidth, device.WorkingHeight);

            // Draw 2D overlay elements (draggables, tools)
            for (LinkedListNode<IDraggableTarget> obj = this.draggables.First; obj != null; obj = obj.Next)
            {
                if (obj.Value == null) continue;
                if (!(obj.Value is DraggableTarget)) continue;
                DraggableTarget draggable = (DraggableTarget)obj.Value;
                draggable.Paint(gc);
            }
            for (LinkedListNode<IDraggableTarget> obj = this.overlayTools.First; obj != null; obj = obj.Next)
            {
                if (obj.Value == null) continue;
                if (!(obj.Value is DraggableTarget)) continue;
                DraggableTarget draggable = (DraggableTarget)obj.Value;
                draggable.Paint(gc);
            }
        }

        public override void End()
        {
            base.End();
            ClearMeshes();
            device?.Dispose();
        }
    }
}
