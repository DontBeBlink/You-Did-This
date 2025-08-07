using UnityEngine;

/// <summary>
/// Centralized audio management system for game sound effects and audio feedback.
/// Provides a singleton interface for playing sound effects throughout the game,
/// with volume control and automatic AudioSource management.
/// 
/// Core Functionality:
/// - Singleton pattern for global audio access from any system
/// - Centralized sound effect library for consistent audio experience
/// - Master volume control for user preferences
/// - Automatic AudioSource component management
/// - Cross-scene persistence for continuous audio management
/// 
/// Supported Sound Types:
/// - Jump sounds for player movement feedback
/// - Clone creation/retraction sounds for clone system feedback
/// - Goal completion sounds for puzzle progression
/// - Level completion sounds for success celebration
/// - Generic sound playing for custom audio clips
/// 
/// Integration Points:
/// - Called by CharacterController2D for movement sounds
/// - Called by CloneManager for clone-related audio
/// - Called by Goal system for completion feedback
/// - Called by level systems for progression audio
/// 
/// Usage: Place on a GameObject in the first scene. Will persist across scene loads
/// and be accessible globally via AudioManager.Instance. Configure audio clips
/// in the inspector for consistent sound design.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip jumpSound;         // Sound played when player or clone jumps
    [SerializeField] private AudioClip cloneCreateSound;  // Sound played when a new clone is created
    [SerializeField] private AudioClip cloneRetractSound; // Sound played when clones are retracted/destroyed
    [SerializeField] private AudioClip goalReachedSound;  // Sound played when a clone reaches a goal
    [SerializeField] private AudioClip levelCompleteSound; // Sound played when all level goals are completed
    
    [Header("Settings")]
    [SerializeField] private float masterVolume = 1f;     // Global volume multiplier (0.0 to 1.0)
    
    // Audio playback component (created automatically if missing)
    private AudioSource audioSource;
    
    // Singleton instance for global access
    private static AudioManager instance;
    public static AudioManager Instance => instance;
    
    /// <summary>
    /// Initialize singleton pattern and set up audio components.
    /// Ensures AudioSource component is available and persists across scene loads.
    /// </summary>
    private void Awake()
    {
        // Enforce singleton pattern with cross-scene persistence
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist audio manager across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }
        
        // Ensure we have an AudioSource component for sound playback
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    /// <summary>
    /// Play the jump sound effect.
    /// Called by CharacterController2D when player or clones jump.
    /// </summary>
    public void PlayJumpSound()
    {
        PlaySound(jumpSound);
    }
    
    /// <summary>
    /// Play the clone creation sound effect.
    /// Called by CloneManager when a new clone is created.
    /// </summary>
    public void PlayCloneCreateSound()
    {
        PlaySound(cloneCreateSound);
    }
    
    /// <summary>
    /// Play the clone retraction sound effect.
    /// Called when clones are destroyed or retracted from the scene.
    /// </summary>
    public void PlayCloneRetractSound()
    {
        PlaySound(cloneRetractSound);
    }
    
    /// <summary>
    /// Play the retract sound effect.
    /// Alias for PlayCloneRetractSound for convenience.
    /// </summary>
    public void PlayRetractSound()
    {
        PlayCloneRetractSound();
    }
    
    /// <summary>
    /// Play the goal reached sound effect.
    /// Called by Goal objects when a clone successfully reaches them.
    /// </summary>
    public void PlayGoalReachedSound()
    {
        PlaySound(goalReachedSound);
    }
    
    /// <summary>
    /// Play the level completion sound effect.
    /// Called when all goals in a level are completed.
    /// </summary>
    public void PlayLevelCompleteSound()
    {
        PlaySound(levelCompleteSound);
    }
    
    /// <summary>
    /// Play any audio clip with master volume applied.
    /// Generic method for playing custom sound effects from other systems.
    /// Uses PlayOneShot to allow overlapping sounds without interruption.
    /// </summary>
    /// <param name="clip">The AudioClip to play</param>
    public void PlaySound(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || audioSource == null) return;

        // Apply master volume and scale
        float volume = Mathf.Clamp01(masterVolume * volumeScale);
        audioSource.PlayOneShot(clip, volume);
    }
    
    /// <summary>
    /// Set the master volume for all audio playback.
    /// Allows for user volume preferences and dynamic volume control.
    /// </summary>
    /// <param name="volume">Volume level from 0.0 (silent) to 1.0 (full volume)</param>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume); // Ensure volume stays within valid range
    }
}