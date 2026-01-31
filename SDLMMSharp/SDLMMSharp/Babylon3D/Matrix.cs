using System;

namespace SDLMMSharp.Babylon3D
{
    /// <summary>
    /// 4x4 transformation matrix for 3D graphics.
    /// Column-major order: M[0-3] = column 0, M[4-7] = column 1, etc.
    /// </summary>
    public struct Matrix
    {
        public float[] M;

        public Matrix(bool init)
        {
            M = new float[16];
        }

        public bool IsIdentity
        {
            get
            {
                if (M[0] != 1.0f || M[5] != 1.0f || M[10] != 1.0f || M[15] != 1.0f) return false;
                if (M[1] != 0.0f || M[2] != 0.0f || M[3] != 0.0f ||
                    M[4] != 0.0f || M[6] != 0.0f || M[7] != 0.0f ||
                    M[8] != 0.0f || M[9] != 0.0f || M[11] != 0.0f ||
                    M[12] != 0.0f || M[13] != 0.0f || M[14] != 0.0f) return false;
                return true;
            }
        }

        public float Determinant
        {
            get
            {
                float temp1 = (M[10] * M[15]) - (M[11] * M[14]);
                float temp2 = (M[9] * M[15]) - (M[11] * M[13]);
                float temp3 = (M[9] * M[14]) - (M[10] * M[13]);
                float temp4 = (M[8] * M[15]) - (M[11] * M[12]);
                float temp5 = (M[8] * M[14]) - (M[10] * M[12]);
                float temp6 = (M[8] * M[13]) - (M[9] * M[12]);

                return ((((M[0] * (((M[5] * temp1) - (M[6] * temp2)) + (M[7] * temp3))) -
                         (M[1] * (((M[4] * temp1) - (M[6] * temp4)) + (M[7] * temp5)))) +
                         (M[2] * (((M[4] * temp2) - (M[5] * temp4)) + (M[7] * temp6)))) -
                         (M[3] * (((M[4] * temp3) - (M[5] * temp5)) + (M[6] * temp6))));
            }
        }

