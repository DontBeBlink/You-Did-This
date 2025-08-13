# Game Design Document: You Did This

## Core Concept
A puzzle platformer where players create "clones" to solve spatial and timing puzzles. Each clone repeats the player’s previous actions, and is sometimes the key to progress or an obstacle to avoid.

## Main Mechanic ✅ **IMPLEMENTED**
- **Clone Creation**: Players can create clones manually (L key) or automatically every 15 seconds
- **Action Recording**: Every player input and physics state is recorded at 50Hz for accurate replay  
- **Physics-Perfect Replay**: Clones reproduce exact movement, timing, and interactions
- **Goal-Based Sticking**: Clones become permanently stuck when reaching their designated goals
- **Clone Management**: Up to 10 clones supported with automatic cleanup of oldest clones
- **Retraction System**: Planned feature for returning to previous clone states

## Gameplay Flow
1. Start as a single character.
2. Navigate platforms, switches, and obstacles.
3. When needed, create a clone:
    - Old clone replays your recorded actions on a loop (or just continues to the goal).
    - You take control of the new clone to solve the next part of the puzzle.
4. Reach a goal with a clone:
    - That clone becomes stuck and acts as a static object (e.g., holding a switch down).
    - You cannot retract to clones that have reached their goal.
5. Use retraction to undo mistakes:
    - Erase your current clone and return to the previous one (unless it’s stuck).
    - Useful for experimenting and iterating on solutions.

## Puzzle Elements
- Pressure plates/switches that must be held down.
- Doors or platforms activated by stuck clones.
- Obstacles requiring precise timing between clones.
- Hazards that require coordination between active and looping clones.

## Win/Lose Conditions
- Win: Get all required clones to their goal locations.
- Lose: All clones are stuck or destroyed, and you cannot progress.

## Visual/Audio Style
- Minimalist, clear silhouettes for clones.
- Distinct colors or effects to indicate stuck/active/looping clones.
- Subtle audio cues when creating, retracting, or sticking a clone.

## Title
**You Did This**

---

### Optional Extensions
- Leaderboards for fewest clones used or fastest completion.
- Secret collectibles requiring clever clone use.
- Advanced puzzles with time-based mechanics.

---

## 🚀 Implementation Status (Solo Dev Showcase)

### ✅ Fully Implemented Features

**Core Clone System**
- Physics-perfect action recording at 50Hz
- Automatic clone creation every 15 seconds
- Manual clone creation with L key
- Up to 10 simultaneous clones with automatic cleanup
- Goal-based clone sticking with visual/audio feedback

**Player Controls**
- Complete 2D platformer movement (walk, jump, dash)
- Object interaction and pickup system
- Unity Input System integration
- Variable jump height based on hold duration

**Puzzle Mechanics**
- Goal system with visual feedback (yellow → green)
- Multiple clone coordination
- Trigger and switch systems
- Checkpoint and respawn functionality

**Polish & Feedback**
- Audio management with clone creation sounds
- Debug information display (F1 key)
- Visual clone differentiation (color, transparency)
- Scene management and level progression

### 🚧 Potential Extensions for Future Projects

**Quality of Life**
- Clone retraction system for undoing mistakes
- Enhanced visual effects for clone actions
- Improved level progression system

**Advanced Features**
- Time-based synchronization puzzles
- Leaderboards for optimization challenges
- Level editor for community content
- Additional puzzle mechanics and elements
- Integration with other game genres

### 🔧 Technical Architecture

**Key Systems Implemented**
- CloneManager: Central clone lifecycle management
- ActionRecorder: High-fidelity input and physics recording
- Goal: Puzzle completion and clone state management
- GameManager: Scene and game state coordination
- AudioManager: Centralized sound effect management

For detailed technical documentation, see [ARCHITECTURE.md](ARCHITECTURE.md).
