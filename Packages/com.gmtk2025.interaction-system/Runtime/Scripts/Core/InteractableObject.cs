using UnityEngine;

namespace GMTK2025.InteractionSystem
{
    /// <summary>
    /// Basic implementation of IInteractable for simple interactive objects.
    /// This component provides the foundation for objects that can be interacted with
    /// by characters using the interaction system.
    /// </summary>
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private bool canInteract = true;
        [SerializeField] private int interactionPriority = 0;
        
        [Header("Visual Feedback")]
        [SerializeField] private bool enableHighlight = true;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private float highlightIntensity = 1.2f;
        
        [Header("Events")]
        public UnityEngine.Events.UnityEvent OnInteractEvent;
        public UnityEngine.Events.UnityEvent OnInteractionEnterEvent;
        public UnityEngine.Events.UnityEvent OnInteractionExitEvent;
        
        // Interface implementation
        public bool CanInteract => canInteract;
        public Transform Transform => transform;
        public GameObject GameObject => gameObject;
        public int InteractionPriority => interactionPriority;
        
        // Internal state
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private bool isHighlighted;
        
        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }
        
        /// <summary>
        /// Called when this object is interacted with.
        /// Override this method to implement custom interaction behavior.
        /// </summary>
        public virtual void OnInteract(IInteractionController interactor)
        {
            Debug.Log($"Interacted with {name}");
            OnInteractEvent?.Invoke();
        }
        
        /// <summary>
        /// Called when an interaction system enters range.
        /// Default implementation applies visual highlighting.
        /// </summary>
        public virtual void OnInteractionEnter(IInteractionController interactor)
        {
            if (enableHighlight && !isHighlighted)
            {
                ApplyHighlight();
            }
            OnInteractionEnterEvent?.Invoke();
        }
        
        /// <summary>
        /// Called when an interaction system exits range.
        /// Default implementation removes visual highlighting.
        /// </summary>
        public virtual void OnInteractionExit(IInteractionController interactor)
        {
            if (isHighlighted)
            {
                RemoveHighlight();
            }
            OnInteractionExitEvent?.Invoke();
        }
        
        /// <summary>
        /// Enable or disable interaction for this object.
        /// </summary>
        public virtual void SetInteractionEnabled(bool enabled)
        {
            canInteract = enabled;
        }
        
        /// <summary>
        /// Set the interaction priority for this object.
        /// Higher priority objects will be preferred when multiple objects are in range.
        /// </summary>
        public virtual void SetInteractionPriority(int priority)
        {
            interactionPriority = priority;
        }
        
        protected virtual void ApplyHighlight()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor * highlightColor * highlightIntensity;
                isHighlighted = true;
            }
        }
        
        protected virtual void RemoveHighlight()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
                isHighlighted = false;
            }
        }
        
        private void OnValidate()
        {
            // Ensure priority is not negative
            if (interactionPriority < 0)
            {
                interactionPriority = 0;
            }
        }
    }
}