# Interaction System

A comprehensive object interaction system for Unity featuring proximity detection, visual feedback, pickup mechanics, and input device icon support. Originally developed for GMTK2025, this system enables rich character-object interactions for puzzles and gameplay.

## 🎯 Features

- **Proximity-Based Detection**: Automatic detection of interactable objects within configurable range
- **Visual Feedback System**: Context-sensitive input icons with automatic device detection (keyboard/gamepad)
- **Pickup and Carry Mechanics**: Full physics-based object pickup, carrying, and throwing system
- **Interface-Based Design**: Flexible interfaces allow easy integration with existing character controllers
- **Priority System**: Handle multiple interactables with configurable priority ordering
- **Event System**: Comprehensive events for integration with other systems
- **Adapter Components**: Seamless integration with existing CharacterController2D implementations
- **Zero Dependencies**: Pure Unity implementation with optional Input System integration for device detection

## 🚀 Quick Start

### 1. Install the Package

Add the package to your Unity project through the Package Manager or by adding this line to your `manifest.json`:

```json
{
  "dependencies": {
    "com.gmtk2025.interaction-system": "1.0.0"
  }
}
```

### 2. Set Up Your Character

Add the `InteractionController` component to your character and configure the adapters:

```csharp
// Option 1: Use the adapter for existing CharacterController2D
var adapter = gameObject.AddComponent<CharacterController2DAdapter>();

// Option 2: Implement the interfaces directly in your character controller
public class MyCharacterController : MonoBehaviour, ICharacterVelocityProvider, ICharacterFacingProvider
{
    public Vector2 GetVelocity() => rigidbody2D.velocity;
    public bool IsFacingRight() => facingRight;
}
```

### 3. Set Up Interactable Objects

#### Simple Interactable Object

```csharp
public class MySwitch : InteractableObject
{
    public override void OnInteract(IInteractionController interactor)
    {
        Debug.Log("Switch activated!");
        // Your interaction logic here
    }
}
```

#### Pickup Object

```csharp
// Add PickupableObject component to any object with Rigidbody2D and Collider2D
var pickup = myObject.AddComponent<PickupableObject>();
pickup.SetCarryPositionOffset(new Vector2(0, 1f));
```

### 4. Basic Usage

```csharp
// In your character's input handling
var interactionController = GetComponent<InteractionController>();

// Interact with the closest object
if (Input.GetKeyDown(KeyCode.E))
{
    interactionController.TryInteract();
}

// Drop carried object
if (Input.GetKeyDown(KeyCode.Q))
{
    interactionController.DropCarriedObject();
}
```

## 📖 Core Components

### InteractionController

The main component that handles object detection, interaction logic, and visual feedback.

**Key Properties:**
- `interactionRange`: Distance for object detection
- `canPickupObjects`: Enable/disable pickup functionality
- `pickupPositionOffset`: Where carried objects are positioned
- `throwForce`: Force applied when throwing objects

**Key Methods:**
- `TryInteract()`: Attempt to interact with the closest object
- `TryPickup()`: Attempt to pick up the closest pickupable object
- `DropCarriedObject(Vector2 throwForce)`: Drop or throw the carried object

### InteractableObject

Base component for objects that can be interacted with.

**Key Features:**
- Automatic visual highlighting when in range
- Configurable interaction priority
- Unity Events for easy Inspector setup
- Override `OnInteract()` for custom behavior

### PickupableObject

Component for objects that can be picked up and carried.

**Key Features:**
- Physics-based pickup and drop mechanics
- Automatic collision and physics management while carried
- Velocity transfer from carrier when dropped
- Drop cooldown to prevent immediate re-pickup

### InteractionFeedbackUI

Provides visual feedback with input device-specific icons.

**Key Features:**
- Automatic input device detection
- Support for keyboard, Xbox, and PlayStation icons
- Automatic icon positioning above interactable objects
- Camera-facing icon rotation

### CharacterController2DAdapter

Adapter component for integrating with existing character controllers.

