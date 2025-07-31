# Technical Architecture - You Did This

This document provides a detailed technical overview of the systems and architecture powering the clone-based puzzle mechanics in **You Did This**.

## 🏗️ System Overview

### Architecture Principles
- **Event-Driven Design**: Systems communicate through events to maintain loose coupling
- **Component-Based**: Unity's component system enables modular, reusable functionality
- **Physics-Perfect Replay**: Clone system maintains exact physics state for deterministic behavior
- **Scalable Clone Management**: Designed to handle multiple simultaneous clones efficiently

### Core System Dependencies
```
PlayerController ──► ActionRecorder ──► CloneManager ──► Clone
       │                    │               │            │
       ▼                    ▼               ▼            ▼
CharacterController2D ◄─────────────────► Goal ◄────► AudioManager
       │                                   │
       ▼                                   ▼
InteractSystem ──────────────────────► GameManager
```

## 🤖 Clone System Architecture

### Core Components

#### CloneManager.cs
**Responsibility**: Central controller for clone lifecycle and coordination

```csharp
public class CloneManager : MonoBehaviour
{
    // Configuration
    [SerializeField] private float loopDuration = 15f;
    [SerializeField] private int maxClones = 10;
    
    // State Management
    private List<Clone> allClones;
    private PlayerController activePlayer;
    private ActionRecorder actionRecorder;
    
    // Events
    public System.Action<Clone> OnCloneCreated;
    public System.Action<Clone> OnCloneStuck;
    public static event System.Action OnLoopStarted;
    public static event System.Action OnLoopEnded;
}
```

**Key Features**:
- **Automatic Loop Management**: Creates clones every `loopDuration` seconds
- **Manual Override**: L key for immediate clone creation
- **Clone Limit Management**: Removes oldest clones when exceeding `maxClones`
- **Event Broadcasting**: Notifies other systems of clone lifecycle events

#### ActionRecorder.cs
**Responsibility**: Physics-accurate recording of player actions for clone replay

```csharp
[System.Serializable]
public struct PlayerAction
{
    public float timestamp;        // Time offset from recording start
    public Vector3 position;       // World position
    public Vector2 speed;          // Physics velocity
    public Vector2 externalForce;  // Applied forces (wind, etc.)
    public float movement;         // Input axis (-1 to 1)
    public bool isJumping;         // Jump input this frame
    public bool isDashing;         // Dash input this frame
    public bool isInteracting;     // Interact input this frame
    public bool isAttacking;       // Attack input this frame
    public bool jumpHeld;          // Jump button held state
    public Vector2 dashDirection;  // Direction for dash
}
```

**Recording Process**:
1. **FixedUpdate Integration**: Records at 50Hz aligned with physics updates
2. **Input Capture**: Tracks both discrete actions and continuous states
3. **Physics State**: Captures position, velocity, and external forces
4. **Frame-Perfect Timing**: Ensures deterministic replay timing

#### Clone.cs
**Responsibility**: Individual clone behavior and action replay

```csharp
public class Clone : MonoBehaviour
{
    // Replay State
    private List<PlayerAction> actionsToReplay;
    private int currentActionIndex;
    private float replayStartTime;
    private bool isReplaying;
    private bool isStuck;
    
    // Physics Integration
    private CharacterController2D character;
    private InteractSystem interact;
}
```

**Replay Process**:
1. **Action Sequencing**: Plays actions based on timestamp alignment
2. **Physics Application**: Applies recorded movement and forces
3. **State Transitions**: Handles jump hold duration and dash timing
4. **Loop Behavior**: Restarts from beginning when reaching end of recording

### Data Flow

#### Recording Phase
```
PlayerController Input ──► ActionRecorder ──► PlayerAction List
                    │                             │
                    ▼                             ▼
           CharacterController2D ────► Physics State Recording
```

#### Clone Creation Phase
```
ActionRecorder.GetRecordedActions() ──► Clone.InitializeClone()
                                    │
CloneManager ──────────────────────► Clone GameObject Creation
                                    │
                                    ▼
                             Event Broadcasting (OnCloneCreated)
```

#### Replay Phase
```
Clone.UpdateReplay() ──► Action Execution ──► CharacterController2D
                    │                      │
                    ▼                      ▼
            Time-based Sequencing    Physics Application
```

## 🎮 Player System Architecture

### PlayerController.cs
**Responsibility**: Input handling and character control coordination

```csharp
public class PlayerController : MonoBehaviour
{
    // Dependencies
    private CharacterController2D character;
    private ActionRecorder actionRecorder;
    private InteractSystem interact;
    private CheckpointSystem checkpoint;
    
    // Input System Integration
    private InputMaster controls;
    private Vector2 axis;
    private bool jumpHeld;
}
```

**Input Processing Flow**:
1. **Unity Input System**: Modern input handling with device flexibility
2. **Action Recording**: Forwards input state to ActionRecorder
3. **Character Control**: Applies input to CharacterController2D
4. **System Coordination**: Manages interaction between all player systems

