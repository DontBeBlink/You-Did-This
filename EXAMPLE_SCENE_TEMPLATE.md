# Example Scene Template - Basic Pressure Plate Puzzle

This document provides a step-by-step template for creating a simple puzzle scene that demonstrates the core mechanics documented in [LEVEL_DESIGN.md](LEVEL_DESIGN.md).

## 🎯 Puzzle Concept: "First Steps"

**Objective**: Teach players to use a clone to hold a pressure plate while they proceed to the goal.

**Learning Goals**:
- Clone creation and replay mechanics
- Pressure plate functionality  
- Goal completion
- Basic spatial reasoning

## 📐 Scene Layout

```
[Start/CloneManager] ──platform── [Pressure Plate] ──raised bridge── [Goal]
                                        │                │
                                  [AreaTrigger]     [Triggerable Door]
```

## 🔧 Implementation Steps

### Step 1: Scene Setup
1. **Create New Scene**
   - File → New Scene
   - Save as "ExamplePuzzle.unity" in Assets/Scenes/

2. **Add Level Manager**
   ```
   Create Empty GameObject: "LevelManager"
   Components to add:
   - PuzzleLevel.cs
   Settings:
   - Level Name: "First Steps"
   - Min Clones: 1
   - Max Clones: 3
   ```

3. **Add Clone Manager**
   ```
   Create Empty GameObject: "CloneManager"
   Position: (0, 0, 0) [player spawn point]
   Components to add:
   - CloneManager.cs
   Settings:
   - Loop Duration: 15f
   - Max Clones: 10
   - Manual Looping: true
   ```

### Step 2: Player Setup
```
Create GameObject: "Player"
Position: (0, 0, 0)
Components:
- PlayerController.cs
- CharacterController2D.cs  
- ActionRecorder.cs
- Rigidbody2D (Gravity Scale: 3, Freeze Z Rotation)
- CapsuleCollider2D (Size: 0.8 x 1.6)
- SpriteRenderer (assign player sprite)
Tags: "Player"
Layer: "Player"
```

### Step 3: Level Geometry
```
Ground Platform:
- Create GameObject: "Ground"
- Transform: Position (0, -2, 0), Scale (10, 1, 1)
- SpriteRenderer: assign platform sprite
- BoxCollider2D: Size (10, 1)

Bridge Platform:
- Create GameObject: "Bridge"  
- Transform: Position (6, 0, 0), Scale (4, 0.5, 1)
- SpriteRenderer: assign bridge sprite
- BoxCollider2D: Size (4, 0.5)
```

### Step 4: Pressure Plate System
```
Pressure Plate GameObject:
- Create GameObject: "PressurePlate"
- Position: (2, -1, 0)
- Components:
  - AreaTrigger.cs
  - BoxCollider2D (Size: 1x0.2, IsTrigger: true)
  - SpriteRenderer (assign pressure plate sprite)
  - Animator (with "active" boolean parameter)
Settings:
- Trigger Mask: Player + Clone layers
- Player Only: false
- One Shot: false
```

### Step 5: Bridge Door System  
```
Bridge Door GameObject:
- Create GameObject: "BridgeDoor"
- Position: (6, 0.5, 0)  
- Components:
  - Triggerable.cs
  - SpriteRenderer (assign door/barrier sprite)
  - Animator (with "active" boolean parameter)
Settings:
- Trigger: Link to PressurePlate (drag PressurePlate into Trigger field)

Animation Setup:
- Create Animator Controller
- Add "active" bool parameter
- Closed state: Door blocks path (default)
- Open state: Door moves up/away when active=true
- Transition: active true/false with immediate transitions
```

### Step 6: Goal Setup
```
Goal GameObject:
- Create GameObject: "Goal"
- Position: (8, 0, 0)
- Components:
  - Goal.cs
  - BoxCollider2D (Size: 1x1, IsTrigger: true)
  - SpriteRenderer (assign goal/flag sprite)
Settings:
- Requires Specific Clone: false
- Is Player Goal: false
- Is Completed: false
- Incomplete Color: Yellow
- Complete Color: Green
```

