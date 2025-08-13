using UnityEngine;

namespace GMTK2025.ActionRecording
{
    /// <summary>
    /// Manages visual and audio effects for action replay clones.
    /// This component handles ghost trails, particle effects, glow effects, and sound effects
    /// in a unified way to keep the main clone script clean and focused.
    /// </summary>
    public class ActionReplayEffects : MonoBehaviour
    {
        [Header("Ghost Trail Settings")]
        [SerializeField] private bool useGhostTrail = true;
        [SerializeField] private float trailLifetime = 1f;
        [SerializeField] private int maxTrailObjects = 10;
        
        [Header("Particle Settings")]
        [SerializeField] private bool useParticleEffects = true;
        [SerializeField] private ParticleSystem spawnParticles;
        [SerializeField] private ParticleSystem ambientParticles;
        
        [Header("Sound Settings")]
        [SerializeField] private bool useSoundEffects = true;
        [SerializeField] private AudioClip spawnSound;
        [SerializeField] private AudioClip stuckSound;
        
        [Header("Glow Settings")]
        [SerializeField] private bool useGlowEffect = true;
        [SerializeField] private float glowIntensity = 0.3f;
        [SerializeField] private float glowRadius = 0.5f;

        // Component references
        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;
        private Light glowLight;
        
        // Ghost trail objects
        private GameObject[] trailObjects;
        private int currentTrailIndex = 0;
        private float lastTrailTime;

        private void Awake()
        {
            SetupComponents();
        }

        private void SetupComponents()
        {
            // Find sprite renderer
            var spriteChild = transform.Find("Sprite");
            if (spriteChild != null)
            {
                spriteRenderer = spriteChild.GetComponent<SpriteRenderer>();
            }
            else
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            // Set up audio source
            if (useSoundEffects)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.volume = 0.5f;
                }
            }

            // Set up ghost trail objects
            if (useGhostTrail)
            {
                SetupGhostTrail();
            }

            // Set up glow effect
            if (useGlowEffect)
            {
                SetupGlowEffect();
            }
        }

        /// <summary>
        /// Configure the effects system with the provided settings.
        /// </summary>
        public void Configure(bool ghostTrail, bool particles, bool sounds, bool glow, Color effectColor)
        {
            useGhostTrail = ghostTrail;
            useParticleEffects = particles;
            useSoundEffects = sounds;
            useGlowEffect = glow;

            // Apply color to effects
            if (glowLight != null)
            {
                glowLight.color = effectColor;
            }

            // Configure particle systems if they exist
            if (spawnParticles != null)
            {
                var main = spawnParticles.main;
                main.startColor = effectColor;
            }

            if (ambientParticles != null)
            {
                var main = ambientParticles.main;
                main.startColor = effectColor;
            }
        }

        private void SetupGhostTrail()
        {
            trailObjects = new GameObject[maxTrailObjects];
            
            for (int i = 0; i < maxTrailObjects; i++)
            {
                GameObject trailObj = new GameObject($"GhostTrail_{i}");
                trailObj.transform.SetParent(transform.parent); // Don't parent to clone to avoid moving with it
                
                SpriteRenderer trailRenderer = trailObj.AddComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    trailRenderer.sprite = spriteRenderer.sprite;
                    Color trailColor = spriteRenderer.color;
                    trailColor.a = 0.1f; // Very transparent for ghost effect
                    trailRenderer.color = trailColor;
                    trailRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
                    trailRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
                }
                
                trailObj.SetActive(false);
                trailObjects[i] = trailObj;
            }
        }

        private void SetupGlowEffect()
        {
            // Try to create a simple glow using a Light component
            glowLight = gameObject.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.range = glowRadius;
            glowLight.intensity = glowIntensity;
            glowLight.color = Color.cyan;
            
            // For 2D games, you might want to disable shadows and use a different blend mode
            glowLight.shadows = LightShadows.None;
        }

        /// <summary>
        /// Play the spawn effect when the clone is created.
        /// </summary>
        public void PlaySpawnEffect()
        {
            if (useParticleEffects && spawnParticles != null)
            {
                spawnParticles.Play();
            }

            if (useSoundEffects && audioSource != null && spawnSound != null)
            {
                audioSource.PlayOneShot(spawnSound);
            }
        }

        /// <summary>
        /// Start continuous effects while the clone is active.
        /// </summary>
        public void StartActiveEffects()
        {
            if (useParticleEffects && ambientParticles != null)
            {
                ambientParticles.Play();
            }

            if (useGlowEffect && glowLight != null)
            {
                glowLight.enabled = true;
            }
        }

        /// <summary>
        /// Stop active effects when the clone becomes inactive.
        /// </summary>
        public void StopActiveEffects()
        {
            if (useParticleEffects && ambientParticles != null)
            {
                ambientParticles.Stop();
            }

            if (useGlowEffect && glowLight != null)
            {
                glowLight.enabled = false;
            }

            // Hide all ghost trail objects
            if (useGhostTrail && trailObjects != null)
            {
                foreach (var trailObj in trailObjects)
                {
                    if (trailObj != null)
                        trailObj.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Start effects for when the clone is stuck at a goal.
        /// </summary>
        public void StartStuckEffects()
        {
            if (useSoundEffects && audioSource != null && stuckSound != null)
            {
                audioSource.PlayOneShot(stuckSound);
            }

            // Could add special stuck visual effects here
        }

        private void Update()
        {
            if (useGhostTrail && trailObjects != null)
            {
                UpdateGhostTrail();
            }
        }

        private void UpdateGhostTrail()
        {
            // Only update trail if enough time has passed
            if (Time.time - lastTrailTime < trailLifetime / maxTrailObjects)
                return;

            lastTrailTime = Time.time;

            // Get the current trail object
            GameObject trailObj = trailObjects[currentTrailIndex];
            
            if (trailObj != null && spriteRenderer != null)
            {
                // Position the trail object at current clone position
                trailObj.transform.position = transform.position;
                trailObj.SetActive(true);
                
                // Update sprite and color to match current clone state
                SpriteRenderer trailRenderer = trailObj.GetComponent<SpriteRenderer>();
                if (trailRenderer != null)
                {
                    trailRenderer.sprite = spriteRenderer.sprite;
                    Color trailColor = spriteRenderer.color;
                    trailColor.a = 0.1f; // Ghost transparency
                    trailRenderer.color = trailColor;
                }
                
                // Start fading out the trail object
                StartCoroutine(FadeOutTrailObject(trailObj));
            }

            // Move to next trail object
            currentTrailIndex = (currentTrailIndex + 1) % maxTrailObjects;
        }

        private System.Collections.IEnumerator FadeOutTrailObject(GameObject trailObj)
        {
            SpriteRenderer renderer = trailObj.GetComponent<SpriteRenderer>();
            if (renderer == null) yield break;

            Color startColor = renderer.color;
            float fadeTime = trailLifetime;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / fadeTime);
                
                Color newColor = startColor;
                newColor.a = alpha;
                renderer.color = newColor;
                
                yield return null;
            }

            trailObj.SetActive(false);
        }

        private void OnDestroy()
        {
            // Clean up trail objects
            if (trailObjects != null)
            {
                foreach (var trailObj in trailObjects)
                {
                    if (trailObj != null)
                        DestroyImmediate(trailObj);
                }
            }
        }
    }
}