### CharacterController2D.cs
**Responsibility**: Physics-based character movement and collision

**Key Features**:
- **Variable Jump Height**: Jump duration affects jump height
- **Dash Mechanics**: Directional dashing with cooldown
- **Physics Integration**: Works with Unity's Rigidbody2D system
- **State Tracking**: Exposes state for recording (JustJumped, JustDashed, etc.)

### InteractSystem.cs
**Responsibility**: Object interaction and pickup mechanics

**Integration Points**:
- **Action Recording**: Interaction state recorded for clone replay
- **Physics Objects**: Works with PickUpObject components
- **Range Detection**: Configurable interaction distance
- **Clone Compatibility**: Clones can replay interactions exactly

## 🎯 Goal and Puzzle System

### Goal.cs
**Responsibility**: Puzzle completion detection and clone state management

```csharp
public class Goal : MonoBehaviour
{
    [SerializeField] private bool requiresSpecificClone = false;
    [SerializeField] private int requiredCloneIndex = -1;
    [SerializeField] private bool isPlayerGoal = false;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Clone or player detection
        // Validation logic
        // State change (stuck) application
    }
}
```

**Goal Types**:
- **General Goals**: Any clone can complete
- **Specific Clone Goals**: Only designated clone index can complete
- **Player Goals**: Only the main player can complete

**State Management**:
- **Clone Sticking**: Permanently stops clone movement and replay
- **Visual Feedback**: Color changes to indicate completion
- **Event Broadcasting**: Notifies other systems of completion

## 🔊 Audio System

### AudioManager.cs
**Responsibility**: Centralized audio playback and management

**Integration**:
- **Clone Events**: Plays sounds for clone creation
- **Goal Completion**: Audio feedback for puzzle completion
- **Singleton Pattern**: Global access for all systems

## 🎛️ Debug and Development Tools

### Debug Information System
- **F1 Toggle**: Runtime debug information display
- **Clone Visualization**: Shows clone count, states, and timers
- **Performance Monitoring**: Tracks clone system performance
- **Visual Gizmos**: Editor visualization for goals and ranges

### Development Workflow
1. **Scene Setup**: CloneManager auto-configures in any scene
2. **Rapid Testing**: Manual clone creation with L key
3. **Visual Debugging**: Gizmos and debug UI for system state
4. **Performance Profiling**: Unity Profiler integration for optimization

## 🔧 Performance Considerations

### Optimization Strategies

#### Clone Management
- **Object Pooling**: Reuse clone GameObjects when possible
- **Cleanup Systems**: Remove null references and destroyed clones
- **Update Frequency**: Optimized update patterns for inactive clones

#### Recording Optimization
- **Fixed Recording Rate**: 50Hz recording prevents data explosion
- **Memory Management**: Circular buffers for long recording sessions
- **Compression**: Minimal data structure for PlayerAction

#### Physics Performance
- **Selective Updates**: Only active clones participate in physics
- **State Caching**: Avoid redundant physics calculations
- **LOD System**: Distant clones can have reduced update frequency

### Memory Management
```csharp
// Automatic cleanup of clone list
private void CleanupCloneList()
{
    allClones.RemoveAll(clone => clone == null);
}

// Memory-efficient action recording
private const float MAX_RECORDING_TIME = 30f;
private const float RECORDING_INTERVAL = 0.02f; // 50Hz
```

## 🔄 Event System

### Core Events
```csharp
// Clone Lifecycle
public System.Action<Clone> OnCloneCreated;
public System.Action<Clone> OnCloneStuck;

// Loop Management  
public static event System.Action OnLoopStarted;
public static event System.Action OnLoopEnded;

// Game State
public System.Action OnNewLoopStarted;
```

### Event-Driven Coordination
- **Loose Coupling**: Systems communicate without direct references
- **State Synchronization**: Events keep UI and game state aligned
- **Extensibility**: New systems can easily hook into existing events

## 🧩 Extension Points

### Adding New Mechanics

#### Recording New Actions
1. **Extend PlayerAction struct** with new fields
2. **Update ActionRecorder** to capture new input/state
3. **Modify Clone replay logic** to execute new actions
4. **Test with multiple clones** to ensure consistency

#### Creating New Puzzle Elements
1. **Implement trigger detection** for clone interaction
2. **Add visual/audio feedback** for state changes
3. **Integrate with Goal system** if needed for completion
4. **Consider multi-clone scenarios** in design

#### Performance Extensions
1. **Implement object pooling** for frequently created objects
2. **Add LOD systems** for complex visual effects
3. **Optimize physics queries** with proper layer usage
4. **Profile and optimize** bottlenecks with Unity Profiler

### Future Architecture Considerations
- **Networking Support**: Clone synchronization across network
- **Save System**: Serializing complex game state with clones
- **Level Editor**: Runtime level creation and validation
- **Analytics Integration**: Tracking player behavior and puzzle solutions

---

This architecture documentation provides the foundation for understanding and extending the clone-based puzzle mechanics. For specific implementation details, refer to the inline code documentation in each component.