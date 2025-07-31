using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class Clone : MonoBehaviour
{
    [Header("Clone Settings")]
    [SerializeField] private Material cloneMaterial;
    [SerializeField] private Color cloneColor = Color.cyan;
    [SerializeField] private float cloneAlpha = 0.4f;
    
    private List<PlayerAction> actionsToReplay;
    private CharacterController2D character;
    private InteractSystem interact;
    private SpriteRenderer spriteRenderer;
    
    private bool isReplaying = false;
    private bool isStuck = false;
    private float replayStartTime;
    private int currentActionIndex = 0;
    private float replayDuration;
    
    // Clone identification
    private int cloneIndex;
    private Goal stuckAtGoal;
    private bool wasJumpHeld = false; // Track previous jump held state for EndJump
    
    private void Awake()
    {
        character = GetComponent<CharacterController2D>();
        interact = GetComponent<InteractSystem>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        SetupCloneVisuals();
    }
    
    private void SetupCloneVisuals()
    {
        if (spriteRenderer != null)
        {
            // Create a copy of the material to avoid affecting the original
            if (cloneMaterial != null)
            {
                spriteRenderer.material = cloneMaterial;
            }
            
            Color color = cloneColor;
            color.a = cloneAlpha;
            spriteRenderer.color = color;
        }
        
        // Optionally add a subtle outline or glow effect
        gameObject.name = $"Clone_{cloneIndex}";
    }
    
    public void InitializeClone(List<PlayerAction> actions, int index)
    {
        if (actions == null || actions.Count == 0)
        {
            Debug.LogWarning($"Clone {index} initialized with no actions to replay");
            actionsToReplay = new List<PlayerAction>();
        }
        else
        {
            actionsToReplay = new List<PlayerAction>(actions);
        }
        
        cloneIndex = index;
        replayDuration = actionsToReplay.Count > 0 ? actionsToReplay[actionsToReplay.Count - 1].timestamp : 0f;
        
        // Disable player input components
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        ActionRecorder recorder = GetComponent<ActionRecorder>();
        if (recorder != null)
        {
            recorder.enabled = false;
        }
        
        SetupCloneVisuals();
        Debug.Log($"Clone {index} initialized with {actionsToReplay.Count} actions, duration: {replayDuration:F1}s");
    }
    
    public void StartReplay()
    {
        if (actionsToReplay == null || actionsToReplay.Count == 0)
        {
            Debug.LogWarning("No actions to replay for clone");
            return;
        }
        
        isReplaying = true;
        replayStartTime = Time.time;
        currentActionIndex = 0;
        wasJumpHeld = false; // Reset jump held state
        Debug.Log($"Clone {cloneIndex} started replaying actions");
    }
    
    private void Update()
    {
        if (isReplaying && !isStuck)
        {
            UpdateReplay();
        }
    }
    
    private void UpdateReplay()
    {
        if (actionsToReplay == null || currentActionIndex >= actionsToReplay.Count)
        {
            // Loop back to the beginning
            if (replayDuration > 0)
            {
                replayStartTime = Time.time;
                currentActionIndex = 0;
                wasJumpHeld = false; // Reset jump held state for loop
                Debug.Log($"Clone {cloneIndex} looping replay");
                this.transform.position = CloneManager.instance.transform.position; // Reset position if needed
            }
            return;
        }
        
        float currentReplayTime = Time.time - replayStartTime;
        
        // Find the current action to execute
        while (currentActionIndex < actionsToReplay.Count && 
               actionsToReplay[currentActionIndex].timestamp <= currentReplayTime)
        {
            ExecuteAction(actionsToReplay[currentActionIndex]);
            currentActionIndex++;
        }
    }
    
    private void ExecuteAction(PlayerAction action)
    {
        if (isStuck) return;
        
        // Debug output for action execution
        bool actionExecuted = false;
        string actionDebug = $"Clone {cloneIndex} executing: ";
        
        // Execute movement
        if (character != null)
        {
            if (action.movement.x != 0 || action.movement.y != 0)
            {
                character.Walk(action.movement.x);
                character.ClimbLadder(action.movement.y);
                actionDebug += $"move({action.movement.x:F2},{action.movement.y:F2}) ";
                actionExecuted = true;
            }
            
            if (action.isJumping)
            {
                if (action.movement.y < 0)
                {
                    character.JumpDown();
                    actionDebug += "jumpDown ";
                }
                else
                {
                    character.Jump();
                    actionDebug += "jump ";
                }
                actionExecuted = true;
            }
            
            // Handle jump hold duration - EndJump when transitioning from held to not held
            if (wasJumpHeld && !action.jumpHeld)
            {
                character.EndJump();
                actionDebug += "endJump ";
                actionExecuted = true;
            }
            wasJumpHeld = action.jumpHeld;
            
            if (action.isDashing)
            {
                character.Dash(action.dashDirection);
                actionDebug += $"dash({action.dashDirection.x:F2},{action.dashDirection.y:F2}) ";
                actionExecuted = true;
            }
        }
        
        // Execute interactions
        if (interact != null)
        {
            if (action.isInteracting)
            {
                interact.Interact();
                actionDebug += "interact ";
                actionExecuted = true;
            }
            
            if (action.isAttacking && interact.PickedUpObject)
            {
                interact.Throw();
                actionDebug += "attack ";
                actionExecuted = true;
            }
        }
        
        // Debug output only if an action was executed
        if (actionExecuted)
        {
            actionDebug += $"at time {action.timestamp:F2}s jumpHeld:{action.jumpHeld}";
            Debug.Log(actionDebug);
        }
    }
    
    public void SetStuck(Goal goal)
    {
        isStuck = true;
        stuckAtGoal = goal;
        isReplaying = false;
        
        // Stop the character from moving
        if (character != null)
        {
            character.Immobile = true;
        }
        
        // Change visual appearance
        if (spriteRenderer != null)
        {
            Color stuckColor = Color.green;
            stuckColor.a = cloneAlpha;
            spriteRenderer.color = stuckColor;
        }
        
        Debug.Log($"Clone {cloneIndex} is now stuck at goal");
    }
    
    public void StopReplay()
    {
        isReplaying = false;
        Debug.Log($"Clone {cloneIndex} stopped replaying");
    }
    
    // Properties
    public bool IsReplaying => isReplaying;
    public bool IsStuck => isStuck;
    public int CloneIndex => cloneIndex;
    public Goal StuckAtGoal => stuckAtGoal;
    public float ReplayProgress => replayDuration > 0 ? (Time.time - replayStartTime) / replayDuration : 0f;
    
    private void OnDestroy()
    {
        Debug.Log($"Clone {cloneIndex} destroyed");
    }
}