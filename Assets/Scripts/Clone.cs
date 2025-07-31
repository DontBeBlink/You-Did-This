using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Individual clone instance that replays recorded player actions for puzzle-solving.
/// Each clone represents a "ghost" of the player's previous actions, faithfully reproducing
/// movement, input timing, and interactions to serve as puzzle elements.
/// 
/// Core Functionality:
/// - Replays a sequence of PlayerAction data with precise timing
/// - Loops replay automatically when reaching the end of actions
/// - Can become "stuck" at goals to act as permanent puzzle elements
/// - Maintains physics-perfect reproduction of original player behavior
/// - Provides visual distinction from the active player (cyan, semi-transparent)
/// 
/// Lifecycle:
/// 1. Created by CloneManager with recorded action sequence
/// 2. Begins replay immediately, starting from spawn position
/// 3. Continuously loops through actions until stuck at a goal
/// 4. When stuck, becomes immobile and changes to green coloring
/// 5. Destroyed when clone limit is reached or level is reset
/// 
/// Usage: Created and managed automatically by CloneManager. Not instantiated directly.
/// </summary>
[RequireComponent(typeof(CharacterController2D))]
public class Clone : MonoBehaviour
{
    [Header("Clone Settings")]
    [SerializeField] private Material cloneMaterial;           // Optional custom material for clone appearance
    [SerializeField] private Color cloneColor = Color.cyan;    // Base color for active clones
    [SerializeField] private float cloneAlpha = 0.4f;          // Transparency level to distinguish from player
    
    // Core components required for action replay
    private List<PlayerAction> actionsToReplay;   // Sequence of actions to reproduce
    private CharacterController2D character;      // Physics and movement controller
    private InteractSystem interact;              // Object interaction system
    private SpriteRenderer spriteRenderer;        // Visual rendering component
    
    // Replay state management
    private bool isReplaying = false;            // Whether currently playing back actions
    private bool isStuck = false;                // Whether stuck at a goal (permanent state)
    private float replayStartTime;               // Time.time when current replay loop started
    private int currentActionIndex = 0;          // Index of next action to execute
    private float replayDuration;                // Total duration of action sequence
    
    // Clone identification and state tracking
    private int cloneIndex;                      // Unique identifier assigned by CloneManager
    private Goal stuckAtGoal;                    // Goal where this clone is stuck (if any)
    private bool wasJumpHeld = false;            // Previous frame's jump hold state for EndJump timing
    
    // Store the last executed action for continuous movement application
    private PlayerAction? lastActionReplayed = null;
    
    /// <summary>
    /// Initialize clone components and set up visual appearance.
    /// Called automatically when the clone GameObject is created.
    /// </summary>
    private void Awake()
    {
        // Get required components for clone functionality
        character = GetComponent<CharacterController2D>();
        interact = GetComponent<InteractSystem>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        SetupCloneVisuals();
    }
    
    /// <summary>
    /// Configure the clone's visual appearance to distinguish it from the player.
    /// Sets up material, color, transparency, and GameObject naming.
    /// </summary>
    private void SetupCloneVisuals()
    {
        if (spriteRenderer != null)
        {
            // Apply custom material if specified to avoid affecting original
            if (cloneMaterial != null)
            {
                spriteRenderer.material = cloneMaterial;
            }
            
            // Set cyan color with transparency to visually distinguish clone
            Color color = cloneColor;
            color.a = cloneAlpha;
            spriteRenderer.color = color;
        }
        
        // Name the GameObject for easier debugging and scene hierarchy viewing
        gameObject.name = $"Clone_{cloneIndex}";
    }
    
    /// <summary>
    /// Initialize the clone with a sequence of recorded player actions.
    /// Disables player input components and prepares the clone for action replay.
    /// Called by CloneManager when creating a new clone.
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
        // Calculate total replay duration from the last action's timestamp
        replayDuration = actionsToReplay.Count > 0 ? actionsToReplay[actionsToReplay.Count - 1].timestamp : 0f;
        
