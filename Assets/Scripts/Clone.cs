using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Individual clone instance that replays recorded player actions for puzzle-solving.
/// Each clone represents a "ghost" of the player's previous actions, faithfully reproducing
/// movement, input timing, and interactions to serve as puzzle elements.
/// </summary>
 [RequireComponent(typeof(CharacterController2D))]
public class Clone : MonoBehaviour
{
    // Store the sprite at the first and last action for accurate start/end ghost visuals
    private Sprite startActionSprite = null;
    private Sprite endActionSprite = null;
    [Header("Clone Settings")]
    [SerializeField] private Material cloneMaterial;           // Optional custom material for clone appearance
    [SerializeField] private Color cloneColor = Color.cyan;    // Base color for active clones
    [SerializeField] private float cloneAlpha = 0.4f;          // Transparency level to distinguish from player

    [Header("Visual Effects")]
    [SerializeField] private bool enableGhostTrail = true;     // Enable ghost trail effect
    [SerializeField] private bool enableParticleEffects = true; // Enable particle effects
    [SerializeField] private bool enableGlowEffect = true;     // Enable glow effect around clone
    [SerializeField] private bool enableSoundEffects = true;   // Enable sound effects for clone actions

    // Core components required for action replay
    private List<PlayerAction> actionsToReplay;   // Sequence of actions to reproduce
    private CharacterController2D character;      // Physics and movement controller
    private InteractSystem interact;              // Object interaction system
    private SpriteRenderer spriteRenderer;        // Visual rendering component
    
    // Visual effects components
    private GhostTrail ghostTrail;                // Trail effect component
    private CloneParticleEffects particleEffects; // Particle effects component
    private CloneSoundEffects soundEffects;       // Sound effects component
    private Component glowLight;                  // 2D light for glow effect (if available)

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
        SetupVisualEffects();
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
    /// Set up visual effects components for enhanced clone appearance.
    /// </summary>
    private void SetupVisualEffects()
    {
        // Set up ghost trail effect
        if (enableGhostTrail)
        {
            ghostTrail = GetComponent<GhostTrail>();
            if (ghostTrail == null)
            {
                // Add our GhostTrail component (no longer requires TrailRenderer)
                ghostTrail = gameObject.AddComponent<GhostTrail>();
            }
        }
        
        // Set up start/end ghost markers
        CloneStartEndGhosts startEndGhosts = GetComponent<CloneStartEndGhosts>();
        if (startEndGhosts == null)
        {
            startEndGhosts = gameObject.AddComponent<CloneStartEndGhosts>();
        }
        
        // Set up particle effects
        if (enableParticleEffects)
        {
            particleEffects = GetComponent<CloneParticleEffects>();
            if (particleEffects == null)
            {
                particleEffects = gameObject.AddComponent<CloneParticleEffects>();
            }
        }
        
        // Set up sound effects
        if (enableSoundEffects)
        {
            soundEffects = GetComponent<CloneSoundEffects>();
            if (soundEffects == null)
            {
                // Add AudioSource component first if not present
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
                // Then add our CloneSoundEffects component
                soundEffects = gameObject.AddComponent<CloneSoundEffects>();
            }
        }
        
        // Set up glow effect using 2D light (if Universal Render Pipeline 2D is available)
        if (enableGlowEffect)
        {
            SetupGlowEffect();
        }
    }
    