**Key Features:**
- Automatic detection of character controller components
- Reflection-based property access for velocity and facing direction
- Fallback to Rigidbody2D if controller integration fails
- Configurable property names for custom controllers

## 🔧 Advanced Usage

### Custom Interactable Types

```csharp
public class Door : InteractableObject
{
    [SerializeField] private bool isLocked;
    [SerializeField] private string requiredKey;
    
    public override void OnInteract(IInteractionController interactor)
    {
        if (isLocked)
        {
            Debug.Log("Door is locked!");
            return;
        }
        
        // Open door logic
        OpenDoor();
    }
    
    public void Unlock(string key)
    {
        if (key == requiredKey)
        {
            isLocked = false;
        }
    }
}
```

### Event Integration

```csharp
public class InteractionEventHandler : MonoBehaviour
{
    private void Start()
    {
        var controller = GetComponent<InteractionController>();
        
        controller.OnInteracted += HandleInteraction;
        controller.OnObjectPickedUp += HandlePickup;
        controller.OnObjectDropped += HandleDrop;
    }
    
    private void HandleInteraction(IInteractable interactable)
    {
        Debug.Log($"Interacted with {interactable.GameObject.name}");
    }
    
    private void HandlePickup(IPickupable pickup)
    {
        Debug.Log($"Picked up {pickup.GameObject.name}");
    }
    
    private void HandleDrop(IPickupable pickup)
    {
        Debug.Log($"Dropped {pickup.GameObject.name}");
    }
}
```

### Custom Input Device Icons

```csharp
public class IconSetup : MonoBehaviour
{
    [SerializeField] private Sprite keyboardIcon;
    [SerializeField] private Sprite xboxIcon;
    [SerializeField] private Sprite psIcon;
    
    private void Start()
    {
        var feedbackUI = GetComponentInChildren<InteractionFeedbackUI>();
        feedbackUI.SetInputIcons(keyboardIcon, xboxIcon, psIcon);
    }
}
```

## 🎮 Integration Examples

### With Action Recording System

```csharp
// The interaction system automatically works with the action recording system
// when both packages are installed. Interactions will be recorded and replayed
// by clones automatically.

public class RecordableCharacter : MonoBehaviour, ICharacterController
{
    private InteractionController interaction;
    
    private void Start()
    {
        interaction = GetComponent<InteractionController>();
    }
    
    public void ExecuteInteraction() // Called during replay
    {
        interaction.TryInteract();
    }
}
```

### With Audio Management System

```csharp
public class AudioInteractable : InteractableObject
{
    [SerializeField] private AudioClip interactionSound;
    
    public override void OnInteract(IInteractionController interactor)
    {
        // Play sound using Audio Management System
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(interactionSound, AudioCategory.SFX);
        }
        
        base.OnInteract(interactor);
    }
}
```

## ⚙️ Configuration

### InteractionController Settings

- **Detection Settings**:
  - `interactionRange`: How far objects can be detected (default: 1.5f)
  - `rangePositionOffset`: Offset for detection center (default: Vector2.zero)
  - `interactableLayerMask`: Which layers to check for interactables (default: Everything)
  - `maxDetectedObjects`: Maximum objects to process per frame (default: 10)

- **Pickup Settings**:
  - `canPickupObjects`: Enable pickup functionality (default: true)
  - `pickupPositionOffset`: Where to position carried objects (default: Vector2(0, 0.5f))
  - `throwForce`: Force applied when throwing (default: Vector2(5f, 3f))

- **Visual Feedback**:
  - `feedbackUI`: Reference to InteractionFeedbackUI component
  - `autoCreateFeedbackUI`: Automatically create feedback UI if none assigned (default: true)

### Performance Considerations

- Interaction detection runs every frame using `Physics2D.OverlapCircleAll`
- Use `maxDetectedObjects` to limit processing for performance
- Consider using larger `interactionRange` values for better user experience
- Visual feedback UI automatically culls when not needed

## 🔍 Troubleshooting

