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
    public bool jumpHeld;

    public PlayerAction(float time, Vector2 move, bool jump, bool dash, Vector2 dashDir, bool interact, bool attack, Vector3 pos, bool jumpHeld)
    {
        timestamp = time;
        movement = move;
        isJumping = jump;
        isDashing = dash;
        dashDirection = dashDir;
        isInteracting = interact;
        isAttacking = attack;
        position = pos;
        this.jumpHeld = jumpHeld;
    }
}

public class ActionRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField] private float recordingInterval = 0.02f; // 50 FPS, matches FixedUpdate
    [SerializeField] private float maxRecordingTime = 30f;

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
    public bool JumpHeld { get; set; }

    public void StartRecording()
    {
        if (isRecording)
        {
            Debug.LogWarning("Already recording! Stopping previous recording.");
            StopRecording();
        }

        isRecording = true;
        recordingStartTime = Time.time;
        lastRecordTime = Time.time;
        recordedActions.Clear();
        Debug.Log($"Started recording player actions at time {recordingStartTime:F2}");
    }

    public void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            float duration = Time.time - recordingStartTime;
            Debug.Log($"Stopped recording. Total actions: {recordedActions.Count}, Duration: {duration:F2}s");
        }
    }

    // This is only used if you want to record from PlayerController (not recommended for physics-perfect replay)
    private void RecordCurrentAction()
    {
        float currentTime = Time.time - recordingStartTime;

        // Stop recording if we've exceeded max time
        if (currentTime > maxRecordingTime)
        {
            isRecording = false;
            Debug.LogWarning("Max recording time reached, stopping recording.");
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
            transform.position,
            JumpHeld // Use the jumpHeld state from PlayerController
        );

        recordedActions.Add(action);

        // Reset input flags AFTER recording
        ResetFrameInputs();
    }

    // Call this from CharacterController2D.FixedUpdate for physics-aligned recording
    public void RecordFromCharacter(CharacterController2D character)
    {
        if (!isRecording) return;

        float currentTime = Time.time - recordingStartTime;

        if (currentTime > maxRecordingTime)
        {
            isRecording = false;
            Debug.LogWarning("Max recording time reached, stopping recording.");
            return;
        }

        // Check if enough time has passed since last recording
        if (Time.time - lastRecordTime < recordingInterval)
        {
            return;
        }
        lastRecordTime = Time.time;

        PlayerAction action = new PlayerAction(
            currentTime,
            CurrentMovement,                // Use input movement from PlayerController
            character.JustJumped,
            character.JustDashed,
            character.LastDashDirection,
            character.JustInteracted,
            character.JustAttacked,
            character.transform.position,
            JumpHeld // Pass the jumpHeld state from PlayerController
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