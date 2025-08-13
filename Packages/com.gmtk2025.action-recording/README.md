# Action Recording System

A physics-perfect action recording and replay system for Unity, originally developed for You Did This. This system enables time-loop mechanics, ghost players, AI behavior recording, and precise action replay with frame-perfect timing.

## 🎯 Features

- **Physics-Perfect Recording**: Captures both input states and physics states for accurate replay
- **Interface-Based Design**: Works with any character controller through abstraction interfaces
- **Visual Effects**: Ghost trails, particle effects, glow effects, and sound effects for clones
- **Loop Management**: Automatic and manual loop creation with customizable duration
- **Event System**: Comprehensive events for integration with other systems
- **Zero Dependencies**: Only requires Unity Input System, no other external packages

## 🚀 Quick Start

### 1. Install the Package

Add the package to your Unity project through the Package Manager or by adding this line to your `manifest.json`:

```json
{
  "dependencies": {
    "com.gmtk2025.action-recording": "1.0.0"
  }
}
```

### 2. Set Up Your Character

Your character controller needs to implement the `ICharacterController` interface, or you can use the provided adapter:

```csharp
// Option 1: Use the adapter (easiest for existing projects)
var adapter = gameObject.AddComponent<CharacterController2DAdapter>();

// Option 2: Implement the interface directly in your character controller
public class MyCharacterController : MonoBehaviour, ICharacterController
{
    public Vector3 Position => transform.position;
    public Vector2 Velocity => rigidbody2D.velocity;
    public bool IsGrounded => /* your grounded check */;
    // ... implement other interface members
}
```

### 3. Set Up the Recording Manager

Add the `ActionRecordingManager` to your scene:

```csharp
// Create a GameObject and add the ActionRecordingManager component
var manager = gameObject.AddComponent<ActionRecordingManager>();

// Set up your clone prefab (should have ActionReplayClone component)
manager.clonePrefab = yourClonePrefab;

// Configure the system
manager.SetPlayerController(yourCharacterController);
```

### 4. Create a Clone Prefab

1. Duplicate your player GameObject
2. Add the `ActionReplayClone` component
3. Add the character controller adapter if needed
4. Remove or disable player input components
5. Save as a prefab

## 📋 Core Components

### ActionRecorder
Records player actions with frame-perfect timing. Captures input states, position, velocity, and external forces.

### ActionReplayClone
Individual clone that replays recorded actions. Handles physics-perfect reproduction of player behavior.

### ActionRecordingManager
Main system manager that coordinates recording, clone creation, and loop management.

### Interface Adapters
- `CharacterController2DAdapter`: Adapts the original GMTK2025 CharacterController2D
- `InteractSystemAdapter`: Adapts the original GMTK2025 InteractSystem

## 🎨 Visual Effects

The system includes a comprehensive effects manager (`ActionReplayEffects`) that handles:

- **Ghost Trails**: Transparent sprite trails that follow clones
- **Particle Effects**: Spawn and ambient particle systems
- **Glow Effects**: Light-based glow around active clones
- **Sound Effects**: Audio feedback for clone actions

## ⚙️ Configuration

### Loop Settings
- `loopDuration`: Duration of each recording loop (0 = infinite)
- `maxClones`: Maximum number of simultaneous clones
- `autoStartFirstLoop`: Whether to start recording automatically
- `enableManualLooping`: Allow manual loop triggering with key press

### Visual Effects
- `enableGhostTrail`: Show transparent trails behind clones
- `enableParticleEffects`: Particle effects for spawn and ambient
- `enableGlowEffect`: Glow lighting around clones
- `enableSoundEffects`: Audio feedback for clone actions

## 🔌 Events and Integration

The system provides comprehensive events for integration:

```csharp
// Subscribe to recording manager events
ActionRecordingManager.Instance.OnCloneCreated += (clone) => {
    Debug.Log($"Clone {clone.CloneIndex} created!");
};

ActionRecordingManager.Instance.OnNewLoopStarted += () => {
    Debug.Log("New recording loop started");
};

// Subscribe to individual clone events
clone.OnReplayComplete += (clone) => {
    Debug.Log($"Clone {clone.CloneIndex} completed replay loop");
};
```

