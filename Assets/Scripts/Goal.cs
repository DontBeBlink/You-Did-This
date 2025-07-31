using UnityEngine;

/// <summary>
/// Represents a puzzle completion target that clones can reach to become permanent puzzle elements.
/// Goals are the primary mechanism for converting dynamic clones into static puzzle components,
/// allowing players to build solutions using their recorded actions.
/// 
/// Core Functionality:
/// - Detects when clones (or player) enter the goal area
/// - Can require specific clones by index for complex puzzle sequencing
/// - Makes reaching clones "stuck" and immobile at the goal position
/// - Provides visual feedback for goal state (yellow=incomplete, green=complete)
/// - Triggers audio and visual effects upon completion
/// - Supports inheritance for specialized goal behaviors
/// 
/// Goal Types:
/// - Generic Goals: Any clone can complete them
/// - Specific Clone Goals: Only a particular clone (by creation order) can complete
/// - Player Goals: Only the active player can complete them
/// 
/// Visual System:
/// - Yellow color indicates incomplete/available goals
/// - Green color indicates completed goals with stuck clones
/// - Optional completion effects (particles, etc.)
/// - Debug gizmos show goal area and specific clone requirements
/// 
/// Usage: Place on GameObjects with Collider2D set as triggers. Configure goal type
/// and requirements in inspector. Goals automatically integrate with CloneManager.
/// </summary>
public class Goal : MonoBehaviour
{
    [Header("Goal Settings")]
    [SerializeField] private bool requiresSpecificClone = false;  // Whether only a specific clone can complete this goal
    [SerializeField] private int requiredCloneIndex = -1;         // Index of the required clone (if specific clone needed)
    [SerializeField] private bool isCompleted = false;            // Current completion state
    [SerializeField] private bool isPlayerGoal = false;           // Whether this goal is for the player instead of clones
    
    [Header("Visual Feedback")]
    [SerializeField] private Color incompleteColor = Color.yellow; // Color when goal is available
    [SerializeField] private Color completeColor = Color.green;    // Color when goal is completed
    [SerializeField] private GameObject completionEffect;          // Optional particle effect on completion
    
    // Component references
    private SpriteRenderer spriteRenderer;  // For visual state indication
    private CloneManager cloneManager;      // For clone management operations
    
    
    
    /// <summary>
    /// Initialize goal components and set initial visual state.
    /// Finds required components and applies initial visual styling.
    /// </summary>
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cloneManager = FindFirstObjectByType<CloneManager>();

        UpdateVisuals();
    }

    /// <summary>
    /// Handle clone or player entering the goal area.
    /// Validates goal requirements and triggers completion if appropriate.
    /// Called automatically by Unity's trigger system.
    /// </summary>
    /// <param name="other">The collider that entered the goal trigger</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle clone goal completion
        Clone clone = other.GetComponent<Clone>();
        if (clone != null && !isCompleted)
        {
            // Check if this goal requires a specific clone by creation index
            if (requiresSpecificClone)
            {
                var allClones = cloneManager.GetAllClones();
                int cloneIndex = allClones.IndexOf(clone);

                if (cloneIndex != requiredCloneIndex)
                {
                    Debug.Log($"Wrong clone reached goal. Required: {requiredCloneIndex}, Got: {cloneIndex}");
                    return; // Wrong clone, don't complete the goal
                }
            }

            CompleteGoal(clone);
        } 
        // Handle player goal completion (alternative goal type)
        else if (isPlayerGoal && other.CompareTag("Player"))
        {
            CompleteGoal(null); // No clone to stick since player completed it
        }
    }
    
    /// <summary>
    /// Execute goal completion sequence including clone sticking and feedback.
    /// Makes the reaching clone permanent, updates visuals, plays effects, and triggers audio.
    /// </summary>
    /// <param name="clone">The clone that completed the goal (null for player goals)</param>
    private void CompleteGoal(Clone clone)
    {
        isCompleted = true;
        
        // Make the clone stuck at this goal (permanent puzzle element)
        if (cloneManager != null && clone != null)
        {
            cloneManager.SetCloneStuck(clone, this);
        }
        
        // Update visual appearance to show completion
        UpdateVisuals();
        
        // Spawn completion effect if configured
        if (completionEffect != null)
        {
            Instantiate(completionEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log($"Goal completed by {(clone != null ? "clone" : "player")}!");
        
        // Play audio feedback for goal completion
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGoalReachedSound();
        }
        
        // Allow derived classes to add custom completion logic
        OnGoalCompleted();
    }
    
    /// <summary>
    /// Virtual method for derived classes to implement custom goal completion behavior.
    /// Override this in specialized goal types to add unique functionality like
    /// activating mechanisms, unlocking areas, or triggering story events.
    /// </summary>
    protected virtual void OnGoalCompleted()
    {
        // Override in derived classes for specific goal behaviors
    }
    
    /// <summary>
    /// Update the visual appearance based on completion state.
    /// Changes color from yellow (incomplete) to green (complete).
    /// </summary>
    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isCompleted ? completeColor : incompleteColor;
        }
    }
    
    // Properties for external access to goal state
    
    /// <summary>
    /// Whether this goal has been completed by a clone or player.
    /// </summary>
    public bool IsCompleted => isCompleted;
    
    /// <summary>
    /// The index of the specific clone required to complete this goal.
    /// Returns -1 if any clone can complete the goal.
    /// </summary>
    public int RequiredCloneIndex => requiredCloneIndex;
    
    /// <summary>
    /// Draw debug gizmos in the scene view to visualize goal area and requirements.
    /// Shows goal bounds and indicates if specific clone is required.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Draw goal area with completion state color
        Gizmos.color = isCompleted ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        // Draw additional indicator for specific clone requirements
        if (requiresSpecificClone)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.2f);
        }
    }
}