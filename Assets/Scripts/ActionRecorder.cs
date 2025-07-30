using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlayerAction
{
    public float timestamp;
    public Vector2 movement;
    public bool isJumping;
    public bool isDashing;
    public Vector2 dashDirection;
    public bool isInteracting;
    public bool isAttacking;
    public Vector3 position;
    
    public PlayerAction(float time, Vector2 move, bool jump, bool dash, Vector2 dashDir, bool interact, bool attack, Vector3 pos)
    {
        timestamp = time;
        movement = move;
        isJumping = jump;
        isDashing = dash;
        dashDirection = dashDir;
        isInteracting = interact;
        isAttacking = attack;
        position = pos;
    }
}

public class ActionRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField] private float recordingInterval = 0.05f; // Record every 50ms
    [SerializeField] private float maxRecordingTime = 30f; // Maximum recording length
    
    private List<PlayerAction> recordedActions = new List<PlayerAction>();
    private bool isRecording = false;
    private float recordingStartTime;
    private float lastRecordTime;
    
    // Current frame inputs (set by PlayerController)
    public Vector2 CurrentMovement { get; set; }
    public bool IsJumping { get; set; }
    public bool IsDashing { get; set; }
    public Vector2 DashDirection { get; set; }
    public bool IsInteracting { get; set; }
    public bool IsAttacking { get; set; }
    
    private void Update()
    {
        if (isRecording && Time.time - lastRecordTime >= recordingInterval)
        {
            RecordCurrentAction();
            lastRecordTime = Time.time;
        }
    }
    
    public void StartRecording()
    {
        isRecording = true;
        recordingStartTime = Time.time;
        lastRecordTime = Time.time;
        recordedActions.Clear();
        Debug.Log("Started recording player actions");
    }
    
    public void StopRecording()
    {
        isRecording = false;
        Debug.Log($"Stopped recording. Total actions: {recordedActions.Count}");
    }
    
    private void RecordCurrentAction()
    {
        float currentTime = Time.time - recordingStartTime;
        
        // Stop recording if we've exceeded max time
        if (currentTime > maxRecordingTime)
        {
            StopRecording();
            return;
        }
        
        PlayerAction action = new PlayerAction(
            currentTime,
            CurrentMovement,
            IsJumping,
            IsDashing,
            DashDirection,
            IsInteracting,
            IsAttacking,
            transform.position
        );
        
        recordedActions.Add(action);
    }
    
    public List<PlayerAction> GetRecordedActions()
    {
        return new List<PlayerAction>(recordedActions);
    }
    
    public float GetRecordingDuration()
    {
        return recordedActions.Count > 0 ? recordedActions[recordedActions.Count - 1].timestamp : 0f;
    }
    
    public bool IsRecording => isRecording;
    public int ActionCount => recordedActions.Count;
    
    // Reset input flags each frame (called by PlayerController)
    public void ResetFrameInputs()
    {
        IsJumping = false;
        IsDashing = false;
        IsInteracting = false;
        IsAttacking = false;
        DashDirection = Vector2.zero;
    }
}