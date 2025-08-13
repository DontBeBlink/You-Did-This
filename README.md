# You Did This

A puzzle platformer where players create "clones" to solve spatial and timing puzzles. Each clone repeats the player's previous actions, acting as both the key to progress and potentially an obstacle to avoid.

**🎮 A solo dev project by blink - showcasing reusable game systems and mechanics**

## 🎯 Core Concept

Navigate through puzzle levels by strategically creating clones of yourself. Each clone replays your recorded actions, allowing you to:
- Hold down switches and pressure plates
- Create moving platforms with your past movements
- Coordinate timing-based puzzles
- Build complex solutions through layered actions

## ✨ Key Features

### 🤖 Advanced Clone System
- **Physics-Perfect Replay**: Clones reproduce exact movement, timing, and physics interactions
- **Smart Action Recording**: Captures movement, jumping, dashing, object interactions, and attack patterns
- **Automatic & Manual Creation**: Clones created every 15 seconds or manually with the L key
- **Goal-Based Sticking**: Clones become permanent fixtures when reaching their designated goals

### 🎮 Refined Platformer Controls
- Responsive movement with variable jump height
- Dash mechanics for precise navigation
- Object pickup and throwing system
- Checkpoint-based respawn system

### 🧩 Puzzle Mechanics
- Pressure plates requiring clone coordination
- Timing-based obstacles and platforms
- Multi-step puzzles requiring strategic clone placement
- Goals that transform clones into static puzzle elements

### 🎨 Visual & Audio Design
- Distinct visual feedback for active vs. stuck clones
- **Ghost Trail Effects**: Visual trails that follow clone movement with state-based colors
- **Particle Effects**: Spawn, despawn, and ambient particle systems for enhanced visual feedback
- **Glow Effects**: Ambient lighting around active clones (URP 2D compatible)
- **Enhanced Audio**: Sound effects for clone lifecycle events and actions
- Color-coded goal system (yellow incomplete, green complete)
- Audio cues for clone creation and goal completion
- Debug visualization for development and testing

## 🎮 Controls

| Action | Key/Input | Description |
|--------|-----------|-------------|
| Move | A/D or Arrow Keys | Left/Right movement |
| Jump | Spacebar | Jump (variable height based on hold duration) |
| Dash | Shift | Quick directional dash |
| Interact | E | Pick up objects, activate switches |
| Throw | Left Click | Throw picked up objects |
| Create Clone | F | Manually create a clone (if enabled) |
| Retract Clone | Z | Remove last clone and move to its position |
| Pause | Escape | Pause/Resume game |
| Restart | R | Restart current level |
| Debug Info | F1 | Toggle debug information display |

## 🚀 Quick Start

### Prerequisites
- Unity 6000.2.0b9 or later
- Git for version control

### Setup Instructions
1. **Clone the repository**
   ```bash
   git clone https://github.com/DontBeBlink/GMTK2025.git
   cd GMTK2025
   ```

2. **Open in Unity**
   - Launch Unity Hub
   - Click "Open" and select the cloned project folder
   - Unity will automatically import assets and configure the project

3. **Test the Game**
   - Open any scene from `Assets/Scenes/` (recommend starting with `DemoScene.unity`)
   - Press Play to test core mechanics
   - Use F1 to toggle debug information

### Quick Start Guide
For a comprehensive setup checklist and testing guide, see [QUICK_START.md](QUICK_START.md).

### Clone System Setup
The clone system auto-configures when you add a `CloneManager` component to any GameObject in your scene. See [docs/technical/CLONE_SYSTEM_SETUP.md](docs/technical/CLONE_SYSTEM_SETUP.md) for detailed configuration options.

## 📁 Project Structure

