# Player Prefab Setup Guide

## Required Components for Player Prefab

### GameObject: "Player"

#### Transform
- Position: (0, 0, 0)
- Scale: (1, 1, 1)

#### Rigidbody2D
- Body Type: Dynamic
- Material: None (or create bouncy material if desired)
- Mass: 1
- Linear Drag: 0
- Angular Drag: 0.05
- Gravity Scale: 1
- **Freeze Rotation Z: ✓ (IMPORTANT)**

#### Collider2D (BoxCollider2D recommended)
- Is Trigger: ✗
- Material: None
- Size: (0.8, 0.8) for square sprite
- Offset: (0, 0)

#### SpriteRenderer
- Sprite: Assign any square/capsule sprite
- Color: White (will be changed by Clone script)
- Sorting Layer: Default
- Order in Layer: 0

#### PlayerInput
- Actions: Assign "InputSystem_Actions" asset
- Default Map: "Player"
- Behavior: Send Messages (or Invoke Unity Events)

#### PlayerController (Script)
- Move Speed: 5
- Jump Force: 10
- Ground Layer: "Ground" (create this layer)
- Ground Check Radius: 0.2
- Is Player Controlled: ✓

#### ActionRecorder (Script)
- No configuration needed

#### ActionPlayer (Script)
- Should Loop: ✓

#### Clone (Script)
- Active Color: White
- Replaying Color: Cyan
- Stuck Color: Red

### Child GameObject: "GroundCheck"
- Position: (0, -0.5, 0) relative to player
- This is automatically created by PlayerController if missing

## Creating the Prefab
1. Set up the Player GameObject with all components above
2. Drag it to Project window to create prefab
3. Assign this prefab to CloneManager's "Clone Prefab" field
4. Delete the original from scene (CloneManager will create the first one)