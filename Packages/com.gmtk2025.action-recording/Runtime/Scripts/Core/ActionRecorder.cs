using System.Collections.Generic;
using UnityEngine;

namespace GMTK2025.ActionRecording
{
    /// <summary>
    /// Records player actions and input states for physics-perfect replay by clones.
    /// This component is essential for the clone system, capturing all player inputs
    /// and physical states at precise intervals for accurate reproduction.
    /// 
    /// Key Features:
    /// - Records at 50 FPS (0.02s intervals) to match Unity's FixedUpdate timing
    /// - Captures input states, position, velocity, and external forces
    /// - Uses interface abstractions to work with any character controller
    /// - Provides physics-perfect replay data for clone creation
    /// - Automatic recording duration limits to prevent memory issues
    /// 
    /// Usage: Attach to the player GameObject and reference the character controller
    /// and interaction system through the provided interfaces.
    /// </summary>
    public class ActionRecorder : MonoBehaviour
    {
        [Header("Recording Settings")]
        [SerializeField] private float recordingInterval = 0.02f; // 50 FPS recording rate, matches FixedUpdate
        [SerializeField] private float maxRecordingTime = 30f; // Maximum recording duration to prevent memory issues
        
        [Header("System References")]
        [SerializeField] private MonoBehaviour characterControllerComponent; // Component implementing ICharacterController
        [SerializeField] private MonoBehaviour interactionSystemComponent; // Component implementing IInteractionSystem
        
        // Interface references (set automatically from components or manually)
        private ICharacterController characterController;
        private IInteractionSystem interactionSystem;

        // Storage for recorded action sequence
        private List<PlayerAction> recordedActions = new List<PlayerAction>();
        private bool isRecording = false;
        private float recordingStartTime; // Time.time when recording started
        private float lastRecordTime;    // Time.time of last recorded frame

        // Current frame input states - set by input system each frame
        public float CurrentMovement { get; set; }     // Horizontal movement input (-1 to 1)
        public bool IsJumping { get; set; }            // Jump button pressed this frame
        public bool IsDashing { get; set; }            // Dash button pressed this frame
        public Vector2 DashDirection { get; set; }     // Direction for dash input
        public bool IsInteracting { get; set; }        // Interact button pressed this frame
        public bool IsAttacking { get; set; }          // Attack button pressed this frame
        public bool JumpHeld { get; set; }             // Whether jump button is currently held

        private void Awake()
        {
            // Auto-detect interface implementations if not manually assigned
            if (characterControllerComponent != null)
                characterController = characterControllerComponent as ICharacterController;
            else
                characterController = GetComponent<ICharacterController>();
                
            if (interactionSystemComponent != null)
                interactionSystem = interactionSystemComponent as IInteractionSystem;
            else
                interactionSystem = GetComponent<IInteractionSystem>();
        }

        /// <summary>
        /// Set the character controller interface reference manually
        /// </summary>
        public void SetCharacterController(ICharacterController controller)
        {
            characterController = controller;
        }

        /// <summary>
        /// Set the interaction system interface reference manually
        /// </summary>
        public void SetInteractionSystem(IInteractionSystem system)
        {
            interactionSystem = system;
        }

        /// <summary>
        /// Begin recording player actions and clear any previously recorded data.
        /// Sets up timing references and prepares for action capture.
        /// </summary>
        public void StartRecording()
        {
            // Stop any existing recording to avoid conflicts
            if (isRecording)
            {
                Debug.LogWarning("Already recording! Stopping previous recording.");
                StopRecording();
            }

            // Initialize recording state
            isRecording = true;
            recordingStartTime = Time.time;
            lastRecordTime = Time.time;
            recordedActions.Clear(); // Clear previous recording data

            // Record initial state if character controller is available
            if (characterController != null)
            {
                PlayerAction initialAction = new PlayerAction(
                    0f,
                    CurrentMovement,
                    false, // isJumping
                    false, // isDashing
                    Vector2.zero, // dashDirection
                    false, // isInteracting
                    false, // isAttacking
                    characterController.Position,
                    JumpHeld,
                    characterController.Velocity,
                    Vector2.zero, // externalForce - will be captured during recording
                    characterController.IsGrounded,
                    characterController.IsOnWall,
                    characterController.FacingRight
                );
                recordedActions.Add(initialAction);
            }
        }

