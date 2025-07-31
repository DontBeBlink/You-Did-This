using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

public class CloneManager : MonoBehaviour
{
    [Header("Loop Settings")]
    [SerializeField] private float loopDuration = 15f; // Time until new loop starts
    [SerializeField] private int maxClones = 10; // Maximum number of clones
    [SerializeField] private bool autoStartFirstLoop = true;
    [SerializeField] private bool enableManualLooping = true; // Allow manual loop triggering
    [SerializeField] private Key manualLoopKey = Key.L; // Use new Input System Key

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

    // New loop lifecycle events
    public static event System.Action OnLoopStarted;
    public static event System.Action OnLoopEnded;
    
    public static CloneManager instance { get; private set; }

    private void Awake()
    {
        instance = this;

        
        activePlayer = FindFirstObjectByType<PlayerController>();

        if (activePlayer == null)
        {
            Debug.LogError("CloneManager: No PlayerController found in scene!");
            return;
        }

        activePlayer.transform.position = this.transform.position; // Reset player position to spawn point

        // Add ActionRecorder to player if it doesn't exist
        actionRecorder = activePlayer.GetComponent<ActionRecorder>();
        if (actionRecorder == null)
        {
            actionRecorder = activePlayer.gameObject.AddComponent<ActionRecorder>();
            //Debug.Log("CloneManager: Added ActionRecorder component to player");
        }
        
       // Debug.Log($"CloneManager: Setup complete. Manual loop key: {manualLoopKey}, Auto start: {autoStartFirstLoop}");
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
        // Handle manual loop triggering with debug output using new Input System
        if (enableManualLooping && Keyboard.current != null && Keyboard.current[manualLoopKey].wasPressedThisFrame)
        {
            Debug.Log($"Manual loop key ({manualLoopKey}) pressed! Clone count: {allClones.Count}, Max: {maxClones}");
            if (allClones.Count < maxClones)
            {
                CreateClone();
                StartNewLoop();
                Debug.Log("Manual loop triggered successfully!");
            }
            else
            {
                Debug.LogWarning("Cannot create manual loop: Maximum clone limit reached!");
            }
        }

        // Automatic loop timing
        if (isLoopActive && Time.time - loopStartTime >= loopDuration)
        {

            CreateClone();
            StartNewLoop();
        }
    }
    
    public void StartNewLoop()
    {
        if (actionRecorder == null) return;

        // End previous loop if active
        if (isLoopActive)
        {
            OnLoopEnded?.Invoke();
        }

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
        OnLoopStarted?.Invoke();
    }
    
    public void CreateClone()
    {
        if (actionRecorder == null)
        {
            Debug.LogWarning("No ActionRecorder found - cannot create clone");
            return;
        }
        
        if (actionRecorder.ActionCount == 0)
        {
            Debug.LogWarning("No actions recorded to create clone from");
            return;
        }
        
        // If at maxClones, remove the oldest clone (first in the list)
        if (allClones.Count >= maxClones)
        {
            if (allClones[0] != null)
            {
                Destroy(allClones[0].gameObject);
            }
            allClones.RemoveAt(0);
            //Debug.Log($"Clone limit reached. Oldest clone destroyed to make room for new one.");
        }

        GameObject cloneObject;
        
        if (clonePrefab != null)
        {
            // Use the specified prefab
            cloneObject = Instantiate(clonePrefab, this.transform.position, activePlayer.transform.rotation);
        }
        else
        {
            // Clone the current player object
            cloneObject = Instantiate(activePlayer.gameObject, this.transform.position, activePlayer.transform.rotation);
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
        
        activePlayer.transform.position = this.transform.position; // Reset player position to clone spawn point

        OnCloneCreated?.Invoke(clone);
        //Debug.Log($"Created clone {clone.CloneIndex} with {recordedActions.Count} actions");
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
    
    // Clean up any null references in the clone list
    private void CleanupCloneList()
    {
        allClones.RemoveAll(clone => clone == null);
    }
    
    public void SetLoopDuration(float newDuration)
    {
        loopDuration = Mathf.Max(1f, newDuration);
        Debug.Log($"Loop duration set to {loopDuration} seconds");
    }
    
    // Properties
    public List<Clone> GetAllClones() {
        CleanupCloneList();
        return new List<Clone>(allClones);
    }
    public Clone GetActiveClone() => allClones.Count > 0 ? allClones[allClones.Count - 1] : null;
    public int TotalClones {
        get {
            CleanupCloneList();
            return allClones.Count;
        }
    }
    public int StuckClones {
        get {
            CleanupCloneList();
            return allClones.FindAll(c => c != null && c.IsStuck).Count;
        }
    }
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