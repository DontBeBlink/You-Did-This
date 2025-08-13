# Basic Audio Management Example

This sample demonstrates how to set up and use the Audio Management System in your Unity project.

## Setup Instructions

1. **Import the Sample**: Import this sample from the Package Manager
2. **Create Audio Manager**: Add the AudioManager component to a GameObject in your scene
3. **Configure Audio Clips**: Assign audio clips to the AudioManager in the inspector
4. **Test the System**: Use the example scripts to test audio playback

## What's Included

### Scripts
- `AudioExample.cs`: Demonstrates basic audio playback features
- `VolumeControlUI.cs`: Example UI for volume controls
- `RandomSoundTest.cs`: Shows how to use RandomAudioPlayer

## How to Use

### Basic Setup
1. Create an empty GameObject named "AudioManager"
2. Add the `AudioManager` component from GMTK2025.AudioManagement
3. Assign your audio clips to the appropriate slots in the inspector
4. Configure category settings (volume, 3D audio, etc.)

### Example Usage
```csharp
using GMTK2025.AudioManagement;

// Play a simple sound
AudioManager.Instance.PlaySound(myClip);

// Play with category and volume
AudioManager.Instance.PlaySound(myClip, AudioCategory.SFX, 0.8f);

// Play positioned 3D audio
AudioManager.Instance.PlaySoundAtPosition(myClip, transform.position);

// Music management
AudioManager.Instance.PlayMusic(backgroundMusic, fadeInDuration: 2f);
AudioManager.Instance.StopMusic(fadeOutDuration: 1f);
```

### Volume Controls
```csharp
// Master volume (affects all categories)
AudioManager.Instance.SetMasterVolume(0.8f);

// Category-specific volume
AudioManager.Instance.SetCategoryVolume(AudioCategory.Music, 0.6f);
AudioManager.Instance.SetCategoryVolume(AudioCategory.SFX, 0.9f);

// Mute controls
AudioManager.Instance.SetMasterMute(true);
AudioManager.Instance.SetCategoryMute(AudioCategory.Voice, true);
```

### UI Integration
The example includes a simple UI demonstrating:
- Master volume slider
- Per-category volume sliders
- Mute toggle buttons
- Play sound test buttons

### Random Audio
For varied sound effects:
```csharp
// Set up random audio collection
[SerializeField] private RandomAudioPlayer footsteps;

// Play random footstep sound
footsteps.PlayRandom();
```

## Key Features Demonstrated

### Audio Categories
- **SFX**: Sound effects like jumps, impacts, etc.
- **Music**: Background music with fade controls
- **UI**: Interface sounds like button clicks
- **Ambient**: Environmental/atmospheric sounds
- **Voice**: Dialogue and voice audio

### 3D Audio
- Position-based audio playback
- Configurable spatial blend
- Distance-based volume falloff
- Rolloff curve configuration

### Performance Features
- Audio source pooling for efficiency
- Automatic cleanup of finished sounds
- Memory-optimized playback system

## Customization Tips

### Adding Custom Categories
Extend the AudioCategory enum for project-specific needs:
```csharp
public enum MyAudioCategory
{
    SFX = 0,
    Music = 1,
    UI = 2,
    Ambient = 3,
    Voice = 4,
    Weapons = 5,    // Custom
    Magic = 6       // Custom
}
```

### Integration with Game Events
```csharp
// Subscribe to player events
void OnPlayerJump() => AudioManager.Instance.PlayJumpSound();
void OnPlayerDash() => AudioManager.Instance.PlaySound(dashSound, AudioCategory.SFX);
void OnEnemyDefeated() => AudioManager.Instance.PlaySound(victorySound, AudioCategory.SFX, 1.2f);
```

### Save/Load Audio Settings
```csharp
// Save audio settings to PlayerPrefs
PlayerPrefs.SetFloat("MasterVolume", AudioManager.Instance.GetMasterVolume());
PlayerPrefs.SetFloat("MusicVolume", AudioManager.Instance.GetCategoryVolume(AudioCategory.Music));

// Load audio settings
AudioManager.Instance.SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1f));
AudioManager.Instance.SetCategoryVolume(AudioCategory.Music, PlayerPrefs.GetFloat("MusicVolume", 1f));
```

## Troubleshooting

### Common Issues
- **No sound playing**: Check that AudioManager.Instance is not null and master volume > 0
- **Low volume**: Verify both master volume and category volume settings
- **3D audio not working**: Ensure Audio Listener is present in scene and 3D settings are configured
- **Audio cuts off**: Increase audio source pool size in AudioManager settings

### Debug Tips
- Enable debug logging to see audio playback information
- Check Unity's Audio Mixer window for mixer group assignments
- Verify audio clip import settings (format, quality, etc.)
- Use Unity's Audio Preview in Project window to test clips

## Next Steps

- Explore the full API in the main README
- Set up Audio Mixer groups for advanced audio processing
- Create custom audio triggers for game-specific events
- Implement save/load functionality for audio preferences
- Add audio visualization or level meters for enhanced feedback