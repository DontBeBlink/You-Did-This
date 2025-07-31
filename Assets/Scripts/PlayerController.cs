using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController2D))]
[RequireComponent(typeof(CheckpointSystem))]
/// <summary>
/// Handles player input processing and interfaces with core game systems.
/// This component bridges player input with character movement, clone recording, 
/// and game systems like checkpoints and interactions.
/// 
/// Key Responsibilities:
/// - Process input from Unity's Input System (movement, jump, dash, interact, attack)
/// - Interface with ActionRecorder for clone system recording
/// - Manage respawn system integration with CheckpointSystem
/// - Coordinate with CharacterController2D for movement execution
/// - Handle object interaction through InteractSystem
/// 
/// Input Mapping:
/// - Movement: WASD/Arrow Keys (horizontal + ladder climbing)
/// - Jump: Spacebar (variable height based on hold duration)
/// - Dash: Shift (directional dash based on movement input)  
/// - Interact: E (pickup objects, activate switches)
/// - Attack/Throw: Left Click (throw picked up objects)
/// 
/// Usage: Automatically found and configured by CloneManager. Should be present
/// on the main player GameObject with CharacterController2D and CheckpointSystem.
/// </summary>
public class PlayerController : MonoBehaviour {

    [Header("Respawn Settings")]
    public float softRespawnDelay = 0.5f;      // Delay before respawn sequence starts
    public float softRespawnDuration = 0.5f;   // Duration of respawn fade effect
    
    [Header("Input System")]
    public InputMaster controls;                // Generated Input Actions from Input System

    // Core component references for player functionality
    private CharacterController2D character;   // Handles physics and movement
    private CameraController cameraController; // Manages camera effects and fading
    private CheckpointSystem checkpoint;       // Handles save/load and respawn points
    private InteractSystem interact;           // Manages object pickup and interaction
    private ActionRecorder actionRecorder;     // Records actions for clone system
    
    // Input state tracking
    private Vector2 axis;                      // Current movement input (-1 to 1 for x/y)
    private bool jumpHeld;                     // Whether jump button is currently held

    /// <summary>
    /// Initialize components and set up input system bindings.
    /// Connects all input actions to their corresponding methods and validates
    /// that required components are present in the scene.
    /// </summary>
    void Awake() {
        // Get required components from this GameObject
        character = GetComponent<CharacterController2D>();
        checkpoint = GetComponent<CheckpointSystem>();
        interact = GetComponent<InteractSystem>();
        actionRecorder = GetComponent<ActionRecorder>();
        
        // Find camera controller in scene (required for respawn effects)
        cameraController = GameObject.FindFirstObjectByType<CameraController>();
        if (!cameraController) {
            Debug.LogError("The scene is missing a camera controller! The player script needs it to work properly!");
        }
        
        // Initialize Input System and bind input actions to methods
        controls = new InputMaster();
        controls.Player.Movement.performed += ctx => Move(ctx.ReadValue<Vector2>());
        controls.Player.Movement.canceled += ctx => Move(Vector2.zero);
        controls.Player.Jump.started += Jump;
        controls.Player.Jump.canceled += EndJump;
        controls.Player.Dash.started += Dash;
        controls.Player.Interact.started += Interact;
        controls.Player.AttackA.started += Attack;
    }

    /// <summary>
    /// Process player input and movement during physics updates.
    /// Updates ActionRecorder for clone system before applying movement to ensure
    /// recorded data matches the executed actions for physics-perfect replay.
    /// </summary>
    void FixedUpdate()
    {
        // Update action recorder BEFORE applying movement for accurate clone recording
        if (actionRecorder != null)
        {
            //actionRecorder.CurrentMovement = axis.x;  // Movement is set in Move() method
            actionRecorder.JumpHeld = jumpHeld;         // Track jump hold state for variable jump height
        }
        
        // Apply movement to character controller
        character.Walk(axis.x);        // Horizontal movement (-1 to 1)
        character.ClimbLadder(axis.y); // Vertical movement for ladders (-1 to 1)
    }

