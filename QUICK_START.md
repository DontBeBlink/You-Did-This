# Quick Start Checklist

Use this checklist to quickly set up and test the **You Did This** project:

## ✅ Initial Setup

### Prerequisites
- [ ] Unity 6000.2.0b9 or later installed
- [ ] Git installed and configured
- [ ] Unity Hub (recommended for version management)

### Repository Setup
- [ ] Fork the repository on GitHub (if contributing)
- [ ] Clone repository: `git clone https://github.com/DontBeBlink/GMTK2025.git`
- [ ] Open project in Unity Hub (Add → Select cloned folder)
- [ ] Wait for Unity to import assets (may take 2-3 minutes)

### First Test
- [ ] Open `Assets/Scenes/DemoScene.unity`
- [ ] Press Play button in Unity
- [ ] Test basic movement (WASD/Arrow keys, Spacebar to jump)
- [ ] Press **L** to create a manual clone
- [ ] Press **F1** to toggle debug information
- [ ] Verify clone replays your actions

## 🎮 Core Functionality Test

### Movement Testing
- [ ] Player moves left/right with A/D or arrow keys
- [ ] Player jumps with Spacebar (variable height based on hold time)
- [ ] Player can dash with Shift key
- [ ] Player can interact with E key (if objects present)

### Clone System Testing
- [ ] Manual clone creation works (L key)
- [ ] Clone appears and starts replaying actions
- [ ] Debug info shows clone count (F1 key)
- [ ] Automatic clone creation after 15 seconds
- [ ] Clone spawn position at CloneManager location

### Goal System Testing (if goals present in scene)
- [ ] Clone reaches goal and becomes "stuck"
- [ ] Goal color changes from yellow to green
- [ ] Audio feedback plays on goal completion
- [ ] Stuck clone stops moving and stays at goal

## 🔧 Development Setup

### IDE Configuration
- [ ] Install Visual Studio Tools for Unity extension (if using VS Code)
- [ ] Verify IntelliSense works for Unity scripts
- [ ] Test debugging capabilities (breakpoints work)

### Unity Configuration
- [ ] Verify Input System package is installed (Window → Package Manager)
- [ ] Check that URP (Universal Render Pipeline) is active
- [ ] Confirm 2D Sprite package is installed
- [ ] Test that scenes load without errors

### Performance Baseline
- [ ] Game runs at stable 60 FPS in empty scene
- [ ] No console errors in clean project state
- [ ] Memory usage stable with no leaks in simple tests
- [ ] Multiple clones (3-5) maintain performance

## 🧪 Advanced Testing

### Multi-Clone Scenarios
- [ ] Create 5+ clones and verify performance remains stable
- [ ] Test maximum clone limit (10 clones) - oldest should be removed
- [ ] Verify all clones replay actions correctly
- [ ] Test clone creation while other clones are active

### System Integration
- [ ] Pause/Resume works (Escape key)
- [ ] Level restart works (R key)  
- [ ] Debug information accurate (F1 key)
- [ ] Audio feedback working for all actions

### Edge Cases
- [ ] Clone creation with no recorded actions
- [ ] Multiple clones reaching goals simultaneously
- [ ] Rapid clone creation (spam L key)
- [ ] Scene reload with active clones

## 🚨 Troubleshooting

### Common Issues & Solutions

**"No PlayerController found in scene"**
- Solution: Ensure scene has GameObject with PlayerController component

**Clone not appearing after pressing L**
- Check console for errors
- Verify CloneManager exists in scene
- Confirm manual looping is enabled in CloneManager settings

**Clone not replaying actions correctly**
- Verify ActionRecorder component is present on player
- Check that recording started (debug info shows active recording)
- Ensure no console errors during action recording

**Performance issues with multiple clones**
- Monitor with Unity Profiler (Window → Analysis → Profiler)
- Check clone count in debug mode (F1)
- Verify frame rate in Statistics window

**Audio not playing**
- Check AudioManager exists in scene
- Verify audio clips assigned in CloneManager
- Test system volume and Unity audio settings

### Debug Information Guide
When F1 debug mode is active, you should see:
- **Clone Count**: Current/Maximum clones
- **Loop Timer**: Time until next automatic clone
- **Recording Status**: Whether actions are being recorded
- **System Performance**: Frame rate and memory indicators

## 📋 Development Workflow

### Making Changes
1. **Create feature branch**: `git checkout -b feature/your-feature`
2. **Test changes**: Follow this checklist after modifications
3. **Verify no regressions**: Ensure existing features still work
4. **Performance check**: Test with multiple clones active
5. **Documentation**: Update relevant docs if needed

### Before Committing
- [ ] No console errors or warnings
- [ ] All basic functionality tests pass
- [ ] Performance remains stable
- [ ] New features work with clone system
- [ ] Debug mode shows accurate information

### Level Design Testing
If creating new levels:
- [ ] CloneManager placed at desired spawn point
- [ ] Goals have proper colliders (Trigger enabled)
- [ ] Puzzle solvable with intended clone count
- [ ] Visual feedback clear for all interactive elements
- [ ] Performance good with maximum expected clones

---

**Need Help?** Check the [CONTRIBUTING.md](CONTRIBUTING.md) guide or create an issue on GitHub.

**Ready to Contribute?** See our [contribution guidelines](CONTRIBUTING.md) for code standards and submission process.