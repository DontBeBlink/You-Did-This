# Audio Management System

A comprehensive singleton-based audio management system for Unity, originally developed for GMTK2025. This system provides centralized audio control with category-based organization, volume management, and advanced audio features.

## 🎯 Features

- **Singleton Pattern**: Global audio access from anywhere in your project
- **Category-Based Organization**: Separate controls for SFX, Music, UI, Ambient, and Voice
- **Per-Category Volume Control**: Individual volume and mute settings for each category
- **Audio Pooling**: Performance-optimized audio source management
- **2D and 3D Audio Support**: Automatic spatial audio configuration
- **Music Management**: Fade in/out support and seamless music transitions
- **Cross-Scene Persistence**: Audio manager persists across scene loads
- **Event System**: Audio events for integration with other systems
- **Zero Dependencies**: Pure Unity implementation with no external requirements

## 🚀 Quick Start

### 1. Install the Package

Add the package to your Unity project through the Package Manager or by adding this line to your `manifest.json`:

```json
{
  "dependencies": {
    "com.gmtk2025.audio-management": "1.0.0"
  }
}
```

### 2. Set Up the Audio Manager

1. Create an empty GameObject in your first scene
2. Add the `AudioManager` component
3. Configure your audio clips and settings in the inspector
4. The AudioManager will persist across all scenes automatically

### 3. Play Sounds

```csharp
using GMTK2025.AudioManagement;

// Play a simple sound effect
AudioManager.Instance.PlaySound(myAudioClip);

// Play sound with specific category and volume
AudioManager.Instance.PlaySound(myAudioClip, AudioCategory.UI, 0.8f);

// Play 3D positioned audio
AudioManager.Instance.PlaySoundAtPosition(myAudioClip, transform.position);
```

## 📋 Core Components

### AudioManager
The main singleton that manages all audio playback, categories, and settings.

### AudioCategory Enum
- `SFX`: Sound effects
- `Music`: Background music  
- `UI`: User interface sounds
- `Ambient`: Environmental/atmospheric sounds
- `Voice`: Dialogue and voice sounds

### AudioTrigger
Component for event-driven audio playback (UI buttons, collisions, etc.).

### RandomAudioPlayer
Plays randomized audio clips with variation for repetitive sounds.

## 🎨 Category-Based Audio Management

Each audio category has its own settings:

```csharp
// Set volume for specific categories
AudioManager.Instance.SetCategoryVolume(AudioCategory.Music, 0.7f);
AudioManager.Instance.SetCategoryVolume(AudioCategory.SFX, 0.9f);

// Mute categories independently
AudioManager.Instance.SetCategoryMute(AudioCategory.Voice, true);

// Master volume controls all categories
AudioManager.Instance.SetMasterVolume(0.8f);
```

## 🎵 Music Management

### Basic Music Playback
```csharp
// Play music with loop
AudioManager.Instance.PlayMusic(backgroundMusic, loop: true);

// Play music with fade-in
AudioManager.Instance.PlayMusic(backgroundMusic, fadeInDuration: 2f);

// Stop music with fade-out
AudioManager.Instance.StopMusic(fadeOutDuration: 1.5f);
```

### Advanced Music Control
```csharp
// The AudioManager handles music transitions automatically
AudioManager.Instance.PlayMusic(newTrack, fadeInDuration: 3f);
// Previous music will fade out while new music fades in
```

## 🔊 Audio Effects and Variations

### Random Audio Clips
```csharp
// Set up random audio collection
[SerializeField] private RandomAudioClip footstepSounds;

// Play with variation
footstepSounds.PlayRandom(AudioCategory.SFX, transform.position);

// Play with pitch variation for more variety
footstepSounds.PlayRandomWithPitch(AudioCategory.SFX, transform.position);
```

### Audio Triggers
```csharp
// Add AudioTrigger component to any GameObject
// Configure in inspector or via code:
AudioTrigger trigger = GetComponent<AudioTrigger>();
trigger.PlayAudio(); // Plays configured sound
trigger.PlayAudio(customClip); // Plays custom sound with trigger settings
```

## ⚙️ Configuration

### Category Settings
Each category can be configured with:
- **Volume**: Base volume level (0-1)
- **Muted**: Category mute state
- **Mixer Group**: Audio mixer group assignment
- **3D Audio**: Enable spatial audio
- **Spatial Blend**: 2D to 3D audio blend
- **Distance Settings**: Min/max distance for 3D audio
- **Rolloff Mode**: How audio fades with distance

