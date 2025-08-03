using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

/// <summary>
/// Attach this to a PowerUp and assign in the inspector for custom effects.
/// </summary>
public class PowerUpCustomEvent : MonoBehaviour
{
    // Example: Give 1 more clone and zoom out camera
    public float cameraZoomOutAmount = 2f;
    public float cameraZoomDuration = 1f;

    public void OnPowerUp(GameObject player)
    {
        // Give 1 more clone
        var cloneManager = FindFirstObjectByType<CloneManager>();
        if (cloneManager != null)
        {
            cloneManager.GetType().GetField("maxClones", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(cloneManager, cloneManager.TotalClones + 1);
        }

        // If using CinemachineCamera (Cinemachine 3.x+)
        var cineCam = FindFirstObjectByType<CinemachineCamera>();
        if (cineCam != null)
        {
            // Start smooth zoom transition
            StartCoroutine(SmoothCameraZoom(cineCam, cameraZoomOutAmount, cameraZoomDuration));
        }
        Debug.Log("Custom PowerUp: +1 clone and Cinemachine camera zooming out smoothly!");
    }
    
    private IEnumerator SmoothCameraZoom(CinemachineCamera cineCam, float zoomAmount, float duration)
    {
        float elapsedTime = 0;
        bool isOrthographic = cineCam.Lens.Orthographic;
        
        // Store starting values
        float startOrthoSize = cineCam.Lens.OrthographicSize;
        float targetOrthoSize = startOrthoSize + zoomAmount;
        
        float startFOV = cineCam.Lens.FieldOfView;
        float targetFOV = startFOV + (zoomAmount * 10f);
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration); // Normalized time (0-1)
            
            // Apply smooth easing
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            if (isOrthographic)
                cineCam.Lens.OrthographicSize = Mathf.Lerp(startOrthoSize, targetOrthoSize, smoothT);
            else
                cineCam.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, smoothT);
            
            yield return null;
        }
        
        // Ensure we end at exactly the target value
        if (isOrthographic)
            cineCam.Lens.OrthographicSize = targetOrthoSize;
        else
            cineCam.Lens.FieldOfView = targetFOV;
    }
}