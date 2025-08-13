using System.Collections;
using System.Collections.Generic;
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
[RequireComponent(typeof(Collider2D))
]
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

    [Header("Instant Mode")]
    [SerializeField] private bool instantTeleport = false;          // If true, teleport instantly and preserve momentum
    [SerializeField] private bool preserveMomentum = true;          // If true, keep (rotated) velocity magnitude
    [SerializeField] private float exitSpeedScale = 1f;             // Multiplier applied to exit velocity

    public enum FacingVectorMode { TransformUp, TransformRight, CustomLocal }
    [Header("Facing Direction")]
    [SerializeField] private FacingVectorMode facingVectorMode = FacingVectorMode.TransformUp;
    [SerializeField] private Vector2 customFacingLocal = Vector2.up; // Used when mode = CustomLocal (normalized)

    [Header("Visual Effects")]
    [SerializeField] private bool useTransitionEffects = true;      // Use CloneManager's transition system
    [SerializeField] private bool playPortalSound = true;           // Play audio during teleportation

    [Header("Transition Timing")]
    [SerializeField] private float moveToPortalDuration = 0.2f;
    [SerializeField] private float preTeleportDelay = 0.5f;
    [SerializeField] private float postTeleportSettleDelay = 0.2f;
    [SerializeField] private float stopEffectsDelay = 0.3f;

    [Header("Anti-Bounce Settings")]
    [SerializeField] private float portalImmunityDuration = 0.35f;   // Ignore all portals for this long after teleport
    [SerializeField] private float targetExitOffset = 0.5f;          // Push player out of target portal collider

    [Header("Camera")]
    [SerializeField] private bool warpCinemachineOnTeleport = true; // Snap camera to player on teleport to avoid disorienting lag

    // Internal state tracking
    private bool isOnCooldown = false;                              // Prevent rapid re-triggering
    private bool isActivated = false;                               // Track if portal has been used (for one-time portals)
    private CloneManager cloneManager;                              // Reference for transition effects
    
    // Tracks per-player portal immunity window
    private static readonly Dictionary<int, float> portalImmunityUntil = new Dictionary<int, float>();

    /// <summary>
    /// Initialize portal components and validate setup.
    /// </summary>
    private void Awake()
    {
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
        // Check player immunity first
        var player = other.GetComponentInParent<PlayerController>();
        if (player != null && IsPlayerImmuneToPortals(player)) return;

        // Check if portal can be activated
        if (!CanActivatePortal(other))
            return;

        // Get the PlayerController component (allow child colliders)
        player = other.GetComponentInParent<PlayerController>();
        if (player == null)
        {
            Debug.LogWarning($"Portal '{gameObject.name}': No PlayerController found on triggering object or its parents!");
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

        // Additional immunity gate (covers cases where OnTriggerEnter2D wasn't passed a PlayerController yet)
        var player = other.GetComponentInParent<PlayerController>();
        if (player != null && IsPlayerImmuneToPortals(player)) return false;

        return true;
    }

    private bool IsPlayerImmuneToPortals(PlayerController player)
    {
        int id = player.GetInstanceID();
        if (portalImmunityUntil.TryGetValue(id, out float until))
        {
            return Time.time < until;
        }
        return false;
    }

    private void GrantPortalImmunity(PlayerController player, float duration)
    {
        int id = player.GetInstanceID();
        portalImmunityUntil[id] = Time.time + duration;
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

        // Try to locate a Portal component near/at the target for cooldown + exit offset
        Portal targetPortal = null;
        if (useTransformAsTarget && targetTransform != null)
        {
            targetPortal = targetTransform.GetComponentInParent<Portal>();
            if (targetPortal == null)
                targetPortal = targetTransform.GetComponentInChildren<Portal>(true);
        }

        // Start target portal cooldown early (best-effort)
        if (targetPortal != null)
        {
            targetPortal.StartPortalCooldown();
        }

        // Perform teleportation
        if (instantTeleport)
        {
            TeleportPlayerWithMomentum(player, finalTargetPosition, targetPortal);
        }
        else if (useTransitionEffects)
        {
            yield return StartCoroutine(PlayPortalTransition(player, finalTargetPosition));
        }
        else
        {
            // Direct teleportation without effects
            TeleportPlayer(player, finalTargetPosition, targetPortal);
        }

        // Mark as activated (for one-time portals)
        if (disableAfterUse)
        {
            isActivated = true;
        }

        // Start cooldown timer
        StartCoroutine(CooldownTimer());

        Debug.Log($"Portal '{gameObject.name}': Teleported player to {finalTargetPosition}");
    }

    /// <summary>
    /// Public method to start the portal's cooldown externally.
    /// </summary>
    public void StartPortalCooldown()
    {
        if (!isOnCooldown)
        {
            isOnCooldown = true; // ensure cooldown actually engages
            StartCoroutine(CooldownTimer());
        }
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

        // Preserve world Z
        float playerZ = player.transform.position.z;
        targetPos.z = playerZ;

        // Find target portal again for exit offset/cooldown safety
        Portal targetPortal = null;
        if (useTransformAsTarget && targetTransform != null)
        {
            targetPortal = targetTransform.GetComponentInParent<Portal>();
            if (targetPortal == null)
                targetPortal = targetTransform.GetComponentInChildren<Portal>(true);
        }

        if (character != null)
        {
            // Trigger transition animation and immobilize player briefly
            Animator playerAnimator = player.GetComponent<Animator>();
            if (playerAnimator != null)
                playerAnimator.SetTrigger("invulnerable");

            character.Immobile = true;
            character.SetGravityScale(0f);

            // Move player towards center of portal
            yield return character.MoveTowardsRoutine(this.transform.position, moveToPortalDuration);

            // Play portal sound
            if (playPortalSound && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRetractSound();
            }

            // Start CloneManager recording effects just before teleporting
            if (cloneManager != null)
            {
                cloneManager.TriggerStartRecordingEffects();
            }

            // Wait for pre-teleport animation
            yield return new WaitForSeconds(preTeleportDelay);

            // Teleport with exit offset + target cooldown
            TeleportPlayer(player, targetPos, targetPortal);

            // Short settle delay
            yield return new WaitForSeconds(postTeleportSettleDelay);

            // Reset physics state at new position
            character.SetPhysicsState(player.transform.position, Vector2.zero, Vector2.zero);
            character.SetGravityScale(1f);
            character.Immobile = false;
        }
        else
        {
            // Fallback: direct teleportation if no character controller
            TeleportPlayer(player, targetPos, targetPortal);
        }

        // Additional effects delay before stopping CloneManager effects
        yield return new WaitForSeconds(stopEffectsDelay);

        if (cloneManager != null)
        {
            cloneManager.TriggerStopRecordingEffects();
        }
    }

    // Compute a world-space facing vector for this portal based on settings
    private static Vector2 GetFacingVectorWorld(Portal portal)
    {
        switch (portal.facingVectorMode)
        {
            case FacingVectorMode.TransformRight:
                return portal.transform.right.normalized;
            case FacingVectorMode.CustomLocal:
                return portal.transform.TransformDirection((Vector3)portal.customFacingLocal).normalized;
            case FacingVectorMode.TransformUp:
            default:
                return portal.transform.up.normalized;
        }
    }

    // Rotate a Vector2 by angle in degrees (Z axis)
    private static Vector2 Rotate(Vector2 v, float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float cs = Mathf.Cos(rad);
        float sn = Mathf.Sin(rad);
        return new Vector2(v.x * cs - v.y * sn, v.x * sn + v.y * cs);
    }

    // Instant teleport that preserves and rotates momentum according to portal facing directions
    private void TeleportPlayerWithMomentum(PlayerController player, Vector3 targetPos, Portal targetPortal)
    {
        // Determine entry/exit facing
        Vector2 entryForward = GetFacingVectorWorld(this);
        Vector2 exitForward = targetPortal != null ? GetFacingVectorWorld(targetPortal) : entryForward;
        float deltaAngle = Vector2.SignedAngle(entryForward, exitForward);

        // Push out along exit facing
        Vector3 exitPush = (Vector3)exitForward.normalized * targetExitOffset;

        // Preserve Z
        targetPos.z = player.transform.position.z;

        // Use CharacterController2D velocity, not Rigidbody2D (controller ignores rb velocity)
        var character = player.GetComponent<CharacterController2D>();
        Vector2 inVel = Vector2.zero;
        if (character != null)
        {
            inVel = character.TotalSpeed; // current movement from custom controller
        }
        else
        {
            // Fallback to Rigidbody2D if no character (rare for your setup)
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) inVel = rb.linearVelocity;
        }

        // Compute out velocity
        Vector2 outVel = preserveMomentum ? Rotate(inVel, deltaAngle) * exitSpeedScale : Vector2.zero;

        // Teleport position
        Vector3 oldPos = player.transform.position;
        Vector3 newPos = targetPos + exitPush;
        player.transform.position = newPos;

        // Snap Cinemachine camera(s) to the new player position to prevent damping lag
        if (warpCinemachineOnTeleport && cloneManager != null)
        {
            cloneManager.WarpCamerasFollowing(player.transform, newPos - oldPos);
        }

        // Apply velocity via CharacterController2D so it actually takes effect
        if (character != null)
        {
            // Put momentum into externalForce so it decays with your friction system
            // Keep speed zero to avoid double-counting
            character.SetPhysicsState(newPos, Vector2.zero, outVel);

            // Update facing based on exit velocity if any
            if (outVel.sqrMagnitude > 0.0001f)
            {
                bool facingRight = outVel.x >= 0f;
                var anim = character.GetComponent<Animator>();
                if (anim != null) anim.SetBool("facingRight", facingRight);
            }
        }
        else
        {
            // Fallback to Rigidbody2D if no character
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = outVel;
        }

        // Grant temporary immunity to portals
        GrantPortalImmunity(player, portalImmunityDuration);

        // Ensure target portal is on cooldown even if found late
        if (targetPortal != null)
        {
            targetPortal.StartPortalCooldown();
        }
    }

    // Overload to apply offset/cooldown (non-instant path uses this)
    private void TeleportPlayer(PlayerController player, Vector3 targetPos, Portal targetPortal)
    {
        // Use exit portal facing for push
        Vector2 exitForward = targetPortal != null ? GetFacingVectorWorld(targetPortal) : GetFacingVectorWorld(this);
        Vector3 pushOut = (Vector3)exitForward.normalized * targetExitOffset;

        // Teleport position
        Vector3 oldPos = player.transform.position;
        targetPos.z = oldPos.z;
        Vector3 newPos = targetPos + pushOut;
        player.transform.position = newPos;

        // Snap Cinemachine camera(s) to the new player position to prevent damping lag
        if (warpCinemachineOnTeleport && cloneManager != null)
        {
            cloneManager.WarpCamerasFollowing(player.transform, newPos - oldPos);
        }

        // Grant temporary immunity to all portals after teleport
        GrantPortalImmunity(player, portalImmunityDuration);

        // Ensure target portal is on cooldown even if found late
        if (targetPortal != null)
        {
            targetPortal.StartPortalCooldown();
        }
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

    private void OnValidate()
    {
        if (customFacingLocal.sqrMagnitude < 1e-6f) customFacingLocal = Vector2.up;
        else customFacingLocal.Normalize();
        if (exitSpeedScale < 0f) exitSpeedScale = 0f;
    }
}