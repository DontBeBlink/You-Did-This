using UnityEngine;

/// <summary>
/// Creates special ghost markers at clone start and end positions to help visualize the clone's path.
/// These ghosts are visually distinct from the normal ghost trail and show the beginning and end
/// of each clone's recorded action sequence.
/// </summary>
public class CloneStartEndGhosts : MonoBehaviour
{
    [Header("Start/End Ghost Settings")]
    [SerializeField] private Color startGhostColor = Color.lightCyan;    // Color for start position ghost
    [SerializeField] private Color endGhostColor = Color.darkCyan;       // Color for end position ghost
    [SerializeField] private float ghostAlpha = 0.4f;              // Alpha for start/end ghosts (more visible than trail)
    [SerializeField] private float ghostScale = 1f;              // Scale multiplier for start/end ghosts
    [SerializeField] private bool showStartGhost = true;            // Whether to show start position ghost
    [SerializeField] private bool showEndGhost = false;              // Whether to show end position ghost

    public bool ShowStartGhost
    {
        get { return showStartGhost; }
        set { showStartGhost = value; }
    }

    public bool ShowEndGhost
    {
        get { return showEndGhost; }
        set { showEndGhost = value; }
    }

    // References
    private Clone parentClone;
    private SpriteRenderer parentSpriteRenderer;
    private GameObject startGhost;
    private GameObject endGhost;
    private bool isInitialized = false;

    /// <summary>
    /// Initialize the start/end ghost system.
    /// </summary>
    private void Awake()
    {
        parentClone = GetComponent<Clone>();
        parentSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        isInitialized = true;
    }

    /// <summary>
    /// Create start and end ghosts when the clone starts replaying.
    /// </summary>
    private void Start()
    {
        if (!isInitialized || parentClone == null || parentSpriteRenderer == null)
            return;

    }

    /// <summary>
    /// Create the start and end position ghost markers.
    /// </summary>
    private void CreateStartEndGhosts()
    {
        if (parentClone == null) return;

        // Get first and last actions
        PlayerAction? firstAction = parentClone.FirstAction;
        PlayerAction? lastAction = parentClone.LastAction;

        // Create start ghost
        if (showStartGhost && firstAction.HasValue)
        {
            startGhost = CreateGhostMarker(firstAction.Value, startGhostColor, "StartGhost", firstAction.Value.facingRight);
        }

        // Create end ghost (only if different from start position)
        if (showEndGhost && lastAction.HasValue)
        {
            bool isDifferentPosition = !firstAction.HasValue ||
                Vector3.Distance(firstAction.Value.position, lastAction.Value.position) > 0.1f;

            if (isDifferentPosition)
            {
                endGhost = CreateGhostMarker(lastAction.Value, endGhostColor, "EndGhost", lastAction.Value.facingRight);
            }
        }
    }