```
Assets/
├── Scenes/           # Game levels and test scenes
├── Scripts/          # Core game logic (34+ C# scripts)
│   ├── CloneManager.cs      # Main clone system controller
│   ├── ActionRecorder.cs    # Records player actions for replay
│   ├── Clone.cs             # Individual clone behavior
│   ├── PlayerController.cs  # Player input and movement
│   ├── Goal.cs              # Puzzle completion triggers
│   └── ...                  # Additional game systems
├── Prefabs/          # Reusable game objects
├── Materials/        # Visual materials and shaders  
├── Sprites/          # 2D artwork and textures
├── SFX/              # Audio clips and sound effects
└── Input/            # Input System configuration
```

## 🎯 Game Design

The game follows a core loop of:
1. **Explore** the level and identify puzzle requirements
2. **Record** actions by moving through the level
3. **Create Clone** to replay those actions automatically  
4. **Coordinate** between your current character and active clones
5. **Reach Goals** to make clones permanent puzzle elements
6. **Iterate** using retraction to undo and refine solutions

See [docs/design/GDD_YouDidThis.md](docs/design/GDD_YouDidThis.md) for the complete game design document.

## 🛠️ For Developers

### Key Systems Overview
- **Clone Management**: Handles creation, replay, and lifecycle of player clones
- **Action Recording**: Physics-accurate recording of all player inputs and states
- **Goal System**: Defines puzzle completion conditions and clone sticking behavior
- **Interaction System**: Object pickup, throwing, and environmental interactions
- **Audio Management**: Centralized sound effect and music management
- **Level Management**: Scene loading, progression, and game state management

### Contributing
This is a solo development project by blink, created as a fun project with potential for future reuse and to help other solo developers. The modular systems and packages are designed to be reusable in other games.

For technical questions or suggestions about the game systems, feel free to open GitHub Issues. The codebase serves as both a playable game and a reference implementation for clone-based mechanics.

### Level Design
For developers interested in learning from or building upon these systems:
- **[Level Design Guide](docs/design/LEVEL_DESIGN.md)**: Comprehensive guide to creating levels using the existing systems
- **[Puzzle Ideas](docs/design/PUZZLE_IDEAS.md)**: Detailed puzzle concepts, room layouts, and implementation plans
- **[Example Scene Template](docs/design/EXAMPLE_SCENE_TEMPLATE.md)**: Step-by-step tutorial for creating a basic puzzle scene
- All guides cover the trigger system, logic gates, goals, and step-by-step creation workflows

### Technical Architecture
For detailed technical documentation about the game's systems and architecture, see [docs/technical/ARCHITECTURE.md](docs/technical/ARCHITECTURE.md).

### Building the Game
To create builds for distribution or testing, see our [docs/technical/BUILD.md](docs/technical/BUILD.md) guide with platform-specific instructions.

## 📚 Documentation

All detailed documentation has been organized in the [`docs/`](docs/) folder:

- **📋 [Planning](docs/planning/)** - System extraction and project planning documents
- **🎮 [Design](docs/design/)** - Game design and level creation guides  
- **🔧 [Technical](docs/technical/)** - Technical implementation and setup guides

See the [Documentation Index](docs/README.md) for a complete overview.

## 📝 Development Status

**Current Version**: Solo Dev Showcase & System Demo

### Implemented Features ✅
- Complete clone system with physics-perfect replay
- Full platformer movement system with dash mechanics
- Object interaction and throwing system  
- Goal-based puzzle completion
- Audio management and feedback
- Debug tools and visualization
- Multiple test levels and scenes
- Modular packages for reuse in other projects

### Potential Extensions 🚀
These systems could be extended for future games or projects:
- Leaderboards for optimization challenges (fewest clones, fastest time)
- Advanced time-based mechanics and synchronization puzzles
- Secret collectibles requiring creative clone coordination
- Level editor for community-created content
- Integration with other game genres (puzzle-RPG, strategy games)

## 📞 Support & Questions

- **Issues**: Report bugs or ask technical questions via GitHub Issues
- **Code Reference**: This project serves as a reference for clone-based game mechanics
- **Solo Dev Project**: Created by blink as a fun project and system showcase

## 📄 License

This project is licensed under the terms specified in [LICENSE.md](LICENSE.md).

---

**Made with ❤️ by blink - A solo dev showcase of reusable game systems**
