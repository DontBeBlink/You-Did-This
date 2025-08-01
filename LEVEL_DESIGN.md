# Level Design Guide - You Did This

This comprehensive guide covers everything you need to know about creating engaging puzzle levels for **You Did This**. The game's clone-based mechanics allow for complex, layered puzzles that require strategic thinking and timing.

## 🎯 Core Level Design Principles

### The Clone-Driven Puzzle Loop
Every level in **You Did This** follows this fundamental pattern:
1. **Explore**: Player examines the level to understand the puzzle
2. **Record**: Player performs actions that are recorded by the system
3. **Clone**: Player creates clones that replay their recorded actions
4. **Coordinate**: Player coordinates between current position and clone actions
5. **Solve**: Clones reach goals and become permanent puzzle elements
6. **Progress**: All goals completed, level transitions to next

### Design Philosophy
- **Layered Solutions**: Puzzles should require multiple clones working together
- **Clear Visual Language**: Players should immediately understand what each element does
- **Progressive Complexity**: Early actions teach mechanics for later complexity
- **Multiple Approaches**: Allow creative solutions while maintaining core requirements

## 🏗️ Level Architecture Components

### Essential Components

#### 1. CloneManager
**Purpose**: Central controller for clone creation and management
```
Required in every level scene
Position: Where clones spawn (usually player start position)
Settings:
- Loop Duration: 15s (standard) - when automatic clones are created
- Max Clones: 10 (default) - prevents performance issues
- Manual Looping: Enabled (allows L key clone creation)
```

#### 2. Goals (Flags)
**Purpose**: Define puzzle completion targets that make clones permanent
```
Components: Goal.cs, Collider2D (trigger), SpriteRenderer
Visual States:
- Yellow: Incomplete, available for clones
- Green: Completed, clone is stuck at this position
Types:
- Generic: Any clone can complete
- Specific Clone: Only designated clone index can complete
- Player Goal: Only the active player can complete
```

#### 3. Player Controller
**Purpose**: Handles player input and coordinates with recording system
```
Components: PlayerController.cs, CharacterController2D.cs, ActionRecorder.cs
Key Features:
- Movement recording for clone replay
- Interaction system for triggers and objects
- Physics-perfect replay capability
```

### Logic System Components

#### 4. Trigger Objects
Base class for all interactive elements:

**TriggerObject (Base Class)**
- `Active` property: Current state (true/false)
- `oneShot`: Stays active after first trigger
- Animation integration for visual feedback

**AreaTrigger (Pressure Plates)**
```
Purpose: Triggered by objects/players entering area
Settings:
- Trigger Mask: Which layers activate it
- Player Only: Restrict to player character
Use Cases: Weight-activated switches, proximity sensors
```

**InteractableTrigger (Levers/Switches)**
```
Purpose: Manually activated by player interaction (E key)
Settings:
- One Shot: Whether it stays active after use
Use Cases: Levers, buttons, manual switches
```

#### 5. Logic Gates
**LogicGateTrigger**: Combines multiple triggers with boolean logic

**Available Gate Types:**
- **Is**: Passes through input A directly
- **Not**: Inverts input A
- **And**: True when both A and B are true
- **Or**: True when either A or B is true
- **Xor**: True when A and B differ
- **Nand**: Inverted And gate
- **Nor**: Inverted Or gate
- **And3/Or3/Xor3**: Three-input variants

**Visual Feedback**: 
- Green sphere: Gate is active (output true)
- Red sphere: Gate is inactive (output false)

#### 6. Triggerable Objects (Doors/Platforms)
**Triggerable**: Objects that respond to trigger states
```
Purpose: Visual/mechanical responses to trigger activation
Components: Triggerable.cs, Animator
Common Applications:
- Doors: Open/close based on trigger state
- Platforms: Move/activate when triggered
- Barriers: Lower/raise with trigger changes
Animation: "active" parameter controls visual state
```

## 📋 Step-by-Step Level Creation

### Phase 1: Scene Setup

1. **Create New Scene**
   ```
   File → New Scene
   Save in Assets/Scenes/ with descriptive name
   ```

2. **Add Essential GameObjects**
   ```
   Create Empty GameObject: "LevelManager"
   - Add PuzzleLevel.cs component
   - Configure level settings (name, goals, completion)
   
   Create Empty GameObject: "CloneManager" 
   - Add CloneManager.cs component
   - Position at player spawn point
   ```

