# You Did This

A puzzle platformer where players create "clones" to solve spatial and timing puzzles. Each clone repeats the player's previous actions, acting as both the key to progress and potentially an obstacle

🎮 A solo dev project by blink - showcasing reusable game systems and mechanics

---

## 📦 Unity Packages (Free to Use – MIT)

I want people to reuse the packages in this repo. All packages under /Packages are free to use in your own projects (commercial and non-commercial) under the MIT license. Attribution is appreciated but not required. See LICENSE.md for details.

- Minimum Unity for packages: 2022.3+
- Input System: Action Recording depends on com.unity.inputsystem >= 1.5.0

Install via Unity Package Manager (recommended)
- Unity → Window → Package Manager → + → Add package from git URL…
- Paste one of these URLs:

Action Recording System
- Git URL: https://github.com/DontBeBlink/You-Did-This.git?path=Packages/com.gmtk2025.action-recording#main
- Docs: Packages/com.gmtk2025.action-recording/README.md
- Notes: Physics-perfect recording/replay. Interfaces to adapt to any controller. Includes samples. Depends on Input System.

Interaction System
- Git URL: https://github.com/DontBeBlink/You-Did-This.git?path=Packages/com.gmtk2025.interaction-system#main
- Docs: Packages/com.gmtk2025.interaction-system/README.md
- Notes: Proximity detection, pickup/carry/throw, visual feedback, adapters, events.

Audio Management System
- Git URL: https://github.com/DontBeBlink/You-Did-This.git?path=Packages/com.gmtk2025.audio-management#main
- Docs: Packages/com.gmtk2025.audio-management/README.md
- Notes: Singleton audio manager, categories (SFX/Music/UI/Ambient/Voice), pooling, music fades.

Install via manifest.json (alternative)
Add entries to Packages/manifest.json dependencies:
```json
{
  "dependencies": {
    "com.gmtk2025.action-recording": "https://github.com/DontBeBlink/You-Did-This.git?path=Packages/com.gmtk2025.action-recording#main",
    "com.gmtk2025.interaction-system": "https://github.com/DontBeBlink/You-Did-This.git?path=Packages/com.gmtk2025.interaction-system#main",
    "com.gmtk2025.audio-management": "https://github.com/DontBeBlink/You-Did-This.git?path=Packages/com.gmtk2025.audio-management#main"
  }
}
```

Copy–paste (fastest, not versioned)
- Copy the specific package folder(s) from /Packages into your project (or copy scripts/prefabs you need).
- Follow each package's README for setup.

Support and questions
- If you hit issues integrating a package, open a GitHub Issue with your Unity version and a minimal repro.

License summary
- MIT for code in this repo. You can use, modify, and ship it. Attribution is appreciated but not required.
- Respect licenses of any third‑party assets if present.

---

## 🎯 Core Concept

Navigate through puzzle levels by strategically creating clones of yourself. Each clone replays your recorded actions, allowing you to:
- Hold down switches and pressure plates
- Create moving platforms with your past movements
- Coordinate timing-based puzzles
- Build complex solutions through layered actions

## ✨ Key Features

### 🤖 Advanced Clone System
- Physics-Perfect Replay: Clones reproduce exact movement, timing, and physics interactions
- Smart Action Recording: Captures movement, jumping, dashing, object interactions, and attack patterns
- Automatic & Manual Creation: Clones created every 15 seconds or manually with the L/F key (project-configurable)
- Goal-Based Sticking: Clones become permanent fixtures when reaching their designated goals

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
- Ghost Trail Effects: Visual trails that follow clone movement with state-based colors
- Particle Effects: Spawn, despawn, and ambient particle systems for enhanced visual feedback
- Glow Effects: Ambient lighting around active clones (URP 2D compatible)
- Enhanced Audio: Sound effects for clone lifecycle events and actions
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
- Unity 6000.2.0b9 or later for the demo project
- Packages themselves target Unity 2022.3+ (see above)
- Git for version control

