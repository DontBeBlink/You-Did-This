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

**Purpose**: Creates a visual trail that follows clone movement using Unity's TrailRenderer.

**Key Features**:
- Configurable trail color, width, and duration
- Different visual states for active vs stuck clones
- Performance optimized for multiple clones
- Automatic state management based on clone behavior

**Configuration**:
```csharp
[Header("Trail Settings")]
trailTime = 0.5f;                 // How long trail persists
trailWidth = 0.1f;               // Width of trail
activeTrailColor = Color.cyan;    // Color for active clones
stuckTrailColor = Color.green;    // Color for stuck clones
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
- **Trail**: Cyan colored trail following movement
- **Particles**: Ambient particles emanating from clone
- **Glow**: Soft cyan glow around clone
- **Sound**: Spawn sound when created

### Stuck Clone
- **Trail**: Green colored trail (reduced opacity)
- **Particles**: Green burst effect when becoming stuck
- **Glow**: Green glow indicating permanent state
- **Sound**: Special stuck sound effect

### Inactive Clone
- **Trail**: Disabled
- **Particles**: No ambient effects
- **Glow**: Dimmed or disabled
- **Sound**: No movement sounds

## Technical Implementation

### TrailRenderer Integration
```csharp
// Ghost trail automatically configures TrailRenderer
trailRenderer.time = trailTime;
trailRenderer.startWidth = trailWidth;
trailRenderer.material = glowMaterial; // Uses existing glow materials
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
- **Trail Time**: 0.5s provides good visual feedback without overhead
- **Particle Count**: 20-30 particles per effect for good visual quality

## Customization

### Adding Custom Materials
Place custom trail materials in `Assets/Materials/` and they will be automatically detected.

### Audio Configuration
Add custom audio clips to the CloneSoundEffects component:
- `spawnSound`: Played when clone is created
- `despawnSound`: Played when clone is destroyed
- `stuckSound`: Played when clone becomes stuck at goal

### Color Schemes
Modify color settings in the Clone component to match your game's visual style:
```csharp
cloneColor = Color.cyan;        // Base clone color
activeTrailColor = Color.cyan;  // Trail color for active clones
stuckTrailColor = Color.green;  // Trail color for stuck clones
```

## Troubleshooting

### Common Issues

**Trails not appearing**:
- Ensure `enableGhostTrail` is true in Clone component
- Check that clone is in active replay state
- Verify TrailRenderer component is added

**Particles not playing**:
- Confirm `enableParticleEffects` is enabled
- Check particle system max particles setting
- Ensure clone state changes are triggering effects

**Audio not playing**:
- Verify `enableSoundEffects` is true
- Check AudioSource component is present
- Confirm audio clips are assigned

**Performance issues**:
- Reduce particle counts in CloneParticleEffects
- Disable ambient particles if not needed
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