    /// <summary>
    /// Set up glow effect around the clone.
    /// </summary>
    private void SetupGlowEffect()
    {
        // Try to use UnityEngine.Rendering.Universal.Light2D if available
        // This will work if the project uses URP 2D renderer
        try
        {
            var light2DType = System.Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
            if (light2DType != null)
            {
                var light2DComponent = gameObject.GetComponent(light2DType);
                if (light2DComponent == null)
                {
                    light2DComponent = gameObject.AddComponent(light2DType);
                    
                    // Configure the 2D light using reflection
                    var colorProperty = light2DType.GetProperty("color");
                    var intensityProperty = light2DType.GetProperty("intensity");
                    var innerRadiusProperty = light2DType.GetProperty("pointLightInnerRadius");
                    var outerRadiusProperty = light2DType.GetProperty("pointLightOuterRadius");
                    
                    if (colorProperty != null) colorProperty.SetValue(light2DComponent, cloneColor);
                    if (intensityProperty != null) intensityProperty.SetValue(light2DComponent, 0.3f);
                    if (innerRadiusProperty != null) innerRadiusProperty.SetValue(light2DComponent, 0.1f);
                    if (outerRadiusProperty != null) outerRadiusProperty.SetValue(light2DComponent, 0.5f);
                }
            }
        }
        catch (System.Exception)
        {
            // If 2D lighting is not available, fall back to sprite glow material
            if (spriteRenderer != null && spriteRenderer.material.name.Contains("Default"))
            {
                Material glowMaterial = Resources.Load<Material>("Materials/Sprite Glow");
                if (glowMaterial != null)
                {
                    spriteRenderer.material = glowMaterial;
                }
            }
        }
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

        // Capture the start sprite from the player at the time the clone was created
        // This represents the sprite state when recording began
        if (spriteRenderer != null)
        {
            startActionSprite = spriteRenderer.sprite;
        }

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
    }

