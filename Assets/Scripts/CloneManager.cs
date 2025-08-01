using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central management system for the clone mechanics in "You Did This".
/// Handles creation, lifecycle, and coordination of player clones that replay recorded actions.
/// </summary>
public class CloneManager : MonoBehaviour
{
    [Header("Loop Settings")]
    [SerializeField] private float loopDuration = 15f; // Time until new loop starts automatically
    [SerializeField] private int maxClones = 10; // Maximum number of clones allowed simultaneously
    [SerializeField] private bool autoStartFirstLoop = true; // Whether to start recording automatically on scene start
    [SerializeField] private bool enableManualLooping = true; // Allow manual clone creation via input
    [SerializeField] private Key manualLoopKey = Key.F; // Input key for manual clone creation
    [SerializeField] private bool IsMasterOfTime = false; // If true, player controls their own loop and position is not reset


    [Header("Clone Prefab")]
    [SerializeField] private GameObject clonePrefab; // Optional custom clone prefab (uses player copy if null)
    
    [Header("Audio")]
    [SerializeField] private AudioClip cloneCreateSound; // Sound effect played when creating clones
    
    // Core component references
    private List<Clone> allClones = new List<Clone>(); // All active clones in creation order
    private PlayerController activePlayer; // Reference to the player being recorded
    private ActionRecorder actionRecorder; // Records player actions for clone replay
    private CameraController cameraController;
    
    // Loop timing and state
    private float loopStartTime; // Time.time when current loop started
    private bool isLoopActive = false; // Whether we're currently recording a loop
    private int nextCloneIndex = 0; // Unique identifier for the next clone to be created

    private bool isTransitioning = false;
    private bool isAutoLooping = false; // Add this field to prevent multiple auto loops

    // Events for other systems to respond to clone lifecycle changes
    public System.Action<Clone> OnCloneCreated; // Fired when a new clone is created
    public System.Action<Clone> OnCloneStuck; // Fired when a clone becomes stuck at a goal
    public System.Action OnNewLoopStarted; // Fired when a new recording loop begins

    // Static events for broader system coordination
    public static event System.Action OnLoopStarted; // Global notification of loop start
    public static event System.Action OnLoopEnded; // Global notification of loop end
    
    /// <summary>
    /// Singleton instance for easy access from other systems
    /// </summary>
    public static CloneManager instance { get; private set; }

    /// <summary>
    /// Initialize the CloneManager singleton and set up core components.
    /// Finds the PlayerController in the scene and ensures it has an ActionRecorder component.
    /// Resets the player to the spawn point position.
    /// </summary>
    private void Awake()
    {
        // Singleton pattern enforcement
        instance = this;

        // Find the player controller in the scene
        activePlayer = FindFirstObjectByType<PlayerController>();

        // Find the camera controller for future use
        cameraController = FindFirstObjectByType<CameraController>();

        if (activePlayer == null)
        {
            Debug.LogError("CloneManager: No PlayerController found in scene!");
            return;
        }

        // Reset player position to the spawn point (CloneManager's transform position)
        activePlayer.transform.position = this.transform.position;

        // Ensure the player has an ActionRecorder component for recording actions
        actionRecorder = activePlayer.GetComponent<ActionRecorder>();
        if (actionRecorder == null)
        {
            actionRecorder = activePlayer.gameObject.AddComponent<ActionRecorder>();
        }
    }
    
    /// <summary>
    /// Start the first recording loop if auto-start is enabled.
    /// Called after Awake() when all objects are initialized.
    /// </summary>
    private void Start()
    {
        if (autoStartFirstLoop && activePlayer != null)
        {
            StartNewLoop();
        }
    }
    
