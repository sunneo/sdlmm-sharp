namespace SDLMMSharp.Babylon3D
{
    /// <summary>
    /// Camera for 3D scene viewing.
    /// </summary>
    public class Camera
    {
        public Vector3 Position;
        public Vector3 Target;

        public Camera()
        {
            Position = Vector3.Zero;
            Target = Vector3.Zero;
        }

        public Camera(Vector3 position, Vector3 target)
        {
            Position = position;
            Target = target;
        }
    }
}
