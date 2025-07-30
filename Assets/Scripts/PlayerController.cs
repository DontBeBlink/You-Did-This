using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    
    [Header("Clone Settings")]
    [SerializeField] private bool isPlayerControlled = true;
    [SerializeField] private bool isStuck = false;
    
    private Rigidbody2D rb;
    private ActionRecorder actionRecorder;
    private ActionPlayer actionPlayer;
    private CloneManager cloneManager;
    
    private Vector2 moveInput;
    private bool isGrounded;
    private bool jumpPressed;
    
    // Input actions
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction nextAction;
    private InputAction previousAction;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        actionRecorder = GetComponent<ActionRecorder>();
        actionPlayer = GetComponent<ActionPlayer>();
        cloneManager = FindObjectOfType<CloneManager>();
        
        // Create ground check if it doesn't exist
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }
    
    private void OnEnable()
    {
        if (isPlayerControlled)
        {
            EnableInput();
        }
    }
    
    private void OnDisable()
    {
        DisableInput();
    }
    
    private void EnableInput()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput == null) return;
        
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        nextAction = playerInput.actions["Next"];
        previousAction = playerInput.actions["Previous"];
        
        jumpAction.performed += OnJumpPerformed;
        nextAction.performed += OnNextPerformed;
        previousAction.performed += OnPreviousPerformed;
    }
    
    private void DisableInput()
    {
        if (jumpAction != null) jumpAction.performed -= OnJumpPerformed;
        if (nextAction != null) nextAction.performed -= OnNextPerformed;
        if (previousAction != null) previousAction.performed -= OnPreviousPerformed;
    }
    
    private void Update()
    {
        if (isStuck) return;
        
        CheckGrounded();
        
        if (isPlayerControlled)
        {
            HandleInput();
        }
        
        HandleMovement();
    }
    
    private void HandleInput()
    {
        if (moveAction != null)
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            SetMoveInput(input);
            
            // Record movement if recording
            if (actionRecorder && actionRecorder.IsRecording && input != Vector2.zero)
            {
                actionRecorder.RecordAction(PlayerAction.ActionType.Move, input);
            }
        }
    }
    
    private void HandleMovement()
    {
        // Horizontal movement
        rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
        
        // Jump
        if (jumpPressed && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpPressed = false;
        }
    }
    
    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }
    
    public void Jump()
    {
        jumpPressed = true;
    }
    
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        Jump();
        
        // Play jump sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayJumpSound();
        }
        
        // Record jump if recording
        if (actionRecorder && actionRecorder.IsRecording)
        {
            actionRecorder.RecordAction(PlayerAction.ActionType.Jump, Vector2.zero, true);
        }
    }
    
    private void OnNextPerformed(InputAction.CallbackContext context)
    {
        if (cloneManager)
        {
            cloneManager.CreateClone();
        }
    }
    
    private void OnPreviousPerformed(InputAction.CallbackContext context)
    {
        if (cloneManager)
        {
            cloneManager.RetractToLastClone();
        }
    }
    
    public void SetPlayerControlled(bool controlled)
    {
        isPlayerControlled = controlled;
        if (controlled)
        {
            EnableInput();
            if (actionRecorder) actionRecorder.StartRecording();
            if (actionPlayer) actionPlayer.StopPlayback();
        }
        else
        {
            DisableInput();
            if (actionRecorder) actionRecorder.StopRecording();
        }
    }
    
    public void SetStuck(bool stuck)
    {
        isStuck = stuck;
        if (stuck)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            if (actionPlayer) actionPlayer.StopPlayback();
        }
        else
        {
            rb.isKinematic = false;
        }
    }
    
    public bool IsPlayerControlled => isPlayerControlled;
    public bool IsStuck => isStuck;
    public bool IsGrounded => isGrounded;
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}