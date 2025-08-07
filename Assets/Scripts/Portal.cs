using System.Collections;
using UnityEngine;

/// <summary>
/// Portal component that teleports the player to a specific target position when they enter its trigger collider.
/// Reuses the same animation and transition system as the clone retract feature for consistency.
/// 
/// Core Functionality:
/// - Trigger-based teleportation when player enters the collider
/// - Configurable target position for flexible portal placement
/// - Reuses CloneManager's transition effects for consistent visual feedback
/// - Supports animation and audio feedback during teleportation
/// - Prevents rapid re-triggering with cooldown system
/// 
/// Setup Requirements:
/// - GameObject with Portal script
/// - Collider component with "Is Trigger" enabled
/// - Target position configured in inspector or via code
/// - CloneManager must be present in scene for transition effects
/// 
/// Usage: Place Portal prefabs in scenes where teleportation is needed.
/// Configure the target position in the inspector or set via code.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private Vector3 targetPosition = Vector3.zero;  // Position to teleport player to
    [SerializeField] private bool useTransformAsTarget = false;      // Use another GameObject's position as target
    [SerializeField] private Transform targetTransform;              // Alternative: use Transform's position as target
    
    [Header("Portal Behavior")]
    [SerializeField] private float cooldownTime = 1f;               // Prevent rapid re-triggering
    [SerializeField] private bool disableAfterUse = false;          // One-time use portal
    [SerializeField] private bool requiresPlayerTag = true;         // Only activate for objects with "Player" tag
    
    [Header("Visual Effects")]
    [SerializeField] private bool useTransitionEffects = true;      // Use CloneManager's transition system
    [SerializeField] private bool playPortalSound = true;           // Play audio during teleportation
    
    // Internal state tracking
    private bool isOnCooldown = false;                              // Prevent rapid re-triggering
    private bool isActivated = false;                               // Track if portal has been used (for one-time portals)
    private CloneManager cloneManager;                              // Reference for transition effects
    private Collider2D portalCollider;                             // Portal's trigger collider
    
    /// <summary>
    /// Initialize portal components and validate setup.
    /// </summary>
    private void Awake()
    {
        // Get required components
        portalCollider = GetComponent<Collider2D>();
        
        // Ensure the collider is configured as a trigger
        if (portalCollider != null && !portalCollider.isTrigger)
        {
            Debug.LogWarning($"Portal '{gameObject.name}': Collider should be set as trigger for portal functionality!");
            portalCollider.isTrigger = true;
        }
        
        // Find CloneManager for transition effects
        cloneManager = FindFirstObjectByType<CloneManager>();
        if (cloneManager == null && useTransitionEffects)
        {
            Debug.LogWarning($"Portal '{gameObject.name}': No CloneManager found in scene. Transition effects will be disabled.");
            useTransitionEffects = false;
        }
    }
    
    /// <summary>
    /// Handle player entering the portal trigger.
    /// </summary>
    /// <param name="other">The collider that entered the portal</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if portal can be activated
        if (!CanActivatePortal(other))
            return;
        
        // Get the PlayerController component
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.LogWarning($"Portal '{gameObject.name}': No PlayerController found on triggering object!");
            return;
        }
        
        // Activate the portal
        StartCoroutine(ActivatePortal(player));
    }
    
    /// <summary>
    /// Check if the portal can be activated by the given collider.
    /// </summary>
    /// <param name="other">The collider attempting to activate the portal</param>
    /// <returns>True if portal can be activated, false otherwise</returns>
    private bool CanActivatePortal(Collider2D other)
    {
        // Check if portal is on cooldown
        if (isOnCooldown)
        {
            return false;
        }
        
        // Check if portal has already been used (for one-time portals)
        if (disableAfterUse && isActivated)
        {
            return false;
        }
        
        // Check if object has the required tag
        if (requiresPlayerTag && !other.CompareTag("Player"))
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Perform the portal teleportation with transition effects.
    /// </summary>
    /// <param name="player">The PlayerController to teleport</param>
    private IEnumerator ActivatePortal(PlayerController player)
    {
        // Set cooldown to prevent rapid re-triggering
        isOnCooldown = true;
        
        // Calculate target position
        Vector3 finalTargetPosition = GetFinalTargetPosition();
        
        // Perform teleportation with transition effects
        if (useTransitionEffects)
        {
            yield return StartCoroutine(PlayPortalTransition(player, finalTargetPosition));
        }
        else
        {
            // Direct teleportation without effects
            TeleportPlayer(player, finalTargetPosition);
        }
        
        // Mark as activated (for one-time portals)
        if (disableAfterUse)
        {
            isActivated = true;
            portalCollider.enabled = false; // Disable the trigger
        }
        
        // Start cooldown timer
        StartCoroutine(CooldownTimer());
        
        Debug.Log($"Portal '{gameObject.name}': Teleported player to {finalTargetPosition}");
    }
    
    /// <summary>
    /// Calculate the final target position based on portal settings.
    /// </summary>
    /// <returns>The world position to teleport the player to</returns>
    private Vector3 GetFinalTargetPosition()
    {
        if (useTransformAsTarget && targetTransform != null)
        {
            return targetTransform.position;
        }
        else
        {
            return targetPosition;
        }
    }
    
    /// <summary>
    /// Play portal transition effects reusing CloneManager's animation system.
    /// This provides consistent visual feedback between portal and retract features.
    /// </summary>
    /// <param name="player">The PlayerController to animate</param>
    /// <param name="targetPos">The position to teleport to</param>
    private IEnumerator PlayPortalTransition(PlayerController player, Vector3 targetPos)
    {
        CharacterController2D character = player.GetComponent<CharacterController2D>();
        
        if (character != null)
        {
            // Trigger transition animation and immobilize player briefly
            Animator playerAnimator = player.GetComponent<Animator>();
            if (playerAnimator != null)
                playerAnimator.SetTrigger("pickup"); // Reuse pickup animation for portal
            
            // Immobilize player during transition
            character.Immobile = true;
            character.SetPhysicsState(character.transform.position, Vector2.zero, Vector2.zero);
            
            // Play portal sound (reusing retract sound for consistency)
            if (playPortalSound && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRetractSound();
            }
            
            // Wait for animation
            yield return new WaitForSeconds(0.5f);
            
            // Teleport the player
            TeleportPlayer(player, targetPos);
            
            // Reset physics state at new position
            character.SetPhysicsState(targetPos, Vector2.zero, Vector2.zero);
            
            // Re-enable player movement
            character.Immobile = false;
        }
        else
        {
            // Fallback: direct teleportation if no character controller
            TeleportPlayer(player, targetPos);
        }
        
        // Wait for any additional effects
        yield return new WaitForSeconds(0.3f);
    }
    
    /// <summary>
    /// Directly teleport the player to the target position.
    /// </summary>
    /// <param name="player">The PlayerController to teleport</param>
    /// <param name="targetPos">The position to teleport to</param>
    private void TeleportPlayer(PlayerController player, Vector3 targetPos)
    {
        player.transform.position = targetPos;
    }
    
    /// <summary>
    /// Handle cooldown timer to prevent rapid re-triggering.
    /// </summary>
    private IEnumerator CooldownTimer()
    {
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }
    
    #region Public Properties and Methods
    
    /// <summary>
    /// Set the target position for this portal.
    /// </summary>
    /// <param name="newTarget">The new target position</param>
    public void SetTargetPosition(Vector3 newTarget)
    {
        targetPosition = newTarget;
        useTransformAsTarget = false;
    }
    
    /// <summary>
    /// Set the target transform for this portal.
    /// </summary>
    /// <param name="newTargetTransform">The transform to use as target</param>
    public void SetTargetTransform(Transform newTargetTransform)
    {
        targetTransform = newTargetTransform;
        useTransformAsTarget = true;
    }
    
    /// <summary>
    /// Get the current target position for this portal.
    /// </summary>
    /// <returns>The current target position</returns>
    public Vector3 GetTargetPosition()
    {
        return GetFinalTargetPosition();
    }
    
    /// <summary>
    /// Whether this portal is currently on cooldown.
    /// </summary>
    public bool IsOnCooldown => isOnCooldown;
    
    /// <summary>
    /// Whether this portal has been activated (relevant for one-time portals).
    /// </summary>
    public bool IsActivated => isActivated;
    
    /// <summary>
    /// Reset the portal to its initial state (for one-time portals).
    /// </summary>
    public void ResetPortal()
    {
        isActivated = false;
        isOnCooldown = false;
        if (portalCollider != null)
        {
            portalCollider.enabled = true;
        }
    }
    
    #endregion
    
    #region Gizmos for Scene View
    
    /// <summary>
    /// Draw gizmos in the scene view to visualize portal connections.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Draw portal position
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw target position
        Vector3 target = GetFinalTargetPosition();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target, 0.5f);
        
        // Draw connection line
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, target);
        
        // Draw direction arrow at target
        Vector3 direction = (target - transform.position).normalized;
        Vector3 arrowTip = target + direction * 0.3f;
        Gizmos.DrawLine(target, arrowTip);
    }
    
    #endregion
}