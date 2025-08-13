using UnityEngine;

namespace GMTK2025.InteractionSystem
{
    /// <summary>
    /// Interface for objects that can be interacted with by characters.
    /// Provides a standardized way to implement interactive objects in the game world.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Whether this object can currently be interacted with.
        /// </summary>
        bool CanInteract { get; }
        
        /// <summary>
        /// The transform of the interactable object for positioning feedback UI.
        /// </summary>
        Transform Transform { get; }
        
        /// <summary>
        /// The GameObject of the interactable object.
        /// </summary>
        GameObject GameObject { get; }
        
        /// <summary>
        /// Called when a character interacts with this object.
        /// </summary>
        /// <param name="interactor">The interaction system that initiated the interaction</param>
        void OnInteract(IInteractionController interactor);
        
        /// <summary>
        /// Called when an interaction system enters the interaction range.
        /// Useful for highlighting or preparing the object for interaction.
        /// </summary>
        /// <param name="interactor">The interaction system that entered range</param>
        void OnInteractionEnter(IInteractionController interactor);
        
        /// <summary>
        /// Called when an interaction system exits the interaction range.
        /// Useful for removing highlights or cleaning up interaction preparation.
        /// </summary>
        /// <param name="interactor">The interaction system that exited range</param>
        void OnInteractionExit(IInteractionController interactor);
        
        /// <summary>
        /// Get the priority of this interactable object.
        /// Higher priority objects will be preferred when multiple objects are in range.
        /// </summary>
        int InteractionPriority { get; }
    }
}