    /// <summary>
    /// Handle manual clone creation input and automatic loop timing.
    /// Checks for manual loop key input and automatic loop duration completion.
    /// </summary>
    private void Update()
    {
        if (IsMasterOfTime)
        {
            // Master of Time: F starts/stops recording and creates a clone with no padding or fade
            if (Keyboard.current[manualLoopKey].wasPressedThisFrame)
            {
                if (!actionRecorder.IsRecording)
                {
                    // Start recording
                    actionRecorder.StartRecording();
                    isLoopActive = true;
                    loopStartTime = Time.time;
                    OnNewLoopStarted?.Invoke();
                    OnLoopStarted?.Invoke();
                }
                else
                {
                    // Stop recording and create clone (no padding, no fade, no position reset)
                    actionRecorder.StopRecording();
                    CreateClone();
                    isLoopActive = false;
                    OnLoopEnded?.Invoke();
                }
                return;
            }
        }
        else
        {
            // Normal manual loop (L key)
            if (enableManualLooping && Keyboard.current[manualLoopKey].wasPressedThisFrame)
            {
                StartCoroutine(ManualLoopCoroutine());
                return;
            }

            // Automatic loop (guard against multiple triggers)
            if (isLoopActive && !isAutoLooping && Time.time - loopStartTime >= loopDuration)
            {
                StartCoroutine(AutomaticLoopCoroutine());
            }
        }
    }

    private IEnumerator ManualLoopCoroutine()
    {
        // Fade out and disable input before ending loop
        yield return StartCoroutine(LoopTransitionCoroutine(false));

        actionRecorder.StopRecording();
        actionRecorder.PadActionsToDuration(loopDuration);
        CreateClone();
        StartNewLoop(); // Start a new loop immediately

        // Reset all clones' replay to the start of their action list
        foreach (var clone in allClones)
        {
            if (clone != null && !clone.IsStuck)
                clone.StartReplay();
        }
    }

    private IEnumerator AutomaticLoopCoroutine()
    {
        isAutoLooping = true; // Set guard

        // Fade out and disable input before ending loop
        yield return StartCoroutine(LoopTransitionCoroutine(false));

        CreateClone();
        StartNewLoop();

        isAutoLooping = false; // Release guard
    }

    // Master of Time coroutine: player triggers their own loop, position is NOT reset
    private IEnumerator MasterOfTimeLoopCoroutine()
    {
        // Fade out and disable input before ending loop
        yield return StartCoroutine(LoopTransitionCoroutine(false));

        actionRecorder.StopRecording();
        actionRecorder.PadActionsToDuration(loopDuration);
        CreateClone();
        StartNewLoop_MasterOfTime(); // Custom loop start for Master of Time

        // Reset all clones' replay to the start of their action list
        foreach (var clone in allClones)
        {
            if (clone != null && !clone.IsStuck)
                clone.StartReplay();
        }
    }
    
    /// <summary>
    /// Start a new recording loop, creating a clone from previous actions if any were recorded.
    /// Stops the current recording, fires lifecycle events, and begins recording new actions.
    /// This method is called both automatically (every loopDuration seconds) and manually (L key).
    /// </summary>
    public void StartNewLoop()
    {
        if (actionRecorder == null) return;

        // End previous loop if one was active
        if (isLoopActive)
        {
            OnLoopEnded?.Invoke();
        }

        // Stop recording current actions to prepare for clone creation
        if (actionRecorder.IsRecording)
        {
            actionRecorder.StopRecording();
        }

        // Begin new loop state
        isLoopActive = true;
        loopStartTime = Time.time;

        // Start recording new actions for the next potential clone
        actionRecorder.StartRecording();

        // Notify other systems that a new loop has started
        OnNewLoopStarted?.Invoke();
        OnLoopStarted?.Invoke();

        // Fade in and enable input after starting new loop
        StartLoopTransition(true);
    }

    // Custom StartNewLoop for Master of Time (does NOT reset player position)
    private void StartNewLoop_MasterOfTime()
    {
        if (actionRecorder == null) return;

        // End previous loop if one was active
        if (isLoopActive)
        {
            OnLoopEnded?.Invoke();
        }

        // Stop recording current actions to prepare for clone creation
        if (actionRecorder.IsRecording)
        {
            actionRecorder.StopRecording();
        }

        // Begin new loop state
        isLoopActive = true;
        loopStartTime = Time.time;

        // Start recording new actions for the next potential clone
        actionRecorder.StartRecording();

        // Notify other systems that a new loop has started
        OnNewLoopStarted?.Invoke();
        OnLoopStarted?.Invoke();

        // Fade in and enable input after starting new loop
        StartLoopTransition(true);
    }
    
