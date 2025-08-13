using UnityEngine;

namespace GMTK2025.ActionRecording
{
    /// <summary>
    /// Adapter class that implements ICharacterController for the original GMTK2025 CharacterController2D.
    /// This allows the action recording system to work with the existing character controller
    /// without modifying the original code.
    /// 
    /// Usage: Add this component to any GameObject that has CharacterController2D.
    /// The action recording system will automatically detect and use this adapter.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    public class CharacterController2DAdapter : MonoBehaviour, ICharacterController
    {
        private CharacterController2D characterController;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            characterController = GetComponent<CharacterController2D>();
            
            // Find sprite renderer - check child "Sprite" first, then self
            var spriteChild = transform.Find("Sprite");
            if (spriteChild != null)
            {
                spriteRenderer = spriteChild.GetComponent<SpriteRenderer>();
            }
            else
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        #region ICharacterController Implementation

        public Vector3 Position => transform.position;

        public Vector2 Velocity 
        { 
            get 
            {
                // Use reflection to access private speed field
                var speedField = characterController.GetType().GetField("speed", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (speedField != null)
                {
                    return (Vector2)speedField.GetValue(characterController);
                }
                return Vector2.zero;
            }
        }

        public bool IsGrounded => characterController.Grounded;

        public bool IsOnWall => characterController.OnWall;

        public bool FacingRight => characterController.FacingRight;

        public SpriteRenderer SpriteRenderer => spriteRenderer;

        public void ApplyMovement(float movement)
        {
            // Set the movement input for the character controller
            // This mimics what PlayerController does
            characterController.Move(movement);
        }

        public void Jump()
        {
            characterController.Jump();
        }

        public void EndJump()
        {
            characterController.EndJump();
        }

        public void Dash(Vector2 direction)
        {
            characterController.Dash(direction);
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void ApplyExternalForce(Vector2 force)
        {
            // Use reflection to access and modify the private externalForce field
            var externalForceField = characterController.GetType().GetField("externalForce", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (externalForceField != null)
            {
                Vector2 currentForce = (Vector2)externalForceField.GetValue(characterController);
                externalForceField.SetValue(characterController, currentForce + force);
            }
        }

        #endregion

        /// <summary>
        /// Get access to the underlying CharacterController2D for advanced operations.
        /// </summary>
        public CharacterController2D UnderlyingController => characterController;
    }
}