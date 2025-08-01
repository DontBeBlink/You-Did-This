using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attach this script to any object you want to reset to its original position at the start of each loop.
/// It listens for CloneManager.OnLoopStarted and resets the object's transform.
/// </summary>
public class LoopResettable : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    // For TriggerObject/Triggerable support
    private bool? originalTriggerActive;
    private bool? originalTriggerableActive;
    private Animator triggerAnimator;
    private Animator triggerableAnimator;
    private TriggerObject triggerObject;
    private Triggerable triggerable;

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;

        // Save TriggerObject state if present
        triggerObject = GetComponent<TriggerObject>();
        if (triggerObject != null)
        {
            originalTriggerActive = triggerObject.Active;
            triggerAnimator = triggerObject.GetComponent<Animator>();
        }

        // Save Triggerable state if present
        triggerable = GetComponent<Triggerable>();
        if (triggerable != null)
        {
            // If Triggerable uses a TriggerObject, prefer that
            if (triggerable.trigger != null)
            {
                originalTriggerableActive = triggerable.trigger.Active;
            }
            else
            {
                // Fallback: try to get animator state
                triggerableAnimator = triggerable.GetComponent<Animator>();
            }
        }
    }

    private void OnEnable()
    {
        CloneManager.OnLoopStarted += ResetToOriginal;
    }

    private void OnDisable()
    {
        CloneManager.OnLoopStarted -= ResetToOriginal;
    }

    private void ResetToOriginal()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        // Reset TriggerObject state (must use Trigger() since Active is read-only)
        if (triggerObject != null && originalTriggerActive.HasValue)
        {
            if (triggerObject.Active != originalTriggerActive.Value)
            {
                triggerObject.Trigger();
            }
            if (triggerAnimator != null)
            {
                triggerAnimator.SetBool(TriggerObject.ANIMATION_ACTIVE, originalTriggerActive.Value);
            }
        }

        // Reset Triggerable state (if it uses a TriggerObject)
        if (triggerable != null && originalTriggerableActive.HasValue && triggerable.trigger != null)
        {
            if (triggerable.trigger.Active != originalTriggerableActive.Value)
            {
                triggerable.trigger.Trigger();
            }
            if (triggerableAnimator != null)
            {
                triggerableAnimator.SetBool(TriggerObject.ANIMATION_ACTIVE, originalTriggerableActive.Value);
            }
        }
    }
}
