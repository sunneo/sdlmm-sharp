using System;
using System.Collections.Generic;
using System.IO;

namespace SDLMMSharp.Babylon3D
{
    /// <summary>
    /// 3D mesh with vertices, faces, position, rotation, and texture.
    /// </summary>
    public class Mesh : IDisposable
    {
        public string Name { get; set; }
        public Vertex[] Vertices { get; set; }
        public Face[] Faces { get; set; }
        public Vector3 Rotation;
        public Vector3 Position;
        public Texture Texture { get; set; }
        private bool disposed = false;

        public int VertexCount => Vertices?.Length ?? 0;
        public int FaceCount => Faces?.Length ?? 0;

        public Mesh(string name, int vertexCount, int faceCount)
        {
            Name = name;
            Vertices = new Vertex[vertexCount];
            Faces = new Face[faceCount];
            Rotation = Vector3.Zero;
            Position = Vector3.Zero;
        }

        /// <summary>
        /// Create a simple cube mesh.
        /// </summary>
        public static Mesh CreateCube(string name = "Cube")
        {
            // A cube needs 24 vertices (4 per face) for proper texture mapping
            Mesh mesh = new Mesh(name, 24, 12);

            // Front face (Z = 1)
            mesh.Vertices[0] = new Vertex(new Vector3(-1, 1, 1));   // Top-left
            mesh.Vertices[1] = new Vertex(new Vector3(1, 1, 1));    // Top-right
            mesh.Vertices[2] = new Vertex(new Vector3(-1, -1, 1));  // Bottom-left
            mesh.Vertices[3] = new Vertex(new Vector3(1, -1, 1));   // Bottom-right

            // Back face (Z = -1)
            mesh.Vertices[4] = new Vertex(new Vector3(1, 1, -1));   // Top-left (from back)
            mesh.Vertices[5] = new Vertex(new Vector3(-1, 1, -1));  // Top-right (from back)
            mesh.Vertices[6] = new Vertex(new Vector3(1, -1, -1));  // Bottom-left (from back)
            mesh.Vertices[7] = new Vertex(new Vector3(-1, -1, -1)); // Bottom-right (from back)

            // Top face (Y = 1)
            mesh.Vertices[8] = new Vertex(new Vector3(-1, 1, -1));  // Top-left
            mesh.Vertices[9] = new Vertex(new Vector3(1, 1, -1));   // Top-right
            mesh.Vertices[10] = new Vertex(new Vector3(-1, 1, 1));  // Bottom-left
            mesh.Vertices[11] = new Vertex(new Vector3(1, 1, 1));   // Bottom-right

            // Bottom face (Y = -1)
            mesh.Vertices[12] = new Vertex(new Vector3(-1, -1, 1)); // Top-left
            mesh.Vertices[13] = new Vertex(new Vector3(1, -1, 1));  // Top-right
            mesh.Vertices[14] = new Vertex(new Vector3(-1, -1, -1));// Bottom-left
            mesh.Vertices[15] = new Vertex(new Vector3(1, -1, -1)); // Bottom-right

            // Left face (X = -1)
            mesh.Vertices[16] = new Vertex(new Vector3(-1, 1, -1)); // Top-left
            mesh.Vertices[17] = new Vertex(new Vector3(-1, 1, 1));  // Top-right
            mesh.Vertices[18] = new Vertex(new Vector3(-1, -1, -1));// Bottom-left
            mesh.Vertices[19] = new Vertex(new Vector3(-1, -1, 1)); // Bottom-right

            // Right face (X = 1)
            mesh.Vertices[20] = new Vertex(new Vector3(1, 1, 1));   // Top-left
            mesh.Vertices[21] = new Vertex(new Vector3(1, 1, -1));  // Top-right
            mesh.Vertices[22] = new Vertex(new Vector3(1, -1, 1));  // Bottom-left
            mesh.Vertices[23] = new Vertex(new Vector3(1, -1, -1)); // Bottom-right

            // Set normals for each face
            for (int i = 0; i < 4; i++) mesh.Vertices[i].Normal = new Vector3(0, 0, 1);     // Front
            for (int i = 4; i < 8; i++) mesh.Vertices[i].Normal = new Vector3(0, 0, -1);    // Back
            for (int i = 8; i < 12; i++) mesh.Vertices[i].Normal = new Vector3(0, 1, 0);    // Top
            for (int i = 12; i < 16; i++) mesh.Vertices[i].Normal = new Vector3(0, -1, 0);  // Bottom
            for (int i = 16; i < 20; i++) mesh.Vertices[i].Normal = new Vector3(-1, 0, 0);  // Left
            for (int i = 20; i < 24; i++) mesh.Vertices[i].Normal = new Vector3(1, 0, 0);   // Right

            // Set texture coordinates - each face gets the full texture (0,0) to (1,1)
            for (int i = 0; i < 6; i++)
            {
                int baseIdx = i * 4;
                mesh.Vertices[baseIdx + 0].TextureCoordinates = new Vector3(0, 0, 0); // Top-left
                mesh.Vertices[baseIdx + 1].TextureCoordinates = new Vector3(1, 0, 0); // Top-right
                mesh.Vertices[baseIdx + 2].TextureCoordinates = new Vector3(0, 1, 0); // Bottom-left
                mesh.Vertices[baseIdx + 3].TextureCoordinates = new Vector3(1, 1, 0); // Bottom-right
            }

            // Define the 12 faces (2 triangles per side)
            mesh.Faces[0] = new Face(0, 1, 2);   // Front
            mesh.Faces[1] = new Face(1, 3, 2);
            mesh.Faces[2] = new Face(4, 5, 6);   // Back
            mesh.Faces[3] = new Face(5, 7, 6);
            mesh.Faces[4] = new Face(8, 9, 10);  // Top
            mesh.Faces[5] = new Face(9, 11, 10);
            mesh.Faces[6] = new Face(12, 13, 14);// Bottom
            mesh.Faces[7] = new Face(13, 15, 14);
            mesh.Faces[8] = new Face(16, 17, 18);// Left
            mesh.Faces[9] = new Face(17, 19, 18);
            mesh.Faces[10] = new Face(20, 21, 22);// Right
            mesh.Faces[11] = new Face(21, 23, 22);

            return mesh;
        }

