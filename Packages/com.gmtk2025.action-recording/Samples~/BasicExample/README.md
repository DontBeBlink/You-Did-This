# Basic Action Recording Example

This sample demonstrates how to set up the Action Recording System in your Unity project.

## Setup Instructions

1. **Import the Sample**: Import this sample from the Package Manager
2. **Open the Scene**: Load `BasicExampleScene` from the Scenes folder
3. **Play the Scene**: Press Play to see the system in action

## What's Included

### Scripts
- `SimpleCharacterController.cs`: Example implementation of ICharacterController
- `BasicInputHandler.cs`: Simple input handling for the example
- `ExampleSetup.cs`: Shows how to configure the recording system

### Prefabs
- `ExamplePlayer.prefab`: Player with SimpleCharacterController and adapters
- `ExampleClone.prefab`: Clone prefab set up for action replay

### Scene
- `BasicExampleScene.unity`: Complete example scene with platform setup

## How It Works

1. **Player Movement**: Use WASD or arrow keys to move the player
2. **Manual Loop**: Press F to start/end recording loops manually
3. **Automatic Loop**: The system automatically creates loops every 10 seconds
4. **Clone Behavior**: Clones replay your exact movements and actions

## Key Components

### On the Player GameObject:
- `SimpleCharacterController`: Basic 2D movement controller
- `CharacterController2DAdapter`: Adapter for the recording system
- `ActionRecorder`: Records player actions
- `BasicInputHandler`: Handles input for movement and actions

### On the Manager GameObject:
- `ActionRecordingManager`: Main system coordinator
- Audio source for clone creation sounds

### Clone Prefab:
- `ActionReplayClone`: Replays recorded actions
- `ActionReplayEffects`: Visual and audio effects
- Same character controller setup as player (without input)

## Customization

### Movement Settings
Adjust movement in `SimpleCharacterController`:
```csharp
public float moveSpeed = 5f;
public float jumpForce = 10f;
```

### Recording Settings
Configure in `ActionRecordingManager`:
```csharp
public float loopDuration = 10f;  // Loop length in seconds
public int maxClones = 5;         // Maximum number of clones
```

### Visual Effects
Enable/disable effects in `ActionReplayClone`:
```csharp
public bool enableGhostTrail = true;
public bool enableParticleEffects = true;
public bool enableGlowEffect = true;
```

## Integration Tips

1. **Existing Projects**: Use the adapter components to integrate with existing character controllers
2. **Custom Controllers**: Implement ICharacterController interface directly
3. **Visual Effects**: Customize the ActionReplayEffects component for your game's style
4. **Events**: Subscribe to system events for integration with other game systems

## Troubleshooting

### Common Issues
- **No recording**: Make sure ActionRecorder has a valid ICharacterController reference
- **Clone not moving**: Verify clone prefab has ActionReplayClone component
- **No visual effects**: Check that effects are enabled and materials are assigned

### Debug Information
Enable debug logging in ActionRecordingManager to see detailed system information.

## Next Steps

- Explore the API documentation in the main README
- Implement custom character controllers using the ICharacterController interface  
- Create custom visual effects by extending ActionReplayEffects
- Integrate with your game's UI and progression systems