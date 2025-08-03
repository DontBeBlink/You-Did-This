using UnityEngine;

/// <summary>
/// Manages particle effects for clone lifecycle events such as spawning and despawning.
/// Provides visual feedback when clones are created, destroyed, or change state,
/// enhancing the overall visual polish of the clone system.
/// 
/// Features:
/// - Spawn particle burst when clone is created
/// - Despawn particle effect when clone is destroyed
/// - State change particles when clone becomes stuck
/// - Configurable particle settings for different events
/// - Automatic cleanup and memory management
/// </summary>
public class CloneParticleEffects : MonoBehaviour
{
    [Header("Spawn Effects")]
    [SerializeField] private ParticleSystem spawnParticles;
    [SerializeField] private Color spawnColor = Color.cyan;
    [SerializeField] private int spawnParticleCount = 30;
    [SerializeField] private float spawnBurstRadius = 0.5f;
    
    [Header("Despawn Effects")]
    [SerializeField] private ParticleSystem despawnParticles;
    [SerializeField] private Color despawnColor = Color.white;
    [SerializeField] private int despawnParticleCount = 20;
    [SerializeField] private float despawnBurstRadius = 0.3f;
    
    [Header("Stuck Effects")]
    [SerializeField] private ParticleSystem stuckParticles;
    [SerializeField] private Color stuckColor = Color.green;
    [SerializeField] private int stuckParticleCount = 15;
    [SerializeField] private float stuckBurstRadius = 0.4f;
    
    [Header("Ambient Effects")]
    [SerializeField] private ParticleSystem ambientParticles;
    [SerializeField] private bool enableAmbientParticles = true;
    [SerializeField] private float ambientEmissionRate = 5f;
    
    private Clone parentClone;
    private bool isInitialized = false;
    
    /// <summary>
    /// Initialize particle systems and get clone reference.
    /// </summary>
    private void Awake()
    {
        parentClone = GetComponent<Clone>();
        InitializeParticleSystems();
    }
    
    /// <summary>
    /// Create and configure particle systems if they don't exist.
    /// </summary>
    private void InitializeParticleSystems()
    {
        // Create spawn particles if not assigned
        if (spawnParticles == null)
        {
            spawnParticles = CreateParticleSystem("SpawnParticles", spawnColor, spawnParticleCount);
        }
        
        // Create despawn particles if not assigned
        if (despawnParticles == null)
        {
            despawnParticles = CreateParticleSystem("DespawnParticles", despawnColor, despawnParticleCount);
        }
        
        // Create stuck particles if not assigned
        if (stuckParticles == null)
        {
            stuckParticles = CreateParticleSystem("StuckParticles", stuckColor, stuckParticleCount);
        }
        
        // Create ambient particles if enabled and not assigned
        if (enableAmbientParticles && ambientParticles == null)
        {
            ambientParticles = CreateAmbientParticleSystem("AmbientParticles", spawnColor);
        }
        
        isInitialized = true;
    }
    
