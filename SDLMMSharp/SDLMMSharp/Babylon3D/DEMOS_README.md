# NBody3D and MissileCmd3D Demos

This directory contains two new 3D demo applications for SDLMM-Sharp, ported from the ref-sdlmm C implementations.

## NBody3D Demo

An N-Body gravitational simulation demonstrating particle physics in 3D space.

### Features
- Simulates 2000 particles with gravitational interactions
- Real-time physics calculation using N-body algorithm
- Multiple rendering modes (pixel, circle outline, filled circle)
- Adjustable simulation speed factor
- Centralized or offset viewing modes
- FPS counter and performance monitoring

### Controls
- **0-3**: Switch between display modes
  - 0: Circle outline
  - 1: Single pixel
  - 2: Pixel + Circle outline
  - 3: Filled circle (default)
- **C**: Toggle centralized view mode
- **R**: Toggle randomized simulation factor
- **H**: Toggle help display

### Implementation Details
- Based on `ref-sdlmm/exams/nbody.c`
- Uses gravitational force calculation: F = G * m / d²
- Velocity clamping to prevent extreme speeds
- Depth-based color coding for 3D effect

## MissileCmd3D Demo

A missile defense game inspired by the classic Missile Command arcade game.

### Features
- Wave-based enemy missile attacks
- Player-controlled anti-missile defense system
- Explosion chain reactions
- Multiple buildings to protect
- Score tracking and game state management
- Auto-restart on wave completion

### Gameplay
- Enemy missiles fall from the top of the screen
- Click to launch defensive missiles at cursor position
- Create explosions to destroy incoming missiles
- Protect your buildings from being destroyed
- Chain explosions for bonus points

### Controls
- **Mouse Move**: Aim launcher
- **Mouse Click**: Launch defensive missile

### Implementation Details
- Based on `ref-sdlmm/exams/missilecmd.c`
- Collision detection using distance calculations
- Explosion radius mechanics
- Limited missile ammunition (45 per wave)
- Dynamic enemy generation
- Building health system

## Babylon3D Extensions

Both demos utilize the Babylon3D rendering system with the following enhancements:

### New Features
- **Sphere Mesh Generation**: `Mesh.CreateSphere()` method for UV sphere generation
- **Particle Rendering**: Efficient rendering for large numbers of particles
- **2D Overlay Support**: Draw 2D elements over 3D scenes

## Building and Running

1. Open `SDLMMSharp.sln` in Visual Studio
2. Build the solution (Ctrl+Shift+B)
3. Run the application
4. Click "NBody 3D Demo" or "Missile Cmd 3D" buttons on the main form

## Technical Requirements

- .NET Framework 4.5 or higher
- SharpDX 4.2.0 (for Direct2D/Direct3D rendering)
- Windows OS (for DirectX support)

## Architecture

Both demos follow a similar pattern:

1. **Form Class** (`NBody3DDemo`, `MissileCmd3DDemo`)
   - Manages window, controls, and UI
   - Handles user input
   - Updates FPS and game state displays

2. **Scene Class** (`NBody3DScene`, `MissileCmd3DScene`)
   - Extends `Babylon3DScene`
   - Implements game logic and physics
   - Handles rendering
   - Manages simulation state

3. **Engine Integration**
   - Uses `BaseEngine` for rendering loop
   - Timer-based updates at ~60 FPS
   - Integrates with SharpDX renderer

## Performance

- **NBody3D**: Handles 2000 particles at 60 FPS with optimized N-body calculations
- **MissileCmd3D**: Smooth rendering of up to 20 enemy missiles and 16 defensive missiles

## Future Enhancements

Possible improvements:
- Hardware acceleration using compute shaders for N-body simulation
- Texture and sprite loading for MissileCmd3D
- Sound effects integration
- Additional visual effects (trails, particles, etc.)
- Save/load game state
- High score tracking
- Difficulty levels
