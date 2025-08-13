using System.Collections.Generic;
using UnityEngine;

namespace GMTK2025.InteractionSystem
{
    /// <summary>
    /// Interface for systems that can interact with objects and pick up items.
    /// Provides standardized interaction capabilities for characters and AI.
    /// </summary>
    public interface IInteractionController
    {
        /// <summary>
        /// The transform of the entity that owns this interaction system.
        /// </summary>
        Transform Transform { get; }
        
        /// <summary>
        /// The GameObject of the entity that owns this interaction system.
        /// </summary>
        GameObject GameObject { get; }
        
        /// <summary>
        /// Whether this controller can currently pick up objects.
        /// </summary>
        bool CanPickupObjects { get; }
        
        /// <summary>
        /// The currently carried object, if any.
        /// </summary>
        IPickupable CarriedObject { get; }
        
        /// <summary>
        /// List of all interactable objects currently in range.
        /// </summary>
        IReadOnlyList<IInteractable> InteractableObjects { get; }
        
        /// <summary>
        /// The closest interactable object, if any.
        /// </summary>
        IInteractable ClosestInteractable { get; }
        
        /// <summary>
        /// Attempt to interact with the closest interactable object.
        /// </summary>
        /// <returns>True if an interaction occurred, false otherwise</returns>
        bool TryInteract();
        
        /// <summary>
        /// Attempt to pick up the closest pickupable object.
        /// </summary>
        /// <returns>True if a pickup occurred, false otherwise</returns>
        bool TryPickup();
        
        /// <summary>
        /// Drop or throw the currently carried object.
        /// </summary>
        /// <param name="throwForce">Force to apply when dropping/throwing</param>
        /// <returns>True if an object was dropped, false if nothing was being carried</returns>
        bool DropCarriedObject(Vector2 throwForce = default);
        
        /// <summary>
        /// Get the current velocity of this controller (for physics interactions).
        /// </summary>
        Vector2 Velocity { get; }
        
        /// <summary>
        /// Whether this controller is facing right (for directional interactions).
        /// </summary>
        bool FacingRight { get; }
        
        /// <summary>
        /// Event triggered when an interaction occurs.
        /// </summary>
        event System.Action<IInteractable> OnInteracted;
        
        /// <summary>
        /// Event triggered when an object is picked up.
        /// </summary>
        event System.Action<IPickupable> OnObjectPickedUp;
        
        /// <summary>
        /// Event triggered when an object is dropped.
        /// </summary>
        event System.Action<IPickupable> OnObjectDropped;
    }
}