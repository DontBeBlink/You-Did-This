using UnityEngine;

/// <summary>
/// Manages audio effects specific to clone actions and state changes.
/// Provides clone-specific audio feedback while delegating movement sounds to CharacterSoundManager.
/// 
/// Features:
/// - Spawn/despawn sound effects
/// - Stuck state audio feedback
/// - Integration with universal CharacterSoundManager for movement sounds
/// - Volume and pitch modulation for variety
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class CloneSoundEffects : MonoBehaviour
{
    [Header("Clone-Specific Sound Effects")]
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip despawnSound;
    [SerializeField] private AudioClip stuckSound;
    
    [Header("Audio Settings")]
    [SerializeField] private float spawnVolume = 0.3f;
    [SerializeField] private float despawnVolume = 0.2f;
    [SerializeField] private float stuckVolume = 0.4f;
    [SerializeField] private float pitchVariation = 0.2f;
    
    private AudioSource audioSource;
    private CharacterSoundManager characterSoundManager;
    private Clone parentClone;
    
    /// <summary>
    /// Initialize audio components and get clone reference.
    /// </summary>
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        parentClone = GetComponent<Clone>();
        characterSoundManager = GetComponent<CharacterSoundManager>();
        
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
    /// Enable or disable movement sounds via CharacterSoundManager.
    /// </summary>
    /// <param name="enabled">Whether movement sounds should be enabled</param>
    public void SetMovementSoundsEnabled(bool enabled)
    {
        if (characterSoundManager != null)
        {
            characterSoundManager.SetMovementSoundsEnabled(enabled);
        }
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
        
        // Also apply to character sound manager for movement sounds
        if (characterSoundManager != null)
        {
            characterSoundManager.SetMasterVolumeMultiplier(masterVolume);
        }
    }
}