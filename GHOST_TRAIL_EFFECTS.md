# Ghost Trail Effects Documentation

This document describes the ghost trail and visual effects system implemented for clones in the GMTK2025 project.

## Overview

The ghost trail system enhances the visual feedback of the clone system by adding:
- **Ghost Trails**: Visual trails that follow clone movement
- **Particle Effects**: Spawn, despawn, and state change particle systems
- **Sound Effects**: Audio feedback for clone lifecycle events
- **Glow Effects**: Ambient lighting around active clones

## Components

### 1. GhostTrail Component

**Purpose**: Creates a visual trail showing past action positions that dissipate over time, providing a "trail of past actions" effect.

**Key Features**:
- Creates afterimage-style ghost sprites at past positions
- Ghosts fade out over time creating a dissipating trail effect  
- Different visual styles for active vs stuck clones
- Performance optimized for multiple clones
- Shows actual past positions rather than continuous trail lines

**Configuration**:
```csharp
[Header("Ghost Trail Settings")]
ghostLifetime = 2.0f;              // How long each ghost persists before fading
spawnInterval = 0.2f;              // Time between ghost spawns
maxGhosts = 10;                    // Maximum number of ghosts at once
activeGhostColor = Color.cyan;     // Color for active clone ghosts
stuckGhostColor = Color.green;     // Color for stuck clone ghosts
```

### 2. CloneParticleEffects Component

**Purpose**: Manages particle effects for clone lifecycle events and ambient effects.

**Key Features**:
- Spawn particle burst when clone is created
- Despawn particle effect when clone is destroyed
- State change particles when clone becomes stuck
- Continuous ambient particles during clone replay
- Configurable particle counts and colors

**Configuration**:
```csharp
[Header("Spawn Effects")]
spawnParticleCount = 30;
spawnColor = Color.cyan;

[Header("Ambient Effects")]
enableAmbientParticles = true;
ambientEmissionRate = 5f;
```

### 3. CloneSoundEffects Component

**Purpose**: Provides audio feedback for clone actions and state changes.

**Key Features**:
- Spawn/despawn sound effects
- Stuck state audio feedback
- Optional movement sounds
- Spatial 3D audio with distance attenuation
- Pitch variation for natural sound

**Configuration**:
```csharp
[Header("Audio Settings")]
spawnVolume = 0.3f;
enableMovementSounds = false;  // Optional movement audio
pitchVariation = 0.2f;         // Sound variety
```

## Integration with Clone System

### Automatic Setup

The visual effects are automatically integrated into the existing clone system:

1. **Clone.cs Enhancement**: The main Clone component now includes setup for all visual effects
2. **State-Based Effects**: Effects automatically change based on clone state (active, stuck, inactive)
3. **Performance Optimization**: Effects are only enabled when needed

### Configuration Options

In the Clone component inspector, you can toggle:
- `enableGhostTrail`: Enable/disable trail effects
- `enableParticleEffects`: Enable/disable particle systems
- `enableGlowEffect`: Enable/disable glow lighting
- `enableSoundEffects`: Enable/disable audio feedback

## Visual States

### Active Clone
- **Trail**: Cyan afterimage ghosts that appear at past positions and fade out over time
- **Particles**: Ambient particles emanating from clone
- **Glow**: Soft cyan glow around clone
- **Sound**: Spawn sound when created

### Stuck Clone
- **Trail**: Green afterimage ghosts (reduced opacity, existing ghosts continue to fade)
- **Particles**: Green burst effect when becoming stuck
- **Glow**: Green glow indicating permanent state
- **Sound**: Special stuck sound effect

### Inactive Clone
- **Trail**: Disabled
- **Particles**: No ambient effects
- **Glow**: Dimmed or disabled
- **Sound**: No movement sounds

## Technical Implementation

### Afterimage Ghost System
```csharp
// Ghost trail creates fading afterimages at past positions
SpawnGhost(); // Creates a new ghost sprite at current position
ghost.UpdateFade(); // Fades ghost over time using alpha transparency
```

### Particle System Creation
```csharp
// Particles are created procedurally with optimized settings
particles.main.maxParticles = particleCount;
particles.main.simulationSpace = ParticleSystemSimulationSpace.World;
```

### 2D Lighting Support
```csharp
// Automatically detects and uses URP 2D lighting when available
var light2DType = System.Type.GetType("UnityEngine.Rendering.Universal.Light2D...");
```

## Performance Considerations

### Optimizations Implemented
- **Particle Limits**: Maximum particle counts prevent performance issues
- **Distance Culling**: Audio effects use spatial falloff
- **State-Based Activation**: Effects only run when clone is active
- **Memory Management**: Automatic cleanup when clones are destroyed

### Recommended Settings
- **Max Clones**: Keep under 10 active clones for optimal performance
- **Ghost Lifetime**: 2.0s provides good visual feedback without clutter
- **Spawn Interval**: 0.2s balances visual quality with performance
- **Max Ghosts**: 10 ghosts per clone prevents memory issues

## Customization

### Adding Custom Materials
Place custom trail materials in `Assets/Materials/` and they will be automatically detected.

### Audio Configuration
Add custom audio clips to the CloneSoundEffects component:
- `spawnSound`: Played when clone is created
- `despawnSound`: Played when clone is destroyed
- `stuckSound`: Played when clone becomes stuck at goal

### Color Schemes
Modify color settings in the Clone component and GhostTrail component to match your game's visual style:
```csharp
cloneColor = Color.cyan;              // Base clone color
activeGhostColor = Color.cyan;        // Ghost color for active clones
stuckGhostColor = Color.green;        // Ghost color for stuck clones
```

## Troubleshooting

### Common Issues

**Trails not appearing**:
- Ensure `enableGhostTrail` is true in Clone component
- Check that clone is in active replay state and moving
- Verify clone has moved at least `minMoveDistance` since last ghost
- Ensure `spawnInterval` time has passed since last ghost spawn

**Particles not playing**:
- Confirm `enableParticleEffects` is enabled
- Check particle system max particles setting
- Ensure clone state changes are triggering effects

**Audio not playing**:
- Verify `enableSoundEffects` is true
- Check AudioSource component is present
- Confirm audio clips are assigned

**Performance issues**:
- Reduce `maxGhosts` count in GhostTrail component
- Increase `spawnInterval` to spawn ghosts less frequently
- Reduce `ghostLifetime` to make ghosts dissipate faster
- Increase `minMoveDistance` to spawn fewer ghosts
- Limit maximum number of active clones

### Debug Information

The system provides debug logging for:
- Clone creation and destruction
- State changes (active → stuck)
- Effect initialization status

Enable debug logs by uncommenting debug statements in the Clone.cs file.

## Integration Notes

### Existing System Compatibility
- **CloneManager**: No changes required, automatically works with enhanced clones
- **Goals**: Existing goal system automatically triggers stuck state effects
- **ActionRecorder**: Recording system unchanged, effects are purely visual

### Future Enhancements
The system is designed to be extensible:
- Additional particle effects can be added to CloneParticleEffects
- New trail styles can be implemented in GhostTrail
- Audio feedback can be expanded with more sound types
- Visual effects can be data-driven through ScriptableObjects