    /// <summary>
    /// Handle movement input from Input System.
    /// Updates both local movement state and ActionRecorder for clone system.
    /// Called when movement input is performed or canceled.
    /// </summary>
    /// <param name="_axis">Movement vector from input (-1 to 1 for x/y axes)</param>
    private void Move(Vector2 _axis) {
        axis = _axis;
        
        // Immediately update ActionRecorder with movement for frame-perfect recording
        if (actionRecorder != null)
        {
            actionRecorder.CurrentMovement = _axis.x;
        }
    }

    /// <summary>
    /// Handle jump input when jump button is pressed.
    /// Supports both regular jumping and drop-through platform jumping.
    /// Updates ActionRecorder state for clone system reproduction.
    /// </summary>
    /// <param name="context">Input action context from Input System</param>
    private void Jump(InputAction.CallbackContext context) {
        if (axis.y < 0) {
            // Down + Jump = Drop through one-way platforms
            character.JumpDown();
        } else {
            // Regular jump
            character.Jump();
        }
        jumpHeld = true;
        // Note: JustJumped flag is automatically set by CharacterController2D.Jump() method
    }

    /// <summary>
    /// Handle jump button release for variable jump height.
    /// Shorter button presses result in lower jumps for precise platforming.
    /// </summary>
    /// <param name="context">Input action context from Input System</param>
    private void EndJump(InputAction.CallbackContext context) {
        character.EndJump();
        jumpHeld = false;
    }

    /// <summary>
    /// Handle dash input when dash button is pressed.
    /// Dashes in the direction of current movement input.
    /// </summary>
    /// <param name="context">Input action context from Input System</param>
    private void Dash(InputAction.CallbackContext context) {
        character.Dash(axis);
        // Note: JustDashed flag is automatically set by CharacterController2D.Dash() method
    }

    /// <summary>
    /// Handle interaction input when interact button is pressed.
    /// Used for picking up objects, activating switches, and other environmental interactions.
    /// </summary>
    /// <param name="context">Input action context from Input System</param>
    private void Interact(InputAction.CallbackContext context) {
        if (interact) {
            interact.Interact();
            // Note: JustInteracted flag is automatically set by InteractSystem.Interact() method
        }
    }

    /// <summary>
    /// Handle attack input when attack button is pressed.
    /// Throws currently picked up objects, if any. No effect if not holding an object.
    /// </summary>
    /// <param name="context">Input action context from Input System</param>
    private void Attack(InputAction.CallbackContext context) {
        if (interact && interact.PickedUpObject) {
            interact.Throw();
            // Note: JustAttacked flag is automatically set by InteractSystem.Throw() method
        }
    }

    /// <summary>
    /// Initiate a soft respawn sequence that returns the player to the last checkpoint.
    /// This respawn preserves player stats and progress while repositioning them.
    /// Uses a fade effect to smooth the transition.
    /// </summary>
    public void SoftRespawn() {
        character.Immobile = true; // Prevent movement during respawn sequence
        Invoke("StartSoftRespawn", softRespawnDelay);
    }

    /// <summary>
    /// Begin the visual fade-out effect for soft respawn.
    /// Called after the soft respawn delay to start the transition sequence.
    /// </summary>
    private void StartSoftRespawn() {
        cameraController.FadeOut();
        Invoke("EndSoftRespawn", softRespawnDuration);
    }

    /// <summary>
    /// Complete the soft respawn by repositioning the player and fading back in.
    /// Restores player mobility and completes the respawn sequence.
    /// </summary>
    private void EndSoftRespawn() {
        checkpoint.ReturnToSoftCheckpoint(); // Move player to checkpoint position
        cameraController.FadeIn();          // Fade camera back in
        character.Immobile = false;         // Restore player movement
    }

    /// <summary>
    /// Enable input processing when the component becomes active.
    /// Called automatically by Unity when the GameObject is enabled.
    /// </summary>
    void OnEnable() {
        controls.Player.Enable();
    }

    /// <summary>
    /// Disable input processing when the component becomes inactive.
    /// Called automatically by Unity when the GameObject is disabled.
    /// Prevents input processing for clones or when player is inactive.
    /// </summary>
    void OnDisable() {
        controls.Player.Disable();
    }
}