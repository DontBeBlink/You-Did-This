using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AreaTrigger : TriggerObject
{
    [Tooltip("Which layers' objects can trigger this")]
    public LayerMask triggerMask;
    [Tooltip("If enabled, will only trigger if a player enters the area")]
    public bool playerOnly;

    /// <summary>
    /// Returns true if there's a valid object inside the area, or if oneShot and triggered.
    /// </summary>
    public override bool Active { get { return oneShot ? triggered : objCount > 0; } }

    private int objCount;
    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (oneShot && triggered)
            return;

        if ((triggerMask == (triggerMask | (1 << other.gameObject.layer))) &&
            (!playerOnly || other.CompareTag("Player")))
        {
            objCount++;
            if (oneShot)
                triggered = true;
            animator.SetBool(ANIMATION_ACTIVE, Active);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (oneShot && triggered)
            return;

        if ((triggerMask == (triggerMask | (1 << other.gameObject.layer))) &&
            (!playerOnly || other.CompareTag("Player")))
        {
            objCount = Mathf.Max(0, objCount - 1);
            animator.SetBool(ANIMATION_ACTIVE, Active);
        }
    }
}