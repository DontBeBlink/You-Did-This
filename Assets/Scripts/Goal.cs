using UnityEngine;

public class Goal : MonoBehaviour
{
    [Header("Goal Settings")]
    [SerializeField] private bool requiresSpecificClone = false;
    [SerializeField] private int requiredCloneIndex = -1;
    [SerializeField] private bool isCompleted = false;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color incompleteColor = Color.yellow;
    [SerializeField] private Color completeColor = Color.green;
    [SerializeField] private GameObject completionEffect;
    
    private SpriteRenderer spriteRenderer; 
    private CloneManager cloneManager;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cloneManager = FindFirstObjectByType<CloneManager>();
        
        UpdateVisuals();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Clone clone = other.GetComponent<Clone>();
        if (clone != null && !isCompleted)
        {
            // Check if this is the correct clone (if specific clone required)
            if (requiresSpecificClone)
            {
                var allClones = cloneManager.GetAllClones();
                int cloneIndex = allClones.IndexOf(clone);
                
                if (cloneIndex != requiredCloneIndex)
                {
                    Debug.Log($"Wrong clone reached goal. Required: {requiredCloneIndex}, Got: {cloneIndex}");
                    return;
                }
            }
            
            CompleteGoal(clone);
        }
    }
    
    private void CompleteGoal(Clone clone)
    {
        isCompleted = true;
        
        // Make the clone stuck
        if (cloneManager != null)
        {
            cloneManager.SetCloneStuck(clone, this);
        }
        
        // Visual feedback
        UpdateVisuals();
        
        if (completionEffect != null)
        {
            Instantiate(completionEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log($"Goal completed by clone!");
        
        // Play goal reached sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGoalReachedSound();
        }
        
        // Trigger any additional completion logic here
        OnGoalCompleted();
    }
    
    protected virtual void OnGoalCompleted()
    {
        // Override in derived classes for specific goal behaviors
    }
    
    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isCompleted ? completeColor : incompleteColor;
        }
    }
    
    public bool IsCompleted => isCompleted;
    public int RequiredCloneIndex => requiredCloneIndex;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isCompleted ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        if (requiresSpecificClone)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.2f);
        }
    }
}