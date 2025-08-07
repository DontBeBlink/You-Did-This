# Portal System Documentation

## Overview

The Portal system provides teleportation functionality for level design, allowing players to be instantly transported between different locations. Portals reuse the same transition and animation system as the clone retract feature for consistent visual and audio feedback.

## Portal Component

### Basic Setup

1. **Create Portal GameObject**:
   - Create an empty GameObject
   - Add the `Portal` script
   - Add a `Collider2D` component (set as trigger)
   - Position in your scene

2. **Configure Portal Settings**:
   ```csharp
   [Header("Portal Settings")]
   [SerializeField] private Vector3 targetPosition;      // Where to teleport the player
   [SerializeField] private bool useTransformAsTarget;   // Use another object's position
   [SerializeField] private Transform targetTransform;   // Reference to target object
   ```

### Portal Behavior Options

```csharp
[Header("Portal Behavior")]
[SerializeField] private float cooldownTime = 1f;        // Prevent rapid re-triggering
[SerializeField] private bool disableAfterUse = false;   // One-time use portal
[SerializeField] private bool requiresPlayerTag = true;  // Only activate for "Player" tag
```

### Visual Effects

```csharp
[Header("Visual Effects")]
[SerializeField] private bool useTransitionEffects = true; // Use clone transition system
[SerializeField] private bool playPortalSound = true;      // Play audio during teleport
```

## Usage Examples

### Basic Teleport Portal

```csharp
// Simple portal that teleports to a specific position
Portal portal = gameObject.GetComponent<Portal>();
portal.SetTargetPosition(new Vector3(10f, 5f, 0f));
```

### Object-Linked Portal

```csharp
// Portal that teleports to another object's position
Portal portal = gameObject.GetComponent<Portal>();
portal.SetTargetTransform(exitPoint.transform);
```

### One-Time Portal

```csharp
// Portal that can only be used once
Portal portal = gameObject.GetComponent<Portal>();
portal.disableAfterUse = true;
portal.SetTargetPosition(secretArea);
```

## Integration with Clone System

The Portal system reuses the same visual and audio effects as the clone retract feature:

- **Animation**: Uses the "pickup" animation trigger
- **Audio**: Plays the retract sound effect for consistency
- **Transition**: Same player immobilization and fade effects
- **Physics**: Resets player physics state at the target position

## Public Methods

### Configuration
- `SetTargetPosition(Vector3 newTarget)` - Set teleport destination
- `SetTargetTransform(Transform newTargetTransform)` - Use object as destination
- `GetTargetPosition()` - Get current destination

### State Management
- `ResetPortal()` - Reset one-time portals to usable state
- `IsOnCooldown` - Check if portal is in cooldown period
- `IsActivated` - Check if one-time portal has been used

## Level Design Tips

1. **Visual Indicators**: Add visual elements to clearly show portal entrances and exits
2. **Consistent Theming**: Use similar visual design for portals and clone effects
3. **Gameplay Flow**: Place portals strategically to enhance puzzle solutions
4. **Cooldown Management**: Adjust cooldown times to prevent exploitation
5. **One-Way vs Two-Way**: Consider if portals should work in both directions

## Scene Gizmos

When a Portal is selected in the Scene view, it displays:
- Blue sphere at portal position
- Red sphere at target position  
- Yellow line connecting portal to target
- Direction arrow at target position

## Audio Integration

Portals use the existing AudioManager system:
- Reuses `cloneRetractSound` for consistency
- Respects master volume settings
- Integrates with the singleton AudioManager pattern

## Performance Considerations

- Portals use trigger colliders for efficient collision detection
- Cooldown system prevents performance issues from rapid triggering
- One-time portals automatically disable their colliders when used
- Scene gizmos only draw when the portal is selected

## Troubleshooting

### Portal Not Working
- Ensure Collider2D is set as trigger
- Check that target position is valid
- Verify player has "Player" tag if `requiresPlayerTag` is enabled
- Check if portal is on cooldown or already used (for one-time portals)

### No Transition Effects
- Ensure CloneManager is present in the scene
- Check `useTransitionEffects` setting
- Verify AudioManager is available for sound effects

### Performance Issues
- Increase cooldown time if portals are triggering too frequently
- Use one-time portals where appropriate to reduce ongoing collision checks
- Ensure trigger colliders are appropriately sized