using System.Collections.Generic;
using UnityEngine;

public class ActionRecorder : MonoBehaviour
{
    [SerializeField] private List<PlayerAction> recordedActions = new List<PlayerAction>();
    private float recordingStartTime;
    private bool isRecording = false;
    
    public void StartRecording()
    {
        recordedActions.Clear();
        recordingStartTime = Time.time;
        isRecording = true;
        Debug.Log("Started recording actions");
    }
    
    public void StopRecording()
    {
        isRecording = false;
        Debug.Log($"Stopped recording. Recorded {recordedActions.Count} actions");
    }
    
    public void RecordAction(PlayerAction.ActionType actionType, Vector2 direction, bool isPressed = false)
    {
        if (!isRecording) return;
        
        float relativeTime = Time.time - recordingStartTime;
        PlayerAction action = new PlayerAction(actionType, direction, relativeTime, isPressed);
        recordedActions.Add(action);
    }
    
    public List<PlayerAction> GetRecordedActions()
    {
        return new List<PlayerAction>(recordedActions);
    }
    
    public bool IsRecording => isRecording;
    public int ActionCount => recordedActions.Count;
}