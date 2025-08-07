using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the clone loop system that records player actions and creates clones to replay them.
/// Handles automatic loops, manual clone creation, and integrates with camera and audio effects.
/// 
/// Core responsibilities:
/// - Recording player actions through ActionRecorder
/// - Creating and managing clone lifecycle
/// - Coordinating camera zoom and visual effects during recording
/// - Managing automatic and manual loop creation
/// - Providing events for other systems to react to loop changes
/// </summary>
public class CloneManager : MonoBehaviour
{
    [Header("Loop Settings")]
    [SerializeField] private float loopDuration = 15f;
    [SerializeField] private int maxClones = 10;
    [SerializeField] private bool autoStartFirstLoop = true;
    [SerializeField] private bool enableManualLooping = true;
    [SerializeField] private Key manualLoopKey = Key.F;
    [SerializeField] private bool masterOfTimeMode = false;

    [Header("Visual Effects")]
    [SerializeField] private Unity.Cinemachine.CinemachineCamera cineCamera;
    [SerializeField] private float zoomInAmount = -2f;
    [SerializeField] private float zoomDuration = 0.5f;
    [SerializeField] private Animator recordingVolume; // Animator controlling bloom/volume effects

    [Header("Clone Setup")]
    [SerializeField] private GameObject clonePrefab;

    [Header("Audio")]
    [SerializeField] private AudioClip cloneCreateSound;

    // Core system references
    private List<Clone> allClones = new List<Clone>();
    private PlayerController activePlayer;
    private ActionRecorder actionRecorder;
    private CameraController cameraController;

    // Loop state tracking
    private float loopStartTime;
    private bool isRecording = false;
    private int nextCloneIndex = 0;
    private bool isProcessingLoop = false;
    private Sprite recordingStartSprite = null; // Store sprite when recording starts

    // Camera zoom state
    private float originalOrthoSize;
    private float originalFOV;
    private Coroutine cameraZoomCoroutine;

    // Events for external system integration
    public System.Action<Clone> OnCloneCreated;
    public System.Action<Clone> OnCloneStuck;
    public System.Action OnNewLoopStarted;
    public static event System.Action OnLoopStarted;
    public static event System.Action OnLoopEnded;
    public static CloneManager instance { get; private set; }

    #region Unity Lifecycle

    /// <summary>
    /// Initialize core references and validate required components.
    /// </summary>
    private void Awake()
    {
        instance = this;
        
        // Find required components
        activePlayer = FindFirstObjectByType<PlayerController>();
        cameraController = FindFirstObjectByType<CameraController>();

        if (activePlayer == null)
        {
            Debug.LogError("CloneManager: No PlayerController found in scene!");
            return;
        }

        // Set player spawn position and ensure ActionRecorder is present
        activePlayer.transform.position = this.transform.position;
        actionRecorder = activePlayer.GetComponent<ActionRecorder>();
        if (actionRecorder == null)
            actionRecorder = activePlayer.gameObject.AddComponent<ActionRecorder>();
    }

    /// <summary>
    /// Initialize camera settings and start first loop if configured.
    /// </summary>
    private void Start()
    {
        InitializeCameraSettings();
        
        if (autoStartFirstLoop && activePlayer != null)
            StartNewLoop();
    }