### Performance Settings
- **Audio Source Pool Size**: Number of pooled audio sources
- **Master Volume**: Global volume multiplier
- **Master Mute**: Global mute toggle

## 🔌 Events and Integration

### Audio Events
```csharp
// Subscribe to audio events
AudioManager.OnAudioPlayed += (clip, category) => {
    Debug.Log($"Played {clip.name} in category {category}");
};

AudioManager.OnMasterVolumeChanged += (volume) => {
    Debug.Log($"Master volume changed to {volume}");
};

AudioManager.OnCategoryVolumeChanged += (category, volume) => {
    Debug.Log($"{category} volume changed to {volume}");
};
```

### Integration with UI
```csharp
// Slider integration for volume controls
public void OnMasterVolumeSlider(float value)
{
    AudioManager.Instance.SetMasterVolume(value);
}

public void OnMusicVolumeSlider(float value)
{
    AudioManager.Instance.SetCategoryVolume(AudioCategory.Music, value);
}
```

## 💡 Usage Examples

### Game Events
```csharp
// Player actions
AudioManager.Instance.PlayJumpSound();        // Predefined jump sound
AudioManager.Instance.PlayGoalReachedSound(); // Predefined goal sound

// Custom game events
AudioManager.Instance.PlaySound(explosionSound, AudioCategory.SFX, 1.2f);
AudioManager.Instance.PlaySoundAtPosition(impactSound, hitPosition, AudioCategory.SFX);
```

### UI Sounds
```csharp
// Button clicks
AudioManager.Instance.PlaySound(buttonClickSound, AudioCategory.UI);

// Menu transitions
AudioManager.Instance.PlaySound(whooshSound, AudioCategory.UI, 0.8f);
```

### Environmental Audio
```csharp
// Ambient loops
AudioSource windLoop = AudioManager.Instance.PlayLoopingSound(windSound, AudioCategory.Ambient);

// Stop looping sounds
AudioManager.Instance.StopLoopingSound(windLoop);
```

## 🔧 Advanced Features

### 3D Audio Setup
```csharp
// Configure category for 3D audio
var sfxSettings = AudioManager.Instance.GetCategorySettings(AudioCategory.SFX);
sfxSettings.use3D = true;
sfxSettings.spatialBlend = 1f;
sfxSettings.minDistance = 1f;
sfxSettings.maxDistance = 50f;
sfxSettings.rolloffMode = AudioRolloffMode.Logarithmic;
```

### Audio Mixer Integration
```csharp
// Assign mixer groups to categories in the AudioManager inspector
// or via code:
var musicSettings = AudioManager.Instance.GetCategorySettings(AudioCategory.Music);
musicSettings.mixerGroup = myMusicMixerGroup;
```

### Custom Audio Categories
While the system comes with 5 predefined categories, you can extend it:

```csharp
// Extend the AudioCategory enum in your project
public enum CustomAudioCategory
{
    SFX = 0,
    Music = 1,
    UI = 2,
    Ambient = 3,
    Voice = 4,
    Weapons = 5,    // Custom category
    Magic = 6       // Custom category
}
```

## 📊 Performance Considerations

- **Audio Source Pooling**: Reuses AudioSource components for better performance
- **Automatic Cleanup**: Pooled sources are returned automatically when sounds finish
- **Memory Management**: No memory leaks from audio source creation
- **Configurable Pool Size**: Adjust pool size based on your game's needs

## 🎮 Integration with Original GMTK2025 Systems

The audio manager includes predefined methods that match the original GMTK2025 AudioManager:

```csharp
// Direct compatibility with original system
AudioManager.Instance.PlayJumpSound();
AudioManager.Instance.PlayCloneCreateSound();
AudioManager.Instance.PlayCloneRetractSound();
AudioManager.Instance.PlayGoalReachedSound();
AudioManager.Instance.PlayLevelCompleteSound();
```

## 🤝 Contributing

This package was extracted from the GMTK2025 project. For issues, improvements, or contributions, please visit the [main repository](https://github.com/DontBeBlink/GMTK2025).

## 📄 License

MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Credits

Originally developed for GMTK2025 "You Did This" by the GMTK2025 team. Enhanced and packaged for community use with category-based organization and advanced audio features.