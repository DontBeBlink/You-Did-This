using System;
using System.Collections.Generic;
using UnityEngine;

namespace GMTK2025.AudioManagement
{
    /// <summary>
    /// Audio categories for organized sound management.
    /// Allows for separate volume controls and effect processing per category.
    /// </summary>
    public enum AudioCategory
    {
        SFX,        // Sound effects
        Music,      // Background music
        UI,         // User interface sounds
        Ambient,    // Ambient/environmental sounds
        Voice       // Voice/dialogue sounds
    }

    /// <summary>
    /// Configuration for an audio category including volume and effects settings.
    /// </summary>
    [Serializable]
    public class AudioCategorySettings
    {
        [Range(0f, 1f)]
        public float volume = 1f;
        public bool muted = false;
        public AudioMixerGroup mixerGroup;
        public bool use3D = false;
        
        [Header("3D Audio Settings")]
        [Range(0f, 1f)]
        public float spatialBlend = 0f;
        public float minDistance = 1f;
        public float maxDistance = 500f;
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    }

    /// <summary>
    /// Comprehensive singleton-based audio management system for Unity.
    /// Originally developed for GMTK2025, enhanced with category-based organization,
    /// volume controls, and advanced audio features.
    /// 
    /// Features:
    /// - Singleton pattern for global audio access
    /// - Category-based audio organization (SFX, Music, UI, etc.)
    /// - Per-category volume and mute controls  
    /// - 2D and 3D audio support
    /// - Audio pooling for performance
    /// - Cross-scene persistence
    /// - Fade in/out support
    /// - Audio event system
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Master Settings")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField] private bool masterMute = false;
        [SerializeField] private int audioSourcePoolSize = 10;

        [Header("Category Settings")]
        [SerializeField] private AudioCategorySettings sfxSettings = new AudioCategorySettings();
        [SerializeField] private AudioCategorySettings musicSettings = new AudioCategorySettings();
        [SerializeField] private AudioCategorySettings uiSettings = new AudioCategorySettings();
        [SerializeField] private AudioCategorySettings ambientSettings = new AudioCategorySettings();
        [SerializeField] private AudioCategorySettings voiceSettings = new AudioCategorySettings();

        [Header("Predefined Audio Clips")]
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip cloneCreateSound;
        [SerializeField] private AudioClip cloneRetractSound;
        [SerializeField] private AudioClip goalReachedSound;
        [SerializeField] private AudioClip levelCompleteSound;

        // Singleton instance
        private static AudioManager instance;
        public static AudioManager Instance => instance;

        // Audio source pool for performance
        private Queue<AudioSource> availableAudioSources = new Queue<AudioSource>();
        private List<AudioSource> allAudioSources = new List<AudioSource>();
        private Dictionary<AudioCategory, AudioCategorySettings> categorySettings;

        // Currently playing music
        private AudioSource currentMusicSource;
        private Coroutine musicFadeCoroutine;

