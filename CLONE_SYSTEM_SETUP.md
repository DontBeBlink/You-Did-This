# Clone Loop System Setup Guide

## Quick Setup
1. Add a **CloneManager** component to any GameObject in your scene
2. The CloneManager will automatically:
   - Find the PlayerController in the scene
   - Add an ActionRecorder component to the player
   - Start recording and looping every 15 seconds

## Configuration Options

### CloneManager Settings
- **Loop Duration**: Time between automatic loop creation (default: 15 seconds)
- **Max Clones**: Maximum number of clones allowed (default: 10)
- **Auto Start First Loop**: Whether to start recording immediately
- **Enable Manual Looping**: Allow manual loop triggering with L key
- **Clone Prefab**: Optional prefab to use for clones (if null, duplicates player)
- **Clone Create Sound**: Audio clip to play when creating clones

### ActionRecorder Settings
- **Recording Interval**: How often to record actions (default: 0.05s / 50ms)
- **Max Recording Time**: Maximum length of a single recording (default: 30s)

## Testing
- Press **L** key to manually trigger a loop creation (if enabled)
- Use **F1** key to toggle debug information display
- Debug info shows clone count, loop timer, and system status

## Integration Notes
- Clones automatically disable PlayerController and ActionRecorder components
- Clones will become "stuck" when they reach Goal objects
- The camera will continue following the original player (not clones)
- All player actions (movement, jump, dash, interact, attack) are recorded and replayed

## Troubleshooting
- Ensure your scene has a PlayerController component
- Ensure Goal objects have Collider2D with "Is Trigger" enabled
- Check that CloneManager is in the scene and properly configured
- Use debug mode (F1) to monitor system status