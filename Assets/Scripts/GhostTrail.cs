using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a ghost trail effect for clones showing past action positions that dissipate over time.
/// This component creates afterimage-style ghost sprites at positions where the clone performed actions,
/// which then fade out over time to create a visual trail of the clone's past movements and actions.
/// 
/// Features:
/// - Creates ghostly afterimages at past action positions
/// - Afterimages fade out over time creating a dissipating trail effect
/// - Different visual styles for active vs stuck clones
/// - Performance optimized for multiple clones
/// - Shows actual past positions rather than continuous trail
/// </summary>
public class GhostTrail : MonoBehaviour
{
    [Header("Ghost Trail Settings")]
    [SerializeField] private float ghostLifetime = 2.0f;            // How long each ghost persists before fading
    [SerializeField] private float spawnInterval = 0.2f;            // Time between ghost spawns
    [SerializeField] private int maxGhosts = 10;                    // Maximum number of ghosts at once
    [SerializeField] private Color activeGhostColor = Color.cyan;   // Color for active clone ghosts
    [SerializeField] private Color stuckGhostColor = Color.green;   // Color for stuck clone ghosts
    [SerializeField] private float ghostAlpha = 0.3f;              // Starting alpha for ghost sprites
    
    [Header("Performance")]
    [SerializeField] private float minMoveDistance = 0.5f;          // Minimum distance moved before spawning ghost
    
    // Ghost tracking and management
    private List<GhostAfterimage> activeGhosts = new List<GhostAfterimage>();
    private Clone parentClone;
    private SpriteRenderer parentSpriteRenderer;
    private bool isInitialized = false;
    private float lastGhostTime;
    private Vector3 lastGhostPosition;
    
    /// <summary>
    /// Individual ghost afterimage that fades over time
    /// </summary>
    private class GhostAfterimage
    {
        public GameObject gameObject;
        public SpriteRenderer spriteRenderer;
        public float spawnTime;
        public float lifetime;
        public Color originalColor;
        
        public bool IsExpired => Time.time - spawnTime >= lifetime;
        
        public void UpdateFade()
        {
            if (spriteRenderer == null) return;
            
            float age = Time.time - spawnTime;
            float fadeProgress = age / lifetime;
            Color color = originalColor;
            color.a = originalColor.a * (1f - fadeProgress);
            spriteRenderer.color = color;
        }
    }
    
    /// <summary>
    /// Initialize the ghost trail system.
    /// </summary>
    private void Awake()
    {
        parentClone = GetComponent<Clone>();
        parentSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        lastGhostPosition = transform.position;
        lastGhostTime = Time.time;
        isInitialized = true;
    }
    
