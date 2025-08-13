using UnityEngine;
using GMTK2025.InteractionSystem;

namespace GMTK2025.InteractionSystem.Examples
{
    /// <summary>
    /// Example character controller for the interaction system samples.
    /// Provides basic movement and interaction input handling.
    /// </summary>
    public class ExampleCharacterController : MonoBehaviour, ICharacterVelocityProvider, ICharacterFacingProvider
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 10f;
        
        [Header("Interaction Input")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private KeyCode dropKey = KeyCode.Q;
        
        // Components
        private Rigidbody2D rb;
        private InteractionController interactionController;
        
        // State
        private bool facingRight = true;
        private bool isGrounded;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            interactionController = GetComponent<InteractionController>();
            
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.freezeRotation = true;
            }
            
            if (interactionController == null)
            {
                interactionController = gameObject.AddComponent<InteractionController>();
            }
        }
        
        private void Start()
        {
            // Set this as the velocity and facing provider for the interaction controller
            interactionController.SetVelocityProvider(this);
            interactionController.SetFacingProvider(this);
        }
        
        private void Update()
        {
            HandleMovementInput();
            HandleInteractionInput();
            UpdateFacingDirection();
        }
        
        private void HandleMovementInput()
        {
            // Horizontal movement
            float horizontal = Input.GetAxis("Horizontal");
            
            Vector2 velocity = rb.velocity;
            velocity.x = horizontal * moveSpeed;
            rb.velocity = velocity;
            
            // Jump
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
        
        private void HandleInteractionInput()
        {
            // Interact
            if (Input.GetKeyDown(interactKey))
            {
                interactionController.TryInteract();
            }
            
            // Drop carried object
            if (Input.GetKeyDown(dropKey))
            {
                interactionController.DropCarriedObject();
            }
        }
        
        private void UpdateFacingDirection()
        {
            Vector2 velocity = rb.velocity;
            
            if (Mathf.Abs(velocity.x) > 0.1f)
            {
                facingRight = velocity.x > 0;
                
                // Flip sprite if needed
                Vector3 scale = transform.localScale;
                scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }
        
        private void OnCollisionStay2D(Collision2D collision)
        {
            // Simple ground check
            if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                isGrounded = true;
            }
        }
        
        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                isGrounded = false;
            }
        }
        
        #region Interface Implementation
        
        public Vector2 GetVelocity()
        {
            return rb != null ? rb.velocity : Vector2.zero;
        }
        
        public bool IsFacingRight()
        {
            return facingRight;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Set the movement speed of the character.
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }
        
        /// <summary>
        /// Set the jump force of the character.
        /// </summary>
        public void SetJumpForce(float force)
        {
            jumpForce = force;
        }
        
        /// <summary>
        /// Manually set the facing direction.
        /// </summary>
        public void SetFacingRight(bool right)
        {
            facingRight = right;
            
            Vector3 scale = transform.localScale;
            scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        
        #endregion
    }
}