        /// <summary>
        /// Load mesh from OBJ file.
        /// </summary>
        public static Mesh LoadObj(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine($"Error: Cannot open OBJ file '{filename}'");
                return null;
            }

            string[] lines = File.ReadAllLines(filename);

            // First pass: count vertices, normals, texture coordinates, and faces
            int vertexCount = 0;
            int normalCount = 0;
            int texCoordCount = 0;
            int faceCount = 0;

            foreach (string line in lines)
            {
                if (line.StartsWith("v ")) vertexCount++;
                else if (line.StartsWith("vn ")) normalCount++;
                else if (line.StartsWith("vt ")) texCoordCount++;
                else if (line.StartsWith("f ")) faceCount++;
            }

            if (vertexCount == 0 || faceCount == 0)
            {
                Console.WriteLine($"Error: Invalid OBJ file '{filename}' (no vertices or faces)");
                return null;
            }

            // Allocate temporary arrays
            Vector3[] vertices = new Vector3[vertexCount];
            Vector3[] normals = normalCount > 0 ? new Vector3[normalCount] : null;
            Vector3[] texCoords = texCoordCount > 0 ? new Vector3[texCoordCount] : null;

            // Second pass: read vertex data
            int vIdx = 0, vnIdx = 0, vtIdx = 0;

