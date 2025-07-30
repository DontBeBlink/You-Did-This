# Unity Setup Instructions for "You Did This" Puzzle Platformer

## Scene Setup

### 1. Player Setup
1. Create an empty GameObject and name it "Player"
2. Add the following components:
   - `Rigidbody2D` (set Freeze Rotation Z to true)
   - `BoxCollider2D` or `CapsuleCollider2D`
   - `SpriteRenderer` (assign a simple square/capsule sprite)
   - `PlayerInput` (create from InputSystem_Actions.inputactions)
   - `PlayerController` script
   - `ActionRecorder` script
   - `ActionPlayer` script
   - `Clone` script

### 2. Ground Setup
1. Create a GameObject and name it "Ground"
2. Add components:
   - `SpriteRenderer` (use Ground.mat material)
   - `BoxCollider2D`
3. Scale and position to create platforms
4. Set Layer to "Ground" (create this layer in Tags & Layers)

### 3. Clone Manager Setup
1. Create an empty GameObject and name it "CloneManager"
2. Add the `CloneManager` script
3. Create a prefab from the Player GameObject
4. Assign this prefab to the Clone Prefab field in CloneManager
5. Optionally create an empty GameObject as "CloneParent" for organization

### 4. Goals Setup
1. Create a GameObject and name it "Goal"
2. Add components:
   - `SpriteRenderer` (use a distinct color/material)
   - `BoxCollider2D` (set as Trigger)
   - `Goal` script
3. Position where you want clones to get "stuck"

### 5. Camera Setup
1. Set Main Camera to follow the active player
2. You may want to add a simple camera follow script

### 6. UI Setup (Optional)
1. Create a Canvas
2. Add Text elements for clone count and instructions
3. Add the `GameUI` script to manage UI updates

## Input Mapping
The following inputs are already configured:
- **WASD/Arrows**: Movement
- **Space**: Jump  
- **2**: Create new clone (Next action)
- **1**: Retract to previous clone (Previous action)

## Layer Setup
1. Create a "Ground" layer for platforms
2. Set the Ground Check Layer Mask in PlayerController to "Ground"

## Materials
- Use the existing Ground.mat for platforms
- Use the existing Walls.mat for walls/obstacles
- Create distinct materials for different clone states (optional)

## Testing
1. Play the scene
2. Move around with WASD and jump with Space
3. Press 2 to create a clone - the previous character should start replaying your movements
4. Press 1 to retract back to the previous clone
5. Move a clone into a Goal trigger to make it "stuck"

## Core Mechanics Implemented
- ✅ Player movement and jumping
- ✅ Clone creation system
- ✅ Action recording and playback
- ✅ Clone looping (replays recorded actions)
- ✅ Clone getting stuck at goals
- ✅ Retraction system (with stuck clone prevention)
- ✅ Visual feedback for clone states

## Next Steps
- Design puzzle levels that require clone coordination
- Add sound effects and particle effects
- Implement win/lose conditions
- Add level progression system