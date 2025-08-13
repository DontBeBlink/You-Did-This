using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GMTK2025.ActionRecording
{
    /// <summary>
    /// Manages the action recording and clone replay system.
    /// Handles recording player actions and creating clones to replay them in a loop-based mechanic.
    /// 
    /// This refactored version uses interface abstractions to work with any character controller
    /// and input system, making it reusable across different Unity projects.
    /// 
    /// Core responsibilities:
    /// - Recording player actions through ActionRecorder
    /// - Creating and managing clone lifecycle
    /// - Coordinating visual effects during recording
    /// - Managing automatic and manual loop creation
    /// - Providing events for other systems to react to loop changes
    /// </summary>
    public class ActionRecordingManager : MonoBehaviour
    {
        [Header("Loop Settings")]
        [SerializeField] private float loopDuration = 15f;
        [SerializeField] private int maxClones = 10;
        [SerializeField] private bool autoStartFirstLoop = true;
        [SerializeField] private bool enableManualLooping = true;
        [SerializeField] private KeyCode manualLoopKey = KeyCode.F;

        [Header("Clone Setup")]
        [SerializeField] private GameObject clonePrefab;
        [SerializeField] private Transform playerSpawnPoint;

        [Header("System References")]
        [SerializeField] private MonoBehaviour playerControllerComponent; // Component implementing ICharacterController
        [SerializeField] private ActionRecorder actionRecorder;

        [Header("Audio")]
        [SerializeField] private AudioClip cloneCreateSound;
        [SerializeField] private AudioSource audioSource;

        [Header("Visual Effects")]
        [SerializeField] private bool enableRecordingEffects = true;
        [SerializeField] private Animator recordingEffectAnimator; // Optional animator for recording visual effects

        // Interface references
        private ICharacterController playerController;

        // Core system state
        private List<ActionReplayClone> allClones = new List<ActionReplayClone>();
        private bool isRecording = false;
        private int nextCloneIndex = 0;
        private bool isProcessingLoop = false;
        private float loopStartTime;

        // Events for external system integration
        public System.Action<ActionReplayClone> OnCloneCreated;
        public System.Action<ActionReplayClone> OnCloneStuck;
        public System.Action OnNewLoopStarted;
        public System.Action OnLoopEnded;

        // Singleton access for convenience (optional)
        public static ActionRecordingManager Instance { get; private set; }

        #region Unity Lifecycle

        /// <summary>
        /// Initialize core references and validate required components.
        /// </summary>
        private void Awake()
        {
            Instance = this;
            SetupComponentReferences();
        }

        private void SetupComponentReferences()
        {
            // Auto-detect player controller if not manually assigned
            if (playerControllerComponent != null)
                playerController = playerControllerComponent as ICharacterController;
            else
                playerController = FindObjectOfType<MonoBehaviour>() as ICharacterController;

            if (playerController == null)
            {
                Debug.LogError("ActionRecordingManager: No ICharacterController found in scene!");
                return;
            }

            // Set up action recorder if not assigned
            if (actionRecorder == null)
            {
                if (playerControllerComponent != null)
                {
                    actionRecorder = playerControllerComponent.GetComponent<ActionRecorder>();
                    if (actionRecorder == null)
                        actionRecorder = playerControllerComponent.gameObject.AddComponent<ActionRecorder>();
                }
                else
                {
                    Debug.LogError("ActionRecordingManager: ActionRecorder not found and cannot be auto-created without player reference");
                }
            }

            // Set up audio source if not assigned
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null && cloneCreateSound != null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                }
            }

            // Configure action recorder
            if (actionRecorder != null)
            {
                actionRecorder.SetCharacterController(playerController);
            }
        }

        /// <summary>
        /// Initialize system and start first loop if configured.
        /// </summary>
        private void Start()
        {
            // Set player spawn position if spawn point is set
            if (playerSpawnPoint != null && playerController != null)
            {
                playerController.SetPosition(playerSpawnPoint.position);
            }

            // Start first loop automatically if enabled
            if (autoStartFirstLoop)
            {
                StartCoroutine(DelayedFirstLoop());
            }
        }

        private System.Collections.IEnumerator DelayedFirstLoop()
        {
            yield return new WaitForSeconds(0.1f); // Small delay to ensure everything is initialized
            StartNewLoop();
        }

        /// <summary>
        /// Handle manual loop input.
        /// </summary>
        private void Update()
        {
            if (enableManualLooping && Input.GetKeyDown(manualLoopKey))
            {
                if (!isProcessingLoop)
                {
                    TriggerManualLoop();
                }
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Set the player controller interface reference manually.
        /// </summary>
        public void SetPlayerController(ICharacterController controller)
        {
            playerController = controller;
            if (actionRecorder != null)
            {
                actionRecorder.SetCharacterController(controller);
            }
        }

        /// <summary>
        /// Manually trigger a new loop (creates a clone and starts recording).
        /// </summary>
        public void TriggerManualLoop()
        {
            if (isRecording)
            {
                EndCurrentLoop();
            }
            else
            {
                StartNewLoop();
            }
        }

        /// <summary>
        /// Start a new recording loop.
        /// </summary>
        public void StartNewLoop()
        {
            if (isProcessingLoop || actionRecorder == null)
                return;

            StartCoroutine(StartNewLoopCoroutine());
        }

        /// <summary>
        /// End the current loop and create a clone.
        /// </summary>
        public void EndCurrentLoop()
        {
            if (!isRecording)
                return;

            StartCoroutine(EndCurrentLoopCoroutine());
        }

        /// <summary>
        /// Remove the most recent clone.
        /// </summary>
        public void RemoveLastClone()
        {
            if (allClones.Count > 0)
            {
                var lastClone = allClones[allClones.Count - 1];
                allClones.RemoveAt(allClones.Count - 1);
                
                if (lastClone != null)
                {
                    Destroy(lastClone.gameObject);
                }
            }
        }

        /// <summary>
        /// Remove all existing clones.
        /// </summary>
        public void ClearAllClones()
        {
            foreach (var clone in allClones)
            {
                if (clone != null)
                {
                    Destroy(clone.gameObject);
                }
            }
            allClones.Clear();
            nextCloneIndex = 0;
        }

        #endregion

        #region Loop Management

        private System.Collections.IEnumerator StartNewLoopCoroutine()
        {
            isProcessingLoop = true;

            // Start recording visual effects
            if (enableRecordingEffects && recordingEffectAnimator != null)
            {
                recordingEffectAnimator.SetBool("isRecording", true);
            }

            // Small delay for visual effect startup
            yield return new WaitForSeconds(0.1f);

            // Start recording
            isRecording = true;
            loopStartTime = Time.time;
            actionRecorder.StartRecording();

            OnNewLoopStarted?.Invoke();

            // Automatic loop ending
            if (loopDuration > 0)
            {
                yield return new WaitForSeconds(loopDuration);
                
                if (isRecording) // Only end if still recording
                {
                    yield return StartCoroutine(EndCurrentLoopCoroutine());
                }
            }

            isProcessingLoop = false;
        }

        private System.Collections.IEnumerator EndCurrentLoopCoroutine()
        {
            if (!isRecording)
                yield break;

            isProcessingLoop = true;

            // Stop recording
            actionRecorder.StopRecording();
            isRecording = false;

            // Stop recording visual effects
            if (enableRecordingEffects && recordingEffectAnimator != null)
            {
                recordingEffectAnimator.SetBool("isRecording", false);
            }

            // Small delay for visual effect
            yield return new WaitForSeconds(0.1f);

            // Create clone if we have recorded actions and haven't hit the limit
            var recordedActions = actionRecorder.GetRecordedActions();
            if (recordedActions.Count > 0 && allClones.Count < maxClones)
            {
                CreateClone(recordedActions);
            }

            OnLoopEnded?.Invoke();

            isProcessingLoop = false;
        }

        #endregion

        #region Clone Creation

        private void CreateClone(List<PlayerAction> recordedActions)
        {
            if (clonePrefab == null)
            {
                Debug.LogError("ActionRecordingManager: Clone prefab not assigned!");
                return;
            }

            // Pad actions to loop duration if needed
            if (loopDuration > 0)
            {
                actionRecorder.PadActionsToDuration(loopDuration);
                recordedActions = actionRecorder.GetRecordedActions();
            }

            // Determine spawn position
            Vector3 spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : transform.position;
            if (recordedActions.Count > 0)
            {
                spawnPosition = recordedActions[0].position;
            }

            // Instantiate clone
            GameObject cloneObject = Instantiate(clonePrefab, spawnPosition, Quaternion.identity);
            ActionReplayClone clone = cloneObject.GetComponent<ActionReplayClone>();

            if (clone == null)
            {
                Debug.LogError("ActionRecordingManager: Clone prefab must have ActionReplayClone component!");
                Destroy(cloneObject);
                return;
            }

            // Set up clone interfaces to match player
            clone.SetCharacterController(cloneObject.GetComponent<ICharacterController>());
            clone.SetInteractionSystem(cloneObject.GetComponent<IInteractionSystem>());

            // Initialize and start clone
            clone.InitializeClone(recordedActions, nextCloneIndex++);
            
            // Subscribe to clone events
            clone.OnCloneStuck += HandleCloneStuck;
            
            allClones.Add(clone);
            clone.StartReplay();

            // Play audio effect
            if (audioSource != null && cloneCreateSound != null)
            {
                audioSource.PlayOneShot(cloneCreateSound);
            }

            // Notify external systems
            OnCloneCreated?.Invoke(clone);

            Debug.Log($"Created clone {clone.CloneIndex} with {recordedActions.Count} actions");
        }

        private void HandleCloneStuck(ActionReplayClone clone)
        {
            OnCloneStuck?.Invoke(clone);
        }

        #endregion

        #region Getters

        /// <summary>
        /// Get all currently active clones.
        /// </summary>
        public List<ActionReplayClone> GetAllClones()
        {
            return new List<ActionReplayClone>(allClones);
        }

        /// <summary>
        /// Get the number of active clones.
        /// </summary>
        public int CloneCount => allClones.Count;

        /// <summary>
        /// Check if currently recording actions.
        /// </summary>
        public bool IsRecording => isRecording;

        /// <summary>
        /// Get the current loop duration setting.
        /// </summary>
        public float LoopDuration => loopDuration;

        /// <summary>
        /// Get the remaining time in the current loop (if auto-ending is enabled).
        /// </summary>
        public float RemainingLoopTime
        {
            get
            {
                if (!isRecording || loopDuration <= 0)
                    return 0f;
                
                float elapsed = Time.time - loopStartTime;
                return Mathf.Max(0f, loopDuration - elapsed);
            }
        }

        #endregion
    }
}