        /// <summary>
        /// Stop the current recording session and finalize the recorded action sequence.
        /// The recorded actions are now ready to be used for clone creation.
        /// </summary>
        public void StopRecording()
        {
            if (isRecording)
            {
                isRecording = false;
                float duration = Time.time - recordingStartTime;
                Debug.Log($"Recording stopped. Total actions: {recordedActions.Count}, Duration: {duration:F2}s");
            }
        }

        /// <summary>
        /// Record a physics-perfect frame of player action data.
        /// This should be called from FixedUpdate to ensure physics alignment.
        /// </summary>
        public void RecordCurrentFrame()
        {
            if (!isRecording || characterController == null) return;

            float currentTime = Time.time - recordingStartTime;

            // Enforce maximum recording time to prevent memory issues
            if (currentTime > maxRecordingTime)
            {
                isRecording = false;
                Debug.LogWarning("Max recording time reached, stopping recording.");
                return;
            }

            // Check if enough time has passed since last record
            if (Time.time - lastRecordTime < recordingInterval) return;

            lastRecordTime = Time.time;
            
            // Record complete action state including physics data
            PlayerAction action = new PlayerAction(
                currentTime,
                CurrentMovement,
                IsJumping,
                IsDashing,
                DashDirection,
                IsInteracting,
                IsAttacking,
                characterController.Position,
                JumpHeld,
                characterController.Velocity,
                Vector2.zero, // External forces handled by character controller
                characterController.IsGrounded,
                characterController.IsOnWall,
                characterController.FacingRight
            );
            
            recordedActions.Add(action);
            
            // Reset frame-specific inputs after recording
            ResetFrameInputs();
        }

        /// <summary>
        /// Pad the recorded actions so the last action is repeated until the specified duration
        /// </summary>
        public void PadActionsToDuration(float targetDuration)
        {
            if (recordedActions.Count == 0) return;
            
            PlayerAction last = recordedActions[recordedActions.Count - 1];
            float lastTime = last.timestamp;
            
            if (lastTime >= targetDuration - 0.0001f) return;
            
            // Pad with last action, updating timestamp
            while (lastTime < targetDuration - 0.0001f)
            {
                lastTime += recordingInterval;
                PlayerAction padded = last;
                padded.timestamp = lastTime;
                recordedActions.Add(padded);
            }
        }

        /// <summary>
        /// Get a copy of all recorded actions for clone creation.
        /// Returns a new list to prevent external modification of the original data.
        /// </summary>
        public List<PlayerAction> GetRecordedActions()
        {
            return new List<PlayerAction>(recordedActions);
        }

        /// <summary>
        /// Get the total duration of the recorded action sequence.
        /// </summary>
        public float GetRecordingDuration()
        {
            return recordedActions.Count > 0 ? recordedActions[recordedActions.Count - 1].timestamp : 0f;
        }

        /// <summary>
        /// Whether the recorder is currently active and recording actions.
        /// </summary>
        public bool IsRecording => isRecording;
        
        /// <summary>
        /// Total number of actions recorded in the current session.
        /// </summary>
        public int ActionCount => recordedActions.Count;

        /// <summary>
        /// Reset all frame-specific input flags to their default state.
        /// Should be called after inputs are processed each frame.
        /// </summary>
        public void ResetFrameInputs()
        {
            IsJumping = false;
            IsDashing = false;
            IsInteracting = false;
            IsAttacking = false;
            DashDirection = Vector2.zero;
        }
    }
}