    /// <summary>
    /// Create a ghost marker at the specified action position.
    /// </summary>
    /// <param name="action">The action containing position and state data</param>
    /// <param name="color">Color for the ghost marker</param>
    /// <param name="name">Name for the ghost GameObject</param>
    /// <param name="facingRight">Whether the clone was facing right at this position</param>
    /// <returns>The created ghost GameObject</returns>
    private GameObject CreateGhostMarker(PlayerAction action, Color color, string name, bool facingRight)
    {
        if (parentSpriteRenderer == null) return null;

        // Create ghost GameObject
        GameObject ghostObject = new GameObject($"Clone_{parentClone.CloneIndex}_{name}");
        ghostObject.transform.position = action.position;
        ghostObject.transform.rotation = parentSpriteRenderer.transform.rotation;

        // Set scale and direction
        Vector3 scale = parentSpriteRenderer.transform.localScale * ghostScale;
        if (!facingRight)
            scale.x = -Mathf.Abs(scale.x);
        else
            scale.x = Mathf.Abs(scale.x);
        ghostObject.transform.localScale = scale;

        // Copy sprite renderer
        SpriteRenderer ghostRenderer = ghostObject.AddComponent<SpriteRenderer>();
        Sprite spriteToUse = null;

        if (name == "StartGhost" && parentClone != null)
        {
            spriteToUse = parentClone.StartActionSprite;
            // Only create if we have a valid sprite
            if (spriteToUse == null)
            {
                //Destroy(ghostObject);
                return null;
            }
        }
        else if (name == "EndGhost" && parentClone != null)
        {
            spriteToUse = parentClone.EndActionSprite;
            // Only create if we have a valid sprite
            if (spriteToUse == null)
            {
                //Destroy(ghostObject);
                return null;
            }
        }
        else
        {
            // For any other ghosts, fallback to parent's current sprite
            spriteToUse = parentSpriteRenderer.sprite;
        }

        ghostRenderer.sprite = spriteToUse;
        ghostRenderer.sortingLayerName = parentSpriteRenderer.sortingLayerName;
        ghostRenderer.sortingOrder = parentSpriteRenderer.sortingOrder - 2; // Render behind clone and trail ghosts
        ghostRenderer.material = parentSpriteRenderer.material;

        // Set ghost color
        Color ghostColor = color;
        ghostColor.a = ghostAlpha;
        ghostRenderer.color = ghostColor;

        // Add a subtle pulsing effect to make it more noticeable
        GhostPulse pulseEffect = ghostObject.AddComponent<GhostPulse>();
        pulseEffect.Initialize(ghostColor, 1.0f, 0.3f); // Slow pulse

        return ghostObject;
    }

    /// <summary>
    /// Update ghosts if clone state changes.
    /// </summary>
    private void Update()
    {
        if (!isInitialized || parentClone == null) return;

        // Update ghost colors based on clone state
        if (parentClone.IsStuck)
        {
            UpdateGhostColor(startGhost, Color.green);
            UpdateGhostColor(endGhost, Color.green);
        }
    }

    /// <summary>
    /// Update the color of a ghost marker.
    /// </summary>
    /// <param name="ghost">The ghost GameObject to update</param>
    /// <param name="newColor">The new color to apply</param>
    private void UpdateGhostColor(GameObject ghost, Color newColor)
    {
        if (ghost == null) return;

        SpriteRenderer renderer = ghost.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            newColor.a = ghostAlpha;
            renderer.color = newColor;

            // Update pulse effect color too
            GhostPulse pulseEffect = ghost.GetComponent<GhostPulse>();
            if (pulseEffect != null)
            {
                pulseEffect.UpdateBaseColor(newColor);
            }
        }
    }

    /// <summary>
    /// Clean up ghost markers when the component is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (startGhost != null)
        {
            DestroyImmediate(startGhost);
        }

        if (endGhost != null)
        {
            DestroyImmediate(endGhost);
        }
    }
    // Add this to CloneStartEndGhosts.cs (inside the class)
    public void RefreshGhosts()
    {
        // Destroy old ghosts if they exist
        if (startGhost != null) Destroy(startGhost);
        if (endGhost != null) Destroy(endGhost);
        CreateStartEndGhosts();
    }
}

/// <summary>
/// Simple component to add a pulsing effect to ghost markers.
/// </summary>
public class GhostPulse : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private float pulseSpeed;
    private float pulseAmount;
    private float startTime;

    /// <summary>
    /// Initialize the pulse effect.
    /// </summary>
    /// <param name="color">Base color for pulsing</param>
    /// <param name="speed">Speed of the pulse</param>
    /// <param name="amount">Amount of alpha variation</param>
    public void Initialize(Color color, float speed, float amount)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = color;
        pulseSpeed = speed;
        pulseAmount = amount;
        startTime = Time.time;
    }

    /// <summary>
    /// Update the base color for pulsing.
    /// </summary>
    /// <param name="newColor">New base color</param>
    public void UpdateBaseColor(Color newColor)
    {
        baseColor = newColor;
    }

    /// <summary>
    /// Update the pulsing effect each frame.
    /// </summary>
    private void Update()
    {
        if (spriteRenderer == null) return;

        float pulse = Mathf.Sin((Time.time - startTime) * pulseSpeed) * pulseAmount;
        Color currentColor = baseColor;
        currentColor.a = Mathf.Clamp01(baseColor.a + pulse);
        spriteRenderer.color = currentColor;
    }
}