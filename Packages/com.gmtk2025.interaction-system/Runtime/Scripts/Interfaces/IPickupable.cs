using UnityEngine;

namespace GMTK2025.InteractionSystem
{
    /// <summary>
    /// Interface for objects that can be picked up and carried by characters.
    /// Extends IInteractable to provide pickup-specific functionality.
    /// </summary>
    public interface IPickupable : IInteractable
    {
        /// <summary>
        /// Whether this object can currently be picked up.
        /// </summary>
        bool CanPickup { get; }
        
        /// <summary>
        /// Whether this object is currently being carried by someone.
        /// </summary>
        bool IsBeingCarried { get; }
        
        /// <summary>
        /// The Rigidbody2D component for physics manipulation.
        /// </summary>
        Rigidbody2D Rigidbody { get; }
        
        /// <summary>
        /// The Collider2D component for collision handling.
        /// </summary>
        Collider2D Collider { get; }
        
        /// <summary>
        /// Called when this object is picked up by a character.
        /// </summary>
        /// <param name="carrier">The interaction system that picked up this object</param>
        void OnPickup(IInteractionController carrier);
        
        /// <summary>
        /// Called when this object is dropped or thrown by a character.
        /// </summary>
        /// <param name="force">The force to apply when dropped/thrown</param>
        /// <param name="carrierVelocity">The velocity of the character that was carrying this object</param>
        void OnDrop(Vector2 force, Vector2 carrierVelocity);
        
        /// <summary>
        /// Get the local position offset for where this object should be positioned when carried.
        /// </summary>
        Vector2 CarryPositionOffset { get; }
    }
}