        // Disable player input components to prevent interference with replay
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        ActionRecorder recorder = GetComponent<ActionRecorder>();
        if (recorder != null)
        {
            recorder.enabled = false;
        }
        
        SetupCloneVisuals();
      //  Debug.Log($"Clone {index} initialized with {actionsToReplay.Count} actions, duration: {replayDuration:F1}s");
    }
    
    /// <summary>
    /// Begin replaying the recorded action sequence.
    /// Sets initial physics state and starts the replay timing system.
    /// Called by CloneManager immediately after clone creation.
    /// </summary>
    public void StartReplay()
    {
        // Validate that we have actions to replay
        if (actionsToReplay == null || actionsToReplay.Count == 0)
        {
            Debug.LogWarning("No actions to replay for clone");
            return;
        }

        // Ensure we have the required CharacterController2D component
        if (character == null)
        {
            character = GetComponent<CharacterController2D>();
            if (character == null)
            {
                Debug.LogError("Clone: CharacterController2D component missing!");
                return;
            }
        }
        
        // Set initial physics state from the first recorded action
        if (actionsToReplay != null && actionsToReplay.Count > 0)
        {
            var firstAction = actionsToReplay[0];
            character.SetPhysicsState(firstAction.position, firstAction.speed, firstAction.externalForce);
        }

        // Initialize replay state
        isReplaying = true;
        replayStartTime = Time.time;
        currentActionIndex = 0;
        wasJumpHeld = false; // Reset jump held state for clean start
        //Debug.Log($"Clone {cloneIndex} started replaying actions");
    }
    
    /// <summary>
    /// Update replay progress each frame for action timing.
    /// Handles action sequence progression and automatic looping.
    /// </summary>
    private void Update()
    {
        if (isReplaying && !isStuck)
        {
            UpdateReplay();
        }
    }

    /// <summary>
    /// Apply continuous movement from the last replayed action during physics updates.
    /// Ensures smooth movement between discrete action frames.
    /// </summary>
    private void FixedUpdate()
    {
        if (isReplaying && !isStuck && character != null && lastActionReplayed.HasValue)
        {
            character.Walk(lastActionReplayed.Value.movement);
        }
    }
    
    /// <summary>
    /// Core replay logic that processes the action sequence based on timing.
    /// Handles action execution, looping, and timing synchronization.
    /// </summary>
    private void UpdateReplay()
    {
        // Check if we've reached the end of the action sequence
        if (actionsToReplay == null || currentActionIndex >= actionsToReplay.Count)
        {
            // Automatically loop back to the beginning for continuous replay
            if (replayDuration > 0)
            {
                replayStartTime = Time.time;
                currentActionIndex = 0;
                wasJumpHeld = false; // Reset jump state for clean loop restart
                //Debug.Log($"Clone {cloneIndex} looping replay");
                // Reset position to spawn point for loop consistency
                this.transform.position = CloneManager.instance.transform.position;
                lastActionReplayed = null;
            }
            return;
        }

        float currentReplayTime = Time.time - replayStartTime;

        // Execute all actions whose timestamp has been reached
        while (currentActionIndex < actionsToReplay.Count && 
               actionsToReplay[currentActionIndex].timestamp <= currentReplayTime)
        {
            ExecuteAction(actionsToReplay[currentActionIndex]);
            lastActionReplayed = actionsToReplay[currentActionIndex];
            currentActionIndex++;
        }

        // Ensure we have an action to apply for continuous movement
        if (lastActionReplayed == null && actionsToReplay.Count > 0)
        {
            lastActionReplayed = actionsToReplay[0];
        }
    }
    
    /// <summary>
    /// Execute a single PlayerAction, reproducing the player's behavior at that moment.
    /// Handles movement, jumping, dashing, and interaction commands with precise timing.
    /// This is the core method that translates recorded data back into game actions.
    /// </summary>
    /// <param name="action">The PlayerAction to execute</param>
    private void ExecuteAction(PlayerAction action)
    {
        //Debug.Log($"[Replay] t={action.timestamp:F2} move={action.movement} jump={action.isJumping} dash={action.isDashing} held={action.jumpHeld}");
        if (isStuck) return; // Stuck clones don't execute actions
        
        // Execute movement and physics actions
        if (character != null)
        {
            // Apply movement input and set physics state to match recorded data
            character.Walk(action.movement);
            character.SetPhysicsState(action.position, action.speed, action.externalForce);
            
            // Execute jump if it was initiated this frame
            if (action.isJumping)
            {
                character.Jump();
            }
            
            // Handle variable jump height by ending jump when button is released
            // This reproduces the original jump duration precisely
            if (wasJumpHeld && !action.jumpHeld)
            {
                character.EndJump();
            }
            wasJumpHeld = action.jumpHeld;
            
            // Execute dash if it was initiated this frame
            if (action.isDashing)
            {
                character.Dash(action.dashDirection);
            }
        }
        
        // Execute interaction actions
        if (interact != null)
        {
            // Reproduce object interactions (pickup, switches, etc.)
            if (action.isInteracting)
            {
                interact.Interact();
            }
            
            // Reproduce object throwing (attack while holding object)
            if (action.isAttacking && interact.PickedUpObject)
            {
                interact.Throw();
            }
        }
    }
    
    /// <summary>
    /// Mark this clone as "stuck" at a goal, making it a permanent puzzle element.
    /// Stuck clones stop replaying actions and become immobile obstacles or platforms.
    /// This is called by Goal objects when the clone reaches them.
    /// </summary>
    /// <param name="goal">The Goal object where this clone is now stuck</param>
    public void SetStuck(Goal goal)
    {
        isStuck = true;
        stuckAtGoal = goal;
        isReplaying = false;
        
        // Make the character completely immobile
        if (character != null)
        {
            character.Immobile = true;
        }
        
        // Change visual appearance to green to indicate stuck state
        if (spriteRenderer != null)
        {
            Color stuckColor = Color.green;
            stuckColor.a = cloneAlpha;
            spriteRenderer.color = stuckColor;
        }
        
        Debug.Log($"Clone {cloneIndex} is now stuck at goal");
    }
    
    /// <summary>
    /// Stop the replay without marking as stuck.
    /// Used for pausing clones or temporary replay suspension.
    /// </summary>
    public void StopReplay()
    {
        isReplaying = false;
        Debug.Log($"Clone {cloneIndex} stopped replaying");
    }
    
    // Properties for external access to clone state
    
    /// <summary>
    /// Whether this clone is currently replaying its action sequence.
    /// False when paused, stopped, or stuck.
    /// </summary>
    public bool IsReplaying => isReplaying;
    
    /// <summary>
    /// Whether this clone is stuck at a goal and acting as a permanent puzzle element.
    /// Stuck clones cannot be destroyed by clone limits and do not replay actions.
    /// </summary>
    public bool IsStuck => isStuck;
    
    /// <summary>
    /// Unique identifier assigned by CloneManager for this clone.
    /// Used for debugging and clone management operations.
    /// </summary>
    public int CloneIndex => cloneIndex;
    
    /// <summary>
    /// The Goal object where this clone is stuck, if any.
    /// Null if the clone is not stuck.
    /// </summary>
    public Goal StuckAtGoal => stuckAtGoal;
    
    /// <summary>
    /// Current progress through the action sequence (0.0 to 1.0).
    /// Useful for UI indicators or debugging replay timing.
    /// </summary>
    public float ReplayProgress => replayDuration > 0 ? (Time.time - replayStartTime) / replayDuration : 0f;
    
    /// <summary>
    /// Cleanup logging when clone is destroyed.
    /// Called automatically when GameObject is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        //Debug.Log($"Clone {cloneIndex} destroyed");
    }
}