3. **Player Setup**
   ```
   Add player prefab or create GameObject with:
   - PlayerController.cs
   - CharacterController2D.cs  
   - ActionRecorder.cs
   - Rigidbody2D, Collider2D
   - Tag: "Player"
   ```

### Phase 2: Level Geometry

1. **Create Platforms and Walls**
   ```
   Use Sprites or Unity primitives
   Add Collider2D components for physics
   Set up appropriate layers:
   - Ground: Static collision
   - Platforms: One-way platforms if needed
   - Walls: Boundary collision
   ```

2. **Design Layout Principles**
   ```
   Clear sightlines: Players should see puzzle elements
   Logical flow: Natural progression from start to goals
   Clone paths: Ensure recorded actions have clear paths
   Visual hierarchy: Important elements stand out
   ```

### Phase 3: Puzzle Implementation

1. **Place Goals (Flags)**
   ```
   For each required goal:
   - Create GameObject with Goal.cs
   - Add Collider2D, set as Trigger
   - Add SpriteRenderer with flag sprite
   - Configure goal type in inspector
   - Position strategically in puzzle solution
   ```

2. **Add Trigger Elements**
   ```
   Pressure Plates (AreaTrigger):
   - Create GameObject with AreaTrigger.cs
   - Set Trigger Mask to appropriate layers
   - Add Collider2D as trigger
   - Add visual feedback (sprite/animation)
   
   Levers (InteractableTrigger):
   - Create GameObject with InteractableTrigger.cs + TriggerObject.cs
   - Add Collider2D for interaction range
   - Set up visual feedback animation
   - Position within player reach
   ```

3. **Implement Logic Gates**
   ```
   Create GameObject with LogicGateTrigger.cs
   Configure inputs:
   - Drag trigger objects to Input A, B, C slots
   - Select appropriate gate type
   - Test logic with preview gizmos
   ```

4. **Add Responsive Elements**
   ```
   Doors/Barriers:
   - Create GameObject with Triggerable.cs
   - Add Animator with "active" bool parameter
   - Link to logic gate or direct trigger
   - Set up open/close animations
   ```

### Phase 4: Testing and Refinement

1. **Basic Functionality Test**
   ```
   - Player movement and controls work
   - Clone creation and replay functions
   - Triggers activate correctly
   - Logic gates respond to inputs
   - Goals detect clones properly
   ```

2. **Puzzle Solution Validation**
   ```
   - Solve puzzle manually to verify solvability
   - Test with different clone counts
   - Ensure timing windows are achievable
   - Check for unintended shortcuts
   ```

3. **Performance Testing**
   ```
   - Test with maximum expected clones (5-10)
   - Monitor frame rate and performance
   - Optimize collision detection if needed
   - Ensure responsive UI and feedback
   ```

## 🎨 Visual Design Guidelines

### Color Coding System
```
Goals:
- Yellow (#FFD700): Incomplete/available
- Green (#00FF00): Completed/clone stuck

Triggers:
- Blue (#0080FF): Pressure plates
- Orange (#FF8000): Interactive levers
- Purple (#8000FF): Logic gates

Doors/Barriers:
- Red (#FF0000): Closed/inactive
- Green (#00FF00): Open/active
```

### Visual Hierarchy
1. **Primary Elements**: Goals should be most prominent
2. **Secondary Elements**: Key triggers and switches
3. **Supporting Elements**: Logic gates and connections
4. **Environmental**: Platforms, walls, decorative elements

### Feedback Systems
```
Immediate Feedback:
- Color changes for state transitions
- Animation for trigger activation
- Audio cues for clone creation and goals

Progressive Feedback:
- Goal completion count
- Clone status indicators
- Progress toward puzzle solution
```

## 🔧 Technical Considerations

### Performance Guidelines
```
Recommended Limits:
- Maximum Goals: 10 per level
- Maximum Triggers: 15 per level
- Maximum Logic Gates: 8 per level
- Expected Clone Count: 3-7 for complex puzzles

Optimization Tips:
- Use object pooling for frequently spawned effects
- Minimize complex physics interactions
- Group related objects under parent GameObjects
- Use appropriate layer settings for collision filtering
```