### Setup Instructions
1) Clone the repository
```bash
git clone https://github.com/DontBeBlink/You-Did-This.git
cd You-Did-This
```

2) Open in Unity
- Launch Unity Hub
- Click "Open" and select the cloned project folder
- Unity will automatically import assets and configure the project

3) Test the Game
- Open any scene from Assets/Scenes/ (recommend starting with DemoScene.unity)
- Press Play to test core mechanics
- Use F1 to toggle debug information

### Quick Start Guide
For a comprehensive setup checklist and testing guide, see QUICK_START.md.

### Clone System Setup
The clone system auto-configures when you add a CloneManager component to any GameObject in your scene. See docs/technical/CLONE_SYSTEM_SETUP.md for detailed [...]

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

Packages/
├── com.gmtk2025.action-recording/     # UPM package: Action Recording System (MIT)
├── com.gmtk2025.interaction-system/   # UPM package: Interaction System (MIT)
└── com.gmtk2025.audio-management/     # UPM package: Audio Management System (MIT)
```

## 🎯 Game Design

The game follows a core loop of:
1. Explore the level and identify puzzle requirements
2. Record actions by moving through the level
3. Create Clone to replay those actions automatically
4. Coordinate between your current character and active clones
5. Reach Goals to make clones permanent puzzle elements
6. Iterate using retraction to undo and refine solutions

See docs/design/GDD_YouDidThis.md for the complete game design document.

## 🛠️ For Developers

### Key Systems Overview
- Clone Management: Handles creation, replay, and lifecycle of player clones
- Action Recording: Physics-accurate recording of all player inputs and states
- Goal System: Defines puzzle completion conditions and clone sticking behavior
- Interaction System: Object pickup, throwing, and environmental interactions
- Audio Management: Centralized sound effect and music management
- Level Management: Scene loading, progression, and game state management

### Contributing
This is a solo development project by blink, created as a fun project with potential for future reuse and to help other solo developers. The modular systems and packages are designed to be reusable in [...]

For technical questions or suggestions about the game systems, feel free to open GitHub Issues. The codebase serves as both a playable game and a reference implementation for clone-based mechanics.

### Level Design
- Level Design Guide: docs/design/LEVEL_DESIGN.md
- Puzzle Ideas: docs/design/PUZZLE_IDEAS.md
- Example Scene Template: docs/design/EXAMPLE_SCENE_TEMPLATE.md

### Technical Architecture
See docs/technical/ARCHITECTURE.md for details.

### Building the Game
See docs/technical/BUILD.md for platform-specific instructions.

## 📚 Documentation

All detailed documentation has been organized in the docs/ folder:
- Planning: docs/planning/
- Design: docs/design/
- Technical: docs/technical/

See the Documentation Index at docs/README.md.

## 📝 Development Status

Current Version: Solo Dev Showcase & System Demo

Implemented Features ✅
- Complete clone system with physics-perfect replay
- Full platformer movement system with dash mechanics
- Object interaction and throwing system
- Goal-based puzzle completion
- Audio management and feedback
- Debug tools and visualization
- Multiple test levels and scenes
- Modular packages for reuse in other projects

Potential Extensions 🚀
- Leaderboards for optimization challenges (fewest clones, fastest time)
- Advanced time-based mechanics and synchronization puzzles
- Secret collectibles requiring creative clone coordination
- Level editor for community-created content
- Integration with other game genres (puzzle-RPG, strategy games)

## 📞 Support & Questions

- Issues: Report bugs or ask technical questions via GitHub Issues
- Code Reference: This project serves as a reference for clone-based game mechanics
- Solo Dev Project: Created by blink as a fun project and system showcase

## 📄 License

This project is licensed under the terms specified in LICENSE.md.
- Package reuse: You are welcome to use the systems in your own projects under this license. Attribution is appreciated but not required.
- Third-party assets: If any third-party assets are included, please respect their individual licenses.

---

Made with ❤️ by blink - A solo dev showcase of reusable game systems
