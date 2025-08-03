using UnityEngine;

/// <summary>
/// Manages audio effects for clone actions and state changes.
/// Provides subtle audio feedback to enhance the clone system experience
/// without overwhelming the player with too much sound.
/// 
/// Features:
/// - Spawn/despawn sound effects
/// - Stuck state audio feedback
/// - Movement sounds for clones (optional)
/// - Volume and pitch modulation for variety
/// - Audio source pooling for performance
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class CloneSoundEffects : MonoBehaviour
{
    [Header("Sound Effects")]
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip despawnSound;
    [SerializeField] private AudioClip stuckSound;
    [SerializeField] private AudioClip[] movementSounds;
    
    [Header("Audio Settings")]
    [SerializeField] private float spawnVolume = 0.3f;
    [SerializeField] private float despawnVolume = 0.2f;
    [SerializeField] private float stuckVolume = 0.4f;
    [SerializeField] private float movementVolume = 0.1f;
    [SerializeField] private float pitchVariation = 0.2f;
    [SerializeField] private bool enableMovementSounds = false;
    
    [Header("Movement Sound Settings")]
    [SerializeField] private float movementSoundInterval = 0.5f;
    [SerializeField] private float minimumMovementSpeed = 1.0f;
    
    private AudioSource audioSource;
    private Clone parentClone;
    private CharacterController2D character;
    private float lastMovementSoundTime;
    private Vector3 lastPosition;
    
    /// <summary>
    /// Initialize audio components and get clone reference.
    /// </summary>
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        parentClone = GetComponent<Clone>();
        character = GetComponent<CharacterController2D>();
        
        SetupAudioSource();
    }
    
    /// <summary>
    /// Configure the audio source for clone sound effects.
    /// </summary>
    private void SetupAudioSource()
    {
        if (audioSource == null) return;
        
        // Configure audio source for clone sounds
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.8f; // Mostly 3D spatial audio
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = 10f;
        audioSource.minDistance = 1f;
        
        // Slightly lower priority than player sounds
        audioSource.priority = 150;
    }
    
    /// <summary>
    /// Initialize position tracking for movement sounds.
    /// </summary>
    private void Start()
    {
        lastPosition = transform.position;
    }
    
    /// <summary>
    /// Monitor clone movement for optional movement sounds.
    /// </summary>
    private void Update()
    {
        if (enableMovementSounds && parentClone != null && parentClone.IsReplaying)
        {
            CheckForMovementSound();
        }
    }
    
    /// <summary>
    /// Check if clone is moving enough to play movement sound.
    /// </summary>
    private void CheckForMovementSound()
    {
        if (Time.time - lastMovementSoundTime < movementSoundInterval) return;
        
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        float speed = distanceMoved / Time.deltaTime;
        
        if (speed >= minimumMovementSpeed && movementSounds.Length > 0)
        {
            PlayMovementSound();
            lastMovementSoundTime = Time.time;
        }
        
        lastPosition = transform.position;
    }
    
    /// <summary>
    /// Play spawn sound effect.
    /// </summary>
    public void PlaySpawnSound()
    {
        PlaySound(spawnSound, spawnVolume);
    }
    
    /// <summary>
    /// Play despawn sound effect.
    /// </summary>
    public void PlayDespawnSound()
    {
        PlaySound(despawnSound, despawnVolume);
    }
    
    /// <summary>
    /// Play stuck sound effect when clone becomes stuck at goal.
    /// </summary>
    public void PlayStuckSound()
    {
        PlaySound(stuckSound, stuckVolume);
    }
    
    /// <summary>
    /// Play movement sound effect.
    /// </summary>
    private void PlayMovementSound()
    {
        if (movementSounds.Length > 0)
        {
            AudioClip randomMovementSound = movementSounds[Random.Range(0, movementSounds.Length)];
            PlaySound(randomMovementSound, movementVolume);
        }
    }
    
    /// <summary>
    /// Play a sound effect with optional pitch variation.
    /// </summary>
    /// <param name="clip">Audio clip to play</param>
    /// <param name="volume">Volume level (0-1)</param>
    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null || audioSource == null) return;
        
        // Add pitch variation for more natural sound
        float pitch = 1.0f + Random.Range(-pitchVariation, pitchVariation);
        audioSource.pitch = pitch;
        
        // Play the sound
        audioSource.PlayOneShot(clip, volume);
    }
    
    /// <summary>
    /// Use AudioManager if available, otherwise use local AudioSource.
    /// </summary>
    /// <param name="clip">Audio clip to play</param>
    /// <param name="volume">Volume level (0-1)</param>
    private void PlaySoundThroughManager(AudioClip clip, float volume)
    {
        if (clip == null) return;
        
        // Try to use AudioManager if available
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(clip, volume);
        }
        else
        {
            // Fall back to local audio source
            PlaySound(clip, volume);
        }
    }
    
    /// <summary>
    /// Enable or disable movement sounds.
    /// </summary>
    /// <param name="enabled">Whether movement sounds should be enabled</param>
    public void SetMovementSoundsEnabled(bool enabled)
    {
        enableMovementSounds = enabled;
    }
    
    /// <summary>
    /// Set the volume for all clone sound effects.
    /// </summary>
    /// <param name="masterVolume">Master volume multiplier (0-1)</param>
    public void SetMasterVolume(float masterVolume)
    {
        if (audioSource != null)
        {
            audioSource.volume = masterVolume;
        }
    }
}