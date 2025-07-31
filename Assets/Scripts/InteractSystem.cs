using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Comprehensive interaction system that enables characters to interact with objects and pick up items.
/// Handles detection of nearby interactable objects, visual feedback with input icons,
/// and object pickup/throwing mechanics essential for puzzle solving.
/// 
/// Core Functionality:
/// - Proximity-based detection of interactable objects using physics overlap checks
/// - Visual feedback system with context-sensitive input icons
/// - Object pickup and throwing mechanics for puzzle manipulation
/// - Integration with action recording system for clone replay
/// - Support for different input device icon display (keyboard/gamepad)
/// 
/// Interaction Types Supported:
/// - Switch activation and button pressing
/// - Object pickup and carrying (when canPickup is enabled)
/// - Object throwing with configurable force and direction
/// - Generic object interaction through InteractableObject interface
/// 
/// Visual Feedback:
/// - Automatic input icon display above interactable objects
/// - Context-sensitive icons based on input device
/// - Range visualization in scene view for development
/// - Icon positioning with configurable offsets
/// 
/// Integration Points:
/// - Used by PlayerController for player interactions
/// - Used by Clone system for reproducing player interactions
/// - Interfaces with CharacterController2D for action flags
/// - Works with all objects implementing InteractableObject interface
/// 
/// Usage: Required component on any character that needs interaction capabilities.
/// Automatically detects nearby interactable objects and provides visual feedback.
/// </summary>
[RequireComponent(typeof(CharacterController2D))]
public class InteractSystem : MonoBehaviour {

    [Header("Interaction Detection")]
    [Tooltip("How distant objects can be to be interacted with (visible in a magenta gizmo)")]
    public float interactRange = 0.5f;
    [Tooltip("Offset for the range's starting position relative to the character position")]
    public Vector2 rangePositionOffset;
    
    [Header("Visual Feedback")]
    [Tooltip("GameObject for the icon that will appear above interactables")]
    public SpriteRenderer inputIcon;
    [Tooltip("Offset for the icon's position relative to the attached object's position")]
    public Vector2 iconPositionOffset;
    [Tooltip("Icon shown when using a keyboard")]
    public Sprite keyboardIcon;
    [Tooltip("Icon shown when using a Xbox-like gamepad")]
    public Sprite xboxIcon;
    [Tooltip("Icon shown when using a PSX-like gamepad")]
    public Sprite psxIcon;
    
    [Header("Pickup System")]
    [Tooltip("When enabled, allows the character to detect and pick up pick-upable objects")]
    public bool canPickup;
    [Tooltip("Offset for the object's position when picked up by a character")]
    public Vector2 pickupPositionOffset;
    [Tooltip("Amount and direction of the force applied when throwing the picked object")]
    public Vector2 throwForce;

    /// <summary>
    /// Currently picked up object, if any. Null if no object is being carried.
    /// </summary>
    public PickUpObject PickedUpObject { get; set; }

    // Internal state and component references
    private InteractableObject closestObject = null;  // Currently detected closest interactable object
    private PhysicsConfig pConfig;                    // Physics configuration for layer masks
    private CharacterController2D character;          // For setting action flags for recording system

    /// <summary>
    /// Initialize interaction system components and find required configuration.
    /// Sets up physics configuration for interaction detection and prepares visual feedback.
    /// </summary>
    void Start() {
        // Initially hide the input icon until an interactable is detected
        inputIcon.enabled = false;
        
        // Get required components
        character = GetComponent<CharacterController2D>();
        
        // Find or create physics configuration for interaction layer masks
        pConfig = GameObject.FindFirstObjectByType<PhysicsConfig>();
        if (!pConfig) {
            pConfig = (PhysicsConfig) new GameObject().AddComponent(typeof(PhysicsConfig));
            pConfig.gameObject.name = "Physics Config";
            Debug.LogWarning("PhysicsConfig not found on the scene! Using default config.");
        }
    }

    /// <summary>
    /// Continuously detect nearby interactable objects and manage visual feedback.
    /// Finds the closest interactable object within range and displays appropriate input icon.
    /// Called every frame to provide responsive interaction detection.
    /// </summary>
    void Update() {
        // Reset closest object detection each frame
        closestObject = null;
        
        // Find all interactable objects within interaction range using physics overlap
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + (Vector3) rangePositionOffset,
            interactRange, pConfig.interactableMask);
            
        // Evaluate each detected object to find the closest valid interactable
        foreach (Collider2D hit in hits) {
            InteractableObject obj = hit.GetComponent<InteractableObject>();
            
            // Validate object for interaction:
            // - Must have InteractableObject component and be interactable
            // - If pickup is disabled, exclude pickup objects
            // - If already carrying something, exclude additional pickup objects
            // - Prefer closer objects if multiple are available
            if (obj && obj.interactable && (canPickup && !PickedUpObject || !obj.GetComponent<PickUpObject>()) &&
                (!closestObject || Vector2.Distance(transform.position, closestObject.transform.position) >
                    Vector2.Distance(transform.position, obj.transform.position))) {
                closestObject = obj;
            }
        }
        
        // Show/hide input icon based on object detection
        inputIcon.enabled = closestObject;
        
        // Position input icon above the closest interactable object
        if (closestObject) {
            inputIcon.transform.parent = closestObject.transform;
            inputIcon.transform.position = closestObject.transform.position + (Vector3) iconPositionOffset;
        } else {
            // Return icon to character when no objects are nearby
            inputIcon.transform.parent = transform;
        }
    }

    /// <summary>
    /// Interact with the closest detected interactable object.
    /// Calls the object's Interact method and sets action recording flag for clone system.
    /// Called by PlayerController when interact input is pressed, or by Clone during replay.
    /// </summary>
    public void Interact() {
        if (closestObject) {
            // Execute the interaction on the closest object
            closestObject.Interact(this);
            closestObject = null; // Clear after interaction to prevent repeat interactions
            
            // Set flag for action recording system (used by clones for replay)
            if (character != null) {
                character.JustInteracted = true;
            }
        }
    }

    /// <summary>
    /// Throw the currently picked up object with configured force.
    /// Releases the object from pickup state and applies physics force for throwing.
    /// Called by PlayerController when attack input is pressed while holding an object.
    /// </summary>
    public void Throw() {
        if (PickedUpObject) {
            // Execute throw with configured force vector
            PickedUpObject.Throw(throwForce);
            
            // Set flag for action recording system (used by clones for replay)
            if (character != null) {
                character.JustAttacked = true;
            }
        }
    }

    /// <summary>
    /// Draw debug gizmos in scene view to visualize interaction range.
    /// Shows a magenta wire sphere indicating the detection area for interactable objects.
    /// Only visible when the GameObject is selected in the editor.
    /// </summary>
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.magenta * new Color(1, 1, 1, 0.5f);
        Gizmos.DrawWireSphere(transform.position + (Vector3) rangePositionOffset, interactRange);
    }
}