### Step 7: Level Configuration
```
PuzzleLevel Configuration:
- Required Goals: Drag Goal GameObject into array
- Min Clones: 1
- Max Clones: 5
- Next Scene: "NextLevel" (or leave empty for completion message)
- Level End Text: "Great job! You've learned the basics!"
```

## 🧪 Testing Checklist

### Basic Functionality
- [ ] Player spawns at CloneManager position
- [ ] Player can move with WASD/Arrow keys
- [ ] Player can jump with Spacebar
- [ ] Pressure plate activates when player steps on it
- [ ] Bridge door opens when pressure plate is active
- [ ] Bridge door closes when pressure plate is inactive

### Clone System
- [ ] Press L key creates a clone
- [ ] Clone replays player actions automatically
- [ ] Clone activates pressure plate when replaying
- [ ] Clone keeps pressure plate active while on it
- [ ] Player can reach goal while clone holds plate

### Goal System
- [ ] Goal changes from yellow to green when reached
- [ ] Audio plays when goal is completed
- [ ] Level completion message appears
- [ ] F1 debug info shows correct clone count

### Visual Polish
- [ ] All sprites are assigned and visible
- [ ] Animations play smoothly for door opening/closing
- [ ] Pressure plate visual feedback works
- [ ] Goal color changes are visible
- [ ] No missing visual elements or placeholder sprites

## 🎮 Solution Walkthrough

**Intended Solution**:
1. Player moves right and steps on pressure plate
2. Bridge door opens while player is on plate
3. Player presses L key to create a clone at pressure plate
4. Clone automatically replays the action of walking to and staying on pressure plate
5. Pressure plate remains active due to clone presence
6. Bridge door stays open
7. Player moves across bridge to reach goal
8. Goal completion triggers level completion

**Alternative Approaches**:
- Players might try jumping across without using pressure plate (ensure gap is too wide)
- Players might create clone before stepping on plate (will still work but less elegant)

## 📊 Metrics and Balancing

**Expected Timing**:
- Time to understand puzzle: 10-20 seconds
- Time to solve after understanding: 20-30 seconds
- Total completion time: 30-60 seconds

**Difficulty Indicators**:
- ✅ Single mechanic focus (pressure plate + clone)
- ✅ Clear visual connections (plate → door → goal)
- ✅ Forgiving timing (clone holds plate indefinitely)
- ✅ Obvious solution path once mechanics understood

**Common Player Mistakes**:
- Trying to jump across without using mechanics (solved by making gap impossible to jump)
- Not understanding clone creation (solved by clear tutorial text)
- Creating clone in wrong position (solved by clone replay system being forgiving)

## 🔄 Variations and Extensions

### Beginner Variation: "Guided First Steps"
- Add tutorial text UI explaining each step
- Highlight interactive elements with glowing effects
- Add arrow indicators showing intended path

### Intermediate Variation: "Double Duty"
- Add second pressure plate requiring second clone
- Use AND gate to require both plates simultaneously
- Add second goal for additional challenge

### Advanced Variation: "Timing Challenge"
- Make bridge door close after 3 seconds
- Require precise timing between clone creation and movement
- Add visual timer showing remaining time

## 🛠️ Troubleshooting

### Common Issues and Solutions

**Clone doesn't activate pressure plate**:
- Verify Trigger Mask includes clone layer
- Check that clone has proper Collider2D component
- Ensure pressure plate BoxCollider2D is set as trigger

**Bridge door doesn't respond to pressure plate**:
- Verify Triggerable component has Trigger field linked to pressure plate
- Check that Animator has "active" boolean parameter
- Ensure animation transitions are set up correctly

**Goal doesn't respond to clone**:
- Verify Goal component has proper settings
- Check that goal GameObject has Collider2D as trigger
- Ensure clone has appropriate tag/layer for detection

**Performance issues with multiple clones**:
- Check CloneManager max clone settings
- Monitor with Unity Profiler during testing
- Optimize collision detection if needed

---

This template provides a complete, working example that demonstrates the fundamental mechanics of **You Did This** while serving as a practical reference for level creators following the documentation in [LEVEL_DESIGN.md](LEVEL_DESIGN.md).

For more complex puzzle ideas, see [PUZZLE_IDEAS.md](PUZZLE_IDEAS.md).