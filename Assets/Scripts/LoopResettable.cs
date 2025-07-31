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

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
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
    }
}