        public void Invert()
        {
            float l1 = M[0], l2 = M[1], l3 = M[2], l4 = M[3];
            float l5 = M[4], l6 = M[5], l7 = M[6], l8 = M[7];
            float l9 = M[8], l10 = M[9], l11 = M[10], l12 = M[11];
            float l13 = M[12], l14 = M[13], l15 = M[14], l16 = M[15];

            float l17 = (l11 * l16) - (l12 * l15);
            float l18 = (l10 * l16) - (l12 * l14);
            float l19 = (l10 * l15) - (l11 * l14);
            float l20 = (l9 * l16) - (l12 * l13);
            float l21 = (l9 * l15) - (l11 * l13);
            float l22 = (l9 * l14) - (l10 * l13);
            float l23 = ((l6 * l17) - (l7 * l18)) + (l8 * l19);
            float l24 = -(((l5 * l17) - (l7 * l20)) + (l8 * l21));
            float l25 = ((l5 * l18) - (l6 * l20)) + (l8 * l22);
            float l26 = -(((l5 * l19) - (l6 * l21)) + (l7 * l22));
            float l27 = 1.0f / ((((l1 * l23) + (l2 * l24)) + (l3 * l25)) + (l4 * l26));
            float l28 = (l7 * l16) - (l8 * l15);
            float l29 = (l6 * l16) - (l8 * l14);
            float l30 = (l6 * l15) - (l7 * l14);
            float l31 = (l5 * l16) - (l8 * l13);
            float l32 = (l5 * l15) - (l7 * l13);
            float l33 = (l5 * l14) - (l6 * l13);
            float l34 = (l7 * l12) - (l8 * l11);
            float l35 = (l6 * l12) - (l8 * l10);
            float l36 = (l6 * l11) - (l7 * l10);
            float l37 = (l5 * l12) - (l8 * l9);
            float l38 = (l5 * l11) - (l7 * l9);
            float l39 = (l5 * l10) - (l6 * l9);

            M[0] = l23 * l27;
            M[4] = l24 * l27;
            M[8] = l25 * l27;
            M[12] = l26 * l27;
            M[1] = -(((l2 * l17) - (l3 * l18)) + (l4 * l19)) * l27;
            M[5] = (((l1 * l17) - (l3 * l20)) + (l4 * l21)) * l27;
            M[9] = -(((l1 * l18) - (l2 * l20)) + (l4 * l22)) * l27;
            M[13] = (((l1 * l19) - (l2 * l21)) + (l3 * l22)) * l27;
            M[2] = (((l2 * l28) - (l3 * l29)) + (l4 * l30)) * l27;
            M[6] = -(((l1 * l28) - (l3 * l31)) + (l4 * l32)) * l27;
            M[10] = (((l1 * l29) - (l2 * l31)) + (l4 * l33)) * l27;
            M[14] = -(((l1 * l30) - (l2 * l32)) + (l3 * l33)) * l27;
            M[3] = -(((l2 * l34) - (l3 * l35)) + (l4 * l36)) * l27;
            M[7] = (((l1 * l34) - (l3 * l37)) + (l4 * l38)) * l27;
            M[11] = -(((l1 * l35) - (l2 * l37)) + (l4 * l39)) * l27;
            M[15] = (((l1 * l36) - (l2 * l38)) + (l3 * l39)) * l27;
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            Matrix result = new Matrix(true);
            result.M[0] = a.M[0] * b.M[0] + a.M[1] * b.M[4] + a.M[2] * b.M[8] + a.M[3] * b.M[12];
            result.M[1] = a.M[0] * b.M[1] + a.M[1] * b.M[5] + a.M[2] * b.M[9] + a.M[3] * b.M[13];
            result.M[2] = a.M[0] * b.M[2] + a.M[1] * b.M[6] + a.M[2] * b.M[10] + a.M[3] * b.M[14];
            result.M[3] = a.M[0] * b.M[3] + a.M[1] * b.M[7] + a.M[2] * b.M[11] + a.M[3] * b.M[15];
            result.M[4] = a.M[4] * b.M[0] + a.M[5] * b.M[4] + a.M[6] * b.M[8] + a.M[7] * b.M[12];
            result.M[5] = a.M[4] * b.M[1] + a.M[5] * b.M[5] + a.M[6] * b.M[9] + a.M[7] * b.M[13];
            result.M[6] = a.M[4] * b.M[2] + a.M[5] * b.M[6] + a.M[6] * b.M[10] + a.M[7] * b.M[14];
            result.M[7] = a.M[4] * b.M[3] + a.M[5] * b.M[7] + a.M[6] * b.M[11] + a.M[7] * b.M[15];
            result.M[8] = a.M[8] * b.M[0] + a.M[9] * b.M[4] + a.M[10] * b.M[8] + a.M[11] * b.M[12];
            result.M[9] = a.M[8] * b.M[1] + a.M[9] * b.M[5] + a.M[10] * b.M[9] + a.M[11] * b.M[13];
            result.M[10] = a.M[8] * b.M[2] + a.M[9] * b.M[6] + a.M[10] * b.M[10] + a.M[11] * b.M[14];
            result.M[11] = a.M[8] * b.M[3] + a.M[9] * b.M[7] + a.M[10] * b.M[11] + a.M[11] * b.M[15];
            result.M[12] = a.M[12] * b.M[0] + a.M[13] * b.M[4] + a.M[14] * b.M[8] + a.M[15] * b.M[12];
            result.M[13] = a.M[12] * b.M[1] + a.M[13] * b.M[5] + a.M[14] * b.M[9] + a.M[15] * b.M[13];
            result.M[14] = a.M[12] * b.M[2] + a.M[13] * b.M[6] + a.M[14] * b.M[10] + a.M[15] * b.M[14];
            result.M[15] = a.M[12] * b.M[3] + a.M[13] * b.M[7] + a.M[14] * b.M[11] + a.M[15] * b.M[15];
            return result;
        }

        public static Matrix Identity()
        {
            Matrix result = new Matrix(true);
            result.M[0] = result.M[5] = result.M[10] = result.M[15] = 1.0f;
            result.M[1] = result.M[2] = result.M[3] = 0.0f;
            result.M[4] = result.M[6] = result.M[7] = 0.0f;
            result.M[8] = result.M[9] = result.M[11] = 0.0f;
            result.M[12] = result.M[13] = result.M[14] = 0.0f;
            return result;
        }

        public static Matrix Zero()
        {
            Matrix result = new Matrix(true);
            for (int i = 0; i < 16; i++) result.M[i] = 0;
            return result;
        }

        public static Matrix RotationX(float angle)
        {
            Matrix result = new Matrix(true);
            float s = (float)Math.Sin(angle);
            float c = (float)Math.Cos(angle);

            result.M[0] = 1.0f; result.M[1] = 0.0f; result.M[2] = 0.0f; result.M[3] = 0.0f;
            result.M[4] = 0.0f; result.M[5] = c; result.M[6] = s; result.M[7] = 0.0f;
            result.M[8] = 0.0f; result.M[9] = -s; result.M[10] = c; result.M[11] = 0.0f;
            result.M[12] = 0.0f; result.M[13] = 0.0f; result.M[14] = 0.0f; result.M[15] = 1.0f;
            return result;
        }

        public static Matrix RotationY(float angle)
        {
            Matrix result = new Matrix(true);
            float s = (float)Math.Sin(angle);
            float c = (float)Math.Cos(angle);

            result.M[0] = c; result.M[1] = 0.0f; result.M[2] = -s; result.M[3] = 0.0f;
            result.M[4] = 0.0f; result.M[5] = 1.0f; result.M[6] = 0.0f; result.M[7] = 0.0f;
            result.M[8] = s; result.M[9] = 0.0f; result.M[10] = c; result.M[11] = 0.0f;
            result.M[12] = 0.0f; result.M[13] = 0.0f; result.M[14] = 0.0f; result.M[15] = 1.0f;
            return result;
        }