    /// <summary>
    /// Create a new clone that will replay the previously recorded player actions.
    /// Handles clone limit enforcement by destroying the oldest clone if necessary.
    /// The clone will spawn at the CloneManager's position and begin replaying immediately.
    /// </summary>
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
        
        // Enforce clone limit by removing oldest clone (FIFO queue behavior)
        if (allClones.Count >= maxClones)
        {
            if (allClones[0] != null)
            {
                Destroy(allClones[0].gameObject);
            }
            allClones.RemoveAt(0);
        }

        GameObject cloneObject;
        
        // Create the clone GameObject using either custom prefab or player copy
        if (clonePrefab != null)
        {
            // Use the specified prefab (allows for custom clone appearance/behavior)
            cloneObject = Instantiate(clonePrefab, this.transform.position, activePlayer.transform.rotation);
        }
        else
        {
            // Clone the current player object (copies all components and settings)
            cloneObject = Instantiate(activePlayer.gameObject, this.transform.position, activePlayer.transform.rotation);
        }
        
        // Ensure the cloned object has a Clone component for replay behavior
        Clone clone = cloneObject.GetComponent<Clone>();
        if (clone == null)
        {
            clone = cloneObject.AddComponent<Clone>();
        }
        
        // Initialize the clone with the recorded actions and start replay
        List<PlayerAction> recordedActions = actionRecorder.GetRecordedActions();
        clone.InitializeClone(recordedActions, nextCloneIndex++);
        
        // Add to our managed collection and start the replay
        allClones.Add(clone);
        clone.StartReplay();
        
        // Only reset player position if NOT Master of Time
        if (!IsMasterOfTime)
            activePlayer.transform.position = this.transform.position;

