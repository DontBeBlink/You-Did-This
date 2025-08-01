using UnityEngine;

public class TimedInteractableTrigger : InteractableTrigger
{
    [Header("Timed Lever Settings")]
    public float onDuration = 6f; // Duration the lever stays on
    public bool toggleable = true; // If true, can be turned off early by interacting again

    private float timer = 0f;
    private bool isTiming = false;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isTiming)
        {
            timer -= Time.deltaTime;
            // Animate the lever based on timer progress (0 = on, 1 = off)
            if (animator)
            {
                float t = Mathf.Clamp01(1f - (timer / onDuration));
                animator.SetFloat("LeverProgress", t); // You need to use this parameter in your animation
            }
            if (timer <= 0f)
            {
                isTiming = false;
                SetTriggerActive(false);
                if (animator)
                {
                    animator.SetBool(TriggerObject.ANIMATION_ACTIVE, false);
                    animator.SetFloat("LeverProgress", 1f);
                }
            }
        }
    }

    public override void Interact(InteractSystem _system)
    {
        if (!isTiming)
        {
            SetTriggerActive(true);
            if (animator)
            {
                animator.SetBool(TriggerObject.ANIMATION_ACTIVE, true);
                animator.SetFloat("LeverProgress", 0f);
            }
            timer = onDuration;
            isTiming = true;
        }
        else if (toggleable)
        {
            // Turn off early if toggleable
            isTiming = false;
            SetTriggerActive(false);
            if (animator)
            {
                animator.SetBool(TriggerObject.ANIMATION_ACTIVE, false);
                animator.SetFloat("LeverProgress", 1f);
            }
        }
    }
    // Helper to set the trigger's active state, since 'Active' setter is inaccessible
    private void SetTriggerActive(bool value)
    {
        // Try to set via base class if possible
        // If 'Active' is protected in base, this will work
        // Otherwise, implement the correct logic here
        // base.Active = value; // Uncomment if accessible
        if (trigger != null)
        {
            // Use Trigger() to toggle if needed, or set via reflection if absolutely necessary
            // Here, we try to set the protected property via reflection as a fallback
            var type = trigger.GetType();
            var prop = type.GetProperty("Active", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(trigger, value);
            }
            else
            {
                // As a fallback, try to call Trigger() if the state is not as desired
                if (trigger.Active != value)
                {
                    trigger.Trigger();
                }
            }
        }
    }
}