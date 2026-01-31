namespace SDLMMSharp.Babylon3D
{
    /// <summary>
    /// Triangle face defined by vertex indices.
    /// </summary>
    public struct Face
    {
        public int A;
        public int B;
        public int C;

        public Face(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }
    }

    /// <summary>
    /// Vertex with position, normal, and texture coordinates.
    /// </summary>
    public struct Vertex
    {
        public Vector3 Normal;
        public Vector3 Coordinates;
        public Vector3 WorldCoordinates;
        public Vector3 TextureCoordinates;

        public Vertex(Vector3 coordinates)
        {
            Coordinates = coordinates;
            Normal = Vector3.Up;
            WorldCoordinates = Vector3.Zero;
            TextureCoordinates = Vector3.Zero;
        }

        public Vertex(Vector3 coordinates, Vector3 normal, Vector3 texCoords)
        {
            Coordinates = coordinates;
            Normal = normal;
            TextureCoordinates = texCoords;
            WorldCoordinates = Vector3.Zero;
        }
    }
}
