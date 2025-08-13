using UnityEngine;
using GMTK2025.InteractionSystem;

namespace GMTK2025.InteractionSystem.Examples
{
    /// <summary>
    /// Example interaction object that changes color when activated.
    /// Demonstrates basic IInteractable implementation.
    /// </summary>
    public class ColorSwitch : InteractableObject
    {
        [Header("Color Switch Settings")]
        [SerializeField] private Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow };
        [SerializeField] private bool randomColor = false;
        
        private SpriteRenderer spriteRenderer;
        private int currentColorIndex = 0;
        
        protected override void Awake()
        {
            base.Awake();
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"ColorSwitch {name} requires a SpriteRenderer component!");
            }
        }
        
        private void Start()
        {
            // Set initial color
            if (spriteRenderer != null && colors.Length > 0)
            {
                spriteRenderer.color = colors[0];
            }
        }
        
        public override void OnInteract(IInteractionController interactor)
        {
            base.OnInteract(interactor);
            ChangeColor();
        }
        
        private void ChangeColor()
        {
            if (spriteRenderer == null || colors.Length == 0) return;
            
            if (randomColor)
            {
                currentColorIndex = Random.Range(0, colors.Length);
            }
            else
            {
                currentColorIndex = (currentColorIndex + 1) % colors.Length;
            }
            
            spriteRenderer.color = colors[currentColorIndex];
            Debug.Log($"Color switch {name} changed to {colors[currentColorIndex]}");
        }
        
        /// <summary>
        /// Set a specific color by index.
        /// </summary>
        public void SetColor(int colorIndex)
        {
            if (spriteRenderer == null || colors.Length == 0) return;
            
            currentColorIndex = Mathf.Clamp(colorIndex, 0, colors.Length - 1);
            spriteRenderer.color = colors[currentColorIndex];
        }
        
        /// <summary>
        /// Add a new color to the available colors.
        /// </summary>
        public void AddColor(Color newColor)
        {
            var newColors = new Color[colors.Length + 1];
            colors.CopyTo(newColors, 0);
            newColors[colors.Length] = newColor;
            colors = newColors;
        }
    }
}