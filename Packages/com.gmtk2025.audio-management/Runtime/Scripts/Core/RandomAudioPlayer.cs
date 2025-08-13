using UnityEngine;
using System.Collections.Generic;

namespace GMTK2025.AudioManagement
{
    /// <summary>
    /// Manages randomized audio clips for variety in repetitive sounds.
    /// Useful for footsteps, hits, destruction sounds, etc.
    /// </summary>
    [System.Serializable]
    public class RandomAudioClip
    {
        [SerializeField] private AudioClip[] clips;
        [SerializeField, Range(0f, 1f)] private float baseVolume = 1f;
        [SerializeField, Range(0f, 0.5f)] private float volumeVariation = 0.1f;
        [SerializeField, Range(0.5f, 2f)] private float basePitch = 1f;
        [SerializeField, Range(0f, 0.5f)] private float pitchVariation = 0.1f;
        [SerializeField] private bool avoidRepeats = true;

        private int lastPlayedIndex = -1;
        private AudioSource tempAudioSource;

        /// <summary>
        /// Play a random clip from the collection.
        /// </summary>
        public void PlayRandom(AudioCategory category = AudioCategory.SFX, Vector3? position = null)
        {
            if (clips == null || clips.Length == 0 || AudioManager.Instance == null) return;

            int clipIndex = GetRandomClipIndex();
            AudioClip clipToPlay = clips[clipIndex];
            
            if (clipToPlay == null) return;

            float volume = baseVolume + Random.Range(-volumeVariation, volumeVariation);
            volume = Mathf.Clamp01(volume);

            if (position.HasValue)
            {
                AudioManager.Instance.PlaySoundAtPosition(clipToPlay, position.Value, category, volume);
            }
            else
            {
                AudioManager.Instance.PlaySound(clipToPlay, category, volume);
            }

            lastPlayedIndex = clipIndex;
        }

        /// <summary>
        /// Play a random clip with pitch variation (requires creating a temporary AudioSource).
        /// </summary>
        public void PlayRandomWithPitch(AudioCategory category = AudioCategory.SFX, Vector3? position = null)
        {
            if (clips == null || clips.Length == 0) return;

            int clipIndex = GetRandomClipIndex();
            AudioClip clipToPlay = clips[clipIndex];
            
            if (clipToPlay == null) return;

            // Create temporary audio source for pitch control
            if (tempAudioSource == null)
            {
                GameObject tempObj = new GameObject("TempAudioSource");
                tempAudioSource = tempObj.AddComponent<AudioSource>();
                tempAudioSource.playOnAwake = false;
            }

            float volume = baseVolume + Random.Range(-volumeVariation, volumeVariation);
            float pitch = basePitch + Random.Range(-pitchVariation, pitchVariation);
            
            volume = Mathf.Clamp01(volume);
            pitch = Mathf.Clamp(pitch, 0.1f, 3f);

            tempAudioSource.clip = clipToPlay;
            tempAudioSource.volume = volume;
            tempAudioSource.pitch = pitch;
            
            if (position.HasValue)
            {
                tempAudioSource.transform.position = position.Value;
                tempAudioSource.spatialBlend = 1f;
            }
            else
            {
                tempAudioSource.spatialBlend = 0f;
            }

            tempAudioSource.Play();

            lastPlayedIndex = clipIndex;

            // Clean up after clip finishes
            if (Application.isPlaying)
            {
                Object.Destroy(tempAudioSource.gameObject, clipToPlay.length + 0.1f);
                tempAudioSource = null;
            }
        }

        private int GetRandomClipIndex()
        {
            if (clips.Length == 1) return 0;

            int index;
            do
            {
                index = Random.Range(0, clips.Length);
            }
            while (avoidRepeats && index == lastPlayedIndex && clips.Length > 1);

            return index;
        }

        /// <summary>
        /// Get a random clip without playing it.
        /// </summary>
        public AudioClip GetRandomClip()
        {
            if (clips == null || clips.Length == 0) return null;
            
            int index = GetRandomClipIndex();
            lastPlayedIndex = index;
            return clips[index];
        }

        /// <summary>
        /// Get the number of clips in this collection.
        /// </summary>
        public int ClipCount => clips != null ? clips.Length : 0;
    }

    /// <summary>
    /// Component wrapper for RandomAudioClip to use in the inspector.
    /// </summary>
    public class RandomAudioPlayer : MonoBehaviour
    {
        [Header("Random Audio Collection")]
        [SerializeField] private RandomAudioClip randomClips;
        [SerializeField] private AudioCategory category = AudioCategory.SFX;
        [SerializeField] private bool use3D = false;
        [SerializeField] private bool usePitchVariation = false;

        /// <summary>
        /// Play a random clip from the collection.
        /// </summary>
        public void PlayRandom()
        {
            Vector3? position = use3D ? transform.position : (Vector3?)null;
            
            if (usePitchVariation)
            {
                randomClips.PlayRandomWithPitch(category, position);
            }
            else
            {
                randomClips.PlayRandom(category, position);
            }
        }

        /// <summary>
        /// Get a random clip without playing it.
        /// </summary>
        public AudioClip GetRandomClip()
        {
            return randomClips.GetRandomClip();
        }
    }
}