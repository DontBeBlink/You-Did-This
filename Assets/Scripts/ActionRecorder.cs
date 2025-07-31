using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data structure representing a single frame of player input and state.
/// Used by the clone system to create physics-perfect replays of player actions.
/// 
/// This struct captures all necessary information to recreate player behavior:
/// - Input states (movement, jumping, dashing, interactions)
/// - Physical states (position, velocity, external forces)
/// - Timing information for precise synchronization
/// </summary>
[System.Serializable]
public struct PlayerAction
{
    public float timestamp;        // Time offset from recording start
    public float movement;         // Horizontal movement input (-1 to 1)
    public bool isJumping;         // Whether jump was initiated this frame
    public bool isDashing;         // Whether dash was initiated this frame
    public Vector2 dashDirection;  // Direction of dash input
    public bool isInteracting;     // Whether interact was pressed this frame
    public bool isAttacking;       // Whether attack was pressed this frame
    public Vector3 position;       // World position at this frame
    public bool jumpHeld;          // Whether jump button is currently held
    public Vector2 speed;          // Current velocity vector
    public Vector2 externalForce;  // External forces applied (wind, pushers, etc.)

    /// <summary>
    /// Create a new PlayerAction with all necessary state information.
    /// </summary>
    public PlayerAction(float time, float move, bool jump, bool dash, Vector2 dashDir, bool interact, bool attack, Vector3 pos, bool jumpHeld, Vector2 speed, Vector2 externalForce)
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
        this.speed = speed;
        this.externalForce = externalForce;
    }
}

