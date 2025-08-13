using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GMTK2025.InteractionSystem
{
    /// <summary>
    /// Comprehensive interaction system that enables characters to interact with objects and pick up items.
    /// Features proximity-based detection, visual feedback with input icons, and object pickup/throwing mechanics.
    /// 
    /// This refactored version uses interface abstractions for maximum flexibility and reusability.
    /// </summary>
    public class InteractionController : MonoBehaviour, IInteractionController
    {
        [Header("Detection Settings")]
        [SerializeField] private float interactionRange = 1.5f;
        [SerializeField] private Vector2 rangePositionOffset = Vector2.zero;
        [SerializeField] private LayerMask interactableLayerMask = -1;
        [SerializeField] private int maxDetectedObjects = 10;

        [Header("Pickup Settings")]
        [SerializeField] private bool canPickupObjects = true;
        [SerializeField] private Vector2 pickupPositionOffset = new Vector2(0, 0.5f);
        [SerializeField] private Vector2 throwForce = new Vector2(5f, 3f);

        [Header("Visual Feedback")]
        [SerializeField] private InteractionFeedbackUI feedbackUI;
        [SerializeField] private bool autoCreateFeedbackUI = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color debugRangeColor = Color.magenta;

        // Interface implementation properties
        public Transform Transform => transform;
        public GameObject GameObject => gameObject;
        public bool CanPickupObjects => canPickupObjects;
        public IPickupable CarriedObject { get; private set; }
        public IReadOnlyList<IInteractable> InteractableObjects => detectedInteractables.AsReadOnly();
        public IInteractable ClosestInteractable { get; private set; }
        public Vector2 Velocity => cachedVelocity;
        public bool FacingRight => cachedFacingRight;

        // Events
        public event System.Action<IInteractable> OnInteracted;
        public event System.Action<IPickupable> OnObjectPickedUp;
        public event System.Action<IPickupable> OnObjectDropped;

        // Internal state
        private List<IInteractable> detectedInteractables = new List<IInteractable>();
        private List<IInteractable> previousInteractables = new List<IInteractable>();
        private Rigidbody2D rigidBody;
        private Vector2 cachedVelocity;
        private bool cachedFacingRight = true;
        
        // Interface references for flexibility
        private ICharacterVelocityProvider velocityProvider;
        private ICharacterFacingProvider facingProvider;

        #region Unity Lifecycle

        private void Awake()
        {
            SetupComponents();
        }

        private void Start()
        {
            SetupFeedbackUI();
        }

        private void Update()
        {
            UpdateCachedValues();
            DetectInteractables();
            UpdateClosestInteractable();
            UpdateFeedbackUI();
            UpdateCarriedObjectPosition();
        }

        #endregion

        #region Setup Methods

        private void SetupComponents()
        {
            rigidBody = GetComponent<Rigidbody2D>();
            
            // Try to find velocity and facing providers
            velocityProvider = GetComponent<ICharacterVelocityProvider>();
            facingProvider = GetComponent<ICharacterFacingProvider>();
        }

        private void SetupFeedbackUI()
        {
            if (feedbackUI == null && autoCreateFeedbackUI)
            {
                GameObject feedbackObj = new GameObject("InteractionFeedbackUI");
                feedbackObj.transform.SetParent(transform);
                feedbackUI = feedbackObj.AddComponent<InteractionFeedbackUI>();
            }
        }

        #endregion

        #region Detection and Updates

        private void UpdateCachedValues()
        {
            // Update velocity from various sources
            if (velocityProvider != null)
            {
                cachedVelocity = velocityProvider.GetVelocity();
            }
            else if (rigidBody != null)
            {
                cachedVelocity = rigidBody.velocity;
            }
            else
            {
                cachedVelocity = Vector2.zero;
            }

            // Update facing direction from various sources
            if (facingProvider != null)
            {
                cachedFacingRight = facingProvider.IsFacingRight();
            }
            // If no provider, maintain current facing direction
        }

        private void DetectInteractables()
        {
            // Store previous frame's interactables for comparison
            previousInteractables.Clear();
            previousInteractables.AddRange(detectedInteractables);
            
            // Clear current detections
            detectedInteractables.Clear();

            // Detect objects within interaction range
            Vector2 detectionCenter = (Vector2)transform.position + rangePositionOffset;
            Collider2D[] hits = Physics2D.OverlapCircleAll(detectionCenter, interactionRange, interactableLayerMask);

            foreach (var hit in hits.Take(maxDetectedObjects))
            {
                var interactable = hit.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    // Validate pickup objects
                    if (interactable is IPickupable pickup)
                    {
                        // Skip if we can't pickup objects
                        if (!canPickupObjects) continue;
                        
                        // Skip if this object is already being carried
                        if (pickup.IsBeingCarried) continue;
                        
                        // Skip if we're already carrying something
                        if (CarriedObject != null) continue;
                    }

                    detectedInteractables.Add(interactable);
                }
            }

            // Sort by priority and distance
            detectedInteractables.Sort((a, b) =>
            {
                int priorityComparison = b.InteractionPriority.CompareTo(a.InteractionPriority);
                if (priorityComparison != 0) return priorityComparison;
                
                float distA = Vector2.Distance(transform.position, a.Transform.position);
                float distB = Vector2.Distance(transform.position, b.Transform.position);
                return distA.CompareTo(distB);
            });

            // Notify objects about range enter/exit
            NotifyInteractionRangeChanges();
        }

        private void NotifyInteractionRangeChanges()
        {
            // Notify objects that entered range
            foreach (var interactable in detectedInteractables)
            {
                if (!previousInteractables.Contains(interactable))
                {
                    interactable.OnInteractionEnter(this);
                }
            }

            // Notify objects that exited range
            foreach (var interactable in previousInteractables)
            {
                if (!detectedInteractables.Contains(interactable))
                {
                    interactable.OnInteractionExit(this);
                }
            }
        }

        private void UpdateClosestInteractable()
        {
            var previousClosest = ClosestInteractable;
            ClosestInteractable = detectedInteractables.FirstOrDefault();

            // Update feedback UI if closest changed
            if (ClosestInteractable != previousClosest)
            {
                if (feedbackUI != null)
                {
                    feedbackUI.SetTarget(ClosestInteractable);
                }
            }
        }

        private void UpdateFeedbackUI()
        {
            if (feedbackUI != null)
            {
                feedbackUI.SetInteractionAvailable(ClosestInteractable != null);
            }
        }

        private void UpdateCarriedObjectPosition()
        {
            if (CarriedObject != null)
            {
                Vector3 carryPosition = transform.position + (Vector3)pickupPositionOffset;
                if (CarriedObject.Transform != null)
                {
                    CarriedObject.Transform.position = carryPosition;
                }
            }
        }

        #endregion

        #region Public Interface Implementation

        public bool TryInteract()
        {
            if (ClosestInteractable == null) return false;

            var interactable = ClosestInteractable;
            
            // Handle pickup objects specially
            if (interactable is IPickupable pickup && canPickupObjects && CarriedObject == null)
            {
                return TryPickup();
            }
            
            // Regular interaction
            interactable.OnInteract(this);
            OnInteracted?.Invoke(interactable);
            
            return true;
        }

        public bool TryPickup()
        {
            if (!canPickupObjects || CarriedObject != null) return false;
            
            var pickup = ClosestInteractable as IPickupable;
            if (pickup == null || !pickup.CanPickup) return false;

            // Perform pickup
            CarriedObject = pickup;
            pickup.OnPickup(this);
            
            OnObjectPickedUp?.Invoke(pickup);
            return true;
        }

        public bool DropCarriedObject(Vector2 throwForce = default)
        {
            if (CarriedObject == null) return false;

            var objectToDrop = CarriedObject;
            CarriedObject = null;

            // Apply throw force (use default if none specified)
            Vector2 finalForce = throwForce == Vector2.zero ? this.throwForce : throwForce;
            
            // Apply facing direction to horizontal force
            if (!FacingRight)
            {
                finalForce.x = -finalForce.x;
            }

            objectToDrop.OnDrop(finalForce, Velocity);
            OnObjectDropped?.Invoke(objectToDrop);
            
            return true;
        }

        #endregion

        #region Public Utility Methods

        /// <summary>
        /// Set the velocity provider for this interaction controller.
        /// </summary>
        public void SetVelocityProvider(ICharacterVelocityProvider provider)
        {
            velocityProvider = provider;
        }

        /// <summary>
        /// Set the facing provider for this interaction controller.
        /// </summary>
        public void SetFacingProvider(ICharacterFacingProvider provider)
        {
            facingProvider = provider;
        }

        /// <summary>
        /// Force an interaction with a specific object (bypasses proximity checks).
        /// </summary>
        public bool ForceInteract(IInteractable interactable)
        {
            if (interactable == null || !interactable.CanInteract) return false;
            
            interactable.OnInteract(this);
            OnInteracted?.Invoke(interactable);
            return true;
        }

        /// <summary>
        /// Check if a specific object is within interaction range.
        /// </summary>
        public bool IsInRange(IInteractable interactable)
        {
            return detectedInteractables.Contains(interactable);
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos) return;

            // Draw interaction range
            Gizmos.color = debugRangeColor;
            Vector3 center = transform.position + (Vector3)rangePositionOffset;
            Gizmos.DrawWireSphere(center, interactionRange);

            // Draw pickup position
            if (canPickupObjects)
            {
                Gizmos.color = Color.green;
                Vector3 pickupPos = transform.position + (Vector3)pickupPositionOffset;
                Gizmos.DrawWireCube(pickupPos, Vector3.one * 0.2f);
            }
        }

        #endregion
    }

    #region Helper Interfaces

    /// <summary>
    /// Interface for objects that can provide velocity information.
    /// </summary>
    public interface ICharacterVelocityProvider
    {
        Vector2 GetVelocity();
    }

    /// <summary>
    /// Interface for objects that can provide facing direction information.
    /// </summary>
    public interface ICharacterFacingProvider
    {
        bool IsFacingRight();
    }

    #endregion
}