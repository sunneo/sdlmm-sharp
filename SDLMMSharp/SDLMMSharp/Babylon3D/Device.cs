using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SDLMMSharp.Babylon3D
{
    /// <summary>
    /// Software 3D rendering device with back buffer and depth buffer.
    /// Ported from babylon3D.c - provides software rasterization.
    /// </summary>
    public class Device : IDisposable
    {
        private const float PERSPECTIVE_EPSILON = 0.0001f;

        public int WorkingWidth { get; private set; }
        public int WorkingHeight { get; private set; }
        public int[] BackBuffer { get; private set; }
        private int[] depthBuffer;
        private bool disposed = false;

        public Device(int width, int height)
        {
            WorkingWidth = width;
            WorkingHeight = height;
            BackBuffer = new int[width * height];
            depthBuffer = new int[width * height];
        }

        /// <summary>
        /// Clear both back buffer and depth buffer.
        /// Uses optimized parallel processing with chunk-based operations.
        /// </summary>
        public void Clear()
        {
            int e = WorkingHeight * WorkingWidth;
            int chunkSize = 1024; // Process in chunks for better cache utilization
            int numChunks = (e + chunkSize - 1) / chunkSize;

            Parallel.For(0, numChunks, chunk =>
            {
                int start = chunk * chunkSize;
                int end = Math.Min(start + chunkSize, e);
                for (int i = start; i < end; i++)
                {
                    depthBuffer[i] = int.MaxValue;
                    BackBuffer[i] = 0;
                }
            });
        }

        /// <summary>
        /// Clear with a specific color.
        /// Uses optimized parallel processing with chunk-based operations.
        /// </summary>
        public void Clear(int color)
        {
            int e = WorkingHeight * WorkingWidth;
            int chunkSize = 1024;
            int numChunks = (e + chunkSize - 1) / chunkSize;

            Parallel.For(0, numChunks, chunk =>
            {
                int start = chunk * chunkSize;
                int end = Math.Min(start + chunkSize, e);
                for (int i = start; i < end; i++)
                {
                    depthBuffer[i] = int.MaxValue;
                    BackBuffer[i] = color;
                }
            });
        }

        /// <summary>
        /// Put a pixel with depth testing.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PutPixel(int x, int y, int z, int color)
        {
            if (x < 0 || y < 0 || x >= WorkingWidth || y >= WorkingHeight)
                return;

            int idx = y * WorkingWidth + x;
            if (depthBuffer[idx] < z)
                return;

            depthBuffer[idx] = z;
            BackBuffer[idx] = color;
        }

        /// <summary>
        /// Draw a point at Vector3 position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawPoint(Vector3 point, int color)
        {
            PutPixel((int)point.X, (int)point.Y, (int)point.Z, color);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Clamp(float value, float min = 0f, float max = 1f)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        /// <summary>
        /// Project a 3D vertex to 2D screen coordinates.
        /// </summary>
        public Vertex Project(Vertex vertex, Matrix transMat, Matrix world)
        {
            Vector3 point2d = vertex.Coordinates.TransformCoordinates(transMat);
            float x = point2d.X * WorkingWidth + WorkingWidth / 2.0f;
            float y = -point2d.Y * WorkingHeight + WorkingHeight / 2.0f;

            return new Vertex
            {
                Coordinates = new Vector3(x, y, point2d.Z),
                Normal = vertex.Normal.TransformCoordinates(world),
                WorldCoordinates = vertex.Coordinates.TransformCoordinates(world),
                TextureCoordinates = vertex.TextureCoordinates
            };
        }

        /// <summary>
        /// Compute the dot product of normal and light direction for lighting.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ComputeNDotL(Vector3 vertex, Vector3 normal, Vector3 lightPosition)
        {
            Vector3 lightDirection = (lightPosition - vertex).Normalized();
            Vector3 normalNormalized = normal.Normalized();
            return Math.Max(0, Vector3.Dot(normalNormalized, lightDirection));
        }

        /// <summary>
        /// Create a color from ARGB components.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Color4(int r, int g, int b, int a)
        {
            return (a << 24) | (r << 16) | (g << 8) | b;
        }

        /// <summary>
        /// Apply lighting factor to an existing color.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Color4Ref(int refColor, float r, float g, float b, float a)
        {
            int origR = (refColor >> 16) & 0xff;
            int origG = (refColor >> 8) & 0xff;
            int origB = refColor & 0xff;
            int origA = (refColor >> 24) & 0xff;

            return Color4(
                (int)(origR * r),
                (int)(origG * g),
                (int)(origB * b),
                (int)(origA * a)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalculateInverseZ(float z)
        {
            return z != 0.0f ? 1.0f / z : 0.0f;
        }

        private struct DrawData
        {
            public float CurrentY;
            public float NdotLA, NdotLB, NdotLC, NdotLD;
            public float UA, UB, UC, UD;
            public float VA, VB, VC, VD;
            public float ZA, ZB, ZC, ZD;
        }

        private void ProcessScanLine(DrawData data, Vertex va, Vertex vb, Vertex vc, Vertex vd, float color, Texture texture)
        {
            float gradient1 = va.Coordinates.Y != vb.Coordinates.Y ?
                (data.CurrentY - va.Coordinates.Y) / (vb.Coordinates.Y - va.Coordinates.Y) : 1;
            float gradient2 = vc.Coordinates.Y != vd.Coordinates.Y ?
                (data.CurrentY - vc.Coordinates.Y) / (vd.Coordinates.Y - vc.Coordinates.Y) : 1;

            int sx = (int)Interpolate(va.Coordinates.X, vb.Coordinates.X, gradient1);
            int ex = (int)Interpolate(vc.Coordinates.X, vd.Coordinates.X, gradient2);

            float z1 = Interpolate(va.Coordinates.Z, vb.Coordinates.Z, gradient1);
            float z2 = Interpolate(vc.Coordinates.Z, vd.Coordinates.Z, gradient2);

            float snl = Interpolate(data.NdotLA, data.NdotLB, gradient1);
            float enl = Interpolate(data.NdotLC, data.NdotLD, gradient2);

            float sz1 = Interpolate(data.ZA, data.ZB, gradient1);
            float sz2 = Interpolate(data.ZC, data.ZD, gradient2);
            float su = Interpolate(data.UA, data.UB, gradient1);
            float eu = Interpolate(data.UC, data.UD, gradient2);
            float sv = Interpolate(data.VA, data.VB, gradient1);
            float ev = Interpolate(data.VC, data.VD, gradient2);
            float currentY = data.CurrentY;

            // Process each pixel in the scanline
            for (int x = sx; x < ex; x++)
            {
                float gradient = ex > sx ? (float)(x - sx) / (float)(ex - sx) : 0.0f;

                float z = Interpolate(z1, z2, gradient);
                float ndotl = Interpolate(snl, enl, gradient) * color;

                // Perspective-correct texture coordinate interpolation
                float sz = Interpolate(sz1, sz2, gradient);
                float u = Interpolate(su, eu, gradient);
                float v = Interpolate(sv, ev, gradient);

                if (sz > PERSPECTIVE_EPSILON)
                {
                    u /= sz;
                    v /= sz;
                }

                int textureColor = texture != null ? texture.Map(u, v) : 0xffffff;

                // Apply lighting with ambient term
                float lightingFactor = 0.2f + 0.8f * ndotl;
                int finalColor = Color4Ref(textureColor, lightingFactor, lightingFactor, lightingFactor, 1);

                DrawPoint(new Vector3(x, currentY, z), finalColor);
            }
        }

        /// <summary>
        /// Draw a filled triangle with texture mapping and lighting.
        /// </summary>
        public void DrawTriangle(Vertex v1, Vertex v2, Vertex v3, float color, Texture texture, Vector3 lightPos)
        {
            // Sort vertices by Y coordinate
            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                Vertex temp = v2; v2 = v1; v1 = temp;
            }
            if (v2.Coordinates.Y > v3.Coordinates.Y)
            {
                Vertex temp = v2; v2 = v3; v3 = temp;
            }
            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                Vertex temp = v2; v2 = v1; v1 = temp;
            }

            float nl1 = ComputeNDotL(v1.WorldCoordinates, v1.Normal, lightPos);
            float nl2 = ComputeNDotL(v2.WorldCoordinates, v2.Normal, lightPos);
            float nl3 = ComputeNDotL(v3.WorldCoordinates, v3.Normal, lightPos);

            DrawData data = new DrawData();

            float dP1P2 = v2.Coordinates.Y - v1.Coordinates.Y > 0 ?
                (v2.Coordinates.X - v1.Coordinates.X) / (v2.Coordinates.Y - v1.Coordinates.Y) : 0;
            float dP1P3 = v3.Coordinates.Y - v1.Coordinates.Y > 0 ?
                (v3.Coordinates.X - v1.Coordinates.X) / (v3.Coordinates.Y - v1.Coordinates.Y) : 0;

            // Calculate 1/z for perspective-correct texture mapping
            float z1_inv = CalculateInverseZ(v1.Coordinates.Z);
            float z2_inv = CalculateInverseZ(v2.Coordinates.Z);
            float z3_inv = CalculateInverseZ(v3.Coordinates.Z);

            if (dP1P2 > dP1P3)
            {
                for (int y = (int)v1.Coordinates.Y; y <= (int)v3.Coordinates.Y; y++)
                {
                    data.CurrentY = y;

                    if (y < v2.Coordinates.Y)
                    {
                        data.NdotLA = nl1; data.NdotLB = nl3;
                        data.NdotLC = nl1; data.NdotLD = nl2;

                        data.UA = v1.TextureCoordinates.X * z1_inv;
                        data.UB = v3.TextureCoordinates.X * z3_inv;
                        data.UC = v1.TextureCoordinates.X * z1_inv;
                        data.UD = v2.TextureCoordinates.X * z2_inv;

                        data.VA = v1.TextureCoordinates.Y * z1_inv;
                        data.VB = v3.TextureCoordinates.Y * z3_inv;
                        data.VC = v1.TextureCoordinates.Y * z1_inv;
                        data.VD = v2.TextureCoordinates.Y * z2_inv;

                        data.ZA = z1_inv; data.ZB = z3_inv;
                        data.ZC = z1_inv; data.ZD = z2_inv;

                        ProcessScanLine(data, v1, v3, v1, v2, color, texture);
                    }
                    else
                    {
                        data.NdotLA = nl1; data.NdotLB = nl3;
                        data.NdotLC = nl2; data.NdotLD = nl3;

                        data.UA = v1.TextureCoordinates.X * z1_inv;
                        data.UB = v3.TextureCoordinates.X * z3_inv;
                        data.UC = v2.TextureCoordinates.X * z2_inv;
                        data.UD = v3.TextureCoordinates.X * z3_inv;

                        data.VA = v1.TextureCoordinates.Y * z1_inv;
                        data.VB = v3.TextureCoordinates.Y * z3_inv;
                        data.VC = v2.TextureCoordinates.Y * z2_inv;
                        data.VD = v3.TextureCoordinates.Y * z3_inv;

                        data.ZA = z1_inv; data.ZB = z3_inv;
                        data.ZC = z2_inv; data.ZD = z3_inv;

                        ProcessScanLine(data, v1, v3, v2, v3, color, texture);
                    }
                }
            }
            else
            {
                for (int y = (int)v1.Coordinates.Y; y <= (int)v3.Coordinates.Y; y++)
                {
                    data.CurrentY = y;

                    if (y < v2.Coordinates.Y)
                    {
                        data.NdotLA = nl1; data.NdotLB = nl2;
                        data.NdotLC = nl1; data.NdotLD = nl3;

                        data.UA = v1.TextureCoordinates.X * z1_inv;
                        data.UB = v2.TextureCoordinates.X * z2_inv;
                        data.UC = v1.TextureCoordinates.X * z1_inv;
                        data.UD = v3.TextureCoordinates.X * z3_inv;

                        data.VA = v1.TextureCoordinates.Y * z1_inv;
                        data.VB = v2.TextureCoordinates.Y * z2_inv;
                        data.VC = v1.TextureCoordinates.Y * z1_inv;
                        data.VD = v3.TextureCoordinates.Y * z3_inv;

                        data.ZA = z1_inv; data.ZB = z2_inv;
                        data.ZC = z1_inv; data.ZD = z3_inv;

                        ProcessScanLine(data, v1, v2, v1, v3, color, texture);
                    }
                    else
                    {
                        data.NdotLA = nl2; data.NdotLB = nl3;
                        data.NdotLC = nl1; data.NdotLD = nl3;

                        data.UA = v2.TextureCoordinates.X * z2_inv;
                        data.UB = v3.TextureCoordinates.X * z3_inv;
                        data.UC = v1.TextureCoordinates.X * z1_inv;
                        data.UD = v3.TextureCoordinates.X * z3_inv;

                        data.VA = v2.TextureCoordinates.Y * z2_inv;
                        data.VB = v3.TextureCoordinates.Y * z3_inv;
                        data.VC = v1.TextureCoordinates.Y * z1_inv;
                        data.VD = v3.TextureCoordinates.Y * z3_inv;

                        data.ZA = z2_inv; data.ZB = z3_inv;
                        data.ZC = z1_inv; data.ZD = z3_inv;

                        ProcessScanLine(data, v2, v3, v1, v3, color, texture);
                    }
                }
            }
        }

        /// <summary>
        /// Render all meshes to the back buffer.
        /// </summary>
        public void Render(Camera camera, Mesh[] meshes, Vector3? lightPosition = null)
        {
            Vector3 up = Vector3.Up;
            Matrix viewMatrix = Matrix.LookAtLH(camera.Position, camera.Target, up);
            Matrix projectionMatrix = Matrix.PerspectiveFovLH(0.78f, (float)WorkingWidth / WorkingHeight, 0.01f, 1.0f);

            Vector3 lightPos = lightPosition ?? new Vector3(0, 10, 10);

            foreach (Mesh mesh in meshes)
            {
                if (mesh == null) continue;

                Matrix rotationYPR = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z);
                Matrix translation = Matrix.Translation(mesh.Position.X, mesh.Position.Y, mesh.Position.Z);
                Matrix worldMatrix = rotationYPR * translation;
                Matrix res1 = worldMatrix * viewMatrix;
                Matrix transformMatrix = res1 * projectionMatrix;

                for (int i = 0; i < mesh.FaceCount; i++)
                {
                    Face face = mesh.Faces[i];
                    Vertex vertexA = mesh.Vertices[face.A];
                    Vertex vertexB = mesh.Vertices[face.B];
                    Vertex vertexC = mesh.Vertices[face.C];

                    Vertex pixelA = Project(vertexA, transformMatrix, worldMatrix);
                    Vertex pixelB = Project(vertexB, transformMatrix, worldMatrix);
                    Vertex pixelC = Project(vertexC, transformMatrix, worldMatrix);

                    // Backface culling
                    float edge1X = pixelB.Coordinates.X - pixelA.Coordinates.X;
                    float edge1Y = pixelB.Coordinates.Y - pixelA.Coordinates.Y;
                    float edge2X = pixelC.Coordinates.X - pixelA.Coordinates.X;
                    float edge2Y = pixelC.Coordinates.Y - pixelA.Coordinates.Y;
                    float crossZ = edge1X * edge2Y - edge1Y * edge2X;

                    if (crossZ < 0)
                        continue;

                    DrawTriangle(pixelA, pixelB, pixelC, 1.0f, mesh.Texture, lightPos);
                }
            }
        }

        /// <summary>
        /// Render single mesh to the back buffer.
        /// </summary>
        public void Render(Camera camera, Mesh mesh, Vector3? lightPosition = null)
        {
            Render(camera, new[] { mesh }, lightPosition);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    BackBuffer = null;
                    depthBuffer = null;
                }
                disposed = true;
            }
        }
    }
}
