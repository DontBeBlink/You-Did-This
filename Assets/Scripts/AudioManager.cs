using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip cloneCreateSound;
    [SerializeField] private AudioClip cloneRetractSound;
    [SerializeField] private AudioClip goalReachedSound;
    [SerializeField] private AudioClip levelCompleteSound;
    
    [Header("Settings")]
    [SerializeField] private float masterVolume = 1f;
    
    private AudioSource audioSource;
    private static AudioManager instance;
    
    public static AudioManager Instance => instance;
    
    private void Awake()
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
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    public void PlayJumpSound()
    {
        PlaySound(jumpSound);
    }
    
    public void PlayCloneCreateSound()
    {
        PlaySound(cloneCreateSound);
    }
    
    public void PlayCloneRetractSound()
    {
        PlaySound(cloneRetractSound);
    }
    
    public void PlayGoalReachedSound()
    {
        PlaySound(goalReachedSound);
    }
    
    public void PlayLevelCompleteSound()
    {
        PlaySound(levelCompleteSound);
    }
    
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, masterVolume);
        }
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }
}