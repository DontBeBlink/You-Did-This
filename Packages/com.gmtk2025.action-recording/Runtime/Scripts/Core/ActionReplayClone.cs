using System.Collections.Generic;
using UnityEngine;

namespace GMTK2025.ActionRecording
{
    /// <summary>
    /// Individual clone instance that replays recorded player actions for puzzle-solving.
    /// Each clone represents a "ghost" of the player's previous actions, faithfully reproducing
    /// movement, input timing, and interactions to serve as puzzle elements.
    /// 
    /// This refactored version uses interface abstractions to work with any character controller
    /// and interaction system, making it reusable across different Unity projects.
    /// </summary>
    public class ActionReplayClone : MonoBehaviour
    {
        [Header("Clone Settings")]
        [SerializeField] private Material cloneMaterial;           // Optional custom material for clone appearance
        [SerializeField] private Color cloneColor = Color.cyan;    // Base color for active clones
        [SerializeField] private float cloneAlpha = 0.4f;          // Transparency level to distinguish from player

        [Header("Visual Effects")]
        [SerializeField] private bool enableGhostTrail = true;     // Enable ghost trail effect
        [SerializeField] private bool enableParticleEffects = true; // Enable particle effects
        [SerializeField] private bool enableGlowEffect = true;     // Enable glow effect around clone
        [SerializeField] private bool enableSoundEffects = true;   // Enable sound effects for clone actions

        [Header("System References")]
        [SerializeField] private MonoBehaviour characterControllerComponent; // Component implementing ICharacterController
        [SerializeField] private MonoBehaviour interactionSystemComponent; // Component implementing IInteractionSystem

        // Interface references
        private ICharacterController characterController;
        private IInteractionSystem interactionSystem;

        // Core components
        private List<PlayerAction> actionsToReplay;   // Sequence of actions to reproduce
        private SpriteRenderer spriteRenderer;        // Visual rendering component
        
        // Visual effects components (can be null if not used)
        private ActionReplayEffects effectsManager;   // Manages all visual and audio effects

        // Replay state management
        private bool isReplaying = false;            // Whether currently playing back actions
        private bool isStuck = false;                // Whether stuck at a goal (permanent state)
        private float replayStartTime;               // Time.time when current replay loop started
        private int currentActionIndex = 0;          // Index of next action to execute
        private float replayDuration;                // Total duration of action sequence

        // Clone identification and state tracking
        private int cloneIndex;                      // Unique identifier assigned by CloneManager
        private bool wasJumpHeld = false;            // Previous frame's jump hold state for EndJump timing

        // Store the last executed action for continuous movement application
        private PlayerAction? lastActionReplayed = null;

        // Events for system integration
        public System.Action<ActionReplayClone> OnReplayComplete;
        public System.Action<ActionReplayClone> OnCloneStuck;

        /// <summary>
        /// Initialize clone components and set up visual appearance.
        /// </summary>
        private void Awake()
        {
            SetupComponentReferences();
            SetupVisualComponents();
            SetupCloneVisuals();
        }

        private void SetupComponentReferences()
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

            if (characterController == null)
            {
                Debug.LogError($"ActionReplayClone: No ICharacterController found on {gameObject.name}");
            }
        }