        public static Matrix RotationZ(float angle)
        {
            Matrix result = new Matrix(true);
            float s = (float)Math.Sin(angle);
            float c = (float)Math.Cos(angle);

            result.M[0] = c; result.M[1] = s; result.M[2] = 0.0f; result.M[3] = 0.0f;
            result.M[4] = -s; result.M[5] = c; result.M[6] = 0.0f; result.M[7] = 0.0f;
            result.M[8] = 0.0f; result.M[9] = 0.0f; result.M[10] = 1.0f; result.M[11] = 0.0f;
            result.M[12] = 0.0f; result.M[13] = 0.0f; result.M[14] = 0.0f; result.M[15] = 1.0f;
            return result;
        }

        public static Matrix RotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            return RotationZ(roll) * RotationX(pitch) * RotationY(yaw);
        }

        public static Matrix Scaling(float x, float y, float z)
        {
            Matrix result = new Matrix(true);
            result.M[0] = x; result.M[1] = 0.0f; result.M[2] = 0.0f; result.M[3] = 0.0f;
            result.M[4] = 0.0f; result.M[5] = y; result.M[6] = 0.0f; result.M[7] = 0.0f;
            result.M[8] = 0.0f; result.M[9] = 0.0f; result.M[10] = z; result.M[11] = 0.0f;
            result.M[12] = 0.0f; result.M[13] = 0.0f; result.M[14] = 0.0f; result.M[15] = 1.0f;
            return result;
        }

        public static Matrix Translation(float x, float y, float z)
        {
            Matrix result = new Matrix(true);
            result.M[0] = 1.0f; result.M[1] = 0.0f; result.M[2] = 0.0f; result.M[3] = 0.0f;
            result.M[4] = 0.0f; result.M[5] = 1.0f; result.M[6] = 0.0f; result.M[7] = 0.0f;
            result.M[8] = 0.0f; result.M[9] = 0.0f; result.M[10] = 1.0f; result.M[11] = 0.0f;
            result.M[12] = x; result.M[13] = y; result.M[14] = z; result.M[15] = 1.0f;
            return result;
        }

        public static Matrix LookAtLH(Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 zaxis = (target - eye).Normalized();
            Vector3 xaxis = Vector3.Cross(up, zaxis).Normalized();
            Vector3 yaxis = Vector3.Cross(zaxis, xaxis);

            float ex = -Vector3.Dot(xaxis, eye);
            float ey = -Vector3.Dot(yaxis, eye);
            float ez = -Vector3.Dot(zaxis, eye);

            Matrix result = new Matrix(true);
            result.M[0] = xaxis.X; result.M[1] = yaxis.X; result.M[2] = zaxis.X; result.M[3] = 0.0f;
            result.M[4] = xaxis.Y; result.M[5] = yaxis.Y; result.M[6] = zaxis.Y; result.M[7] = 0.0f;
            result.M[8] = xaxis.Z; result.M[9] = yaxis.Z; result.M[10] = zaxis.Z; result.M[11] = 0.0f;
            result.M[12] = ex; result.M[13] = ey; result.M[14] = ez; result.M[15] = 1.0f;
            return result;
        }

        public static Matrix PerspectiveFovLH(float fov, float aspect, float znear, float zfar)
        {
            Matrix result = new Matrix(true);
            float tan = 1.0f / (float)Math.Tan(fov * 0.5f);

            result.M[0] = tan / aspect;
            result.M[1] = result.M[2] = result.M[3] = 0.0f;
            result.M[4] = 0.0f;
            result.M[5] = tan;
            result.M[6] = result.M[7] = 0.0f;
            result.M[8] = result.M[9] = 0.0f;
            result.M[10] = -zfar / (znear - zfar);
            result.M[11] = 1.0f;
            result.M[12] = result.M[13] = 0.0f;
            result.M[14] = (znear * zfar) / (znear - zfar);
            result.M[15] = 0.0f;
            return result;
        }

        public static Matrix Transpose(Matrix matrix)
        {
            Matrix result = new Matrix(true);
            result.M[0] = matrix.M[0]; result.M[1] = matrix.M[4]; result.M[2] = matrix.M[8]; result.M[3] = matrix.M[12];
            result.M[4] = matrix.M[1]; result.M[5] = matrix.M[5]; result.M[6] = matrix.M[9]; result.M[7] = matrix.M[13];
            result.M[8] = matrix.M[2]; result.M[9] = matrix.M[6]; result.M[10] = matrix.M[10]; result.M[11] = matrix.M[14];
            result.M[12] = matrix.M[3]; result.M[13] = matrix.M[7]; result.M[14] = matrix.M[11]; result.M[15] = matrix.M[15];
            return result;
        }
    }
}
