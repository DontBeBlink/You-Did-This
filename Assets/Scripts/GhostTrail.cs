using UnityEngine;

/// <summary>
/// Creates a ghost trail effect for clones using Unity's TrailRenderer.
/// This component adds a visual trail that follows clone movement,
/// enhancing the visual feedback of clone actions and making it easier
/// to track clone paths during puzzle solving.
/// 
/// Features:
/// - Configurable trail color with alpha transparency
/// - Different trail styles for active vs stuck clones
/// - Automatic cleanup when clone is destroyed
/// - Performance optimized for multiple clones
/// </summary>
[RequireComponent(typeof(TrailRenderer))]
public class GhostTrail : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private float trailTime = 0.5f;                 // How long trail persists
    [SerializeField] private float trailWidth = 0.1f;               // Width of trail
    [SerializeField] private Color activeTrailColor = Color.cyan;    // Color for active clones
    [SerializeField] private Color stuckTrailColor = Color.green;    // Color for stuck clones
    [SerializeField] private AnimationCurve trailWidthCurve = AnimationCurve.Linear(0, 1, 1, 0); // Width over lifetime
    
    [Header("Performance")]
    [SerializeField] private int trailVertexCount = 20;             // Number of trail points
    [SerializeField] private float minVertexDistance = 0.1f;        // Minimum distance between points
    
    private TrailRenderer trailRenderer;
    private Clone parentClone;
    private bool isInitialized = false;
    
    /// <summary>
    /// Initialize the trail renderer with default settings.
    /// </summary>
    private void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        parentClone = GetComponent<Clone>();
        
        SetupTrailRenderer();
    }
    
    /// <summary>
    /// Configure the trail renderer with visual settings.
    /// </summary>
    private void SetupTrailRenderer()
    {
        if (trailRenderer == null) return;
        
        // Basic trail settings
        trailRenderer.time = trailTime;
        trailRenderer.startWidth = trailWidth;
        trailRenderer.endWidth = 0f;
        trailRenderer.widthCurve = trailWidthCurve;
        trailRenderer.numCornerVertices = 5;
        trailRenderer.numCapVertices = 5;
        trailRenderer.minVertexDistance = minVertexDistance;
        
        // Material settings - use sprite glow material if available
        Material trailMaterial = Resources.Load<Material>("Materials/Sprite Glow");
        if (trailMaterial != null)
        {
            trailRenderer.material = trailMaterial;
        }
        
        // Set initial color
        UpdateTrailColor(false);
        
        // Performance settings
        trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trailRenderer.receiveShadows = false;
        
        isInitialized = true;
    }
    
    /// <summary>
    /// Update trail color based on clone state.
    /// </summary>
    /// <param name="isStuck">Whether the clone is stuck at a goal</param>
    public void UpdateTrailColor(bool isStuck)
    {
        if (trailRenderer == null) return;
        
        Color trailColor = isStuck ? stuckTrailColor : activeTrailColor;
        trailColor.a = 0.6f; // Set transparency
        
        // Create gradient from full color to transparent
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(trailColor, 0.0f), 
                new GradientColorKey(trailColor, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(trailColor.a, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        
        trailRenderer.colorGradient = gradient;
    }
    
    /// <summary>
    /// Monitor clone state changes to update trail appearance.
    /// </summary>
    private void Update()
    {
        if (!isInitialized || parentClone == null) return;
        
        // Update trail color based on clone state
        UpdateTrailColor(parentClone.IsStuck);
        
        // Disable trail if clone is not moving or replaying
        trailRenderer.enabled = parentClone.IsReplaying || parentClone.IsStuck;
    }
    
    /// <summary>
    /// Enable the trail effect.
    /// </summary>
    public void EnableTrail()
    {
        if (trailRenderer != null)
            trailRenderer.enabled = true;
    }
    
    /// <summary>
    /// Disable the trail effect.
    /// </summary>
    public void DisableTrail()
    {
        if (trailRenderer != null)
            trailRenderer.enabled = false;
    }
    
    /// <summary>
    /// Clear the current trail.
    /// </summary>
    public void ClearTrail()
    {
        if (trailRenderer != null)
            trailRenderer.Clear();
    }
    
    /// <summary>
    /// Configure trail for specific clone state.
    /// </summary>
    /// <param name="cloneState">The state to configure trail for</param>
    public void ConfigureForCloneState(CloneState cloneState)
    {
        switch (cloneState)
        {
            case CloneState.Active:
                UpdateTrailColor(false);
                EnableTrail();
                break;
            case CloneState.Stuck:
                UpdateTrailColor(true);
                EnableTrail();
                break;
            case CloneState.Inactive:
                DisableTrail();
                break;
        }
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