    /// <summary>
    /// Handle input for manual loop creation and monitor automatic loop timing.
    /// </summary>
    private void Update()
    {
        HandleInput();
        UpdateAutoLoopTiming();
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// Process manual loop input based on current mode.
    /// </summary>
    private void HandleInput()
    {
        if (!Keyboard.current[manualLoopKey].wasPressedThisFrame)
            return;

        if (masterOfTimeMode)
        {
            HandleMasterOfTimeInput();
        }
        else if (enableManualLooping)
        {
            StartCoroutine(CreateManualLoop());
        }
    }

    /// <summary>
    /// Handle input in Master of Time mode - toggle recording state.
    /// </summary>
    private void HandleMasterOfTimeInput()
    {
        if (!isRecording)
        {
            StartNewLoop();
        }
        else
        {
            StartCoroutine(EndCurrentLoop());
        }
    }

    #endregion

    #region Loop Management

    /// <summary>
    /// Check if automatic loop should trigger and execute it.
    /// </summary>
    private void UpdateAutoLoopTiming()
    {
        if (masterOfTimeMode || !isRecording || isProcessingLoop)
            return;

        if (Time.time - loopStartTime >= loopDuration)
        {
            StartCoroutine(CreateAutomaticLoop());
        }
    }

    /// <summary>
    /// Start a new recording loop with visual effects.
    /// </summary>
    public void StartNewLoop()
    {
        if (isProcessingLoop)
            return;

        // Stop any current recording
        if (actionRecorder != null && actionRecorder.IsRecording)
            actionRecorder.StopRecording();

        // Set up recording state
        isRecording = true;
        loopStartTime = Time.time;

        // Apply visual effects
        StartRecordingEffects();
        
        // Fire events
        OnNewLoopStarted?.Invoke();
        OnLoopStarted?.Invoke();

    // For Master of Time mode, start recording immediately AND play transition
    if (masterOfTimeMode && actionRecorder != null)
    {
        // Capture the sprite when recording starts
        CaptureRecordingStartSprite();
        actionRecorder.StartRecording();
        // Still play transition but don't wait for it to complete
        StartCoroutine(PlayTransitionEffects(true));
    }
    else
    {
        // Normal mode: Start recording after transition
        StartCoroutine(BeginRecordingAfterTransition());
    }
    }

    /// <summary>
    /// Begin recording after visual transition completes.
    /// </summary>
    private IEnumerator BeginRecordingAfterTransition()
    {
        yield return StartCoroutine(PlayTransitionEffects(true));
        
        if (actionRecorder != null)
        {
            // Capture the sprite when recording starts
            CaptureRecordingStartSprite();
            actionRecorder.StartRecording();
        }
    }

    /// <summary>
    /// Capture the player's sprite when recording starts for accurate start ghost markers.
    /// </summary>
    private void CaptureRecordingStartSprite()
    {
        if (activePlayer != null)
        {
            SpriteRenderer playerSpriteRenderer = activePlayer.GetComponent<SpriteRenderer>();
            if (playerSpriteRenderer != null)
            {
                recordingStartSprite = playerSpriteRenderer.sprite;
                Debug.Log($"CloneManager: Captured recording start sprite: {recordingStartSprite?.name}");
            }
        }
    }

    #endregion

    #region Loop Creation Coroutines

    /// <summary>
    /// Create a clone through manual input (L key).
    /// </summary>
    private IEnumerator CreateManualLoop()
    {
        yield return StartCoroutine(EndCurrentLoop());
        StartNewLoop();
        ResumeAllClones();
    }

    /// <summary>
    /// Create a clone through automatic timing.
    /// </summary>
    private IEnumerator CreateAutomaticLoop()
    {
        isProcessingLoop = true;
        yield return StartCoroutine(EndCurrentLoop());
        StartNewLoop();
        isProcessingLoop = false;
    }

    /// <summary>
    /// End the current loop and create a clone.
    /// </summary>
    private IEnumerator EndCurrentLoop()
    {
        if (!isRecording)
            yield break;

        // Stop recording and apply effects
        StopRecordingEffects();
        
        // Play transition and create clone
        yield return StartCoroutine(PlayTransitionEffects(false));
        
        if (actionRecorder != null)
        {
            actionRecorder.StopRecording();
            if (!masterOfTimeMode)  actionRecorder.PadActionsToDuration(loopDuration);
        }

        CreateClone();
        
        // Reset player position if not in Master of Time mode
        if (!masterOfTimeMode && activePlayer != null)
            activePlayer.transform.position = this.transform.position;

        isRecording = false;
        OnLoopEnded?.Invoke();
    }

    #endregion

    #region Visual Effects

    /// <summary>
    /// Initialize camera settings for zoom effects.
    /// Note: originalOrthoSize/originalFOV will be updated dynamically before each zoom
    /// to capture the current camera state (which may have been modified by other scripts).
    /// </summary>
    private void InitializeCameraSettings()
    {
        if (cineCamera != null)
        {
            originalOrthoSize = cineCamera.Lens.OrthographicSize;
            originalFOV = cineCamera.Lens.FieldOfView;
        }
    }

    /// <summary>
    /// Start visual effects for recording phase.
    /// </summary>
    private void StartRecordingEffects()
    {
        ApplyCameraZoom();
        
        if (recordingVolume != null)
            recordingVolume.SetBool("Recording", true);
    }

    /// <summary>
    /// Stop visual effects for recording phase.
    /// </summary>
    private void StopRecordingEffects()
    {
        RestoreCameraZoom();
        
        if (recordingVolume != null)
            recordingVolume.SetBool("Recording", false);
    }

    /// <summary>
    /// Play transition effects (player animation and camera fade).
    /// </summary>
    private IEnumerator PlayTransitionEffects(bool isLoopStart)
    {
        if (isLoopStart)
        {
            yield return StartCoroutine(PlayLoopStartTransition());
        }
        else
        {
            yield return StartCoroutine(PlayLoopEndTransition());
        }
    }

    /// <summary>
    /// Handle visual transition when starting a loop.
    /// </summary>
    private IEnumerator PlayLoopStartTransition()
    {
        if (activePlayer != null)
        {
            // Trigger pickup animation and immobilize player briefly
            Animator playerAnimator = activePlayer.GetComponent<Animator>();
            if (playerAnimator != null)
                playerAnimator.SetTrigger("pickup");

            CharacterController2D character = activePlayer.GetComponent<CharacterController2D>();
            if (character != null)
            {
                character.Immobile = true;
                character.SetPhysicsState(character.transform.position, Vector2.zero, Vector2.zero);
            }

            yield return new WaitForSeconds(0.5f); // Wait for animation

            // Re-enable player movement
            if (character != null)
                character.Immobile = false;
        }
    }

    /// <summary>
    /// Handle visual transition when ending a loop.
    /// </summary>
    private IEnumerator PlayLoopEndTransition()
    {
        // Play clone creation sound
        if (cloneCreateSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(cloneCreateSound, 0.4f); // will add variable for sound soon

        // Wait for fade effect
        if (cameraController != null)
            yield return new WaitForSeconds(cameraController.fadeOutDuration);
        else
            yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Handle retract transition with animation, audio, and player movement.
    /// Moves player to clone's last recorded action position and applies all recorded state.
    /// </summary>
    private IEnumerator PlayRetractTransition(Clone targetClone)
    {
        if (activePlayer != null)
        {
            // Trigger retract animation and immobilize player briefly
            Animator playerAnimator = activePlayer.GetComponent<Animator>();
            if (playerAnimator != null)
                playerAnimator.SetTrigger("pickup"); // Reuse pickup animation for retract

            CharacterController2D character = activePlayer.GetComponent<CharacterController2D>();
            if (character != null)
            {
                character.Immobile = true;
                character.SetPhysicsState(character.transform.position, Vector2.zero, Vector2.zero);
            }

            // Play retract sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRetractSound();
            }

            yield return new WaitForSeconds(0.5f); // Wait for animation

            // Move player to target clone's last recorded action position and apply all state
            if (targetClone != null && targetClone.LastAction.HasValue)
            {
                PlayerAction lastAction = targetClone.LastAction.Value;
                Vector3 targetPosition = lastAction.position;
                activePlayer.transform.position = targetPosition;
                
                // Apply all the physical state from the last action
                if (character != null)
                {
                    character.SetPhysicsState(targetPosition, lastAction.speed, lastAction.externalForce);
                    
                    // Update animator state to match the last action
                    Animator animator = character.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetFloat("hSpeed", lastAction.speed.x);
                        animator.SetFloat("vSpeed", lastAction.speed.y);
                        animator.SetBool("grounded", lastAction.isGrounded);
                        animator.SetBool("dashing", lastAction.isDashing);
                        animator.SetBool("onWall", lastAction.isOnWall);
                        animator.SetBool("facingRight", lastAction.facingRight);
                    }
                }
                
                Debug.Log($"Retracted to clone {targetClone.CloneIndex} last action position: {targetPosition} with velocity: {lastAction.speed}");
            }
            else if (targetClone != null)
            {
                // Fallback to current clone position if no recorded actions
                Vector3 targetPosition = targetClone.transform.position;
                activePlayer.transform.position = targetPosition;
                
                if (character != null)
                {
                    character.SetPhysicsState(targetPosition, Vector2.zero, Vector2.zero);
                }
                
                Debug.LogWarning($"No recorded actions found for clone {targetClone.CloneIndex}, using current position");
            }

            // Re-enable player movement
            if (character != null)
                character.Immobile = false;
        }

        // Wait for any fade effects
        if (cameraController != null)
            yield return new WaitForSeconds(cameraController.fadeOutDuration);
        else
            yield return new WaitForSeconds(0.3f);
    }

    #endregion

    #region Camera Zoom Effects

    /// <summary>
    /// Apply smooth zoom-in effect during recording.
    /// </summary>
    private void ApplyCameraZoom()
    {
        if (cineCamera == null)
            return;

        // Stop any existing zoom coroutine
        if (cameraZoomCoroutine != null)
        {
            StopCoroutine(cameraZoomCoroutine);
        }

        // Capture current camera values before zooming (in case other scripts changed them)
        originalOrthoSize = cineCamera.Lens.OrthographicSize;
        originalFOV = cineCamera.Lens.FieldOfView;

        // Start zoom-in effect
        float targetOrthoSize = Mathf.Max(0.1f, originalOrthoSize + zoomInAmount);
        float targetFOV = Mathf.Max(1f, originalFOV + zoomInAmount * 10f);
        
        cameraZoomCoroutine = StartCoroutine(SmoothCameraZoom(
            originalOrthoSize, targetOrthoSize,
            originalFOV, targetFOV,
            zoomDuration
        ));
    }

    /// <summary>
    /// Restore camera to original zoom level.
    /// </summary>
    private void RestoreCameraZoom()
    {
        if (cineCamera == null)
            return;

        // Stop any existing zoom coroutine
        if (cameraZoomCoroutine != null)
        {
            StopCoroutine(cameraZoomCoroutine);
        }

        // Start zoom-out effect
        cameraZoomCoroutine = StartCoroutine(SmoothCameraZoom(
            cineCamera.Lens.OrthographicSize, originalOrthoSize,
            cineCamera.Lens.FieldOfView, originalFOV,
            zoomDuration
        ));
    }

    /// <summary>
    /// Smoothly interpolate camera zoom between two values.
    /// </summary>
    private IEnumerator SmoothCameraZoom(float startOrthoSize, float targetOrthoSize, 
                                       float startFOV, float targetFOV, 
                                       float duration)
    {
        float elapsedTime = 0;
        bool isOrthographic = cineCamera.Lens.Orthographic;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsedTime / duration);
            
            if (isOrthographic)
                cineCamera.Lens.OrthographicSize = Mathf.Lerp(startOrthoSize, targetOrthoSize, t);
            else
                cineCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            
            yield return null;
        }
        
