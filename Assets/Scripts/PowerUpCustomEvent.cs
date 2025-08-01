using UnityEngine;
using Unity.Cinemachine;

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
            // For orthographic cameras
            if (cineCam.Lens.Orthographic)
            {
            cineCam.Lens.OrthographicSize += cameraZoomOutAmount;
            }
            else
            {
            cineCam.Lens.FieldOfView += cameraZoomOutAmount * 10f;
            }
        }
        Debug.Log("Custom PowerUp: +1 clone and Cinemachine camera zoomed out!");
    }
}