## 💡 Usage Examples

### Basic Time Loop Mechanic
```csharp
// Start a 15-second recording loop
ActionRecordingManager.Instance.LoopDuration = 15f;
ActionRecordingManager.Instance.StartNewLoop();

// After 15 seconds, a clone will be created and start replaying
```

### Manual Clone Creation
```csharp
// Start recording
ActionRecordingManager.Instance.StartNewLoop();

// Later, manually end the loop to create a clone
ActionRecordingManager.Instance.EndCurrentLoop();
```

### Ghost Player for AI Training
```csharp
// Record player behavior
ActionRecordingManager.Instance.StartNewLoop();
// ... player performs actions
var actions = actionRecorder.GetRecordedActions();

// Use actions to train AI or create ghost opponents
```

## 🔧 Advanced Customization

### Custom Character Controllers

Implement the `ICharacterController` interface in your custom character controller:

```csharp
public class CustomCharacterController : MonoBehaviour, ICharacterController
{
    public Vector3 Position => transform.position;
    public Vector2 Velocity => myVelocity;
    public bool IsGrounded => myGroundCheck;
    public bool IsOnWall => myWallCheck;
    public bool FacingRight => myFacingDirection;
    public SpriteRenderer SpriteRenderer => mySpriteRenderer;

    public void ApplyMovement(float movement) { /* your movement logic */ }
    public void Jump() { /* your jump logic */ }
    public void EndJump() { /* your variable height jump logic */ }
    public void Dash(Vector2 direction) { /* your dash logic */ }
    public void SetPosition(Vector3 position) { transform.position = position; }
    public void ApplyExternalForce(Vector2 force) { /* your force application */ }
}
```

### Custom Interaction Systems

Implement the `IInteractionSystem` interface:

```csharp
public class CustomInteractionSystem : MonoBehaviour, IInteractionSystem
{
    public void TriggerInteraction() { /* your interaction logic */ }
    public void TriggerAttack() { /* your attack logic */ }
    public bool HasInteractableObjects => myInteractableCount > 0;
}
```

## 📖 API Reference

### ActionRecorder
- `StartRecording()`: Begin recording player actions
- `StopRecording()`: Stop recording and finalize action sequence
- `GetRecordedActions()`: Get copy of all recorded actions
- `IsRecording`: Whether currently recording
- `ActionCount`: Number of recorded actions

### ActionReplayClone
- `InitializeClone(actions, index)`: Set up clone with action sequence
- `StartReplay()`: Begin replaying actions
- `StopReplay()`: Stop replay and mark as stuck
- `GetReplayProgress()`: Get progress as percentage (0-1)
- `IsReplaying`: Whether currently replaying
- `CloneIndex`: Unique identifier

### ActionRecordingManager
- `StartNewLoop()`: Start new recording loop
- `EndCurrentLoop()`: End current loop and create clone
- `TriggerManualLoop()`: Manual loop toggle
- `ClearAllClones()`: Remove all clones
- `GetAllClones()`: Get list of active clones
- `IsRecording`: Whether currently recording
- `CloneCount`: Number of active clones

## 🎮 Input Integration

The system works with Unity's Input System. Set up your input handling to call the appropriate methods:

```csharp
// In your input handler
if (loopInput.WasPressedThisFrame())
{
    ActionRecordingManager.Instance.TriggerManualLoop();
}
```

## 🚧 Performance Considerations

- Recording frequency: 50 FPS (0.02s intervals) balances accuracy and performance
- Memory usage: ~50 bytes per action frame, manageable for typical loop durations
- Clone limit: Configurable maximum prevents performance issues
- Automatic cleanup: Trail objects and effects are properly disposed

## 🤝 Contributing

This package was extracted from the GMTK2025 project. For issues, improvements, or contributions, please visit the [main repository](https://github.com/DontBeBlink/GMTK2025).

## 📄 License

MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Credits

Originally developed for GMTK2025 "You Did This" by the GMTK2025 team. Extracted and refactored for community use with interface abstractions and improved reusability.