    /// <summary>
    /// Begin replaying the recorded action sequence.
    /// Sets initial physics state and starts the replay timing system.
    /// Called by CloneManager immediately after clone creation.
    /// </summary>
    public void StartReplay()
    {
        // Drop/throw any picked up object when resetting replay
        if (interact != null && interact.PickedUpObject != null)
        {
            // Throw with zero force (just drop)
            interact.PickedUpObject.Throw(Vector2.zero);
            interact.PickedUpObject = null;
        }

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
        
        // Capture start sprite if not already set during initialization
        if (startActionSprite == null && spriteRenderer != null)
        {
            startActionSprite = spriteRenderer.sprite;
        }
        
        // Trigger visual effects
        if (particleEffects != null)
        {
            particleEffects.PlaySpawnEffect();
            particleEffects.StartAmbientEffects();
        }
        
        if (soundEffects != null)
        {
            soundEffects.PlaySpawnSound();
        }
        
        if (ghostTrail != null)
        {
            ghostTrail.ConfigureForCloneState(CloneState.Active);
        }
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
           // character.Walk(lastActionReplayed.Value.movement);
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
                // Reset position to where the replay started for loop consistency
                if (actionsToReplay != null && actionsToReplay.Count > 0)
                {
                    this.transform.position = actionsToReplay[0].position;
                    
                    // Ensure start sprite is captured if not already set
                    if (startActionSprite == null && spriteRenderer != null)
                    {
                        startActionSprite = spriteRenderer.sprite;
                    }
                }
                lastActionReplayed = null;
            }
            return;
        }

        float currentReplayTime = Time.time - replayStartTime;

        // Execute all actions whose timestamp has been reached
        while (currentActionIndex < actionsToReplay.Count &&
               actionsToReplay[currentActionIndex].timestamp <= currentReplayTime)
        {
            // Capture sprite BEFORE executing action to get the sprite in the correct state
            if (currentActionIndex == 0)
            {
                // For first action, capture start sprite before execution (if not already set)
                if (startActionSprite == null && spriteRenderer != null)
                {
                    startActionSprite = spriteRenderer.sprite;
                }
                ExecuteAction(actionsToReplay[currentActionIndex]);
                
                // Notify ghost script to refresh after setting sprite
                var ghosts = GetComponent<CloneStartEndGhosts>();
                if (ghosts != null) ghosts.RefreshGhosts();
            }
            else if (currentActionIndex == actionsToReplay.Count - 1)
            {
                // For last action, capture end sprite before execution
                if (endActionSprite == null && spriteRenderer != null)
                {
                    endActionSprite = spriteRenderer.sprite;
                }
                ExecuteAction(actionsToReplay[currentActionIndex]);
                
                // Notify ghost script to show end ghost and refresh
                var ghosts = GetComponent<CloneStartEndGhosts>();
                if (ghosts != null)
                {
                    ghosts.ShowEndGhost = true;
                    ghosts.RefreshGhosts();
                }
            }
            else
            {
                ExecuteAction(actionsToReplay[currentActionIndex]);
            }
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
        if (isStuck) return;

        // Execute movement and physics actions
        if (character != null)
        {
            character.Walk(action.movement);
            character.SetPhysicsState(action.position, action.speed, action.externalForce);
            UpdateAnimatorFromAction(action);

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

        // --- NEW: Update animator directly for clones ---
        UpdateAnimatorFromAction(action);

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

    private void UpdateAnimatorFromAction(PlayerAction action)
    {
        if (character == null) return;
        var animator = character.GetComponent<Animator>();
        if (animator == null) return;

        animator.SetFloat("hSpeed", action.speed.x);
        animator.SetFloat("vSpeed", action.speed.y);
        animator.SetBool("grounded", action.isGrounded);
        animator.SetBool("dashing", action.isDashing);
        animator.SetBool("onWall", action.isOnWall);
        animator.SetBool("facingRight", action.facingRight);
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
        
        // Update visual effects for stuck state
        if (particleEffects != null)
        {
            particleEffects.PlayStuckEffect();
        }
        
        if (soundEffects != null)
        {
            soundEffects.PlayStuckSound();
        }
        
        if (ghostTrail != null)
        {
            ghostTrail.ConfigureForCloneState(CloneState.Stuck);
        }
        
        // Update glow effect color if available
        UpdateGlowEffectColor(Color.green);

        Debug.Log($"Clone {cloneIndex} is now stuck at goal");
    }

    /// <summary>
    /// Stop the replay without marking as stuck.
    /// Used for pausing clones or temporary replay suspension.
    /// </summary>
    public void StopReplay()
    {
        isReplaying = false;
        
        // Update visual effects for inactive state
        if (particleEffects != null)
        {
            particleEffects.StopAmbientEffects();
        }
        
        if (ghostTrail != null)
        {
            ghostTrail.ConfigureForCloneState(CloneState.Inactive);
        }
        
        Debug.Log($"Clone {cloneIndex} stopped replaying");
    }
    
    /// <summary>
    /// Update the glow effect color using reflection to access 2D lighting.
    /// </summary>
    /// <param name="newColor">The new color for the glow effect</param>
    private void UpdateGlowEffectColor(Color newColor)
    {
        try
        {
            var light2DType = System.Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
            if (light2DType != null)
            {
                var light2DComponent = gameObject.GetComponent(light2DType);
                if (light2DComponent != null)
                {
                    var colorProperty = light2DType.GetProperty("color");
                    if (colorProperty != null)
                    {
                        colorProperty.SetValue(light2DComponent, newColor);
                    }
                }
            }
        }
        catch (System.Exception)
        {
            // Ignore if 2D lighting is not available
        }
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
    /// Reference to the CharacterController2D component for movement control.
    /// Provides access to physics and movement methods for this clone.
    /// </summary>
    public CharacterController2D Character => character;

    /// <summary>
    /// Current progress through the action sequence (0.0 to 1.0).
    /// Useful for UI indicators or debugging replay timing.
    /// </summary>
    public float ReplayProgress => replayDuration > 0 ? (Time.time - replayStartTime) / replayDuration : 0f;

    /// <summary>
    /// Get the first recorded action in the sequence.
    /// Used for retract system to access starting position and state.
    /// </summary>
    public PlayerAction? FirstAction => actionsToReplay != null && actionsToReplay.Count > 0 ? actionsToReplay[0] : null;

    /// <summary>
    /// Sprite at the first recorded action (for start ghost marker)
    /// </summary>
    public Sprite StartActionSprite => startActionSprite;


    /// <summary>
    /// Get the last recorded action in the sequence.
    /// Used for retract system to access final position and state.
    /// </summary>
    public PlayerAction? LastAction => actionsToReplay != null && actionsToReplay.Count > 0 ? actionsToReplay[actionsToReplay.Count - 1] : null;

    /// <summary>
    /// Sprite at the last recorded action (for end ghost marker)
    /// </summary>
    public Sprite EndActionSprite => endActionSprite;

    /// <summary>
    /// Cleanup logging when clone is destroyed.
    /// Called automatically when GameObject is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        // Trigger despawn effects before destruction
        if (particleEffects != null)
        {
            particleEffects.PlayDespawnEffect();
        }
        
        if (soundEffects != null)
        {
            soundEffects.PlayDespawnSound();
        }
        
        //Debug.Log($"Clone {cloneIndex} destroyed");
    }
}