using UnityEngine;

public class PuzzleLevel : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private string levelName = "Level 1";
    [SerializeField] private Goal[] requiredGoals;
    [SerializeField] private int minClones = 1;
    [SerializeField] private int maxClones = 5;
    
    [Header("Completion")]
    [SerializeField] private GameObject completionEffect;
    [SerializeField] private bool levelCompleted = false;
    [SerializeField] private string nextScene = "NextLevel"; // Name of the next scene to load after completion
    [SerializeField] private string levelEndText = "Congratulations! You have completed the level!";

    // private CloneManager cloneManager;
    private int completedGoals = 0;

    private void Awake()
    {
        requiredGoals = FindObjectsByType<Goal>(FindObjectsSortMode.None);
        if (requiredGoals.Length == 0)
        {
            Debug.LogWarning("No goals found in the scene. Please ensure you have set up goals for this level.");
        }
    }

    private void Start()
    {
        //cloneManager = FindFirstObjectByType<CloneManager>();

        // Subscribe to goal completion events
        foreach (Goal goal in requiredGoals)
        {
            // We'll need to modify Goal script to have events, for now we'll check manually
        }
    }
    
    private void Update()
    {
        CheckLevelCompletion();
    }
    
    private void CheckLevelCompletion()
    {
        if (levelCompleted) return;
        
        completedGoals = 0;
        foreach (Goal goal in requiredGoals)
        {
            if (goal.IsCompleted)
                completedGoals++;
        }
        
        if (completedGoals >= requiredGoals.Length)
        {
            CompleteLevel();
        }
    }
    
    private void CompleteLevel()
    {
        levelCompleted = true;
        
        if (completionEffect != null)
        {
            Instantiate(completionEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log($"Level '{levelName}' completed!");
        
        // You could trigger level progression here
        OnLevelCompleted();
    }

    protected virtual void OnLevelCompleted()
    {
        // Override in derived classes for specific level completion behavior

        if (string.IsNullOrEmpty(nextScene))
        {
            // Show big message if no next scene
            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.ShowLevelCompleteMessage(levelEndText);
                Debug.Log("No next scene defined. Please set the next scene in the PuzzleLevel script.");
            }
            return;
        }

        // for now just go to next level
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
    }
    
    public bool IsCompleted => levelCompleted;
    public int CompletedGoals => completedGoals;
    public int TotalGoals => requiredGoals.Length;
    public string LevelName => levelName;
}