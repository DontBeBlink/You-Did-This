# Clone Loop System Setup Guide

## Quick Setup
1. Add a **CloneManager** component to any GameObject in your scene
2. The CloneManager will automatically:
   - Find the PlayerController in the scene
   - Add an ActionRecorder component to the player
   - Start recording and looping every 15 seconds
   - Set player spawn point to CloneManager's position

## Configuration Options

### CloneManager Settings
- **Loop Duration**: Time between automatic loop creation (default: 15 seconds)
- **Max Clones**: Maximum number of clones allowed (default: 10)
- **Auto Start First Loop**: Whether to start recording immediately
- **Enable Manual Looping**: Allow manual loop triggering with L key
- **Manual Loop Key**: Configurable key for manual clone creation (default: L)
- **Clone Prefab**: Optional prefab to use for clones (if null, duplicates player)
- **Clone Create Sound**: Audio clip to play when creating clones

### ActionRecorder Settings
- **Recording Interval**: How often to record actions (default: 0.02s / 50Hz)
- **Max Recording Time**: Maximum length of a single recording (default: 30s)

**Note**: Recording happens in FixedUpdate for physics-perfect replay timing

## Controls & Usage

### Player Controls
| Input | Action | Description |
|-------|--------|-------------|
| **L** | Manual Clone | Create clone immediately (if manual looping enabled) |
| **F1** | Debug Toggle | Show/hide debug information overlay |
| **ESC** | Pause | Pause/resume game |
| **R** | Restart | Restart current level |

### Debug Information
When enabled with F1 key, shows:
- Current clone count / maximum clones
- Time until next automatic clone creation
- Clone system status and performance metrics
- Individual clone states (active, stuck, replaying)

## Technical Integration

### Automatic Setup Process
1. **Scene Initialization**: CloneManager finds PlayerController automatically
2. **Component Addition**: ActionRecorder added to player if missing
3. **Event Subscription**: Systems connect through event-driven architecture
4. **Recording Start**: Action recording begins immediately if auto-start enabled

### Clone Lifecycle
```
Recording Phase → Clone Creation → Replay Phase → Goal Completion (Stuck)
     ↓               ↓              ↓                    ↓
Player Actions → Action List → Physics Replay → Static Object
```

### Integration with Game Systems

#### Goal System
- Goals detect clone collision and trigger sticking behavior
- Visual feedback changes from yellow (incomplete) to green (complete)
- Audio feedback plays when goals are reached
- Clone becomes immobile and acts as permanent puzzle element

#### Audio System
- Clone creation sounds played through AudioManager
- Goal completion sounds triggered automatically
- Centralized audio management for consistent experience

#### Physics Integration
- Clones record and replay exact physics state (position, velocity, forces)
- 50Hz recording rate matches Unity's FixedUpdate frequency
- Perfect synchronization between player actions and clone replay

## Advanced Configuration

### Custom Clone Prefabs
If you need specialized clone behavior:
1. Create prefab with Clone component
2. Assign to CloneManager's Clone Prefab field
3. Clone will use prefab instead of duplicating player object

### Event System Integration
Connect your systems to clone events:
```csharp
// Subscribe to clone lifecycle events
CloneManager.OnLoopStarted += HandleLoopStart;
CloneManager.OnLoopEnded += HandleLoopEnd;
cloneManager.OnCloneCreated += HandleCloneCreated;
cloneManager.OnCloneStuck += HandleCloneStuck;
```

### Performance Optimization
- **Automatic Cleanup**: Oldest clones removed when exceeding max count
- **Memory Management**: Recorded actions cleared when no longer needed
- **Update Optimization**: Stuck clones stop physics updates
- **LOD Consideration**: Future feature for distant clone optimization

## Troubleshooting

### Common Issues
1. **"No PlayerController found"**: Ensure scene has GameObject with PlayerController component
2. **Clones not appearing**: Check console for errors, verify CloneManager is active
3. **Recording not starting**: Ensure ActionRecorder is enabled and auto-start is true
4. **Physics desync**: Verify recording interval matches FixedUpdate timing

### Debug Checklist
- [ ] CloneManager component exists and is active in scene
- [ ] PlayerController component found and functional
- [ ] ActionRecorder component added to player (automatic)
- [ ] Goal objects have Collider2D with "Is Trigger" enabled
- [ ] Clone count within configured maximum
- [ ] No console errors or warnings

### Performance Monitoring
Use F1 debug mode to monitor:
- **Frame Rate**: Should maintain 60 FPS with multiple clones
- **Clone Count**: Watch for unexpected clone accumulation
- **Memory Usage**: Use Unity Profiler for detailed analysis
- **Physics Performance**: Monitor with multiple active clones

## Best Practices

### Level Design
- Place CloneManager at desired player spawn point
- Test puzzles with maximum clone count (10)
- Ensure goals are clearly visible and accessible
- Design for multiple solution paths using different clone counts

### Code Integration
- Use event system for loose coupling between systems
- Record custom actions by extending PlayerAction struct
- Implement replay logic in Clone.ExecuteAction() for new mechanics
- Follow physics-based timing for consistent behavior

### Development Workflow
1. **Scene Setup**: Add CloneManager and test basic functionality
2. **Puzzle Design**: Create goals and test clone interactions
3. **Iteration**: Use manual clone creation (L key) for rapid testing
4. **Polish**: Add visual and audio feedback for player actions
5. **Performance**: Test with multiple clones and optimize as needed