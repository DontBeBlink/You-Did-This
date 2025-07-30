# Unity Setup Instructions for "You Did This" Puzzle Platformer

## Quick Start
1. Follow the **Player Prefab Setup** (see PrefabSetup.md)
2. Create **Ground platforms** with Ground layer
3. Add **Goals** with trigger colliders
4. Add **CloneManager** to scene
5. Optionally add **Camera Follow** and **UI**

## Scene Setup

### 1. Layer Setup (FIRST!)
1. Go to Edit → Project Settings → Tags and Layers
2. Create a new layer called "Ground"
3. Assign this layer to all platform GameObjects

### 2. Player Setup
1. Create the Player prefab following PrefabSetup.md
2. The first player can be placed in scene manually, or CloneManager will create one

### 3. Ground Setup
1. Create a GameObject and name it "Ground"
2. Add components:
   - `SpriteRenderer` (use Ground.mat material if available)
   - `BoxCollider2D`
3. **Set Layer to "Ground"** (this is crucial for ground detection)
4. Scale and position to create platforms
5. Duplicate to create multiple platforms

### 4. Clone Manager Setup
1. Create an empty GameObject and name it "CloneManager"
2. Add the `CloneManager` script
3. Assign the Player prefab to "Clone Prefab" field
4. Set Max Clones (default: 10)
5. Optionally create "CloneParent" empty GameObject for organization

### 5. Goals Setup
1. Create a GameObject and name it "Goal"
2. Add components:
   - `SpriteRenderer` (use bright color like yellow/green)
   - `BoxCollider2D` **set as Trigger ✓**
   - `Goal` script
3. Position where you want clones to get "stuck"
4. Duplicate for multiple goals

### 6. Camera Setup
1. Add `CameraFollow` script to Main Camera
2. Adjust Follow Speed and Offset as needed
3. Camera will automatically follow the active clone

### 7. Audio Setup (Optional)
1. Create empty GameObject named "AudioManager"
2. Add `AudioManager` script
3. Assign audio clips for different actions
4. AudioManager persists between scenes

### 8. UI Setup (Optional)
1. Create a Canvas (Screen Space - Overlay)
2. Add TextMeshPro text elements for:
   - Clone count display
   - Instructions
   - Status messages
3. Add `GameUI` script to a UI GameObject
4. Assign UI elements to script fields

### 9. Level Management (Optional)
1. Add `PuzzleLevel` script to scene
2. Assign required Goal objects
3. Set min/max clone limits
4. Level automatically detects completion

## Input Mapping
The following inputs are already configured:
- **WASD/Arrows**: Movement
- **Space**: Jump  
- **2**: Create new clone (Next action)
- **1**: Retract to previous clone (Previous action)

## Testing Checklist
- [ ] Player moves with WASD and jumps with Space
- [ ] Ground detection works (player doesn't fall through platforms)
- [ ] Press 2 to create clone - previous character replays actions
- [ ] Press 1 to retract (works unless previous clone is stuck)
- [ ] Clone turns red and stops when touching Goal trigger
- [ ] Camera follows active clone
- [ ] UI shows clone count and status

## Troubleshooting

### Player falls through ground
- Check that platforms have "Ground" layer assigned
- Check PlayerController has correct Ground Layer mask
- Ensure platforms have BoxCollider2D (not trigger)

### Clone creation doesn't work
- Ensure CloneManager has Player prefab assigned
- Check that Player prefab has all required components
- Verify PlayerInput is configured correctly

### Actions not recording/playing
- Check that ActionRecorder and ActionPlayer are on prefab
- Ensure PlayerController references are set up correctly

### Ground detection issues
- Verify GroundCheck child object exists and is positioned correctly
- Check Ground Check Radius in PlayerController
- Ensure Ground Layer mask is set correctly

## Core Mechanics Implemented
- ✅ Player movement and jumping
- ✅ Clone creation system  
- ✅ Action recording and playback
- ✅ Clone looping (replays recorded actions)
- ✅ Clone getting stuck at goals
- ✅ Retraction system (with stuck clone prevention)
- ✅ Visual feedback for clone states
- ✅ Audio feedback for actions
- ✅ Camera follows active clone
- ✅ Basic UI system
- ✅ Level completion detection

## Next Steps
- Design puzzle levels that require clone coordination
- Add particle effects for better visual feedback  
- Implement win/lose conditions for specific puzzles
- Add level progression system
- Create tutorial levels