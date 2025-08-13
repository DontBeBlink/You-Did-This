using UnityEngine;

namespace GMTK2025.ActionRecording
{
    /// <summary>
    /// Adapter class that implements IInteractionSystem for the original GMTK2025 InteractSystem.
    /// This allows the action recording system to work with the existing interaction system
    /// without modifying the original code.
    /// 
    /// Usage: Add this component to any GameObject that has InteractSystem.
    /// The action recording system will automatically detect and use this adapter.
    /// </summary>
    [RequireComponent(typeof(InteractSystem))]
    public class InteractSystemAdapter : MonoBehaviour, IInteractionSystem
    {
        private InteractSystem interactSystem;

        private void Awake()
        {
            interactSystem = GetComponent<InteractSystem>();
        }

        #region IInteractionSystem Implementation

        public void TriggerInteraction()
        {
            if (interactSystem != null)
            {
                interactSystem.Interact();
            }
        }

        public void TriggerAttack()
        {
            if (interactSystem != null)
            {
                interactSystem.Attack();
            }
        }

        public bool HasInteractableObjects
        {
            get
            {
                if (interactSystem != null)
                {
                    // Check if there are any interactable objects in range
                    // This might need to be adjusted based on the InteractSystem implementation
                    return interactSystem.InteractableObjects.Count > 0;
                }
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Get access to the underlying InteractSystem for advanced operations.
        /// </summary>
        public InteractSystem UnderlyingInteractSystem => interactSystem;
    }
}