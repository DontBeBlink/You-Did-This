using UnityEngine;

namespace GMTK2025.ActionRecording
{
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
        public bool isGrounded;        // Whether the character is grounded
        public bool isOnWall;         // Whether the character is on a wall
        public bool facingRight;       // Whether the character is facing right

        /// <summary>
        /// Create a new PlayerAction with all necessary state information.
        /// </summary>
        public PlayerAction(float time, float move, bool jump, bool dash, Vector2 dashDir, 
                          bool interact, bool attack, Vector3 pos, bool jumpHeld, Vector2 speed, 
                          Vector2 externalForce, bool isGrounded = false, bool isOnWall = false, 
                          bool facingRight = false)
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
            this.isGrounded = isGrounded;
            this.isOnWall = isOnWall;
            this.facingRight = facingRight;
        }

        /// <summary>
        /// Create an empty PlayerAction for initialization
        /// </summary>
        public static PlayerAction Empty => new PlayerAction(0, 0, false, false, Vector2.zero, 
                                                            false, false, Vector3.zero, false, 
                                                            Vector2.zero, Vector2.zero, false, 
                                                            false, false);
    }
}