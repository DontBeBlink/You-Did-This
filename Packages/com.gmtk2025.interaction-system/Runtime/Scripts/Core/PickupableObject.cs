using UnityEngine;

namespace GMTK2025.InteractionSystem
{
    /// <summary>
    /// Basic implementation of IPickupable for objects that can be picked up and carried.
    /// Provides physics-based pickup behavior with automatic rigidbody and collider management.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PickupableObject : InteractableObject, IPickupable
    {
        [Header("Pickup Settings")]
        [SerializeField] private bool canPickup = true;
        [SerializeField] private Vector2 carryPositionOffset = Vector2.zero;
        [SerializeField] private bool disablePhysicsWhenCarried = true;
        [SerializeField] private bool disableCollisionWhenCarried = true;
        
        [Header("Drop Settings")]
        [SerializeField] private float dropCooldownTime = 0.5f;
        [SerializeField] private float velocityTransferMultiplier = 1f;
        
        [Header("Pickup Events")]
        public UnityEngine.Events.UnityEvent OnPickupEvent;
        public UnityEngine.Events.UnityEvent OnDropEvent;
        
        // Interface implementation
        public bool CanPickup => canPickup && !IsBeingCarried;
        public bool IsBeingCarried { get; private set; }
        public Rigidbody2D Rigidbody { get; private set; }
        public new Collider2D Collider { get; private set; }
        public Vector2 CarryPositionOffset => carryPositionOffset;
        
        // Internal state
        private IInteractionController currentCarrier;
        private bool wasKinematic;
        private bool wasColliderTrigger;
        private float lastDropTime;
        
        protected override void Awake()
        {
            base.Awake();
            SetupComponents();
        }
        
        private void SetupComponents()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            Collider = GetComponent<Collider2D>();
            
            if (Rigidbody == null)
            {
                Debug.LogError($"PickupableObject {name} requires a Rigidbody2D component!");
            }
            
            if (Collider == null)
            {
                Debug.LogError($"PickupableObject {name} requires a Collider2D component!");
            }
            
            // Store original physics settings
            if (Rigidbody != null)
            {
                wasKinematic = Rigidbody.isKinematic;
            }
            
            if (Collider != null)
            {
                wasColliderTrigger = Collider.isTrigger;
            }
        }
        
        /// <summary>
        /// Handle interaction - attempts to pick up the object.
        /// </summary>
        public override void OnInteract(IInteractionController interactor)
        {
            if (interactor.CanPickupObjects && interactor.CarriedObject == null)
            {
                if (interactor.TryPickup())
                {
                    // Pickup handled by interaction controller
                    return;
                }
            }
            
            // Fall back to base interaction behavior
            base.OnInteract(interactor);
        }
        
        /// <summary>
        /// Called when this object is picked up.
        /// </summary>
        public virtual void OnPickup(IInteractionController carrier)
        {
            if (IsBeingCarried) return;
            
            currentCarrier = carrier;
            IsBeingCarried = true;
            
            // Configure physics for being carried
            if (disablePhysicsWhenCarried && Rigidbody != null)
            {
                Rigidbody.isKinematic = true;
                Rigidbody.velocity = Vector2.zero;
                Rigidbody.angularVelocity = 0f;
            }
            
            // Configure collision for being carried
            if (disableCollisionWhenCarried && Collider != null)
            {
                Collider.isTrigger = true;
            }
            
            OnPickupEvent?.Invoke();
            Debug.Log($"{name} was picked up by {carrier.GameObject.name}");
        }
        
        /// <summary>
        /// Called when this object is dropped or thrown.
        /// </summary>
        public virtual void OnDrop(Vector2 force, Vector2 carrierVelocity)
        {
            if (!IsBeingCarried) return;
            
            currentCarrier = null;
            IsBeingCarried = false;
            lastDropTime = Time.time;
            
            // Restore physics settings
            if (Rigidbody != null)
            {
                Rigidbody.isKinematic = wasKinematic;
                
                // Apply drop force and carrier velocity
                Vector2 finalForce = force + (carrierVelocity * velocityTransferMultiplier);
                Rigidbody.velocity = finalForce;
            }
            
            // Restore collision settings
            if (Collider != null)
            {
                Collider.isTrigger = wasColliderTrigger;
            }
            
            OnDropEvent?.Invoke();
            Debug.Log($"{name} was dropped with force {force}");
        }
        
        /// <summary>
        /// Check if this object can currently be picked up.
        /// Includes cooldown check to prevent immediate re-pickup after dropping.
        /// </summary>
        public override void OnInteractionEnter(IInteractionController interactor)
        {
            // Don't show interaction feedback if we're in drop cooldown
            if (Time.time - lastDropTime < dropCooldownTime)
            {
                return;
            }
            
            base.OnInteractionEnter(interactor);
        }
        
        /// <summary>
        /// Enable or disable pickup capability for this object.
        /// </summary>
        public virtual void SetPickupEnabled(bool enabled)
        {
            canPickup = enabled;
        }
        
        /// <summary>
        /// Set the position offset for when this object is carried.
        /// </summary>
        public virtual void SetCarryPositionOffset(Vector2 offset)
        {
            carryPositionOffset = offset;
        }
        
        /// <summary>
        /// Force drop this object if it's being carried.
        /// </summary>
        public virtual void ForceDrop(Vector2 force = default)
        {
            if (IsBeingCarried && currentCarrier != null)
            {
                currentCarrier.DropCarriedObject(force);
            }
        }
        
        private void OnValidate()
        {
            // Ensure cooldown time is not negative
            if (dropCooldownTime < 0)
            {
                dropCooldownTime = 0;
            }
            
            // Ensure velocity transfer multiplier is reasonable
            if (velocityTransferMultiplier < 0)
            {
                velocityTransferMultiplier = 0;
            }
        }
    }
}