        // Events
        public static event Action<AudioClip, AudioCategory> OnAudioPlayed;
        public static event Action<float> OnMasterVolumeChanged;
        public static event Action<AudioCategory, float> OnCategoryVolumeChanged;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeSingleton();
            InitializeCategorySettings();
            InitializeAudioSourcePool();
        }

        private void InitializeSingleton()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void InitializeCategorySettings()
        {
            categorySettings = new Dictionary<AudioCategory, AudioCategorySettings>
            {
                { AudioCategory.SFX, sfxSettings },
                { AudioCategory.Music, musicSettings },
                { AudioCategory.UI, uiSettings },
                { AudioCategory.Ambient, ambientSettings },
                { AudioCategory.Voice, voiceSettings }
            };
        }

        private void InitializeAudioSourcePool()
        {
            for (int i = 0; i < audioSourcePoolSize; i++)
            {
                CreatePooledAudioSource();
            }
        }

        private AudioSource CreatePooledAudioSource()
        {
            GameObject audioSourceObj = new GameObject($"AudioSource_Pool_{allAudioSources.Count}");
            audioSourceObj.transform.SetParent(transform);
            
            AudioSource audioSource = audioSourceObj.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            
            allAudioSources.Add(audioSource);
            availableAudioSources.Enqueue(audioSource);
            
            return audioSource;
        }

        #endregion

        #region Public API - Basic Sound Playing

        /// <summary>
        /// Play an audio clip with specified category and volume.
        /// </summary>
        public void PlaySound(AudioClip clip, AudioCategory category = AudioCategory.SFX, float volumeScale = 1f, Vector3? position = null)
        {
            if (clip == null || masterMute) return;

            AudioSource audioSource = GetAvailableAudioSource();
            if (audioSource == null) return;

            ConfigureAudioSource(audioSource, category, position);
            
            float finalVolume = CalculateFinalVolume(category, volumeScale);
            audioSource.volume = finalVolume;
            audioSource.clip = clip;
            audioSource.Play();

            OnAudioPlayed?.Invoke(clip, category);
            StartCoroutine(ReturnAudioSourceWhenFinished(audioSource, clip.length));
        }

        /// <summary>
        /// Play a sound at a specific world position (3D audio).
        /// </summary>
        public void PlaySoundAtPosition(AudioClip clip, Vector3 position, AudioCategory category = AudioCategory.SFX, float volumeScale = 1f)
        {
            PlaySound(clip, category, volumeScale, position);
        }

        /// <summary>
        /// Play a looping sound and return the AudioSource for control.
        /// </summary>
        public AudioSource PlayLoopingSound(AudioClip clip, AudioCategory category = AudioCategory.SFX, float volumeScale = 1f)
        {
            if (clip == null || masterMute) return null;

            AudioSource audioSource = GetAvailableAudioSource();
            if (audioSource == null) return null;

            ConfigureAudioSource(audioSource, category);
            audioSource.volume = CalculateFinalVolume(category, volumeScale);
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();

            OnAudioPlayed?.Invoke(clip, category);
            return audioSource;
        }

        /// <summary>
        /// Stop a looping sound and return the AudioSource to the pool.
        /// </summary>
        public void StopLoopingSound(AudioSource audioSource)
        {
            if (audioSource != null && allAudioSources.Contains(audioSource))
            {
                audioSource.Stop();
                audioSource.loop = false;
                ReturnAudioSource(audioSource);
            }
        }

        #endregion

        #region Public API - Predefined Sounds

        public void PlayJumpSound() => PlaySound(jumpSound, AudioCategory.SFX);
        public void PlayCloneCreateSound() => PlaySound(cloneCreateSound, AudioCategory.SFX);
        public void PlayCloneRetractSound() => PlaySound(cloneRetractSound, AudioCategory.SFX);
        public void PlayRetractSound() => PlayCloneRetractSound();
        public void PlayGoalReachedSound() => PlaySound(goalReachedSound, AudioCategory.SFX);
        public void PlayLevelCompleteSound() => PlaySound(levelCompleteSound, AudioCategory.SFX);

        #endregion

        #region Public API - Music Management

        /// <summary>
        /// Play background music with optional fade-in.
        /// </summary>
        public void PlayMusic(AudioClip musicClip, float fadeInDuration = 0f, bool loop = true)
        {
            if (musicClip == null) return;

            // Stop current music if playing
            if (currentMusicSource != null && currentMusicSource.isPlaying)
            {
                StopMusic(fadeInDuration > 0 ? fadeInDuration : 0.5f);
            }

            currentMusicSource = GetAvailableAudioSource();
            if (currentMusicSource == null) return;

            ConfigureAudioSource(currentMusicSource, AudioCategory.Music);
            currentMusicSource.clip = musicClip;
            currentMusicSource.loop = loop;

            if (fadeInDuration > 0)
            {
                if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
                musicFadeCoroutine = StartCoroutine(FadeInMusic(currentMusicSource, fadeInDuration));
            }
            else
            {
                currentMusicSource.volume = CalculateFinalVolume(AudioCategory.Music, 1f);
                currentMusicSource.Play();
            }

            OnAudioPlayed?.Invoke(musicClip, AudioCategory.Music);
        }

        /// <summary>
        /// Stop the currently playing music with optional fade-out.
        /// </summary>
        public void StopMusic(float fadeOutDuration = 0f)
        {
            if (currentMusicSource == null) return;

            if (fadeOutDuration > 0)
            {
                if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
                musicFadeCoroutine = StartCoroutine(FadeOutMusic(currentMusicSource, fadeOutDuration));
            }
            else
            {
                currentMusicSource.Stop();
                ReturnAudioSource(currentMusicSource);
                currentMusicSource = null;
            }
        }

        #endregion

        #region Public API - Volume and Settings

        /// <summary>
        /// Set the master volume for all audio.
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            OnMasterVolumeChanged?.Invoke(masterVolume);
            UpdateAllPlayingVolumes();
        }

        /// <summary>
        /// Set volume for a specific audio category.
        /// </summary>
        public void SetCategoryVolume(AudioCategory category, float volume)
        {
            if (categorySettings.ContainsKey(category))
            {
                categorySettings[category].volume = Mathf.Clamp01(volume);
                OnCategoryVolumeChanged?.Invoke(category, volume);
                UpdateAllPlayingVolumes();
            }
        }

        /// <summary>
        /// Mute or unmute all audio.
        /// </summary>
        public void SetMasterMute(bool muted)
        {
            masterMute = muted;
            UpdateAllPlayingVolumes();
        }

        /// <summary>
        /// Mute or unmute a specific audio category.
        /// </summary>
        public void SetCategoryMute(AudioCategory category, bool muted)
        {
            if (categorySettings.ContainsKey(category))
            {
                categorySettings[category].muted = muted;
                UpdateAllPlayingVolumes();
            }
        }

        /// <summary>
        /// Get the current master volume.
        /// </summary>
        public float GetMasterVolume() => masterVolume;

        /// <summary>
        /// Get the current volume for a specific category.
        /// </summary>
        public float GetCategoryVolume(AudioCategory category)
        {
            return categorySettings.ContainsKey(category) ? categorySettings[category].volume : 1f;
        }

        #endregion

        #region Private Methods

        private AudioSource GetAvailableAudioSource()
        {
            if (availableAudioSources.Count > 0)
            {
                return availableAudioSources.Dequeue();
            }

            // Create new audio source if pool is empty
            return CreatePooledAudioSource();
        }

        private void ReturnAudioSource(AudioSource audioSource)
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.loop = false;
            audioSource.transform.position = transform.position;
            availableAudioSources.Enqueue(audioSource);
        }

        private void ConfigureAudioSource(AudioSource audioSource, AudioCategory category, Vector3? position = null)
        {
            var settings = categorySettings[category];
            
            audioSource.outputAudioMixerGroup = settings.mixerGroup;
            audioSource.spatialBlend = settings.use3D ? settings.spatialBlend : 0f;
            audioSource.minDistance = settings.minDistance;
            audioSource.maxDistance = settings.maxDistance;
            audioSource.rolloffMode = settings.rolloffMode;
            
            if (position.HasValue)
            {
                audioSource.transform.position = position.Value;
                audioSource.spatialBlend = 1f; // Force 3D for positioned audio
            }
            else
            {
                audioSource.transform.position = transform.position;
            }
        }

        private float CalculateFinalVolume(AudioCategory category, float volumeScale)
        {
            if (masterMute) return 0f;
            
            var settings = categorySettings[category];
            if (settings.muted) return 0f;
            
            return masterVolume * settings.volume * volumeScale;
        }

        private void UpdateAllPlayingVolumes()
        {
            foreach (var audioSource in allAudioSources)
            {
                if (audioSource.isPlaying)
                {
                    // Update volume based on current settings
                    // Note: This is a simplified update - in practice you'd need to track original volume scales
                    AudioCategory category = GetAudioSourceCategory(audioSource);
                    audioSource.volume = CalculateFinalVolume(category, 1f);
                }
            }
        }

        private AudioCategory GetAudioSourceCategory(AudioSource audioSource)
        {
            // Simple category detection based on mixer group
            // In practice, you might want to track this more explicitly
            foreach (var kvp in categorySettings)
            {
                if (audioSource.outputAudioMixerGroup == kvp.Value.mixerGroup)
                {
                    return kvp.Key;
                }
            }
            return AudioCategory.SFX; // Default
        }

        private System.Collections.IEnumerator ReturnAudioSourceWhenFinished(AudioSource audioSource, float duration)
        {
            yield return new WaitForSeconds(duration + 0.1f); // Small buffer
            
            if (!audioSource.isPlaying)
            {
                ReturnAudioSource(audioSource);
            }
        }

        private System.Collections.IEnumerator FadeInMusic(AudioSource audioSource, float duration)
        {
            audioSource.volume = 0f;
            audioSource.Play();
            
            float targetVolume = CalculateFinalVolume(AudioCategory.Music, 1f);
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }
            
            audioSource.volume = targetVolume;
            musicFadeCoroutine = null;
        }

        private System.Collections.IEnumerator FadeOutMusic(AudioSource audioSource, float duration)
        {
            float startVolume = audioSource.volume;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
            
            audioSource.Stop();
            ReturnAudioSource(audioSource);
            
            if (currentMusicSource == audioSource)
            {
                currentMusicSource = null;
            }
            
            musicFadeCoroutine = null;
        }

        #endregion
    }
}