using UnityEngine;

namespace GMTK2025.InteractionSystem
{
    /// <summary>
    /// Adapter component that allows existing CharacterController2D scripts to work
    /// with the interaction system's velocity and facing requirements.
    /// Add this component alongside your existing character controller.
    /// </summary>
    public class CharacterController2DAdapter : MonoBehaviour, ICharacterVelocityProvider, ICharacterFacingProvider
    {
        [Header("Adapter Settings")]
        [SerializeField] private bool autoDetectCharacterController = true;
        [SerializeField] private string velocityPropertyName = "rb";
        [SerializeField] private string facingRightPropertyName = "facingRight";
        
        // Component references
        private MonoBehaviour characterController;
        private System.Reflection.PropertyInfo velocityProperty;
        private System.Reflection.FieldInfo velocityField;
        private System.Reflection.PropertyInfo facingProperty;
        private System.Reflection.FieldInfo facingField;
        private Rigidbody2D fallbackRigidbody;
        
        // Cached values
        private Vector2 lastKnownVelocity;
        private bool lastKnownFacingRight = true;
        
        private void Awake()
        {
            SetupAdapter();
        }
        
        private void SetupAdapter()
        {
            if (autoDetectCharacterController)
            {
                // Try to find CharacterController2D or similar controller
                characterController = GetComponent<MonoBehaviour>();
                
                // Look for common character controller types
                var possibleControllers = GetComponents<MonoBehaviour>();
                foreach (var controller in possibleControllers)
                {
                    var typeName = controller.GetType().Name.ToLower();
                    if (typeName.Contains("character") || typeName.Contains("player") || typeName.Contains("controller"))
                    {
                        characterController = controller;
                        break;
                    }
                }
            }
            
            // Setup reflection for velocity access
            SetupVelocityAccess();
            SetupFacingAccess();
            
            // Get fallback rigidbody
            fallbackRigidbody = GetComponent<Rigidbody2D>();
        }
        
        private void SetupVelocityAccess()
        {
            if (characterController == null) return;
            
            var type = characterController.GetType();
            
            // Try to find velocity property or field
            velocityProperty = type.GetProperty(velocityPropertyName);
            if (velocityProperty == null)
            {
                velocityField = type.GetField(velocityPropertyName);
            }
            
            // If not found, try common names
            if (velocityProperty == null && velocityField == null)
            {
                var commonNames = new[] { "rb", "rigidbody", "rigidbody2D", "velocity", "currentVelocity" };
                foreach (var name in commonNames)
                {
                    velocityProperty = type.GetProperty(name);
                    if (velocityProperty != null) break;
                    
                    velocityField = type.GetField(name);
                    if (velocityField != null) break;
                }
            }
        }
        
        private void SetupFacingAccess()
        {
            if (characterController == null) return;
            
            var type = characterController.GetType();
            
            // Try to find facing property or field
            facingProperty = type.GetProperty(facingRightPropertyName);
            if (facingProperty == null)
            {
                facingField = type.GetField(facingRightPropertyName);
            }
            
            // If not found, try common names
            if (facingProperty == null && facingField == null)
            {
                var commonNames = new[] { "facingRight", "isFacingRight", "facingDirection", "direction" };
                foreach (var name in commonNames)
                {
                    facingProperty = type.GetProperty(name);
                    if (facingProperty != null) break;
                    
                    facingField = type.GetField(name);
                    if (facingField != null) break;
                }
            }
        }
        
        /// <summary>
        /// Get the current velocity of the character.
        /// </summary>
        public Vector2 GetVelocity()
        {
            try
            {
                // Try to get velocity from the character controller
                if (velocityProperty != null)
                {
                    var value = velocityProperty.GetValue(characterController);
                    if (value is Rigidbody2D rb)
                    {
                        lastKnownVelocity = rb.velocity;
                        return lastKnownVelocity;
                    }
                    else if (value is Vector2 vel)
                    {
                        lastKnownVelocity = vel;
                        return lastKnownVelocity;
                    }
                }
                
                if (velocityField != null)
                {
                    var value = velocityField.GetValue(characterController);
                    if (value is Rigidbody2D rb)
                    {
                        lastKnownVelocity = rb.velocity;
                        return lastKnownVelocity;
                    }
                    else if (value is Vector2 vel)
                    {
                        lastKnownVelocity = vel;
                        return lastKnownVelocity;
                    }
                }
                
                // Fallback to direct rigidbody access
                if (fallbackRigidbody != null)
                {
                    lastKnownVelocity = fallbackRigidbody.velocity;
                    return lastKnownVelocity;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"CharacterController2DAdapter: Error accessing velocity: {e.Message}");
            }
            
            return lastKnownVelocity;
        }
        
        /// <summary>
        /// Get whether the character is facing right.
        /// </summary>
        public bool IsFacingRight()
        {
            try
            {
                // Try to get facing direction from the character controller
                if (facingProperty != null)
                {
                    var value = facingProperty.GetValue(characterController);
                    if (value is bool facing)
                    {
                        lastKnownFacingRight = facing;
                        return lastKnownFacingRight;
                    }
                }
                
                if (facingField != null)
                {
                    var value = facingField.GetValue(characterController);
                    if (value is bool facing)
                    {
                        lastKnownFacingRight = facing;
                        return lastKnownFacingRight;
                    }
                }
                
                // Fallback: determine facing from velocity
                var velocity = GetVelocity();
                if (Mathf.Abs(velocity.x) > 0.1f)
                {
                    lastKnownFacingRight = velocity.x > 0;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"CharacterController2DAdapter: Error accessing facing direction: {e.Message}");
            }
            
            return lastKnownFacingRight;
        }
        
        /// <summary>
        /// Manually set the character controller reference.
        /// </summary>
        public void SetCharacterController(MonoBehaviour controller)
        {
            characterController = controller;
            SetupVelocityAccess();
            SetupFacingAccess();
        }
        
        /// <summary>
        /// Manually configure property/field names for reflection access.
        /// </summary>
        public void SetPropertyNames(string velocityName, string facingName)
        {
            velocityPropertyName = velocityName;
            facingRightPropertyName = facingName;
            SetupVelocityAccess();
            SetupFacingAccess();
        }
    }
}