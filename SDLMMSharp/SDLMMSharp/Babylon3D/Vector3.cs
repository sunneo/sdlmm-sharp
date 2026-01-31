using System;

namespace SDLMMSharp.Babylon3D
{
    /// <summary>
    /// 3D vector for world space coordinates, normals, and texture coordinates.
    /// </summary>
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 Zero => new Vector3(0, 0, 0);
        public static Vector3 Up => new Vector3(0, 1, 0);
        public static Vector3 Right => new Vector3(1, 0, 0);
        public static Vector3 Forward => new Vector3(0, 0, 1);

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        public float LengthSquared => X * X + Y * Y + Z * Z;

        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.X, -a.Y, -a.Z);
        public static Vector3 operator *(Vector3 a, float s) => new Vector3(a.X * s, a.Y * s, a.Z * s);
        public static Vector3 operator *(float s, Vector3 a) => new Vector3(a.X * s, a.Y * s, a.Z * s);
        public static Vector3 operator /(Vector3 a, float s) => new Vector3(a.X / s, a.Y / s, a.Z / s);
        public static Vector3 operator *(Vector3 a, Vector3 b) => new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static Vector3 operator /(Vector3 a, Vector3 b) => new Vector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);

        public void Normalize()
        {
            float len = Length;
            if (len == 0) return;
            float num = 1.0f / len;
            X *= num;
            Y *= num;
            Z *= num;
        }

        public Vector3 Normalized()
        {
            Vector3 result = this;
            result.Normalize();
            return result;
        }

        public static float Dot(Vector3 left, Vector3 right)
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }

        public static Vector3 Cross(Vector3 left, Vector3 right)
        {
            return new Vector3(
                left.Y * right.Z - left.Z * right.Y,
                left.Z * right.X - left.X * right.Z,
                left.X * right.Y - left.Y * right.X
            );
        }

        public static float Distance(Vector3 a, Vector3 b) =>
            (float)Math.Sqrt(DistanceSquared(a, b));

        public static float DistanceSquared(Vector3 a, Vector3 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            float dz = a.Z - b.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>
        /// Transform the vector by a 4x4 transformation matrix (with homogeneous division).
        /// </summary>
        public Vector3 TransformCoordinates(Matrix transformation)
        {
            float x = (X * transformation.M[0]) + (Y * transformation.M[4]) + (Z * transformation.M[8]) + transformation.M[12];
            float y = (X * transformation.M[1]) + (Y * transformation.M[5]) + (Z * transformation.M[9]) + transformation.M[13];
            float z = (X * transformation.M[2]) + (Y * transformation.M[6]) + (Z * transformation.M[10]) + transformation.M[14];
            float w = (X * transformation.M[3]) + (Y * transformation.M[7]) + (Z * transformation.M[11]) + transformation.M[15];
            return new Vector3(x / w, y / w, z / w);
        }

        /// <summary>
        /// Transform normal by a 4x4 transformation matrix (no translation).
        /// </summary>
        public Vector3 TransformNormal(Matrix transformation)
        {
            float x = (X * transformation.M[0]) + (Y * transformation.M[4]) + (Z * transformation.M[8]);
            float y = (X * transformation.M[1]) + (Y * transformation.M[5]) + (Z * transformation.M[9]);
            float z = (X * transformation.M[2]) + (Y * transformation.M[6]) + (Z * transformation.M[10]);
            return new Vector3(x, y, z);
        }

        public static Vector3 FromArray(float[] f, int offset)
        {
            return new Vector3(f[offset], f[offset + 1], f[offset + 2]);
        }

        public override string ToString() => $"{{X: {X}, Y: {Y}, Z: {Z}}}";
    }
}
