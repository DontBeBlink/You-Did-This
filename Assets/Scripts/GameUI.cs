using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI cloneCountText;
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI loopTimerText;
    [SerializeField] private Button retractButton;
    [SerializeField] private Button createCloneButton;
    
    private CloneManager cloneManager;
    
    private void Start()
    {
        cloneManager = FindFirstObjectByType<CloneManager>();
        
        // Set up button events
        if (retractButton != null)
            retractButton.onClick.AddListener(() => cloneManager?.RetractToLastClone());
        
        if (createCloneButton != null)
            createCloneButton.onClick.AddListener(() => cloneManager?.CreateClone());
            
        // Set initial instructions
        if (instructionsText != null)
        {
            instructionsText.text = "WASD: Move\nSpace: Jump\nAutomatic loop every 15s\nL: Manual loop";
        }
    }
    
    private void Update()
    {
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (cloneManager == null) return;
        
        // Update clone count
        if (cloneCountText != null)
        {
            cloneCountText.text = $"Clones: {cloneManager.TotalClones} | Stuck: {cloneManager.StuckClones}";
        }
        
        // Update loop timer
        if (loopTimerText != null)
        {
            if (cloneManager.IsLoopActive)
            {
                float timeLeft = cloneManager.TimeUntilNextLoop;
                loopTimerText.text = $"Next Loop: {timeLeft:F1}s";
            }
            else
            {
                loopTimerText.text = "Loop Inactive";
            }
        }
        
        // Update status
        if (statusText != null)
        {
            Clone activeClone = cloneManager.GetActiveClone();
            if (activeClone != null)
            {
                statusText.text = $"Active Clone: {cloneManager.ActiveCloneIndex + 1}";
            }
            else
            {
                statusText.text = "Player Active";
            }
        }
        
        // Update button states
        if (retractButton != null)
        {
            bool canRetract = cloneManager.TotalClones > 1;
            if (cloneManager.ActiveCloneIndex > 0)
            {
                var allClones = cloneManager.GetAllClones();
                if (cloneManager.ActiveCloneIndex - 1 < allClones.Count)
                {
                    var previousClone = allClones[cloneManager.ActiveCloneIndex - 1];
                    canRetract = canRetract && !previousClone.IsStuck;
                }
            }
            retractButton.interactable = canRetract;
        }
        
        if (createCloneButton != null)
        {
            createCloneButton.interactable = cloneManager.TotalClones < 10; // Max clones
        }
    }
}