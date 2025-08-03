using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CloneManager : MonoBehaviour
{
    [Header("Loop Settings")]
    [SerializeField] private float loopDuration = 15f;
    [SerializeField] private int maxClones = 10;
    [Header("Cinemachine Effects")]
    [SerializeField] private Unity.Cinemachine.CinemachineCamera cineCamera;
    [SerializeField] private float zoomInAmount = -2f;
    [SerializeField] private Animator recordingVolume; // Animator controlling bloom/volume effects
    [Header("Clone Prefab")]
    [SerializeField] private GameObject clonePrefab;
    [Header("Audio")]
    [SerializeField] private AudioClip cloneCreateSound;

    [SerializeField] private bool autoStartFirstLoop = true;
    [SerializeField] private bool enableManualLooping = true;
    [SerializeField] private Key manualLoopKey = Key.F;
    [SerializeField] private bool IsMasterOfTime = false;

    private List<Clone> allClones = new List<Clone>();
    private PlayerController activePlayer;
    private ActionRecorder actionRecorder;
    private CameraController cameraController;

    private float loopStartTime;
    private bool isLoopActive = false;
    private int nextCloneIndex = 0;
    private bool isTransitioning = false;
    private bool isAutoLooping = false;

    private float originalOrthoSize;
    private float originalFOV;

    // Add these new fields
    private float trueOriginalOrthoSize;
    private float trueOriginalFOV;

    // Events
    public System.Action<Clone> OnCloneCreated;
    public System.Action<Clone> OnCloneStuck;
    public System.Action OnNewLoopStarted;
    public static event System.Action OnLoopStarted;
    public static event System.Action OnLoopEnded;
    public static CloneManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
        activePlayer = FindFirstObjectByType<PlayerController>();
        cameraController = FindFirstObjectByType<CameraController>();

        if (activePlayer == null)
        {
            Debug.LogError("CloneManager: No PlayerController found in scene!");
            return;
        }

        activePlayer.transform.position = this.transform.position;
        actionRecorder = activePlayer.GetComponent<ActionRecorder>();
        if (actionRecorder == null)
            actionRecorder = activePlayer.gameObject.AddComponent<ActionRecorder>();
    }

    private void Start()
    {
        if (autoStartFirstLoop && activePlayer != null)
            StartNewLoop();

        if (cineCamera != null)
        {
            // Save TRUE original values only once at start
            trueOriginalOrthoSize = cineCamera.Lens.OrthographicSize;
            trueOriginalFOV = cineCamera.Lens.FieldOfView;
            
            // Also set the working values
            originalOrthoSize = trueOriginalOrthoSize;
            originalFOV = trueOriginalFOV;
        }
    }

    private void Update()
    {
        if (IsMasterOfTime)
        {
            // Manual loop control only - no timer logic
            if (Keyboard.current[manualLoopKey].wasPressedThisFrame)
            {
                if (!actionRecorder.IsRecording)
                {
                    StartCoroutine(MasterOfTimeLoopCoroutine());
                }
                else
                {
                    StartLoopTransition(false);
                    StopRecordingAndRestoreZoom(); // Use the combined method here
                    CreateClone();
                    isLoopActive = false;
                    OnLoopEnded?.Invoke();
                }
            }
            // Remove the automatic timer logic below for Master of Time mode
            // The loop should only end when the player presses the key
        }
        else
        {
            // Manual loop (L key)
            if (enableManualLooping && Keyboard.current[manualLoopKey].wasPressedThisFrame)
            {
                StartCoroutine(ManualLoopCoroutine());
                return;
            }

            // Automatic loop
            if (isLoopActive && !isAutoLooping && Time.time - loopStartTime >= loopDuration)
            {
                StartCoroutine(AutomaticLoopCoroutine());
            }
        }
    }

    private IEnumerator ManualLoopCoroutine()
    {
        // First transition out (fade out) before creating clone
        StartLoopTransition(false);
        yield return new WaitForSeconds(cameraController != null ? cameraController.fadeOutDuration : 0.5f);
        
        actionRecorder.StopRecording();
        actionRecorder.PadActionsToDuration(loopDuration);
        RestoreCameraZoom();
        CreateClone();
        StartNewLoop();
        
        foreach (var clone in allClones)
            if (clone != null && !clone.IsStuck)
                clone.StartReplay();
    }

    private IEnumerator AutomaticLoopCoroutine()
    {
        isAutoLooping = true;
        
        // First transition out (fade out) before creating clone
        StartLoopTransition(false);
        yield return new WaitForSeconds(cameraController != null ? cameraController.fadeOutDuration : 0.5f);
        
        RestoreCameraZoom(); // Add this
        CreateClone();
        StartNewLoop();
        isAutoLooping = false;
    }

    private IEnumerator MasterOfTimeLoopCoroutine()
    {
    // Turn off recording effect first
    if (recordingVolume != null)
        recordingVolume.SetBool("Recording", false);
    
    // Use the combined method here
    StopRecordingAndRestoreZoom();
    ApplyCameraZoom(); // Changed from RestoreCameraZoom to apply zoom effect
            // IMPORTANT: Enable recording effect
        if (recordingVolume != null)
            recordingVolume.SetBool("Recording", true);

    
    // Do transition and wait briefly
        StartLoopTransition(false);
    yield return new WaitForSeconds(0.5f);
    
    actionRecorder.PadActionsToDuration(loopDuration);
    CreateClone();
    
    // Then start the new loop
    StartNewLoop_MasterOfTime();
    
    foreach (var clone in allClones)
        if (clone != null && !clone.IsStuck)
            clone.StartReplay();
    }

    public void StartNewLoop()
    {
        if (actionRecorder == null) return;
        if (isLoopActive)
            OnLoopEnded?.Invoke();

        if (actionRecorder.IsRecording)
            actionRecorder.StopRecording();

        isLoopActive = true;
        loopStartTime = Time.time;

        OnNewLoopStarted?.Invoke();
        OnLoopStarted?.Invoke();

        // IMPORTANT: Update original values before zooming
        if (cineCamera != null)
        {
            // Capture current camera values before zooming
            originalOrthoSize = cineCamera.Lens.OrthographicSize;
            originalFOV = cineCamera.Lens.FieldOfView;
            
            // Now apply zoom
            if (cineCamera.Lens.Orthographic)
                cineCamera.Lens.OrthographicSize = Mathf.Max(0.1f, originalOrthoSize + zoomInAmount);
            else
                cineCamera.Lens.FieldOfView = Mathf.Max(1f, originalFOV + zoomInAmount * 10f);
        }
        
        // Enable recording effects
        if (recordingVolume != null)
            recordingVolume.SetBool("Recording", true);

        // Start fade-in transition - recording will start after transition
        StartLoopTransition(true);
    }

    private void StartNewLoop_MasterOfTime()
    {
        if (actionRecorder == null) return;
        if (isLoopActive)
            OnLoopEnded?.Invoke();

        if (actionRecorder.IsRecording)
            actionRecorder.StopRecording();

        
        OnNewLoopStarted?.Invoke();
        OnLoopStarted?.Invoke();

        // IMPORTANT: For Master of Time, ZOOM IN just like in regular mode


        
        // Start fade-in transition - recording will start after transition
        StartLoopTransition(true);
    }

    public void StartLoopTransition(bool isLoopStart)
    {
        if (!isTransitioning)
            StartCoroutine(LoopTransitionCoroutine(isLoopStart));
    }

    private IEnumerator LoopTransitionCoroutine(bool isLoopStart)
    {
        isTransitioning = true;
        
        if (cameraController != null)
        {
            if (isLoopStart)
            {
                // Get player animator and trigger pickup animation
                if (activePlayer != null)
                {
                    Animator playerAnimator = activePlayer.GetComponent<Animator>();
                    if (playerAnimator != null)
                    {
                        playerAnimator.SetTrigger("pickup");
                    }
                    
                    // Completely stop player movement
                   // activePlayer.enabled = false;
                    CharacterController2D character = activePlayer.GetComponent<CharacterController2D>();
                    if (character != null)
                    {
                        character.Immobile = true;
                        // Force stop movement
                        character.SetPhysicsState(character.transform.position, Vector2.zero, Vector2.zero);
                    }
                }
                
                if (recordingVolume != null)
                    recordingVolume.SetBool("Recording", true);
                
                //cameraController.FadeIn();
                yield return new WaitForSeconds(0f); // Wait for pickup animation to complete
                
                // Re-enable player input after fade completes and start recording
                if (activePlayer != null)
                {
                    activePlayer.enabled = true;
                    CharacterController2D character = activePlayer.GetComponent<CharacterController2D>();
                    if (character != null)
                    {
                        character.Immobile = false;
                    }
                    
                    // NOW start recording after transition is complete
                    if (actionRecorder != null)
                    {
                        actionRecorder.StartRecording();
                    }
                }
            }
            else
            {
                if (cloneCreateSound != null && AudioManager.Instance != null)
                    AudioManager.Instance.PlaySound(cloneCreateSound);
                if (recordingVolume != null)
                    recordingVolume.SetBool("Recording", false);
               // cameraController.FadeOut();
                yield return new WaitForSeconds(cameraController.fadeOutDuration);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        isTransitioning = false;
    }

    // Clone management
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
        if (allClones.Count >= maxClones)
        {
            if (allClones[0] != null)
                Destroy(allClones[0].gameObject);
            allClones.RemoveAt(0);
        }

        GameObject cloneObject = clonePrefab != null
            ? Instantiate(clonePrefab, this.transform.position, activePlayer.transform.rotation)
            : Instantiate(activePlayer.gameObject, this.transform.position, activePlayer.transform.rotation);

        Clone clone = cloneObject.GetComponent<Clone>();
        if (clone == null)
            clone = cloneObject.AddComponent<Clone>();

        List<PlayerAction> recordedActions = actionRecorder.GetRecordedActions();
        clone.InitializeClone(recordedActions, nextCloneIndex++);
        allClones.Add(clone);
        clone.StartReplay();

        if (!IsMasterOfTime)
            activePlayer.transform.position = this.transform.position;

        OnCloneCreated?.Invoke(clone);
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
        for (int i = allClones.Count - 1; i >= 0; i--)
        {
            if (allClones[i].CloneIndex > lastClone.CloneIndex)
            {
                Destroy(allClones[i].gameObject);
                allClones.RemoveAt(i);
            }
        }
        Debug.Log($"Retracted to clone {lastClone.CloneIndex}");
        return true;
    }

    public void PauseAllClones()
    {
        foreach (Clone clone in allClones)
            if (clone != null)
                clone.StopReplay();
    }

    public void ResumeAllClones()
    {
        foreach (Clone clone in allClones)
            if (clone != null && !clone.IsStuck)
                clone.StartReplay();
    }

    public void DestroyAllClones()
    {
        foreach (Clone clone in allClones)
            if (clone != null)
                Destroy(clone.gameObject);
        allClones.Clear();
        nextCloneIndex = 0;
        Debug.Log("All clones destroyed");
    }

    // Properties
    public int TotalClones { get { CleanupCloneList(); return allClones.Count; } }
    public int StuckClones { get { CleanupCloneList(); return allClones.FindAll(c => c != null && c.IsStuck).Count; } }
    public int ActiveCloneIndex => nextCloneIndex - 1;
    public float CurrentLoopTime => isLoopActive ? Time.time - loopStartTime : 0f;
    public float TimeUntilNextLoop => isLoopActive ? loopDuration - CurrentLoopTime : 0f;
    public float LoopDuration => loopDuration;
    public bool IsLoopActive => isLoopActive;
    
    // Helper methods
    private void CleanupCloneList()
    {
        allClones.RemoveAll(clone => clone == null);
    }

    // Add this method to restore camera zoom
    private void ApplyCameraZoom()
    {
        if (cineCamera != null)
        {
            // Save current values first
            originalOrthoSize = cineCamera.Lens.OrthographicSize;
            originalFOV = cineCamera.Lens.FieldOfView;
            
            // Start smooth zoom coroutine
            StopAllCameraZoomCoroutines();
            StartCoroutine(SmoothCameraZoom(
                originalOrthoSize, 
                Mathf.Max(0.1f, originalOrthoSize + zoomInAmount),
                originalFOV,
                Mathf.Max(1f, originalFOV + zoomInAmount * 10f),
                0.5f)); // Duration in seconds
            
            Debug.Log($"ZOOM IN: Starting smooth transition from {originalOrthoSize} to {originalOrthoSize + zoomInAmount}");
        }
    }

    private void RestoreCameraZoom()
    {
        if (cineCamera != null)
        {
            // Start smooth zoom coroutine
            StopAllCameraZoomCoroutines();
            StartCoroutine(SmoothCameraZoom(
                cineCamera.Lens.OrthographicSize, 
                trueOriginalOrthoSize,
                cineCamera.Lens.FieldOfView,
                trueOriginalFOV,
                0.5f)); // Duration in seconds
            
            Debug.Log($"ZOOM RESTORE: Starting smooth transition to {trueOriginalOrthoSize}");
        }
    }

    private void StopAllCameraZoomCoroutines()
    {
        // Stop any existing zoom coroutines to prevent conflicts
        StopCoroutine("SmoothCameraZoom");
    }

    private IEnumerator SmoothCameraZoom(float startOrthoSize, float targetOrthoSize, 
                                    float startFOV, float targetFOV, 
                                    float duration)
    {
        float elapsedTime = 0;
        bool isOrthographic = cineCamera.Lens.Orthographic;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration); // Normalized time (0-1)
            
            // Apply smooth easing
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            if (isOrthographic)
                cineCamera.Lens.OrthographicSize = Mathf.Lerp(startOrthoSize, targetOrthoSize, smoothT);
            else
                cineCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, smoothT);
            
            yield return null;
        }
        
        // Ensure we end at exactly the target value
        if (isOrthographic)
            cineCamera.Lens.OrthographicSize = targetOrthoSize;
        else
            cineCamera.Lens.FieldOfView = targetFOV;
    }

    // Add this helper method that combines stopping recording and restoring camera zoom
    private void StopRecordingAndRestoreZoom()
    {
        if (actionRecorder != null && actionRecorder.IsRecording)
        {
            actionRecorder.StopRecording();
            RestoreCameraZoom(); // Zoom out when stopping recording
            Debug.Log("Recording stopped and camera zoom restored");
        }
    }
}