        private void SetupVisualComponents()
        {
            // Find sprite renderer - check child "Sprite" first, then self
            var spriteChild = transform.Find("Sprite");
            if (spriteChild != null)
            {
                spriteRenderer = spriteChild.GetComponent<SpriteRenderer>();
            }
            else
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            // Set up effects manager if visual effects are enabled
            if (enableGhostTrail || enableParticleEffects || enableSoundEffects || enableGlowEffect)
            {
                effectsManager = GetComponent<ActionReplayEffects>();
                if (effectsManager == null)
                {
                    effectsManager = gameObject.AddComponent<ActionReplayEffects>();
                }
                effectsManager.Configure(enableGhostTrail, enableParticleEffects, enableSoundEffects, enableGlowEffect, cloneColor);
            }
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
        /// Configure the clone's visual appearance to distinguish it from the player.
        /// </summary>
        private void SetupCloneVisuals()
        {
            if (spriteRenderer != null)
            {
                // Apply custom material if specified
                if (cloneMaterial != null)
                {
                    spriteRenderer.material = cloneMaterial;
                }

                // Set color with transparency to visually distinguish clone
                Color color = cloneColor;
                color.a = cloneAlpha;
                spriteRenderer.color = color;
            }

            // Name the GameObject for easier debugging
            gameObject.name = $"ActionReplayClone_{cloneIndex}";
        }

        /// <summary>
        /// Initialize the clone with a sequence of recorded player actions.
        /// </summary>
        /// <param name="actions">Sequence of PlayerActions to replay</param>
        /// <param name="index">Unique identifier for this clone</param>
        public void InitializeClone(List<PlayerAction> actions, int index)
        {
            // Validate and store the action sequence
            if (actions == null || actions.Count == 0)
            {
                Debug.LogWarning($"Clone {index} initialized with no actions to replay");
                actionsToReplay = new List<PlayerAction>();
            }
            else
            {
                actionsToReplay = new List<PlayerAction>(actions);
            }

            cloneIndex = index;
            replayDuration = actionsToReplay.Count > 0 ? actionsToReplay[actionsToReplay.Count - 1].timestamp : 0f;

            SetupCloneVisuals();
        }

        /// <summary>
        /// Begin replaying the recorded action sequence.
        /// </summary>
        public void StartReplay()
        {
            if (characterController == null)
            {
                Debug.LogError("ActionReplayClone: Cannot start replay without character controller");
                return;
            }

            // Validate that we have actions to replay
            if (actionsToReplay == null || actionsToReplay.Count == 0)
            {
                Debug.LogWarning("No actions to replay for clone");
                return;
            }

            // Set initial physics state from the first recorded action
            if (actionsToReplay.Count > 0)
            {
                var firstAction = actionsToReplay[0];
                characterController.SetPosition(firstAction.position);
            }

            // Initialize replay state
            isReplaying = true;
            isStuck = false;
            replayStartTime = Time.time;
            currentActionIndex = 0;
            wasJumpHeld = false;
            
            // Trigger visual effects
            if (effectsManager != null)
            {
                effectsManager.PlaySpawnEffect();
                effectsManager.StartActiveEffects();
            }
        }

        /// <summary>
        /// Stop the replay and mark the clone as stuck at the current position.
        /// </summary>
        public void StopReplay()
        {
            isReplaying = false;
            isStuck = true;
            
            if (effectsManager != null)
            {
                effectsManager.StopActiveEffects();
                effectsManager.StartStuckEffects();
            }
            
            OnCloneStuck?.Invoke(this);
        }

        /// <summary>
        /// Update replay progress each frame for action timing.
        /// </summary>
        private void Update()
        {
            if (isReplaying && !isStuck)
            {
                UpdateReplay();
            }
        }

        /// <summary>
        /// Core replay logic - processes recorded actions at the appropriate times.
        /// </summary>
        private void UpdateReplay()
        {
            if (actionsToReplay == null || actionsToReplay.Count == 0) return;

            float currentReplayTime = Time.time - replayStartTime;

            // Process all actions that should happen at or before the current time
            while (currentActionIndex < actionsToReplay.Count)
            {
                var action = actionsToReplay[currentActionIndex];
                
                if (action.timestamp > currentReplayTime)
                    break; // Wait for the correct timing

                ExecuteAction(action);
                lastActionReplayed = action;
                currentActionIndex++;
            }

            // Continue applying the last action's movement if replay is ongoing
            if (lastActionReplayed.HasValue && characterController != null)
            {
                characterController.ApplyMovement(lastActionReplayed.Value.movement);
            }

            // Check if replay is complete
            if (currentActionIndex >= actionsToReplay.Count)
            {
                // Restart the replay loop
                currentActionIndex = 0;
                replayStartTime = Time.time;
                
                OnReplayComplete?.Invoke(this);
            }
        }

        /// <summary>
        /// Execute a single recorded action.
        /// </summary>
        private void ExecuteAction(PlayerAction action)
        {
            if (characterController == null) return;

            // Apply movement
            characterController.ApplyMovement(action.movement);

            // Handle jumping
            if (action.isJumping && !wasJumpHeld)
            {
                characterController.Jump();
            }
            
            if (wasJumpHeld && !action.jumpHeld)
            {
                characterController.EndJump();
            }
            
            wasJumpHeld = action.jumpHeld;

            // Handle dashing
            if (action.isDashing)
            {
                characterController.Dash(action.dashDirection);
            }

            // Apply external forces
            if (action.externalForce != Vector2.zero)
            {
                characterController.ApplyExternalForce(action.externalForce);
            }

            // Handle interactions
            if (interactionSystem != null)
            {
                if (action.isInteracting)
                {
                    interactionSystem.TriggerInteraction();
                }
                
                if (action.isAttacking)
                {
                    interactionSystem.TriggerAttack();
                }
            }

            // Position correction if needed (for physics stability)
            Vector3 currentPos = characterController.Position;
            Vector3 targetPos = action.position;
            float distance = Vector3.Distance(currentPos, targetPos);
            
            // Only correct position if there's a significant deviation
            if (distance > 0.1f)
            {
                characterController.SetPosition(Vector3.Lerp(currentPos, targetPos, 0.1f));
            }
        }

        /// <summary>
        /// Get the current replay progress as a percentage (0-1).
        /// </summary>
        public float GetReplayProgress()
        {
            if (!isReplaying || replayDuration <= 0) return 0f;
            
            float currentReplayTime = Time.time - replayStartTime;
            return Mathf.Clamp01(currentReplayTime / replayDuration);
        }

        /// <summary>
        /// Check if this clone is currently replaying actions.
        /// </summary>
        public bool IsReplaying => isReplaying;

        /// <summary>
        /// Check if this clone is stuck at a goal.
        /// </summary>
        public bool IsStuck => isStuck;

        /// <summary>
        /// Get the clone's unique identifier.
        /// </summary>
        public int CloneIndex => cloneIndex;

        /// <summary>
        /// Get the total duration of the replay sequence.
        /// </summary>
        public float ReplayDuration => replayDuration;
    }
}