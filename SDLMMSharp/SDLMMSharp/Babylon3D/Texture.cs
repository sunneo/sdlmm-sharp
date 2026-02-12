using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SDLMMSharp.Babylon3D
{
    /// <summary>
    /// Texture for 3D rendering with pixel data.
    /// </summary>
    public class Texture : IDisposable
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int[] InternalBuffer { get; private set; }
        private bool disposed = false;

        public Texture(int width, int height)
        {
            Width = width;
            Height = height;
            InternalBuffer = new int[width * height];
        }

        /// <summary>
        /// Load texture from file.
        /// </summary>
        public static Texture Load(string filename)
        {
            try
            {
                using (Bitmap bmp = new Bitmap(filename))
                {
                    return FromBitmap(bmp);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Create texture from System.Drawing.Bitmap.
        /// </summary>
        public static Texture FromBitmap(Bitmap bitmap)
        {
            if (bitmap == null) return null;

            int width = bitmap.Width;
            int height = bitmap.Height;
            Texture texture = new Texture(width, height);

            // Lock the bitmap for fast pixel access
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                // Copy pixels to internal buffer
                Marshal.Copy(bmpData.Scan0, texture.InternalBuffer, 0, width * height);
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }

            return texture;
        }

        /// <summary>
        /// Sample texture at UV coordinates.
        /// </summary>
        public int Map(float u, float v)
        {
            if (InternalBuffer == null || Width <= 0 || Height <= 0)
                return 0;

            // Clamp UV coordinates to [0, 1] range
            u = Math.Max(0, Math.Min(1, u));
            v = Math.Max(0, Math.Min(1, v));

            // Convert to texture space
            int x = (int)(u * (Width - 1));
            int y = (int)(v * (Height - 1));

            return InternalBuffer[x + y * Width];
        }

        /// <summary>
        /// Create a Gaussian texture for soft particle sprites.
        /// Uses Hermite interpolation for smooth falloff.
        /// </summary>
        public static Texture CreateGaussian(int size)
        {
            Texture tex = new Texture(size, size);
            float center = size / 2.0f;
            float maxDist = size / 2.0f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy) / maxDist;

                    if (dist > 1.0f) dist = 1.0f;

                    // Hermite interpolation for smooth falloff
                    float t = dist;
                    float u2 = t * t;
                    float u3 = u2 * t;
                    float B0 = 2 * u3 - 3 * u2 + 1;  // Hermite basis

                    float intensity = B0;
                    if (intensity < 0.0f) intensity = 0.0f;

                    // Store as grayscale with alpha
                    int alpha = (int)(intensity * 255.0f);
                    tex.InternalBuffer[y * size + x] = (alpha << 24) | (alpha << 16) | (alpha << 8) | alpha;
                }
            }

            return tex;
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
                    InternalBuffer = null;
                }
                disposed = true;
            }
        }
    }
}
