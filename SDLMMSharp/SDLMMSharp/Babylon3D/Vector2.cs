using System;

namespace SDLMMSharp.Babylon3D
{
    /// <summary>
    /// 2D vector for screen space coordinates and texture mapping.
    /// </summary>
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 Zero => new Vector2(0, 0);

        public float Length => (float)Math.Sqrt(X * X + Y * Y);
        public float LengthSquared => X * X + Y * Y;

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator -(Vector2 a) => new Vector2(-a.X, -a.Y);
        public static Vector2 operator *(Vector2 a, float s) => new Vector2(a.X * s, a.Y * s);
        public static Vector2 operator /(Vector2 a, float s) => new Vector2(a.X / s, a.Y / s);

        public void Normalize()
        {
            float len = Length;
            if (len == 0) return;
            float num = 1.0f / len;
            X *= num;
            Y *= num;
        }

        public Vector2 Normalized()
        {
            Vector2 result = this;
            result.Normalize();
            return result;
        }

        public static Vector2 Min(Vector2 a, Vector2 b) =>
            new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));

        public static Vector2 Max(Vector2 a, Vector2 b) =>
            new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));

        public static float Distance(Vector2 a, Vector2 b) =>
            (float)Math.Sqrt(DistanceSquared(a, b));

        public static float DistanceSquared(Vector2 a, Vector2 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        public Vector2 Transform(Matrix transformation)
        {
            return new Vector2(
                X * transformation.M[0] + Y * transformation.M[4],
                X * transformation.M[1] + Y * transformation.M[5]
            );
        }

        public override string ToString() => $"{{X: {X}, Y: {Y}}}";
    }
}
