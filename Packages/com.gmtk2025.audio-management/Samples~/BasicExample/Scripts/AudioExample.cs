using UnityEngine;
using UnityEngine.UI;

namespace GMTK2025.AudioManagement.Examples
{
    /// <summary>
    /// Example script demonstrating basic audio management features.
    /// Shows how to play sounds, control volumes, and integrate with UI.
    /// </summary>
    public class AudioExample : MonoBehaviour
    {
        [Header("Test Audio Clips")]
        [SerializeField] private AudioClip testSFX;
        [SerializeField] private AudioClip testMusic;
        [SerializeField] private AudioClip testUI;
        [SerializeField] private AudioClip testAmbient;
        [SerializeField] private AudioClip testVoice;

        [Header("UI Elements")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Toggle masterMuteToggle;
        [SerializeField] private Toggle musicMuteToggle;

        [Header("3D Audio Test")]
        [SerializeField] private Transform audioTarget;
        [SerializeField] private float moveSpeed = 5f;

        private bool isPlayingMusic = false;

        private void Start()
        {
            SetupUI();
        }

        private void Update()
        {
            // Move audio target for 3D audio demonstration
            if (audioTarget != null)
            {
                float x = Mathf.Sin(Time.time * moveSpeed) * 3f;
                audioTarget.position = new Vector3(x, audioTarget.position.y, audioTarget.position.z);
            }
        }

        private void SetupUI()
        {
            if (AudioManager.Instance == null) return;

            // Set up sliders
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = AudioManager.Instance.GetMasterVolume();
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.GetCategoryVolume(AudioCategory.SFX);
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.GetCategoryVolume(AudioCategory.Music);
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            // Set up toggles
            if (masterMuteToggle != null)
            {
                masterMuteToggle.onValueChanged.AddListener(OnMasterMuteChanged);
            }

            if (musicMuteToggle != null)
            {
                musicMuteToggle.onValueChanged.AddListener(OnMusicMuteChanged);
            }
        }

        #region Audio Test Methods

        public void PlayTestSFX()
        {
            if (testSFX != null)
            {
                AudioManager.Instance.PlaySound(testSFX, AudioCategory.SFX);
            }
        }

        public void PlayTestSFXAt3D()
        {
            if (testSFX != null && audioTarget != null)
            {
                AudioManager.Instance.PlaySoundAtPosition(testSFX, audioTarget.position, AudioCategory.SFX);
            }
        }

        public void PlayTestUI()
        {
            if (testUI != null)
            {
                AudioManager.Instance.PlaySound(testUI, AudioCategory.UI);
            }
        }

        public void PlayTestAmbient()
        {
            if (testAmbient != null)
            {
                AudioManager.Instance.PlaySound(testAmbient, AudioCategory.Ambient);
            }
        }

        public void PlayTestVoice()
        {
            if (testVoice != null)
            {
                AudioManager.Instance.PlaySound(testVoice, AudioCategory.Voice);
            }
        }

        public void ToggleMusic()
        {
            if (isPlayingMusic)
            {
                AudioManager.Instance.StopMusic(1f); // Fade out over 1 second
                isPlayingMusic = false;
            }
            else
            {
                if (testMusic != null)
                {
                    AudioManager.Instance.PlayMusic(testMusic, 1f, true); // Fade in over 1 second, loop
                    isPlayingMusic = true;
                }
            }
        }

        public void PlayPredefinedSounds()
        {
            // Demonstrate predefined sound methods
            AudioManager.Instance.PlayJumpSound();
            
            // Delay other sounds to hear them clearly
            Invoke(nameof(PlayCloneSound), 0.5f);
            Invoke(nameof(PlayGoalSound), 1f);
        }

        private void PlayCloneSound()
        {
            AudioManager.Instance.PlayCloneCreateSound();
        }

        private void PlayGoalSound()
        {
            AudioManager.Instance.PlayGoalReachedSound();
        }

        #endregion

        #region Volume Control Methods

        public void OnMasterVolumeChanged(float value)
        {
            AudioManager.Instance.SetMasterVolume(value);
        }

        public void OnSFXVolumeChanged(float value)
        {
            AudioManager.Instance.SetCategoryVolume(AudioCategory.SFX, value);
        }

        public void OnMusicVolumeChanged(float value)
        {
            AudioManager.Instance.SetCategoryVolume(AudioCategory.Music, value);
        }

        public void OnMasterMuteChanged(bool isMuted)
        {
            AudioManager.Instance.SetMasterMute(isMuted);
        }

        public void OnMusicMuteChanged(bool isMuted)
        {
            AudioManager.Instance.SetCategoryMute(AudioCategory.Music, isMuted);
        }

        #endregion

        private void OnGUI()
        {
            // Simple debug GUI if no UI is set up
            if (masterVolumeSlider != null) return; // Don't show debug GUI if UI is set up

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
#if UNITY_EDITOR
            GUILayout.Label("Audio Management System Example", EditorGUIUtility.isProSkin ? GUI.skin.box : GUI.skin.label);
#else
            GUILayout.Label("Audio Management System Example", GUI.skin.label);
#endif
            GUILayout.Space(10);

            if (AudioManager.Instance == null)
            {
                GUILayout.Label("AudioManager not found! Add AudioManager component to a GameObject.");
                GUILayout.EndArea();
                return;
            }

            // Volume controls
            GUILayout.Label("Volume Controls:");
            float masterVol = GUILayout.HorizontalSlider(AudioManager.Instance.GetMasterVolume(), 0f, 1f);
            AudioManager.Instance.SetMasterVolume(masterVol);
            GUILayout.Label($"Master Volume: {masterVol:F2}");

            GUILayout.Space(5);

            // Test buttons
            GUILayout.Label("Test Sounds:");
            if (GUILayout.Button("Play SFX")) PlayTestSFX();
            if (GUILayout.Button("Play 3D SFX")) PlayTestSFXAt3D();
            if (GUILayout.Button("Play UI Sound")) PlayTestUI();
            if (GUILayout.Button("Toggle Music")) ToggleMusic();
            if (GUILayout.Button("Play Predefined Sounds")) PlayPredefinedSounds();

            GUILayout.Space(10);
            GUILayout.Label($"Music Playing: {isPlayingMusic}");
            
            GUILayout.EndArea();
        }
    }
}