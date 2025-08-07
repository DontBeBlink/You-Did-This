// 8/6/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;

public class ReplaceMaterialsWithEmission : MonoBehaviour
{
    public Texture2D portalTexture; // Assign the Portal texture in the Inspector
    public Color emissionColor = Color.white; // Set the emission color in the Inspector
    public float emissionIntensity = 1.5f; // Set the emission intensity in the Inspector

    void Start()
    {
        // Find the Effect GameObject
       // GameObject effect = GameObject.Find("Effect").transform.GetChild(0).gameObject;
       // if (effect != null)
       // {
       //     ReplaceMaterialWithEmission(effect, "Effect_Emissive");
       // }
       // else
       // {
       //     Debug.LogWarning("Effect GameObject not found!");
       // }

        // Find the Portal GameObject
        GameObject portal = GameObject.Find("Portal");
        if (portal != null && portalTexture != null)
        {
            ReplaceMaterialWithEmission(portal, "Portal_Emissive", portalTexture);
        }
        else
        {
            Debug.LogWarning("Portal GameObject or texture not found!");
        }
    }

    void ReplaceMaterialWithEmission(GameObject target, string materialName, Texture2D texture = null)
    {
        // Create a new material with emission and alpha clipping enabled
        Material emissiveMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        emissiveMaterial.name = materialName;

        // Set the Albedo texture if provided
        if (texture != null)
        {
            emissiveMaterial.SetTexture("_BaseMap", texture);
        }

        // Enable and configure emission
        emissiveMaterial.EnableKeyword("_EMISSION");
        emissiveMaterial.SetColor("_EmissionColor", emissionColor * emissionIntensity);
        if (texture != null)
        {
            emissiveMaterial.SetTexture("_EmissionMap", texture);
        }

        // Enable alpha clipping (cutout)
        emissiveMaterial.SetFloat("_Surface", 1); // 1 = Transparent
        emissiveMaterial.SetFloat("_AlphaClip", 1); // Enable alpha clipping
        emissiveMaterial.EnableKeyword("_ALPHATEST_ON");

        // Enable realtime global illumination for emission
        emissiveMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        // Assign the new material to the target GameObject
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = emissiveMaterial;
            Debug.Log($"Replaced material on {target.name} with emissive material: {materialName}");
        }
        else
        {
            Debug.LogWarning($"No Renderer found on {target.name}!");
        }
    }
}