    /// <summary>
    /// Create a burst-style particle system for events.
    /// </summary>
    private ParticleSystem CreateParticleSystem(string name, Color color, int particleCount)
    {
        GameObject particleObj = new GameObject(name);
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem particles = particleObj.AddComponent<ParticleSystem>();
        var main = particles.main;
        var emission = particles.emission;
        var shape = particles.shape;
        var velocityOverLifetime = particles.velocityOverLifetime;
        var sizeOverLifetime = particles.sizeOverLifetime;
        var colorOverLifetime = particles.colorOverLifetime;
        
        // Main settings
        main.startLifetime = 1.0f;
        main.startSpeed = 3.0f;
        main.startSize = 0.1f;
        main.startColor = color;
        main.maxParticles = particleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Emission settings (burst only)
        emission.enabled = false; // We'll trigger bursts manually
        
        // Shape settings
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;
        
        // Velocity settings
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(2.0f);
        
        // Size over lifetime
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0, 1);
        sizeCurve.AddKey(1, 0);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);
        
        // Color over lifetime (fade out)
        colorOverLifetime.enabled = true;
        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = colorGradient;
        
        return particles;
    }
    
    /// <summary>
    /// Create an ambient particle system that continuously emits particles.
    /// </summary>
    private ParticleSystem CreateAmbientParticleSystem(string name, Color color)
    {
        GameObject particleObj = new GameObject(name);
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem particles = particleObj.AddComponent<ParticleSystem>();
        var main = particles.main;
        var emission = particles.emission;
        var shape = particles.shape;
        var velocityOverLifetime = particles.velocityOverLifetime;
        var sizeOverLifetime = particles.sizeOverLifetime;
        var colorOverLifetime = particles.colorOverLifetime;
        
        // Main settings
        main.startLifetime = 2.0f;
        main.startSpeed = 0.5f;
        main.startSize = 0.05f;
        Color ambientColor = color;
        ambientColor.a = 0.3f;
        main.startColor = ambientColor;
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Emission settings
        emission.enabled = true;
        emission.rateOverTime = ambientEmissionRate;
        
        // Shape settings
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;
        
        // Velocity settings - slow upward drift
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.5f);
        
        // Size over lifetime
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0, 0);
        sizeCurve.AddKey(0.5f, 1);
        sizeCurve.AddKey(1, 0);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);
        
        // Color over lifetime (fade in/out)
        colorOverLifetime.enabled = true;
        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(ambientColor, 0.0f), new GradientColorKey(ambientColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(0.3f, 0.5f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = colorGradient;
        
        return particles;
    }
    
    /// <summary>
    /// Play spawn particle effect.
    /// </summary>
    public void PlaySpawnEffect()
    {
        if (spawnParticles != null)
        {
            spawnParticles.Emit(spawnParticleCount);
        }
    }
    
    /// <summary>
    /// Play despawn particle effect.
    /// </summary>
    public void PlayDespawnEffect()
    {
        if (despawnParticles != null)
        {
            despawnParticles.Emit(despawnParticleCount);
        }
    }
    
    /// <summary>
    /// Play stuck particle effect when clone becomes stuck at goal.
    /// </summary>
    public void PlayStuckEffect()
    {
        if (stuckParticles != null)
        {
            stuckParticles.Emit(stuckParticleCount);
        }
        
        // Update ambient particles to stuck color
        if (ambientParticles != null)
        {
            var main = ambientParticles.main;
            Color stuckAmbientColor = stuckColor;
            stuckAmbientColor.a = 0.3f;
            main.startColor = stuckAmbientColor;
        }
    }
    
    /// <summary>
    /// Start ambient particle effects.
    /// </summary>
    public void StartAmbientEffects()
    {
        if (ambientParticles != null && enableAmbientParticles)
        {
            ambientParticles.Play();
        }
    }
    
    /// <summary>
    /// Stop ambient particle effects.
    /// </summary>
    public void StopAmbientEffects()
    {
        if (ambientParticles != null)
        {
            ambientParticles.Stop();
        }
    }
    
    /// <summary>
    /// Update particle effects based on clone state.
    /// </summary>
    private void Update()
    {
        if (!isInitialized || parentClone == null) return;
        
        // Enable/disable ambient effects based on clone state
        if (parentClone.IsReplaying && !ambientParticles.isPlaying)
        {
            StartAmbientEffects();
        }
        else if (!parentClone.IsReplaying && ambientParticles.isPlaying)
        {
            StopAmbientEffects();
        }
    }
    
    /// <summary>
    /// Cleanup when clone is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        PlayDespawnEffect();
        // Allow despawn particles to finish before cleanup
        if (despawnParticles != null)
        {
            despawnParticles.transform.SetParent(null);
            Destroy(despawnParticles.gameObject, 2.0f);
        }
    }
}