        // Notify other systems that a new clone was created
        OnCloneCreated?.Invoke(clone);
    }
    
    /// <summary>
    /// Mark a clone as "stuck" at a specific goal, making it a permanent part of the puzzle.
    /// Stuck clones will not be destroyed when new clones are created and become static puzzle elements.
    /// Called by Goal objects when a clone reaches them.
    /// </summary>
    /// <param name="clone">The clone to mark as stuck</param>
    /// <param name="goal">The goal where the clone is stuck</param>
    public void SetCloneStuck(Clone clone, Goal goal)
    {
        if (clone != null && allClones.Contains(clone))
        {
            clone.SetStuck(goal);
            OnCloneStuck?.Invoke(clone);
            Debug.Log($"Clone {clone.CloneIndex} set as stuck at goal");
        }
    }
    
    /// <summary>
    /// Retract the clone history back to the last non-stuck clone.
    /// This allows players to undo recent clone creations while preserving stuck clones.
    /// Useful for puzzle solving when recent actions need to be undone.
    /// </summary>
    /// <returns>True if retraction was successful, false if no valid retraction point exists</returns>
    public bool RetractToLastClone()
    {
        if (allClones.Count == 0) return false;
        
        // Find the last non-stuck clone to retract to
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
        
        // Remove all clones created after the retraction point
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
    
    /// <summary>
    /// Pause all active clones, stopping their replay without destroying them.
    /// Used by game pause systems or special puzzle mechanics.
    /// </summary>
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
    
    /// <summary>
    /// Resume replay for all non-stuck clones.
    /// Stuck clones remain static and are not resumed.
    /// </summary>
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
    
    /// <summary>
    /// Destroy all clones and reset the clone system to initial state.
    /// Used for level resets or complete puzzle restarts.
    /// </summary>
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
    
    /// <summary>
    /// Remove any null references from the clone list.
    /// This handles cases where clones were destroyed externally or by other systems.
    /// </summary>
    private void CleanupCloneList()
    {
        allClones.RemoveAll(clone => clone == null);
    }
    
    /// <summary>
    /// Set the duration for automatic loop creation.
    /// Clamps to minimum of 1 second to prevent system instability.
    /// </summary>
    /// <param name="newDuration">New loop duration in seconds</param>
    public void SetLoopDuration(float newDuration)
    {
        loopDuration = Mathf.Max(1f, newDuration);
        Debug.Log($"Loop duration set to {loopDuration} seconds");
    }
    
    // Properties for external access to clone system state
    
    /// <summary>
    /// Get a copy of all active clones. Automatically cleans up null references.
    /// Returns a new list to prevent external modification of the internal collection.
    /// </summary>
    public List<Clone> GetAllClones() {
        CleanupCloneList();
        return new List<Clone>(allClones);
    }
    
    /// <summary>
    /// Get the most recently created clone (last in the creation order).
    /// Returns null if no clones exist.
    /// </summary>
    public Clone GetActiveClone() => allClones.Count > 0 ? allClones[allClones.Count - 1] : null;
    
    /// <summary>
    /// Total number of active clones (includes both replaying and stuck clones).
    /// Automatically cleans up null references before counting.
    /// </summary>
    public int TotalClones {
        get {
            CleanupCloneList();
            return allClones.Count;
        }
    }
    
    /// <summary>
    /// Number of clones that are stuck at goals and acting as static puzzle elements.
    /// </summary>
    public int StuckClones {
        get {
            CleanupCloneList();
            return allClones.FindAll(c => c != null && c.IsStuck).Count;
        }
    }
    
    /// <summary>
    /// The index that will be assigned to the next created clone.
    /// Represents the total number of clones created since scene start.
    /// </summary>
    public int ActiveCloneIndex => nextCloneIndex - 1;
    
    /// <summary>
    /// How long the current loop has been running (in seconds).
    /// Returns 0 if no loop is currently active.
    /// </summary>
    public float CurrentLoopTime => isLoopActive ? Time.time - loopStartTime : 0f;
    
    /// <summary>
    /// Time remaining before the next automatic clone creation (in seconds).
    /// Returns 0 if no loop is currently active.
    /// </summary>
    public float TimeUntilNextLoop => isLoopActive ? loopDuration - CurrentLoopTime : 0f;
    
    /// <summary>
    /// The configured duration for automatic loops in seconds.
    /// </summary>
    public float LoopDuration => loopDuration;
    
    /// <summary>
    /// Whether a recording loop is currently active.
    /// </summary>
    public bool IsLoopActive => isLoopActive;

    public void StartLoopTransition(bool isLoopStart)
    {
        if (!isTransitioning)
            StartCoroutine(LoopTransitionCoroutine(isLoopStart));
    }

    private IEnumerator LoopTransitionCoroutine(bool isLoopStart)
    {
        isTransitioning = true;

        // Disable player input
        if (activePlayer != null)
            activePlayer.enabled = false;

        if (cameraController != null)
        {
            if (isLoopStart)
            {
                cameraController.FadeIn();
                yield return new WaitForSeconds(cameraController.fadeInDuration);
            }
            else
            {
                // Play audio feedback for clone creation
                if (cloneCreateSound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(cloneCreateSound);
                }
                cameraController.FadeOut();
                yield return new WaitForSeconds(cameraController.fadeOutDuration);
            }
        }
        else
        {
            // Fallback: short delay if no camera controller
            yield return new WaitForSeconds(0.5f);
        }

        // Enable player input after fade in
        if (isLoopStart && activePlayer != null)
            activePlayer.enabled = true;

        isTransitioning = false;
    }
    
    /// <summary>
    /// Draw debug gizmos in the scene view to visualize loop progress.
    /// Shows a growing sphere that indicates how close the current loop is to completion.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!isLoopActive) return;
        
        // Draw a visual indicator of loop progress as an expanding sphere
        Gizmos.color = Color.cyan;
        float progress = CurrentLoopTime / loopDuration;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f + progress * 0.5f);
    }
}