using UnityEngine;
using UnityEngine.InputSystem;

namespace GMTK2025.InteractionSystem
{
    /// <summary>
    /// Provides visual feedback for the interaction system, including input icons and UI prompts.
    /// Automatically detects input device type and displays appropriate icons.
    /// </summary>
    public class InteractionFeedbackUI : MonoBehaviour
    {
        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer inputIcon;
        [SerializeField] private Vector2 iconPositionOffset = new Vector2(0, 1.5f);
        
        [Header("Input Device Icons")]
        [SerializeField] private Sprite keyboardIcon;
        [SerializeField] private Sprite xboxIcon;
        [SerializeField] private Sprite psxIcon;
        [SerializeField] private Sprite defaultIcon;
        
        [Header("Auto Setup")]
        [SerializeField] private bool autoCreateIcon = true;
        [SerializeField] private Color iconColor = Color.white;
        [SerializeField] private float iconSize = 1f;
        
        private IInteractable currentTarget;
        private Camera mainCamera;
        private bool lastInteractionState;
        
        private void Awake()
        {
            SetupComponents();
        }
        
        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }
        
        private void Update()
        {
            UpdateIconDisplay();
            UpdateInputIcon();
        }
        
        private void SetupComponents()
        {
            if (inputIcon == null && autoCreateIcon)
            {
                CreateInputIcon();
            }
            
            if (inputIcon != null)
            {
                inputIcon.enabled = false;
            }
        }
        
        private void CreateInputIcon()
        {
            // Create icon GameObject
            GameObject iconObj = new GameObject("InteractionIcon");
            iconObj.transform.SetParent(transform);
            
            // Add and configure SpriteRenderer
            inputIcon = iconObj.AddComponent<SpriteRenderer>();
            inputIcon.sprite = defaultIcon;
            inputIcon.color = iconColor;
            inputIcon.sortingLayerName = "UI";
            inputIcon.sortingOrder = 100;
            
            // Scale the icon
            iconObj.transform.localScale = Vector3.one * iconSize;
        }
        
        /// <summary>
        /// Set the target interactable object for feedback display.
        /// </summary>
        public void SetTarget(IInteractable target)
        {
            currentTarget = target;
            
            if (inputIcon != null)
            {
                inputIcon.enabled = target != null;
            }
        }
        
        /// <summary>
        /// Set whether interaction is currently available.
        /// </summary>
        public void SetInteractionAvailable(bool available)
        {
            lastInteractionState = available;
            
            if (inputIcon != null)
            {
                inputIcon.enabled = available && currentTarget != null;
            }
        }
        
        private void UpdateIconDisplay()
        {
            if (inputIcon == null || currentTarget == null) return;
            
            // Position icon above the target object
            Vector3 targetPosition = currentTarget.Transform.position + (Vector3)iconPositionOffset;
            inputIcon.transform.position = targetPosition;
            
            // Make icon face camera
            if (mainCamera != null)
            {
                Vector3 lookDirection = mainCamera.transform.position - inputIcon.transform.position;
                lookDirection.z = 0; // Keep in 2D plane
                if (lookDirection != Vector3.zero)
                {
                    inputIcon.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                }
            }
        }
        
        private void UpdateInputIcon()
        {
            if (inputIcon == null) return;
            
            // Detect current input device and set appropriate icon
            Sprite iconToUse = GetCurrentInputIcon();
            if (iconToUse != null && inputIcon.sprite != iconToUse)
            {
                inputIcon.sprite = iconToUse;
            }
        }
        
        private Sprite GetCurrentInputIcon()
        {
            // Try to detect current input device
            if (Gamepad.current != null)
            {
                string deviceName = Gamepad.current.displayName.ToLower();
                
                // Check for PlayStation controllers
                if (deviceName.Contains("playstation") || deviceName.Contains("ps4") || deviceName.Contains("ps5") || deviceName.Contains("dualshock"))
                {
                    return psxIcon ?? defaultIcon;
                }
                
                // Default to Xbox for other gamepads
                return xboxIcon ?? defaultIcon;
            }
            
            // Default to keyboard icon
            return keyboardIcon ?? defaultIcon;
        }
        
        /// <summary>
        /// Manually set the icon sprites for different input types.
        /// </summary>
        public void SetInputIcons(Sprite keyboard, Sprite xbox, Sprite psx, Sprite fallback = null)
        {
            keyboardIcon = keyboard;
            xboxIcon = xbox;
            psxIcon = psx;
            defaultIcon = fallback ?? keyboard;
        }
        
        private void OnDrawGizmosSelected()
        {
            // Show icon position offset in scene view
            Gizmos.color = Color.yellow;
            Vector3 iconPos = transform.position + (Vector3)iconPositionOffset;
            Gizmos.DrawWireCube(iconPos, Vector3.one * 0.2f);
        }
    }
}