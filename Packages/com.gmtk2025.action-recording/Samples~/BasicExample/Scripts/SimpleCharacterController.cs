using UnityEngine;

namespace GMTK2025.ActionRecording.Examples
{
    /// <summary>
    /// Simple 2D character controller that implements ICharacterController interface.
    /// This is a basic example showing how to create a character controller that works
    /// with the Action Recording System.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SimpleCharacterController : MonoBehaviour, ICharacterController
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float dashForce = 15f;
        [SerializeField] private float dashCooldown = 1f;

        [Header("Ground Detection")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer = 1;
        [SerializeField] private float groundCheckRadius = 0.1f;

        [Header("Wall Detection")]
        [SerializeField] private Transform wallCheck;
        [SerializeField] private float wallCheckDistance = 0.5f;

        // Components
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;

        // State tracking
        private bool facingRight = true;
        private float lastDashTime = -10f;
        private Vector2 externalForce;

        #region Unity Lifecycle

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Set up ground check if not assigned
            if (groundCheck == null)
            {
                GameObject groundCheckObj = new GameObject("GroundCheck");
                groundCheckObj.transform.SetParent(transform);
                groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
                groundCheck = groundCheckObj.transform;
            }

            // Set up wall check if not assigned
            if (wallCheck == null)
            {
                GameObject wallCheckObj = new GameObject("WallCheck");
                wallCheckObj.transform.SetParent(transform);
                wallCheckObj.transform.localPosition = new Vector3(0.3f, 0, 0);
                wallCheck = wallCheckObj.transform;
            }
        }

        private void FixedUpdate()
        {
            // Apply external forces
            if (externalForce != Vector2.zero)
            {
                rb.AddForce(externalForce, ForceMode2D.Force);
                externalForce = Vector2.Lerp(externalForce, Vector2.zero, Time.fixedDeltaTime * 5f);
            }
        }

        #endregion

        #region ICharacterController Implementation

        public Vector3 Position => transform.position;

        public Vector2 Velocity => rb.velocity;

        public bool IsGrounded => Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        public bool IsOnWall
        {
            get
            {
                Vector2 direction = facingRight ? Vector2.right : Vector2.left;
                return Physics2D.Raycast(wallCheck.position, direction, wallCheckDistance, groundLayer);
            }
        }

        public bool FacingRight => facingRight;

        public SpriteRenderer SpriteRenderer => spriteRenderer;

        public void ApplyMovement(float movement)
        {
            // Apply horizontal movement
            Vector2 velocity = rb.velocity;
            velocity.x = movement * moveSpeed;
            rb.velocity = velocity;

            // Update facing direction
            if (movement > 0 && !facingRight)
            {
                Flip();
            }
            else if (movement < 0 && facingRight)
            {
                Flip();
            }
        }

        public void Jump()
        {
            if (IsGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        public void EndJump()
        {
            // Variable height jumping - reduce upward velocity when jump is released
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }
        }

        public void Dash(Vector2 direction)
        {
            // Check dash cooldown
            if (Time.time - lastDashTime < dashCooldown)
                return;

            lastDashTime = Time.time;

            // Apply dash force
            if (direction == Vector2.zero)
            {
                direction = facingRight ? Vector2.right : Vector2.left;
            }

            rb.velocity = direction.normalized * dashForce;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void ApplyExternalForce(Vector2 force)
        {
            externalForce += force;
        }

        #endregion

        #region Helper Methods

        private void Flip()
        {
            facingRight = !facingRight;
            transform.Rotate(0, 180, 0);
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = IsGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }

            if (wallCheck != null)
            {
                Gizmos.color = IsOnWall ? Color.green : Color.red;
                Vector3 direction = facingRight ? Vector3.right : Vector3.left;
                Gizmos.DrawRay(wallCheck.position, direction * wallCheckDistance);
            }
        }

        #endregion
    }
}