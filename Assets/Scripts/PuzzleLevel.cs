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

    // private CloneManager cloneManager;
    private int completedGoals = 0;
    
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
        
        // for now just go to next level
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
    }
    
    public bool IsCompleted => levelCompleted;
    public int CompletedGoals => completedGoals;
    public int TotalGoals => requiredGoals.Length;
    public string LevelName => levelName;
}