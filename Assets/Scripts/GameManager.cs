using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private bool pauseOnStart = false;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private KeyCode restartKey = KeyCode.R;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private KeyCode debugKey = KeyCode.F1;
    
    private bool isPaused = false;
    private bool showDebug = false;
    private CloneManager cloneManager;
    private PuzzleLevel currentLevel;
    
    private static GameManager instance;
    public static GameManager Instance => instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        cloneManager = FindFirstObjectByType<CloneManager>();
        currentLevel = FindFirstObjectByType<PuzzleLevel>();
        
        if (pauseOnStart)
        {
            PauseGame();
        }
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
        
        if (Input.GetKeyDown(restartKey))
        {
            RestartLevel();
        }
        
        if (Input.GetKeyDown(debugKey))
        {
            showDebug = !showDebug;
        }
    }
    
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Debug.Log("Game Paused");
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Debug.Log("Game Resumed");
    }
    
    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
    
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    private void OnGUI()
    {
        if (!showDebugInfo || !showDebug) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== DEBUG INFO ===");
        
        if (cloneManager != null)
        {
            GUILayout.Label($"Total Clones: {cloneManager.TotalClones}");
            GUILayout.Label($"Stuck Clones: {cloneManager.StuckClones}");
            GUILayout.Label($"Active Clone Index: {cloneManager.ActiveCloneIndex}");
            GUILayout.Label($"Loop Time: {cloneManager.CurrentLoopTime:F1}s / {cloneManager.LoopDuration:F1}s");
            GUILayout.Label($"Next Loop In: {cloneManager.TimeUntilNextLoop:F1}s");
        }
        
        if (currentLevel != null)
        {
            GUILayout.Label($"Level: {currentLevel.LevelName}");
            GUILayout.Label($"Goals: {currentLevel.CompletedGoals}/{currentLevel.TotalGoals}");
            GUILayout.Label($"Completed: {currentLevel.IsCompleted}");
        }
        
        GUILayout.Label($"Time Scale: {Time.timeScale}");
        GUILayout.Label($"FPS: {(1f / Time.unscaledDeltaTime):F1}");
        
        GUILayout.Space(10);
        GUILayout.Label("Controls:");
        GUILayout.Label($"{pauseKey}: Pause/Resume");
        GUILayout.Label($"{restartKey}: Restart Level");
        GUILayout.Label($"{debugKey}: Toggle Debug");
        
        GUILayout.EndArea();
    }
    
    public bool IsPaused => isPaused;
}