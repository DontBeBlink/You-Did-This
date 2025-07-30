using UnityEngine;

public class Clone : MonoBehaviour
{
    [Header("Clone Settings")]
    [SerializeField] private bool isStuck = false;
    [SerializeField] private CloneState state = CloneState.Active;
    
    public enum CloneState
    {
        Active,     // Currently being controlled by player
        Replaying,  // Playing back recorded actions
        Stuck       // Reached a goal and cannot be controlled
    }
    
    private PlayerController playerController;
    private ActionRecorder actionRecorder;
    private ActionPlayer actionPlayer;
    private SpriteRenderer spriteRenderer;
    
    [Header("Visual Settings")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color replayingColor = Color.cyan;
    [SerializeField] private Color stuckColor = Color.red;
    
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        actionRecorder = GetComponent<ActionRecorder>();
        actionPlayer = GetComponent<ActionPlayer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        UpdateVisuals();
    }
    
    public void SetState(CloneState newState)
    {
        state = newState;
        
        switch (state)
        {
            case CloneState.Active:
                playerController.SetPlayerControlled(true);
                playerController.SetStuck(false);
                if (actionRecorder) actionRecorder.StartRecording();
                break;
                
            case CloneState.Replaying:
                playerController.SetPlayerControlled(false);
                playerController.SetStuck(false);
                if (actionPlayer && actionRecorder)
                {
                    actionPlayer.SetActions(actionRecorder.GetRecordedActions());
                    actionPlayer.StartPlayback();
                }
                break;
                
            case CloneState.Stuck:
                playerController.SetPlayerControlled(false);
                playerController.SetStuck(true);
                if (actionPlayer) actionPlayer.StopPlayback();
                if (actionRecorder) actionRecorder.StopRecording();
                isStuck = true;
                break;
        }
        
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        if (spriteRenderer == null) return;
        
        switch (state)
        {
            case CloneState.Active:
                spriteRenderer.color = activeColor;
                break;
            case CloneState.Replaying:
                spriteRenderer.color = replayingColor;
                break;
            case CloneState.Stuck:
                spriteRenderer.color = stuckColor;
                break;
        }
    }
    
    public CloneState State => state;
    public bool IsStuck => isStuck;
    public ActionRecorder ActionRecorder => actionRecorder;
    public PlayerController PlayerController => playerController;
}