### Debugging Tools
```
Built-in Debug Features:
- F1: Toggle debug information display
- Visual gizmos for goals and ranges
- Console logging for trigger events
- Clone count and status monitoring

Custom Debug Additions:
- Add Debug.Log statements for trigger activations
- Use OnDrawGizmos for custom visual debugging
- Color-code debug output for different systems
```

### Common Pitfalls
```
Avoid These Issues:
- Goals too close together (clone collision)
- Unreachable trigger positions
- Logic gates with missing inputs
- Overly complex timing requirements
- Dead-end paths that waste clones

Solutions:
- Test thoroughly with multiple approaches
- Provide clear visual guidance
- Design forgiving timing windows
- Allow for player experimentation
```

## 📐 Level Metrics and Balancing

### Difficulty Progression
```
Beginner Levels (1-3):
- Single clone solutions
- Direct trigger-to-door relationships
- Clear visual connections
- Forgiving timing

Intermediate Levels (4-7):
- Multi-clone coordination
- Simple logic gates (And, Or)
- Timing-based elements
- Multiple solution paths

Advanced Levels (8+):
- Complex logic chains
- Specific clone requirements
- Precise timing coordination
- Creative mechanic combinations
```

### Success Metrics
```
Good Level Indicators:
- Solvable in 3-7 clones for main solution
- Clear "aha!" moment when solution understood
- 30-60 seconds to solve once solution known
- Multiple approaches possible
- Natural progression teaching new concepts

Problem Indicators:
- Requires excessive trial and error
- Solution depends on pixel-perfect timing
- No clear visual guidance for next steps
- Single rigid solution path
- Frustrating reset/retry cycles
```

### Playtesting Checklist
```
Essential Tests:
- [ ] Can be solved by someone unfamiliar with your design
- [ ] Solution feels logical and satisfying
- [ ] Visual feedback clearly indicates progress
- [ ] Performance remains stable with multiple clones
- [ ] No game-breaking bugs or edge cases
- [ ] Fits within overall game difficulty curve
```

## 🎮 Advanced Techniques

### Complex Logic Patterns
```
Sequential Activation:
Use logic gates to require specific trigger orders
Example: And3 gate requiring A, then B, then C

Conditional Paths:
Use Xor gates to create either/or decisions
Example: Two paths but only one can be active

Reset Mechanisms:
Combine oneShot and non-oneShot triggers
Example: Lever that resets pressure plate states
```

### Clone Management Strategies
```
Specific Clone Goals:
- Require exact clone indices for ordering
- Create dependency chains between clones
- Design "sacrifice" clones for temporary activation

Efficient Clone Usage:
- Design paths that serve multiple purposes
- Create loops that maintain useful clone positions
- Balance exploration vs. committed clone actions
```

### Environmental Storytelling
```
Visual Narrative:
- Arrange elements to suggest intended flow
- Use consistent artistic themes
- Create visual metaphors for mechanical concepts

Contextual Clues:
- Color-code related puzzle elements
- Use spatial relationships to suggest connections
- Provide environmental hints for complex solutions
```

---

## 📚 Reference Materials

### Component Quick Reference
```
Goal.cs: Win condition targets for clones
TriggerObject.cs: Base class for all triggers
AreaTrigger.cs: Pressure plates and proximity triggers
InteractableTrigger.cs: Manual player-activated switches
LogicGateTrigger.cs: Boolean logic combination of triggers
Triggerable.cs: Objects that respond to trigger states
PuzzleLevel.cs: Level management and completion tracking
```

### Input Reference
```
Player Controls:
- WASD/Arrows: Movement
- Space: Jump (variable height)
- Shift: Dash
- E: Interact with objects
- L: Manual clone creation
- R: Restart level
- F1: Debug information
```

### Events and Callbacks
```
Important Events:
- OnCloneCreated: When new clone spawns
- OnCloneStuck: When clone reaches goal
- OnGoalCompleted: When goal state changes
- OnLevelCompleted: When all goals achieved
```

This guide provides the foundation for creating engaging puzzle levels. Combine these systems creatively to build challenging, satisfying experiences that showcase the unique clone mechanics of **You Did This**.

For specific puzzle ideas and detailed room designs, see [PUZZLE_IDEAS.md](PUZZLE_IDEAS.md).