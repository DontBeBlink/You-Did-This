using System.Collections.Generic;
using UnityEngine;

public class ActionPlayer : MonoBehaviour
{
    [SerializeField] private List<PlayerAction> actionsToPlay = new List<PlayerAction>();
    [SerializeField] private bool isPlaying = false;
    [SerializeField] private bool shouldLoop = true;
    
    private float playbackStartTime;
    private int currentActionIndex = 0;
    private PlayerController playerController;
    
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }
    
    public void SetActions(List<PlayerAction> actions)
    {
        actionsToPlay = new List<PlayerAction>(actions);
        currentActionIndex = 0;
    }
    
    public void StartPlayback()
    {
        if (actionsToPlay.Count == 0) return;
        
        isPlaying = true;
        playbackStartTime = Time.time;
        currentActionIndex = 0;
        Debug.Log($"Started playback of {actionsToPlay.Count} actions");
    }
    
    public void StopPlayback()
    {
        isPlaying = false;
        Debug.Log("Stopped playback");
    }
    
    private void Update()
    {
        if (!isPlaying || actionsToPlay.Count == 0 || !playerController) return;
        
        float currentTime = Time.time - playbackStartTime;
        
        // Process all actions that should have happened by now
        while (currentActionIndex < actionsToPlay.Count)
        {
            PlayerAction action = actionsToPlay[currentActionIndex];
            
            if (action.timestamp <= currentTime)
            {
                ExecuteAction(action);
                currentActionIndex++;
            }
            else
            {
                break;
            }
        }
        
        // Check if we've finished all actions
        if (currentActionIndex >= actionsToPlay.Count)
        {
            if (shouldLoop)
            {
                // Restart from beginning
                playbackStartTime = Time.time;
                currentActionIndex = 0;
            }
            else
            {
                StopPlayback();
            }
        }
    }
    
    private void ExecuteAction(PlayerAction action)
    {
        switch (action.actionType)
        {
            case PlayerAction.ActionType.Move:
                playerController.SetMoveInput(action.moveDirection);
                break;
            case PlayerAction.ActionType.Jump:
                if (action.isPressed)
                    playerController.Jump();
                break;
        }
    }
    
    public bool IsPlaying => isPlaying;
    public bool ShouldLoop { get => shouldLoop; set => shouldLoop = value; }
}