            foreach (string line in lines)
            {
                if (line.StartsWith("v "))
                {
                    string[] parts = line.Substring(2).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        float x = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                        float y = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                        float z = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                        vertices[vIdx++] = new Vector3(x, y, z);
                    }
                }
                else if (line.StartsWith("vn "))
                {
                    string[] parts = line.Substring(3).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3 && normals != null)
                    {
                        float x = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                        float y = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                        float z = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                        normals[vnIdx++] = new Vector3(x, y, z);
                    }
                }
                else if (line.StartsWith("vt "))
                {
                    string[] parts = line.Substring(3).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && texCoords != null)
                    {
                        float u = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                        float v = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                        texCoords[vtIdx++] = new Vector3(u, v, 0);
                    }
                }
            }

            // Create mesh
            Mesh mesh = new Mesh(Path.GetFileNameWithoutExtension(filename), faceCount * 3, faceCount);

            // Third pass: read faces
            int fIdx = 0;
            int uniqueVertIdx = 0;

            foreach (string line in lines)
            {
                if (!line.StartsWith("f ")) continue;

                string[] parts = line.Substring(2).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;

                List<int> faceVertices = new List<int>();
                List<int> faceTexCoords = new List<int>();
                List<int> faceNormals = new List<int>();

                foreach (string part in parts)
                {
                    string[] indices = part.Split('/');
                    int v = int.Parse(indices[0]) - 1; // OBJ is 1-based
                    int vt = indices.Length > 1 && !string.IsNullOrEmpty(indices[1]) ? int.Parse(indices[1]) - 1 : -1;
                    int vn = indices.Length > 2 && !string.IsNullOrEmpty(indices[2]) ? int.Parse(indices[2]) - 1 : -1;

                    faceVertices.Add(v);
                    faceTexCoords.Add(vt);
                    faceNormals.Add(vn);
                }

                // Triangulate if necessary (only support triangles for now)
                if (faceVertices.Count >= 3)
                {
                    int v1 = faceVertices[0], v2 = faceVertices[1], v3 = faceVertices[2];
                    int vt1 = faceTexCoords[0], vt2 = faceTexCoords[1], vt3 = faceTexCoords[2];
                    int vn1 = faceNormals[0], vn2 = faceNormals[1], vn3 = faceNormals[2];

                    // Validate indices
                    if (v1 < 0 || v1 >= vertexCount || v2 < 0 || v2 >= vertexCount || v3 < 0 || v3 >= vertexCount)
                        continue;

                    // Create unique vertices for this face
                    mesh.Vertices[uniqueVertIdx].Coordinates = vertices[v1];
                    mesh.Vertices[uniqueVertIdx].Normal = normals != null && vn1 >= 0 && vn1 < normalCount ? normals[vn1] : Vector3.Up;
                    mesh.Vertices[uniqueVertIdx].TextureCoordinates = texCoords != null && vt1 >= 0 && vt1 < texCoordCount ? texCoords[vt1] : Vector3.Zero;

                    mesh.Vertices[uniqueVertIdx + 1].Coordinates = vertices[v2];
                    mesh.Vertices[uniqueVertIdx + 1].Normal = normals != null && vn2 >= 0 && vn2 < normalCount ? normals[vn2] : Vector3.Up;
                    mesh.Vertices[uniqueVertIdx + 1].TextureCoordinates = texCoords != null && vt2 >= 0 && vt2 < texCoordCount ? texCoords[vt2] : Vector3.Zero;

                    mesh.Vertices[uniqueVertIdx + 2].Coordinates = vertices[v3];
                    mesh.Vertices[uniqueVertIdx + 2].Normal = normals != null && vn3 >= 0 && vn3 < normalCount ? normals[vn3] : Vector3.Up;
                    mesh.Vertices[uniqueVertIdx + 2].TextureCoordinates = texCoords != null && vt3 >= 0 && vt3 < texCoordCount ? texCoords[vt3] : Vector3.Zero;

                    mesh.Faces[fIdx] = new Face(uniqueVertIdx, uniqueVertIdx + 1, uniqueVertIdx + 2);

                    uniqueVertIdx += 3;
                    fIdx++;
                }
            }

            Console.WriteLine($"Loaded OBJ '{filename}': {mesh.VertexCount} vertices, {mesh.FaceCount} faces");
            return mesh;
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
                    Texture?.Dispose();
                    Vertices = null;
                    Faces = null;
                }
                disposed = true;
            }
        }
    }
}
