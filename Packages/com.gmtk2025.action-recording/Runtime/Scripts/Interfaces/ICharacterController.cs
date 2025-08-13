using UnityEngine;

namespace GMTK2025.ActionRecording
{
    /// <summary>
    /// Interface for character controllers that can be used with the action recording system.
    /// This abstraction allows the system to work with any character controller implementation
    /// while maintaining physics-perfect recording and replay capabilities.
    /// </summary>
    public interface ICharacterController
    {
        /// <summary>
        /// Current world position of the character
        /// </summary>
        Vector3 Position { get; }
        
        /// <summary>
        /// Current velocity of the character
        /// </summary>
        Vector2 Velocity { get; }
        
        /// <summary>
        /// Whether the character is currently grounded
        /// </summary>
        bool IsGrounded { get; }
        
        /// <summary>
        /// Whether the character is touching a wall
        /// </summary>
        bool IsOnWall { get; }
        
        /// <summary>
        /// Whether the character is currently facing right
        /// </summary>
        bool FacingRight { get; }
        
        /// <summary>
        /// Apply horizontal movement input to the character
        /// </summary>
        /// <param name="movement">Movement input (-1 to 1)</param>
        void ApplyMovement(float movement);
        
        /// <summary>
        /// Initiate a jump action
        /// </summary>
        void Jump();
        
        /// <summary>
        /// End the jump action (for variable height jumps)
        /// </summary>
        void EndJump();
        
        /// <summary>
        /// Initiate a dash action in the specified direction
        /// </summary>
        /// <param name="direction">Dash direction</param>
        void Dash(Vector2 direction);
        
        /// <summary>
        /// Set the character's position directly (for initialization and corrections)
        /// </summary>
        /// <param name="position">Target position</param>
        void SetPosition(Vector3 position);
        
        /// <summary>
        /// Apply external forces to the character (wind, pushers, etc.)
        /// </summary>
        /// <param name="force">External force to apply</param>
        void ApplyExternalForce(Vector2 force);
        
        /// <summary>
        /// Get the character's sprite renderer for visual effects
        /// </summary>
        SpriteRenderer SpriteRenderer { get; }
    }
}