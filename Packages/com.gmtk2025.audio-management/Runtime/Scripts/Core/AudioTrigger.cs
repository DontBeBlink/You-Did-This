using UnityEngine;

namespace GMTK2025.AudioManagement
{
    /// <summary>
    /// Simple audio trigger component that can play sounds in response to Unity events.
    /// Useful for UI buttons, collision triggers, and other event-driven audio needs.
    /// </summary>
    public class AudioTrigger : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private AudioCategory category = AudioCategory.SFX;
        [SerializeField, Range(0f, 1f)] private float volumeScale = 1f;
        [SerializeField] private bool playOnStart = false;
        [SerializeField] private bool play3D = false;

        [Header("Trigger Settings")]
        [SerializeField] private bool playOnEnable = false;
        [SerializeField] private bool playOnCollisionEnter = false;
        [SerializeField] private bool playOnTriggerEnter = false;

        private void Start()
        {
            if (playOnStart)
            {
                PlayAudio();
            }
        }

        private void OnEnable()
        {
            if (playOnEnable)
            {
                PlayAudio();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (playOnCollisionEnter)
            {
                PlayAudio();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (playOnTriggerEnter)
            {
                PlayAudio();
            }
        }

        /// <summary>
        /// Play the configured audio clip. Can be called from UI events or scripts.
        /// </summary>
        public void PlayAudio()
        {
            if (audioClip == null || AudioManager.Instance == null) return;

            if (play3D)
            {
                AudioManager.Instance.PlaySoundAtPosition(audioClip, transform.position, category, volumeScale);
            }
            else
            {
                AudioManager.Instance.PlaySound(audioClip, category, volumeScale);
            }
        }

        /// <summary>
        /// Play a different audio clip through this trigger's settings.
        /// </summary>
        public void PlayAudio(AudioClip clip)
        {
            if (clip == null || AudioManager.Instance == null) return;

            if (play3D)
            {
                AudioManager.Instance.PlaySoundAtPosition(clip, transform.position, category, volumeScale);
            }
            else
            {
                AudioManager.Instance.PlaySound(clip, category, volumeScale);
            }
        }
    }
}