/// <summary>
/// Records player actions and input states for physics-perfect replay by clones.
/// This component is essential for the clone system, capturing all player inputs
/// and physical states at precise intervals for accurate reproduction.
/// 
/// Key Features:
/// - Records at 50 FPS (0.02s intervals) to match Unity's FixedUpdate timing
/// - Captures input states, position, velocity, and external forces
/// - Interfaces with both PlayerController (inputs) and CharacterController2D (physics)
/// - Provides physics-perfect replay data for clone creation
/// - Automatic recording duration limits to prevent memory issues
/// 
/// Usage: Automatically added to PlayerController by CloneManager.
/// The PlayerController sets input states, and CharacterController2D calls
/// RecordFromCharacter() during FixedUpdate for frame-perfect physics recording.
/// </summary>
public class ActionRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField] private float recordingInterval = 0.02f; // 50 FPS recording rate, matches FixedUpdate
    [SerializeField] private float maxRecordingTime = 30f; // Maximum recording duration to prevent memory issues

    // Storage for recorded action sequence
    private List<PlayerAction> recordedActions = new List<PlayerAction>();
    private bool isRecording = false;
    private float recordingStartTime; // Time.time when recording started
    private float lastRecordTime;    // Time.time of last recorded frame

    // Current frame input states - set by PlayerController each frame
    public float CurrentMovement { get; set; }     // Horizontal movement input (-1 to 1)
    public bool IsJumping { get; set; }            // Jump button pressed this frame
    public bool IsDashing { get; set; }            // Dash button pressed this frame
    public Vector2 DashDirection { get; set; }     // Direction for dash input
    public bool IsInteracting { get; set; }        // Interact button pressed this frame
    public bool IsAttacking { get; set; }          // Attack button pressed this frame
    public bool JumpHeld { get; set; }             // Whether jump button is currently held

    /// <summary>
    /// Begin recording player actions and clear any previously recorded data.
    /// Sets up timing references and prepares for action capture.
    /// Called by CloneManager when starting a new loop.
    /// </summary>
    public void StartRecording()
    {
        // Stop any existing recording to avoid conflicts
        if (isRecording)
        {
            Debug.LogWarning("Already recording! Stopping previous recording.");
            StopRecording();
        }

        // Initialize recording state
        isRecording = true;
        recordingStartTime = Time.time;
        lastRecordTime = Time.time;
        recordedActions.Clear(); // Clear previous recording data
       // Debug.Log($"Started recording player actions at time {recordingStartTime:F2}");
    }

    /// <summary>
    /// Stop the current recording session and finalize the recorded action sequence.
    /// The recorded actions are now ready to be used for clone creation.
    /// Called by CloneManager when ending a loop.
    /// </summary>
    public void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            float duration = Time.time - recordingStartTime;
            //Debug.Log($"Stopped recording. Total actions: {recordedActions.Count}, Duration: {duration:F2}s");
        }
    }

    /// <summary>
    /// Record player action using input data only (not physics-perfect).
    /// This method is kept for compatibility but is not recommended for physics-perfect replay.
    /// Use RecordFromCharacter() called from CharacterController2D.FixedUpdate for better results.
    /// </summary>
    private void RecordCurrentAction()
    {
        float currentTime = Time.time - recordingStartTime;

        // Enforce maximum recording time to prevent memory issues
        if (currentTime > maxRecordingTime)
        {
            isRecording = false;
            Debug.LogWarning("Max recording time reached, stopping recording.");
            return;
        }

        // Create action record with input data (no physics data available in this method)
        PlayerAction action = new PlayerAction(
            currentTime,
            CurrentMovement,
            IsJumping,
            IsDashing,
            DashDirection,
            IsInteracting,
            IsAttacking,
            transform.position,
            JumpHeld, // Use the jumpHeld state from PlayerController
            Vector2.zero, // No physics velocity data available
            Vector2.zero  // No external force data available
        );

        recordedActions.Add(action);

        // Reset frame-specific input flags after recording
        ResetFrameInputs();
    }

    /// <summary>
    /// Record a physics-perfect frame of player action data.
    /// This is the preferred recording method as it captures both input states
    /// and physics states (velocity, forces) for accurate clone replay.
    /// 
    /// Called from CharacterController2D.FixedUpdate to ensure physics alignment.
    /// Uses reflection to access private physics fields for complete state capture.
    /// </summary>
    /// <param name="character">The CharacterController2D to record physics data from</param>
    public void RecordFromCharacter(CharacterController2D character)
    {
        if (!isRecording) return;

        float currentTime = Time.time - recordingStartTime;

        // Enforce maximum recording time to prevent memory issues
        if (currentTime > maxRecordingTime)
        {
            isRecording = false;
            Debug.LogWarning("Max recording time reached, stopping recording.");
            return;
        }

        lastRecordTime = Time.time;
        
        // Record complete action state including physics data
        // Uses reflection to access private speed and externalForce fields from CharacterController2D
        PlayerAction action = new PlayerAction(
            currentTime,
            CurrentMovement,                          // Input: horizontal movement
            character.JustJumped,                     // Physics: jump initiated this frame
            character.JustDashed,                     // Physics: dash initiated this frame
            character.LastDashDirection,              // Physics: direction of last dash
            character.JustInteracted,                 // Physics: interaction this frame
            character.JustAttacked,                   // Physics: attack this frame
            character.transform.position,             // Physics: world position
            JumpHeld,                                 // Input: jump button currently held
            // Reflection access to private physics fields for complete state capture
            character.GetType().GetField("speed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null ? (Vector2)character.GetType().GetField("speed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(character) : Vector2.zero,
            character.GetType().GetField("externalForce", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null ? (Vector2)character.GetType().GetField("externalForce", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(character) : Vector2.zero
        );
        recordedActions.Add(action);
    }

    /// <summary>
    /// Get a copy of all recorded actions for clone creation.
    /// Returns a new list to prevent external modification of the original data.
    /// Used by CloneManager when creating clones.
    /// </summary>
    /// <returns>Copy of the complete recorded action sequence</returns>
    public List<PlayerAction> GetRecordedActions()
    {
        return new List<PlayerAction>(recordedActions);
    }

    /// <summary>
    /// Get the total duration of the recorded action sequence.
    /// Useful for determining clone replay length.
    /// </summary>
    /// <returns>Duration in seconds, or 0 if no actions recorded</returns>
    public float GetRecordingDuration()
    {
        return recordedActions.Count > 0 ? recordedActions[recordedActions.Count - 1].timestamp : 0f;
    }

    /// <summary>
    /// Whether the recorder is currently active and recording actions.
    /// </summary>
    public bool IsRecording => isRecording;
    
    /// <summary>
    /// Total number of actions recorded in the current session.
    /// </summary>
    public int ActionCount => recordedActions.Count;

    /// <summary>
    /// Reset all frame-specific input flags to their default state.
    /// Called by PlayerController after inputs are processed each frame to ensure
    /// single-frame actions (like jump presses) are only recorded once.
    /// </summary>
    public void ResetFrameInputs()
    {
        IsJumping = false;
        IsDashing = false;
        IsInteracting = false;
        IsAttacking = false;
        DashDirection = Vector2.zero;
    }
}