### Common Issues

**Objects not detected:**
- Ensure objects have Collider2D components
- Check that objects are on the correct layer (matching `interactableLayerMask`)
- Verify `interactionRange` is large enough
- Make sure objects have `IInteractable` implementations

**Pickup not working:**
- Ensure objects have both Rigidbody2D and Collider2D components
- Check that `canPickupObjects` is enabled on InteractionController
- Verify objects implement `IPickupable` interface
- Check if another object is already being carried

**Visual feedback not showing:**
- Ensure `InteractionFeedbackUI` component is present and configured
- Check that appropriate input device icons are assigned
- Verify that objects are within interaction range
- Check that camera reference is set correctly

**Adapter not working:**
- Verify that CharacterController2DAdapter can find your character controller
- Check property names match your controller's field/property names
- Use `SetPropertyNames()` to manually configure reflection access
- Check console for adapter warning messages

### Debug Features

- Enable `showDebugGizmos` on InteractionController to visualize interaction range
- Use Unity's Debug.Log messages from interactable objects to trace interaction flow
- Monitor the `InteractableObjects` list to see what's being detected

## 📚 API Reference

### Interfaces

#### IInteractable
```csharp
public interface IInteractable
{
    bool CanInteract { get; }
    Transform Transform { get; }
    GameObject GameObject { get; }
    int InteractionPriority { get; }
    
    void OnInteract(IInteractionController interactor);
    void OnInteractionEnter(IInteractionController interactor);
    void OnInteractionExit(IInteractionController interactor);
}
```

#### IPickupable
```csharp
public interface IPickupable : IInteractable
{
    bool CanPickup { get; }
    bool IsBeingCarried { get; }
    Rigidbody2D Rigidbody { get; }
    Collider2D Collider { get; }
    Vector2 CarryPositionOffset { get; }
    
    void OnPickup(IInteractionController carrier);
    void OnDrop(Vector2 force, Vector2 carrierVelocity);
}
```

#### IInteractionController
```csharp
public interface IInteractionController
{
    Transform Transform { get; }
    GameObject GameObject { get; }
    bool CanPickupObjects { get; }
    IPickupable CarriedObject { get; }
    IReadOnlyList<IInteractable> InteractableObjects { get; }
    IInteractable ClosestInteractable { get; }
    Vector2 Velocity { get; }
    bool FacingRight { get; }
    
    bool TryInteract();
    bool TryPickup();
    bool DropCarriedObject(Vector2 throwForce = default);
    
    event System.Action<IInteractable> OnInteracted;
    event System.Action<IPickupable> OnObjectPickedUp;
    event System.Action<IPickupable> OnObjectDropped;
}
```

## 🔄 Migration Guide

### From Original GMTK2025 InteractSystem

The new interaction system maintains similar functionality but with improved architecture:

**Changed:**
- `InteractSystem` → `InteractionController`
- `InteractableObject` is now interface-based
- `PickUpObject` → `PickupableObject` with `IPickupable` interface
- Input icon management moved to `InteractionFeedbackUI`

**Added:**
- Interface-based design for better flexibility
- Adapter components for easy integration
- Event system for external integration
- Priority-based interaction selection
- Automatic visual feedback management

**Migration Steps:**
1. Replace `InteractSystem` components with `InteractionController`
2. Add `CharacterController2DAdapter` if using existing character controllers
3. Update custom interactable objects to inherit from new `InteractableObject` or implement `IInteractable`
4. Replace pickup objects with `PickupableObject` components
5. Configure visual feedback through `InteractionFeedbackUI`

## 📄 License

MIT License - see LICENSE.md for details.

## 🤝 Contributing

This package was extracted from the GMTK2025 project. Contributions, bug reports, and feature requests are welcome!

---

**Part of the GMTK2025 Unity Package Collection**
- [Action Recording System](../com.gmtk2025.action-recording/)
- [Audio Management System](../com.gmtk2025.audio-management/)
- **Interaction System** ← You are here