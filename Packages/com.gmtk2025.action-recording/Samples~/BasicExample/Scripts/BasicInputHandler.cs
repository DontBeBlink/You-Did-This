using UnityEngine;

namespace GMTK2025.ActionRecording.Examples
{
    /// <summary>
    /// Basic input handler for the Simple Character Controller example.
    /// Handles keyboard input and communicates with the Action Recording System.
    /// </summary>
    [RequireComponent(typeof(SimpleCharacterController))]
    [RequireComponent(typeof(ActionRecorder))]
    public class BasicInputHandler : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode manualLoopKey = KeyCode.F;

        // Component references
        private SimpleCharacterController characterController;
        private ActionRecorder actionRecorder;

        // Input state tracking
        private float horizontalInput;
        private bool jumpPressed;
        private bool jumpHeld;
        private bool dashPressed;
        private bool wasJumpHeld;

        private void Awake()
        {
            characterController = GetComponent<SimpleCharacterController>();
            actionRecorder = GetComponent<ActionRecorder>();
        }

        private void Update()
        {
            HandleInput();
            UpdateActionRecorder();
            ApplyInputToCharacter();
        }

        private void HandleInput()
        {
            // Get horizontal movement input
            horizontalInput = Input.GetAxis("Horizontal");

            // Handle jump input
            jumpPressed = Input.GetKeyDown(jumpKey);
            jumpHeld = Input.GetKey(jumpKey);

            // Handle dash input
            dashPressed = Input.GetKeyDown(dashKey);

            // Handle manual loop input
            if (Input.GetKeyDown(manualLoopKey))
            {
                if (ActionRecordingManager.Instance != null)
                {
                    ActionRecordingManager.Instance.TriggerManualLoop();
                }
            }
        }

        private void UpdateActionRecorder()
        {
            if (actionRecorder != null)
            {
                // Set input states for recording
                actionRecorder.CurrentMovement = horizontalInput;
                actionRecorder.IsJumping = jumpPressed;
                actionRecorder.JumpHeld = jumpHeld;
                actionRecorder.IsDashing = dashPressed;

                // Calculate dash direction (simplified - just horizontal)
                if (dashPressed)
                {
                    Vector2 dashDirection = Vector2.zero;
                    if (horizontalInput != 0)
                    {
                        dashDirection = new Vector2(Mathf.Sign(horizontalInput), 0);
                    }
                    else
                    {
                        dashDirection = characterController.FacingRight ? Vector2.right : Vector2.left;
                    }
                    actionRecorder.DashDirection = dashDirection;
                }

                // Record current frame if recording is active
                actionRecorder.RecordCurrentFrame();
            }
        }

        private void ApplyInputToCharacter()
        {
            // Apply movement
            characterController.ApplyMovement(horizontalInput);

            // Apply jump
            if (jumpPressed)
            {
                characterController.Jump();
            }

            // Handle variable height jumping
            if (wasJumpHeld && !jumpHeld)
            {
                characterController.EndJump();
            }
            wasJumpHeld = jumpHeld;

            // Apply dash
            if (dashPressed)
            {
                Vector2 dashDirection = Vector2.zero;
                if (horizontalInput != 0)
                {
                    dashDirection = new Vector2(Mathf.Sign(horizontalInput), 0);
                }
                characterController.Dash(dashDirection);
            }
        }

        private void OnGUI()
        {
            // Simple UI for demonstration
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Action Recording System Example", EditorGUIUtility.isProSkin ? GUI.skin.box : GUI.skin.label);
            GUILayout.Space(10);

            GUILayout.Label("Controls:");
            GUILayout.Label($"Movement: WASD or Arrow Keys");
            GUILayout.Label($"Jump: {jumpKey}");
            GUILayout.Label($"Dash: {dashKey}");
            GUILayout.Label($"Manual Loop: {manualLoopKey}");
            GUILayout.Space(10);

            if (ActionRecordingManager.Instance != null)
            {
                var manager = ActionRecordingManager.Instance;
                GUILayout.Label($"Recording: {(manager.IsRecording ? "YES" : "NO")}");
                GUILayout.Label($"Clones: {manager.CloneCount}");
                
                if (manager.IsRecording && manager.LoopDuration > 0)
                {
                    float remaining = manager.RemainingLoopTime;
                    GUILayout.Label($"Loop Time: {remaining:F1}s");
                }
            }

            GUILayout.EndArea();
        }
    }
}