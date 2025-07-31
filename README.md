This is a game project for the GMTK Game Jam 2025

## Clone Loop System

This project features an automatic clone loop system where:
- Every 15 seconds, the player automatically creates a new "loop"
- The previous version of the player becomes a clone that repeats all recorded actions
- Clones loop their actions continuously until they reach a goal
- When a clone reaches a goal, it becomes "stuck" and stops moving
- The system supports up to 10 clones maximum

### Technical Implementation
- **ActionRecorder.cs**: Records player inputs every 50ms
- **CloneManager.cs**: Manages automatic loop creation and clone lifecycle
- **Clone.cs**: Handles action replay and goal interaction for individual clones
- Integration with existing Goal, UI, and Audio systems
