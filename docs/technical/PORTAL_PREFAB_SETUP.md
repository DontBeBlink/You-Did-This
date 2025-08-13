# Portal Prefab Setup Guide

## Creating a Portal Prefab in Unity

### Step 1: Create Basic GameObject
1. Create empty GameObject in scene
2. Name it "Portal"
3. Position where desired

### Step 2: Add Required Components

#### Collider2D (Trigger)
```
Component: BoxCollider2D or CircleCollider2D
Settings:
- Is Trigger: ✓ (checked)
- Size: Appropriate for portal entry area
```

#### Portal Script
```
Component: Portal (script)
Settings:
- Target Position: Set destination coordinates
- Use Transform As Target: false (or true if using target object)
- Target Transform: (optional, if using object reference)
- Cooldown Time: 1.0
- Disable After Use: false (or true for one-time portal)
- Requires Player Tag: true
- Use Transition Effects: true
- Play Portal Sound: true
```

### Step 3: Visual Elements (Optional)
Add visual indicators to make the portal obvious to players:

#### Sprite Renderer
```
Component: SpriteRenderer
Settings:
- Sprite: Portal texture/sprite
- Color: Distinctive color (blue, purple, etc.)
- Order in Layer: Above background
```

#### Particle Effects (Optional)
```
Component: ParticleSystem
Settings:
- Create swirling or glowing effect
- Loop: true
- Start Lifetime: 2-5 seconds
- Start Speed: Low (0.5-2)
- Shape: Circle or Donut
```

### Step 4: Audio (Optional)
```
Component: AudioSource
Settings:
- Play On Awake: false
- Loop: true
- Volume: 0.3-0.5
- Audio Clip: Ambient portal sound
```

### Step 5: Save as Prefab
1. Drag configured GameObject to Project window
2. Choose "Create Original Prefab"
3. Name it "Portal.prefab"

## Example Portal Configurations

### Basic Teleporter
```csharp
Target Position: (10, 5, 0)
Cooldown Time: 1.0
Disable After Use: false
```

### One-Time Secret Door
```csharp
Target Position: (20, 15, 0)
Cooldown Time: 0.5
Disable After Use: true
```

### Linked Portal System
```csharp
Use Transform As Target: true
Target Transform: ExitPortal_Transform
Cooldown Time: 2.0
```

## Integration with Existing Systems

### Player Tag Setup
Ensure player GameObject has "Player" tag:
1. Select Player GameObject
2. Set Tag dropdown to "Player"
3. If "Player" tag doesn't exist, create it in Tag Manager

### CloneManager Requirement
Portal requires CloneManager in scene for transition effects:
- Should already exist in puzzle scenes
- Portal will disable transition effects if not found
- Check console for warnings if effects don't work

### AudioManager Integration
Portal uses existing AudioManager:
- Uses retract sound for consistency
- Respects master volume settings
- No additional setup required

## Testing Checklist

### Functionality Tests
- [ ] Portal triggers when player enters
- [ ] Player teleports to correct position
- [ ] Cooldown prevents rapid re-triggering
- [ ] One-time portals disable after use
- [ ] Non-player objects don't trigger (if requiresPlayerTag = true)

### Visual/Audio Tests
- [ ] Transition animation plays
- [ ] Portal sound plays during teleport
- [ ] Player immobilization works correctly
- [ ] Physics state resets at destination
- [ ] Camera effects work (if CameraController present)

### Edge Case Tests
- [ ] Portal works with clones (should not trigger for them)
- [ ] Portal works during active recording
- [ ] Portal works when player is carrying objects
- [ ] Portal respects collision layers properly

## Troubleshooting

### Portal Not Triggering
1. Check collider is set as trigger
2. Verify player has "Player" tag
3. Check if portal is on cooldown
4. Verify portal hasn't been disabled (one-time use)

### No Transition Effects
1. Ensure CloneManager exists in scene
2. Check "Use Transition Effects" setting
3. Verify AudioManager exists for sound
4. Check console for error messages

### Player Position Issues
1. Verify target position is valid (not inside walls)
2. Check target transform reference if using linked mode
3. Ensure target area has proper ground/platform

### Performance Problems
1. Increase cooldown time
2. Use smaller trigger colliders
3. Consider using one-time portals where appropriate
4. Check for multiple overlapping portal triggers