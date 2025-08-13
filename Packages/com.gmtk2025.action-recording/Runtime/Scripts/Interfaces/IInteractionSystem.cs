using UnityEngine;

namespace GMTK2025.ActionRecording
{
    /// <summary>
    /// Interface for interaction systems that can be used with the action recording system.
    /// This allows the recording system to capture and replay interaction events
    /// without being tightly coupled to a specific interaction implementation.
    /// </summary>
    public interface IInteractionSystem
    {
        /// <summary>
        /// Trigger an interaction action
        /// </summary>
        void TriggerInteraction();
        
        /// <summary>
        /// Trigger an attack action
        /// </summary>
        void TriggerAttack();
        
        /// <summary>
        /// Check if there are any objects available for interaction
        /// </summary>
        bool HasInteractableObjects { get; }
    }
}