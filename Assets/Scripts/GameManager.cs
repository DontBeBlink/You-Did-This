using UnityEngine;

/// <summary>
/// Central game state management system that handles global game controls and debugging.
/// Manages pause/resume functionality, level restarting, and provides comprehensive
/// debug information overlay for development and testing.
/// 
/// Core Responsibilities:
/// - Global pause/resume system with time scale control
/// - Level restarting functionality
/// - Debug information display for development
/// - Input handling for global game controls (Escape, R, F1)
/// - Singleton pattern for global access
/// - Cross-scene persistence for consistent game state
/// 
/// Features:
/// - Pause system that freezes all game time (Time.timeScale = 0)
/// - Quick level restart for puzzle iteration
/// - Real-time debug overlay showing clone system stats and performance
/// - Configurable key bindings for all controls
/// - Optional pause-on-start for testing scenarios
/// 
/// Debug Information Displayed:
/// - Clone system stats (total, stuck, active index)
/// - Loop timing information (current time, duration, time remaining)
/// - Level progress (goals completed, level name)
/// - Performance metrics (FPS, time scale)
/// - Control reminders for easy reference
/// 
/// Usage: Place on a GameObject in the first scene. Will persist across scene loads
/// and automatically find CloneManager and PuzzleLevel components in each scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private bool pauseOnStart = false;    // Whether to start the game paused (useful for testing)
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;  // Key to pause/resume the game
    [SerializeField] private KeyCode restartKey = KeyCode.R;     // Key to restart the current level
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;    // Whether debug info is available
    [SerializeField] private KeyCode debugKey = KeyCode.F1; // Key to toggle debug overlay
    
    // Game state tracking
    private bool isPaused = false;      // Current pause state
    private bool showDebug = false;     // Whether debug overlay is currently visible
    
    // Scene component references (found dynamically in each scene)
    private CloneManager cloneManager;  // For clone system debug information
    private PuzzleLevel currentLevel;   // For level progress debug information
    
    // Singleton instance for global access
    private static GameManager instance;
    public static GameManager Instance => instance;
    
    /// <summary>
    /// Initialize singleton pattern and ensure persistence across scene loads.
    /// Implements singleton pattern with DontDestroyOnLoad for global game management.
    /// </summary>
    private void Awake()
    {
        // Enforce singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scene changes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }
    }
    
    /// <summary>
    /// Find scene-specific components and apply initial game state.
    /// Called after Awake when all scene objects are initialized.
    /// </summary>
    private void Start()
    {
        // Find components in the current scene (will be called again when scenes change)
        cloneManager = FindFirstObjectByType<CloneManager>();
        currentLevel = FindFirstObjectByType<PuzzleLevel>();
        
        // Apply initial pause state if configured
        if (pauseOnStart)
        {
            PauseGame();
        }
    }
    
    /// <summary>
    /// Handle global input processing each frame.
    /// Processes pause, restart, and debug toggle inputs.
    /// </summary>
    private void Update()
    {
        HandleInput();
    }
    
    /// <summary>
    /// Process global input for game control functions.
    /// Handles pause/resume, level restart, and debug toggle inputs.
    /// Uses legacy Input system for global controls to avoid conflicts with player input.
    /// </summary>
    private void HandleInput()
    {
        // Toggle pause/resume
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
        
        // Restart current level
        if (Input.GetKeyDown(restartKey))
        {
            RestartLevel();
        }
        
        // Toggle debug information overlay
        if (Input.GetKeyDown(debugKey))
        {
            showDebug = !showDebug;
        }
    }
    
    /// <summary>
    /// Pause the game by setting time scale to zero.
    /// Freezes all time-dependent operations while keeping UI and input responsive.
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Freeze all time-dependent operations
        Debug.Log("Game Paused");
    }
    
    /// <summary>
    /// Resume the game by restoring normal time scale.
    /// Returns all time-dependent operations to normal speed.
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Restore normal time flow
        Debug.Log("Game Resumed");
    }
    
    /// <summary>
    /// Toggle between paused and resumed states.
    /// Convenient method for input binding and UI buttons.
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
    
    /// <summary>
    /// Restart the current level by reloading the active scene.
    /// Ensures time scale is restored before reloading to prevent issues.
    /// Useful for puzzle iteration and testing different solutions.
    /// </summary>
    public void RestartLevel()
    {
        Time.timeScale = 1f; // Ensure normal time scale before reloading
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// Render debug information overlay using Unity's OnGUI system.
    /// Displays comprehensive game state information for development and testing.
    /// Only renders when debug info is enabled and visible (F1 key toggle).
    /// </summary>
    private void OnGUI()
    {
        if (!showDebugInfo || !showDebug) return;
        
        // Create debug information panel in top-left corner
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== DEBUG INFO ===");
        
        // Clone system information
        if (cloneManager != null)
        {
            GUILayout.Label($"Total Clones: {cloneManager.TotalClones}");
            GUILayout.Label($"Stuck Clones: {cloneManager.StuckClones}");
            GUILayout.Label($"Active Clone Index: {cloneManager.ActiveCloneIndex}");
            GUILayout.Label($"Loop Time: {cloneManager.CurrentLoopTime:F1}s / {cloneManager.LoopDuration:F1}s");
            GUILayout.Label($"Next Loop In: {cloneManager.TimeUntilNextLoop:F1}s");
        }
        
        // Level progress information
        if (currentLevel != null)
        {
            GUILayout.Label($"Level: {currentLevel.LevelName}");
            GUILayout.Label($"Goals: {currentLevel.CompletedGoals}/{currentLevel.TotalGoals}");
            GUILayout.Label($"Completed: {currentLevel.IsCompleted}");
        }
        
        // Performance and system information
        GUILayout.Label($"Time Scale: {Time.timeScale}");
        GUILayout.Label($"FPS: {(1f / Time.unscaledDeltaTime):F1}");
        
        // Control reminders
        GUILayout.Space(10);
        GUILayout.Label("Controls:");
        GUILayout.Label($"{pauseKey}: Pause/Resume");
        GUILayout.Label($"{restartKey}: Restart Level");
        GUILayout.Label($"{debugKey}: Toggle Debug");
        
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// Whether the game is currently paused (time scale = 0).
    /// </summary>
    public bool IsPaused => isPaused;
}