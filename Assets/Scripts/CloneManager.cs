using System.Collections.Generic;
using UnityEngine;

public class CloneManager : MonoBehaviour
{
    [Header("Loop Settings")]
    [SerializeField] private float loopDuration = 15f; // Time until new loop starts
    [SerializeField] private int maxClones = 10; // Maximum number of clones
    [SerializeField] private bool autoStartFirstLoop = true;
    
    [Header("Clone Prefab")]
    [SerializeField] private GameObject clonePrefab; // If null, will clone the player object
    
    [Header("Audio")]
    [SerializeField] private AudioClip cloneCreateSound;
    
    private List<Clone> allClones = new List<Clone>();
    private PlayerController activePlayer;
    private ActionRecorder actionRecorder;
    private float loopStartTime;
    private bool isLoopActive = false;
    private int nextCloneIndex = 0;
    
    // Events
    public System.Action<Clone> OnCloneCreated;
    public System.Action<Clone> OnCloneStuck;
    public System.Action OnNewLoopStarted;
    
    private void Awake()
    {
        activePlayer = FindFirstObjectByType<PlayerController>();
        if (activePlayer == null)
        {
            Debug.LogError("CloneManager: No PlayerController found in scene!");
            return;
        }
        
        // Add ActionRecorder to player if it doesn't exist
        actionRecorder = activePlayer.GetComponent<ActionRecorder>();
        if (actionRecorder == null)
        {
            actionRecorder = activePlayer.gameObject.AddComponent<ActionRecorder>();
        }
    }
    
    private void Start()
    {
        if (autoStartFirstLoop && activePlayer != null)
        {
            StartNewLoop();
        }
    }
    
    private void Update()
    {
        if (isLoopActive && Time.time - loopStartTime >= loopDuration)
        {
            if (allClones.Count < maxClones)
            {
                CreateClone();
                StartNewLoop();
            }
            else
            {
                Debug.Log("Maximum clone limit reached!");
            }
        }
    }
    
    public void StartNewLoop()
    {
        if (actionRecorder == null) return;
        
        // Stop recording current actions
        if (actionRecorder.IsRecording)
        {
            actionRecorder.StopRecording();
        }
        
        isLoopActive = true;
        loopStartTime = Time.time;
        
        // Start recording new actions
        actionRecorder.StartRecording();
        
        OnNewLoopStarted?.Invoke();
        Debug.Log($"New loop started! Duration: {loopDuration} seconds");
    }
    
    public void CreateClone()
    {
        if (actionRecorder == null || actionRecorder.ActionCount == 0)
        {
            Debug.LogWarning("No actions recorded to create clone from");
            return;
        }
        
        GameObject cloneObject;
        
        if (clonePrefab != null)
        {
            // Use the specified prefab
            cloneObject = Instantiate(clonePrefab, activePlayer.transform.position, activePlayer.transform.rotation);
        }
        else
        {
            // Clone the current player object
            cloneObject = Instantiate(activePlayer.gameObject, activePlayer.transform.position, activePlayer.transform.rotation);
        }
        
        // Get or add Clone component
        Clone clone = cloneObject.GetComponent<Clone>();
        if (clone == null)
        {
            clone = cloneObject.AddComponent<Clone>();
        }
        
        // Initialize the clone with recorded actions
        List<PlayerAction> recordedActions = actionRecorder.GetRecordedActions();
        clone.InitializeClone(recordedActions, nextCloneIndex++);
        
        // Add to our list and start replay
        allClones.Add(clone);
        clone.StartReplay();
        
        // Play sound effect
        if (cloneCreateSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(cloneCreateSound);
        }
        
        OnCloneCreated?.Invoke(clone);
        Debug.Log($"Created clone {clone.CloneIndex} with {recordedActions.Count} actions");
    }
    
    public void SetCloneStuck(Clone clone, Goal goal)
    {
        if (clone != null && allClones.Contains(clone))
        {
            clone.SetStuck(goal);
            OnCloneStuck?.Invoke(clone);
            Debug.Log($"Clone {clone.CloneIndex} set as stuck at goal");
        }
    }
    
    public bool RetractToLastClone()
    {
        if (allClones.Count == 0) return false;
        
        // Find the last non-stuck clone
        Clone lastClone = null;
        for (int i = allClones.Count - 1; i >= 0; i--)
        {
            if (!allClones[i].IsStuck)
            {
                lastClone = allClones[i];
                break;
            }
        }
        
        if (lastClone == null) return false;
        
        // Remove clones created after the last non-stuck one
        for (int i = allClones.Count - 1; i >= 0; i--)
        {
            if (allClones[i].CloneIndex > lastClone.CloneIndex)
            {
                Destroy(allClones[i].gameObject);
                allClones.RemoveAt(i);
            }
        }
        
        // Reset to that clone's position (this would need more implementation)
        Debug.Log($"Retracted to clone {lastClone.CloneIndex}");
        return true;
    }
    
    public void PauseAllClones()
    {
        foreach (Clone clone in allClones)
        {
            if (clone != null)
            {
                clone.StopReplay();
            }
        }
    }
    
    public void ResumeAllClones()
    {
        foreach (Clone clone in allClones)
        {
            if (clone != null && !clone.IsStuck)
            {
                clone.StartReplay();
            }
        }
    }
    
    public void DestroyAllClones()
    {
        foreach (Clone clone in allClones)
        {
            if (clone != null)
            {
                Destroy(clone.gameObject);
            }
        }
        allClones.Clear();
        nextCloneIndex = 0;
        Debug.Log("All clones destroyed");
    }
    
    public void SetLoopDuration(float newDuration)
    {
        loopDuration = Mathf.Max(1f, newDuration);
        Debug.Log($"Loop duration set to {loopDuration} seconds");
    }
    
    // Properties
    public List<Clone> GetAllClones() => new List<Clone>(allClones);
    public Clone GetActiveClone() => allClones.Count > 0 ? allClones[allClones.Count - 1] : null;
    public int TotalClones => allClones.Count;
    public int StuckClones => allClones.FindAll(c => c != null && c.IsStuck).Count;
    public int ActiveCloneIndex => nextCloneIndex - 1;
    public float CurrentLoopTime => isLoopActive ? Time.time - loopStartTime : 0f;
    public float TimeUntilNextLoop => isLoopActive ? loopDuration - CurrentLoopTime : 0f;
    public float LoopDuration => loopDuration;
    public bool IsLoopActive => isLoopActive;
    
    private void OnDrawGizmosSelected()
    {
        if (!isLoopActive) return;
        
        // Draw a visual indicator of loop progress
        Gizmos.color = Color.cyan;
        float progress = CurrentLoopTime / loopDuration;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f + progress * 0.5f);
    }
}