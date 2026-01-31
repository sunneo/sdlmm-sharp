# Babylon3D for SDLMMSharp

This is a C# port of the Babylon3D software 3D rendering engine from the ref-sdlmm C library.

## Features

- **Software 3D Rendering**: Pure C# implementation with no external dependencies for 3D rendering
- **Perspective-correct texture mapping**: High-quality texture rendering with proper perspective correction
- **Lighting**: Diffuse lighting with ambient term
- **Backface culling**: Automatic removal of back-facing triangles
- **Z-buffering**: Proper depth ordering of triangles
- **OBJ file loading**: Load 3D models from standard OBJ format
- **JSON scene loading**: Define scenes in JSON format

## Classes

### Core Math
- `Vector2` - 2D vector for screen coordinates
- `Vector3` - 3D vector for world coordinates
- `Matrix` - 4x4 transformation matrix

### 3D Objects
- `Camera` - View position and target
- `Vertex` - Vertex with position, normal, and texture coordinates
- `Face` - Triangle defined by vertex indices
- `Mesh` - 3D mesh with vertices, faces, position, rotation, and texture
- `Texture` - Texture image for mapping onto meshes
- `Device` - Software rendering device with back buffer and depth buffer

### Scene Management
- `Babylon3DScene` - 3D scene that integrates with SDLMMSharp engine
- `SceneLoader` - Utility for loading scenes from JSON files

### Demo Forms
- `Babylon3DCubeDemo` - Demo of a rotating textured cube
- `SceneViewerForm` - JSON scene viewer with drag-and-drop support

## Usage

### Basic Cube Demo
```csharp
// Run the cube demo
Babylon3DCubeDemo.Run();
```

### Scene Viewer
```csharp
// Run the scene viewer
SceneViewerForm.Run();

// Or with a scene file
SceneViewerForm.Run(new[] { "scene.json" });
```

### Creating a Custom Scene
```csharp
// Create engine and renderer
IRenderer renderer = new SharpDXControl(800, 600);
BaseEngine engine = new BaseEngine(renderer);

// Create 3D scene
Babylon3DScene scene = new Babylon3DScene(engine, 800, 600);
scene.Camera.Position = new Vector3(0, 0, -10);
scene.Camera.Target = Vector3.Zero;
scene.AutoRotate = true;

// Add a cube
Mesh cube = Mesh.CreateCube("MyCube");
cube.Position = new Vector3(0, 0, 10);
cube.Texture = Texture.Load("texture.png");
scene.AddMesh(cube);

// Start rendering
engine.Renderer.SetRootScene(scene);
engine.Start();
```

### JSON Scene Format
```json
{
  "format": 1,
  "width": 800,
  "height": 600,
  "backgroundColor": 0,
  "camera": {
    "position": [0, 0, -10],
    "target": [0, 0, 0]
  },
  "lights": [
    {
      "position": [5, 10, 5],
      "intensity": 1.0,
      "color": 16777215
    }
  ],
  "models": [
    {
      "primitive": "cube",
      "position": [0, 0, 10],
      "rotation": [0, 0, 0]
    }
  ]
}
```

## Performance Optimizations

- **Chunk-based parallel buffer clearing**: Uses optimized parallel processing for buffer clear operations
- **Cached Direct2D bitmap**: Reuses bitmap for pixel transfers to reduce allocation overhead
- **Perspective-correct interpolation**: Proper perspective correction for texture mapping

## Controls (Scene Viewer)

- **O**: Open scene file dialog
- **R**: Toggle camera auto-rotation
- **Space**: Toggle mesh auto-rotation
- **Escape**: Close window
- **Drag & Drop**: Drop JSON scene files onto the window to load them

## Credits

Ported from the C implementation in the ref-sdlmm submodule (babylon3D.c).