    /// <summary>
    /// Create a ghost afterimage at the current position.
    /// </summary>
    private void SpawnGhost()
    {
        if (parentSpriteRenderer == null) return;
        
        // Create ghost GameObject
        GameObject ghostObject = new GameObject($"Ghost_{activeGhosts.Count}");
        ghostObject.transform.position = parentSpriteRenderer.transform.position;
        ghostObject.transform.rotation = parentSpriteRenderer.transform.rotation;

        // Determine look direction (flip X scale if facing left)
        Vector3 ghostScale = parentSpriteRenderer.transform.localScale;
        if (parentClone != null && parentClone.Character != null)
        {
            bool facingRight = parentClone.Character.FacingRight;
            // If not facing right, flip X scale
            if (!facingRight)
                ghostScale.x = -Mathf.Abs(ghostScale.x);
            else
                ghostScale.x = Mathf.Abs(ghostScale.x);
        }
        ghostObject.transform.localScale = ghostScale;

        // Copy sprite renderer
        SpriteRenderer ghostRenderer = ghostObject.AddComponent<SpriteRenderer>();
        ghostRenderer.sprite = parentSpriteRenderer.sprite;
        ghostRenderer.sortingLayerName = parentSpriteRenderer.sortingLayerName;
        ghostRenderer.sortingOrder = parentSpriteRenderer.sortingOrder - 1; // Render behind original;
        ghostRenderer.material = parentSpriteRenderer.material; // Use same material for consistency

        // Set ghost color based on clone state
        Color ghostColor = parentClone != null && parentClone.IsStuck ? stuckGhostColor : activeGhostColor;
        ghostColor.a = ghostAlpha;
        ghostRenderer.color = ghostColor;

        // Create ghost data
        GhostAfterimage ghost = new GhostAfterimage
        {
            gameObject = ghostObject,
            spriteRenderer = ghostRenderer,
            spawnTime = Time.time,
            lifetime = ghostLifetime,
            originalColor = ghostColor
        };

        activeGhosts.Add(ghost);

        // Remove oldest ghost if we've exceeded max count
        if (activeGhosts.Count > maxGhosts)
        {
            DestroyGhost(activeGhosts[0]);
            activeGhosts.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// Destroy a ghost afterimage.
    /// </summary>
    private void DestroyGhost(GhostAfterimage ghost)
    {
        if (ghost.gameObject != null)
        {
            DestroyImmediate(ghost.gameObject);
        }
    }
    
    /// <summary>
    /// Update ghost trail system each frame.
    /// </summary>
    private void Update()
    {
        if (!isInitialized || parentClone == null) return;
        
        // Only create ghosts if clone is replaying and not stuck
        if (parentClone.IsReplaying && !parentClone.IsStuck)
        {
            // Check if enough time has passed and clone has moved enough to spawn a new ghost
            float timeSinceLastGhost = Time.time - lastGhostTime;
            float distanceMoved = Vector3.Distance(transform.position, lastGhostPosition);
            
            if (timeSinceLastGhost >= spawnInterval && distanceMoved >= minMoveDistance)
            {
                SpawnGhost();
                lastGhostTime = Time.time;
                lastGhostPosition = transform.position;
            }
        }
        
        // Update existing ghosts (fade them over time)
        UpdateGhosts();
    }
    
    /// <summary>
    /// Update all active ghost afterimages.
    /// </summary>
    private void UpdateGhosts()
    {
        for (int i = activeGhosts.Count - 1; i >= 0; i--)
        {
            GhostAfterimage ghost = activeGhosts[i];
            
            if (ghost.IsExpired || ghost.gameObject == null)
            {
                DestroyGhost(ghost);
                activeGhosts.RemoveAt(i);
            }
            else
            {
                ghost.UpdateFade();
            }
        }
    }
    
    /// <summary>
    /// Enable the ghost trail effect.
    /// </summary>
    public void EnableTrail()
    {
        enabled = true;
    }
    
    /// <summary>
    /// Disable the ghost trail effect.
    /// </summary>
    public void DisableTrail()
    {
        enabled = false;
    }
    
    /// <summary>
    /// Clear all existing ghost afterimages.
    /// </summary>
    public void ClearTrail()
    {
        for (int i = activeGhosts.Count - 1; i >= 0; i--)
        {
            DestroyGhost(activeGhosts[i]);
        }
        activeGhosts.Clear();
    }
    
    /// <summary>
    /// Configure ghost trail for specific clone state.
    /// </summary>
    /// <param name="cloneState">The state to configure trail for</param>
    public void ConfigureForCloneState(CloneState cloneState)
    {
        switch (cloneState)
        {
            case CloneState.Active:
                EnableTrail();
                break;
            case CloneState.Stuck:
                // Keep existing ghosts but stop creating new ones
                EnableTrail();
                break;
            case CloneState.Inactive:
                DisableTrail();
                ClearTrail();
                break;
        }
    }
    
    /// <summary>
    /// Clean up all ghost afterimages when the component is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        ClearTrail();
    }
}

/// <summary>
/// Enum representing different clone states for trail configuration.
/// </summary>
public enum CloneState
{
    Active,     // Clone is actively replaying actions
    Stuck,      // Clone is stuck at a goal
    Inactive    // Clone is paused or inactive
}