        // Ensure final values are exact
        if (isOrthographic)
            cineCamera.Lens.OrthographicSize = targetOrthoSize;
        else
            cineCamera.Lens.FieldOfView = targetFOV;
        
        cameraZoomCoroutine = null;
    }

    #endregion

    #region Clone Management

    /// <summary>
    /// Create a new clone from recorded actions.
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

        // Ensure we don't exceed maximum clone count
        if (allClones.Count >= maxClones)
        {
            RemoveOldestClone();
        }

        // Create clone object
        GameObject cloneObject = clonePrefab != null
            ? Instantiate(clonePrefab, this.transform.position, activePlayer.transform.rotation)
            : Instantiate(activePlayer.gameObject, this.transform.position, activePlayer.transform.rotation);

        // Set up clone component
        Clone clone = cloneObject.GetComponent<Clone>();
        if (clone == null)
            clone = cloneObject.AddComponent<Clone>();

        // Initialize clone with recorded actions and original player sprite
        List<PlayerAction> recordedActions = actionRecorder.GetRecordedActions();
        
        // Use the sprite that was captured when recording started
        clone.InitializeClone(recordedActions, nextCloneIndex++, recordingStartSprite);
        allClones.Add(clone);
        clone.StartReplay();

        OnCloneCreated?.Invoke(clone);
    }

    /// <summary>
    /// Remove the oldest clone to make room for new ones.
    /// </summary>
    private void RemoveOldestClone()
    {
        if (allClones.Count > 0 && allClones[0] != null)
        {
            Destroy(allClones[0].gameObject);
            allClones.RemoveAt(0);
        }
    }

    /// <summary>
    /// Mark a clone as stuck at a goal.
    /// </summary>
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
    /// Resume replay for all non-stuck clones.
    /// </summary>
    public void ResumeAllClones()
    {
        foreach (var clone in allClones)
        {
            if (clone != null && !clone.IsStuck)
                clone.StartReplay();
        }
    }

    /// <summary>
    /// Pause all clone replays.
    /// </summary>
    public void PauseAllClones()
    {
        foreach (Clone clone in allClones)
            if (clone != null)
                clone.StopReplay();
    }

    /// <summary>
    /// Destroy all clones and reset the system.
    /// </summary>
    public void DestroyAllClones()
    {
        foreach (Clone clone in allClones)
            if (clone != null)
                Destroy(clone.gameObject);
        
        allClones.Clear();
        nextCloneIndex = 0;
        Debug.Log("All clones destroyed");
    }

    /// <summary>
    /// Remove clones back to the last non-stuck clone and move player to that clone's current position.
    /// Includes animation and audio effects for enhanced feedback.
    /// </summary>
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
        
        // Start retract transition with animation and audio
        StartCoroutine(PerformRetractTransition(lastClone));
        
        return true;
    }

    /// <summary>
    /// Perform the retract transition with animation, audio, and player movement.
    /// </summary>
    private IEnumerator PerformRetractTransition(Clone targetClone)
    {
        // Stop current recording if active
        if (actionRecorder != null && actionRecorder.IsRecording)
            actionRecorder.StopRecording();
        
        isRecording = false;
        
        // Play retract transition effects
        yield return StartCoroutine(PlayRetractTransition(targetClone));
        
        // Remove newer clones
        for (int i = allClones.Count - 1; i >= 0; i--)
        {
            if (allClones[i].CloneIndex > targetClone.CloneIndex - 1)
            {
                Destroy(allClones[i].gameObject);
                allClones.RemoveAt(i);
            }
        }
        
        Debug.Log($"Retracted to clone {targetClone.CloneIndex}");
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Clean up null references in the clone list.
    /// </summary>
    private void CleanupCloneList()
    {
        allClones.RemoveAll(clone => clone == null);
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Total number of active clones.
    /// </summary>
    public int TotalClones 
    { 
        get 
        { 
            CleanupCloneList(); 
            return allClones.Count; 
        } 
    }

    /// <summary>
    /// Number of clones that are stuck at goals.
    /// </summary>
    public int StuckClones 
    { 
        get 
        { 
            CleanupCloneList(); 
            return allClones.FindAll(c => c != null && c.IsStuck).Count; 
        } 
    }

    /// <summary>
    /// Index of the most recently created clone.
    /// </summary>
    public int ActiveCloneIndex => nextCloneIndex - 1;

    /// <summary>
    /// Current time elapsed in the active loop.
    /// </summary>
    public float CurrentLoopTime => isRecording ? Time.time - loopStartTime : 0f;

    /// <summary>
    /// Time remaining until next automatic loop creation.
    /// </summary>
    public float TimeUntilNextLoop => isRecording ? loopDuration - CurrentLoopTime : 0f;

    /// <summary>
    /// Configured duration for each loop.
    /// </summary>
    public float LoopDuration => loopDuration;

    /// <summary>
    /// Whether a recording loop is currently active.
    /// </summary>
    public bool IsLoopActive => isRecording;

    #endregion
}