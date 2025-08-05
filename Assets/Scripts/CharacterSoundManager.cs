using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Universal character sound manager for all character movement and action audio.
/// Provides comprehensive sound effects for player and clone movement with customizable settings.
/// Handles movement detection, audio playback, and integration with character physics.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class CharacterSoundManager : MonoBehaviour 
{
    [Header("Jump Sound Effects")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private float jumpVolume = 1f;
    [SerializeField] private float landVolume = 0.8f;

    [Header("Movement Sound Effects")]
    [SerializeField] private AudioClip[] walkSounds;
    [SerializeField] private AudioClip[] runSounds;
    [SerializeField] private float walkVolume = 0.3f;
    [SerializeField] private float runVolume = 0.5f;
    [SerializeField] private bool enableMovementSounds = true;

    [Header("Movement Sound Settings")]
    [SerializeField] private float walkSoundInterval = 0.6f;
    [SerializeField] private float runSoundInterval = 0.4f;
    [SerializeField] private float minimumWalkSpeed = 0.5f;
    [SerializeField] private float runSpeedThreshold = 3.0f;

    [Header("Special Movement Sound Effects")]
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip wallSlideSound;
    [SerializeField] private AudioClip wallJumpSound;
    [SerializeField] private float dashVolume = 0.7f;
    [SerializeField] private float wallSlideVolume = 0.4f;
    [SerializeField] private float wallJumpVolume = 0.8f;

    [Header("Audio Settings")]
    [SerializeField] private float pitchVariation = 0.1f;
    [SerializeField] private bool use3DAudio = true;
    [SerializeField] private float spatialBlend = 0.8f;

    private AudioSource audioSource;
    private CharacterController2D characterController;
    private Vector3 lastPosition;
    private float lastMovementSoundTime;
    private bool wasGrounded = true;
    private bool wasWallSliding = false;

    /// <summary>
    /// Initialize audio components and get character controller reference.
    /// </summary>
    void Start() 
    {
        audioSource = GetComponent<AudioSource>();
        characterController = GetComponent<CharacterController2D>();
        
        SetupAudioSource();
        lastPosition = transform.position;
    }

    /// <summary>
    /// Configure the audio source for character sounds.
    /// </summary>
    private void SetupAudioSource()
    {
        if (audioSource == null) return;
        
        audioSource.playOnAwake = false;
        
        if (use3DAudio)
        {
            audioSource.spatialBlend = spatialBlend;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = 15f;
            audioSource.minDistance = 1f;
        }
        else
        {
            audioSource.spatialBlend = 0f; // 2D sound
        }
        
        audioSource.priority = 128; // Default priority for character sounds
    }

    /// <summary>
    /// Monitor character movement and state for sound triggers.
    /// </summary>
    void Update() 
    {
        if (characterController == null) return;

        CheckForLanding();
        CheckForWallSliding();
        
        if (enableMovementSounds)
        {
            CheckForMovementSound();
        }
        
        UpdateLastStates();
    }

    /// <summary>
    /// Check if character just landed and play landing sound.
    /// </summary>
    private void CheckForLanding()
    {
        bool isGrounded = characterController.Collisions.below;
        
        if (!wasGrounded && isGrounded)
        {
            PlayLandSound();
        }
        
        wasGrounded = isGrounded;
    }

    /// <summary>
    /// Check for wall sliding state changes.
    /// </summary>
    private void CheckForWallSliding()
    {
        bool isWallSliding = characterController.Collisions.hHit && !characterController.Collisions.below;
        
        if (!wasWallSliding && isWallSliding)
        {
            PlayWallSlideSound();
        }
        
        wasWallSliding = isWallSliding;
    }

    /// <summary>
    /// Check if character is moving enough to play movement sound.
    /// </summary>
    private void CheckForMovementSound()
    {
        if (!characterController.Collisions.below) return; // Only play movement sounds when grounded
        
        float currentTime = Time.time;
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        float speed = distanceMoved / Time.deltaTime;
        
        // Determine if we should play walk or run sounds
        bool isRunning = speed >= runSpeedThreshold;
        float soundInterval = isRunning ? runSoundInterval : walkSoundInterval;
        
        if (currentTime - lastMovementSoundTime >= soundInterval && speed >= minimumWalkSpeed)
        {
            PlayMovementSound(isRunning);
            lastMovementSoundTime = currentTime;
        }
    }

    /// <summary>
    /// Update stored states for next frame comparison.
    /// </summary>
    private void UpdateLastStates()
    {
        lastPosition = transform.position;
    }

    /// <summary>
    /// Play jump sound effect.
    /// </summary>
    public void PlayJumpSound(float volumeScale = 1f) 
    {
        PlaySound(jumpSound, jumpVolume * volumeScale);
    }

    /// <summary>
    /// Play landing sound effect.
    /// </summary>
    public void PlayLandSound(float volumeScale = 1f)
    {
        PlaySound(landSound, landVolume * volumeScale);
    }

    /// <summary>
    /// Play movement sound effect (walk or run).
    /// </summary>
    private void PlayMovementSound(bool isRunning)
    {
        AudioClip[] soundArray = isRunning ? runSounds : walkSounds;
        float volume = isRunning ? runVolume : walkVolume;
        
        if (soundArray.Length > 0)
        {
            AudioClip randomSound = soundArray[Random.Range(0, soundArray.Length)];
            PlaySound(randomSound, volume);
        }
    }

    /// <summary>
    /// Play dash sound effect.
    /// </summary>
    public void PlayDashSound(float volumeScale = 1f)
    {
        PlaySound(dashSound, dashVolume * volumeScale);
    }

    /// <summary>
    /// Play wall slide sound effect.
    /// </summary>
    public void PlayWallSlideSound(float volumeScale = 1f)
    {
        PlaySound(wallSlideSound, wallSlideVolume * volumeScale);
    }

    /// <summary>
    /// Play wall jump sound effect.
    /// </summary>
    public void PlayWallJumpSound(float volumeScale = 1f)
    {
        PlaySound(wallJumpSound, wallJumpVolume * volumeScale);
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
    /// Enable or disable movement sounds.
    /// </summary>
    /// <param name="enabled">Whether movement sounds should be enabled</param>
    public void SetMovementSoundsEnabled(bool enabled)
    {
        enableMovementSounds = enabled;
    }

    /// <summary>
    /// Set the master volume multiplier for all sounds.
    /// </summary>
    /// <param name="volumeMultiplier">Volume multiplier (0-1)</param>
    public void SetMasterVolumeMultiplier(float volumeMultiplier)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volumeMultiplier);
        }
    }

    /// <summary>
    /// Get reference to the AudioSource component.
    /// </summary>
    public AudioSource GetAudioSource()
    {
        return audioSource;
    }
}