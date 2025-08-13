# Clone/Action Recording System - Extraction Implementation Guide

## Overview

This guide provides detailed technical instructions for extracting the Clone/Action Recording System from the GMTK2025 project into a standalone, reusable Unity package. This system enables physics-perfect action recording and replay, making it valuable for time-loop mechanics, ghost players, AI behavior recording, or demonstration systems.

## 🎯 System Value Proposition

The Clone/Action Recording System offers:
- **Physics-Perfect Replay**: Frame-accurate recording at 50Hz matching Unity's FixedUpdate
- **Event-Driven Architecture**: Loose coupling through comprehensive event system
- **Configurable Lifecycle**: Automatic and manual clone creation with customizable parameters
- **Memory Efficient**: Optimized data structures and cleanup systems
- **Scalable Management**: Handles multiple simultaneous clones with performance optimization

## 📋 Files to Extract

### Core System Files (Required)
```
Assets/Scripts/ActionRecorder.cs       - Records player actions with timing
Assets/Scripts/CloneManager.cs         - Manages clone lifecycle and coordination
Assets/Scripts/Clone.cs                - Individual clone replay behavior
Assets/Scripts/CloneParticleEffects.cs - Visual effects for clone lifecycle
Assets/Scripts/CloneSoundEffects.cs    - Audio effects for clone events
```

### Supporting Documentation
```
CLONE_SYSTEM_SETUP.md                  - Original setup and usage guide
ARCHITECTURE.md                        - System architecture documentation
```

### Dependencies to Abstract
```
Assets/Scripts/PlayerController.cs     - INPUT PROVIDER (needs interface)
Assets/Scripts/CharacterController2D.cs - CHARACTER CONTROLLER (needs interface)
Assets/Scripts/Goal.cs                 - GOAL SYSTEM (needs interface)
```

## 🏗️ Extraction Steps

### Step 1: Create Package Structure

```
com.yourdomain.action-recording/
├── Runtime/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── ActionRecorder.cs
│   │   │   ├── CloneManager.cs
│   │   │   ├── Clone.cs
│   │   │   └── PlayerAction.cs
│   │   ├── Interfaces/
│   │   │   ├── ICharacterController.cs
│   │   │   ├── IInputProvider.cs
│   │   │   ├── IActionReplayable.cs
│   │   │   └── IGoalSystem.cs
│   │   ├── Events/
│   │   │   └── ActionRecordingEvents.cs
│   │   ├── Configuration/
│   │   │   └── ActionRecordingConfig.cs
│   │   └── Effects/
│   │       ├── CloneParticleEffects.cs
│   │       └── CloneSoundEffects.cs
│   ├── Prefabs/
│   │   ├── CloneManager.prefab
│   │   ├── DefaultClone.prefab
│   │   └── CloneEffects.prefab
│   └── ActionRecording.asmdef
├── Editor/
│   ├── Scripts/
│   │   ├── ActionRecorderEditor.cs
│   │   ├── CloneManagerEditor.cs
│   │   └── ActionRecordingSettingsProvider.cs
│   └── ActionRecordingEditor.asmdef
├── Tests/
│   ├── Runtime/
│   │   └── ActionRecordingTests.cs
│   ├── Editor/
│   │   └── ActionRecordingEditorTests.cs
│   └── ActionRecordingTests.asmdef
├── Samples~/
│   ├── BasicExample/
│   │   ├── Scenes/
│   │   ├── Scripts/
│   │   └── Prefabs/
│   └── AdvancedIntegration/
│       ├── Scenes/
│       ├── Scripts/
│       └── Prefabs/
├── Documentation~/
│   ├── api-reference.md
│   ├── setup-guide.md
│   ├── integration-examples.md
│   └── troubleshooting.md
├── package.json
├── README.md
├── CHANGELOG.md
└── LICENSE.md
```

### Step 2: Create Interface Abstractions

#### ICharacterController Interface
```csharp
using UnityEngine;

namespace ActionRecording.Interfaces
{
    /// <summary>
    /// Interface for character controllers that can be recorded and replayed.
    /// Provides access to physics state and movement control necessary for action recording.
    /// </summary>
    public interface ICharacterController
    {
        // Position and Physics State
        Vector3 Position { get; }
        Vector2 Velocity { get; }
        Vector2 ExternalForce { get; }
        bool IsGrounded { get; }
        bool IsOnWall { get; }
        bool FacingRight { get; }
        
        // Movement State Flags (for recording discrete actions)
        bool JustJumped { get; }
        bool JustDashed { get; }
        bool JustInteracted { get; }
        Vector2 LastDashDirection { get; }
        
        // Movement Control (for replay)
        void ApplyMovement(float movement);
        void ApplyJump(bool jump, bool jumpHeld);
        void ApplyDash(Vector2 direction);
        void ApplyInteraction(bool interact);
        
        // Physics Control (for perfect replay)
        void SetPosition(Vector3 position);
        void SetVelocity(Vector2 velocity);
        void SetExternalForce(Vector2 force);
        void SetFacingDirection(bool facingRight);
        
        // Events
        event System.Action OnCharacterStateChanged;
    }
}
```

#### IInputProvider Interface
```csharp
using UnityEngine;

namespace ActionRecording.Interfaces
{
    /// <summary>
    /// Interface for input providers that supply player input for action recording.
    /// Abstracts input system dependencies for flexibility across different input solutions.
    /// </summary>
    public interface IInputProvider
    {
        // Current Input State
        float Movement { get; }
        bool IsJumping { get; }
        bool JumpHeld { get; }
        bool IsDashing { get; }
        Vector2 DashDirection { get; }
        bool IsInteracting { get; }
        bool IsAttacking { get; }
        
        // Input Events
        event System.Action<float> OnMovementChanged;
        event System.Action OnJumpPressed;
        event System.Action OnJumpReleased;
        event System.Action<Vector2> OnDashPressed;
        event System.Action OnInteractPressed;
        event System.Action OnAttackPressed;
        
        // Input State Control (for disabling during replay)
        void SetInputEnabled(bool enabled);
        bool IsInputEnabled { get; }
    }
}
```

#### IActionReplayable Interface
```csharp
using System.Collections.Generic;
using UnityEngine;

namespace ActionRecording.Interfaces
{
    /// <summary>
    /// Interface for objects that can replay recorded actions.
    /// Used by clones and other replay systems to execute recorded player actions.
    /// </summary>
    public interface IActionReplayable
    {
        // Replay Control
        void StartReplay(List<PlayerAction> actions);
        void StopReplay();
        void PauseReplay();
        void ResumeReplay();
        void RestartReplay();
        
        // Replay State
        bool IsReplaying { get; }
        bool IsPaused { get; }
        bool IsStuck { get; }
        int CurrentActionIndex { get; }
        float ReplayProgress { get; } // 0-1
        
        // Action Execution
        void ExecuteAction(PlayerAction action);
        void SetStuck(bool stuck);
        
        // Events
        event System.Action<IActionReplayable> OnReplayStarted;
        event System.Action<IActionReplayable> OnReplayEnded;
        event System.Action<IActionReplayable> OnReplayPaused;
        event System.Action<IActionReplayable> OnReplayResumed;
        event System.Action<IActionReplayable> OnStuckStateChanged;
        event System.Action<IActionReplayable, PlayerAction> OnActionExecuted;
    }
}
```

#### IGoalSystem Interface
```csharp
using UnityEngine;

namespace ActionRecording.Interfaces
{
    /// <summary>
    /// Interface for goal systems that can interact with clones.
    /// Allows clones to become "stuck" when reaching goals without tight coupling.
    /// </summary>
    public interface IGoalSystem
    {
        bool CanCloneComplete(IActionReplayable clone, int cloneIndex);
        bool CanPlayerComplete(GameObject player);
        void CompleteGoal(IActionReplayable clone);
        void CompleteGoal(GameObject player);
        
        // Events
        event System.Action<IActionReplayable> OnCloneGoalCompleted;
        event System.Action<GameObject> OnPlayerGoalCompleted;
    }
}
```

### Step 3: Modify Core Classes for Interface Support

#### Modified ActionRecorder.cs
```csharp
using System.Collections.Generic;
using UnityEngine;
using ActionRecording.Interfaces;

namespace ActionRecording.Core
{
    /// <summary>
    /// Records player actions with physics-perfect accuracy for clone replay.
    /// Abstracts character controller and input dependencies through interfaces.
    /// </summary>
    public class ActionRecorder : MonoBehaviour
    {
        [Header("Recording Settings")]
        [SerializeField] private float recordingInterval = 0.02f; // 50 FPS
        [SerializeField] private float maxRecordingTime = 30f;
        
        // Interface Dependencies (configured automatically or manually)
        private ICharacterController characterController;
        private IInputProvider inputProvider;
        
        // Recording State
        private List<PlayerAction> recordedActions = new List<PlayerAction>();
        private bool isRecording = false;
        private float recordingStartTime;
        private float lastRecordTime;
        
        // Auto-Discovery of Dependencies
        private void Awake()
        {
            // Try to find components that implement our interfaces
            characterController = GetComponent<ICharacterController>() ?? 
                                 GetComponentInParent<ICharacterController>();
            inputProvider = GetComponent<IInputProvider>() ?? 
                           GetComponentInParent<IInputProvider>();
            
            if (characterController == null)
                Debug.LogError($"ActionRecorder: No ICharacterController found on {gameObject.name}");
            if (inputProvider == null)
                Debug.LogError($"ActionRecorder: No IInputProvider found on {gameObject.name}");
        }
        
        // Manual Dependency Injection (for advanced use cases)
        public void SetCharacterController(ICharacterController controller)
        {
            characterController = controller;
        }
        
        public void SetInputProvider(IInputProvider provider)
        {
            inputProvider = provider;
        }
        
        // Rest of the recording logic stays largely the same,
        // but uses interface methods instead of direct component access
        public void StartRecording()
        {
            if (characterController == null || inputProvider == null)
            {
                Debug.LogError("ActionRecorder: Missing required dependencies");
                return;
            }
            
            recordedActions.Clear();
            isRecording = true;
            recordingStartTime = Time.time;
            lastRecordTime = Time.time;
            
            // Record initial state
            RecordCurrentState();
            
            ActionRecordingEvents.OnRecordingStarted?.Invoke();
        }
        
        private void FixedUpdate()
        {
            if (isRecording && Time.time - lastRecordTime >= recordingInterval)
            {
                RecordCurrentState();
            }
        }
        
        private void RecordCurrentState()
        {
            if (characterController == null || inputProvider == null) return;
            
            float currentTime = Time.time - recordingStartTime;
            
            PlayerAction action = new PlayerAction(
                currentTime,
                inputProvider.Movement,
                characterController.JustJumped,
                characterController.JustDashed,
                characterController.LastDashDirection,
                characterController.JustInteracted,
                inputProvider.IsAttacking,
                characterController.Position,
                inputProvider.JumpHeld,
                characterController.Velocity,
                characterController.ExternalForce,
                characterController.IsGrounded,
                characterController.IsOnWall,
                characterController.FacingRight
            );
            
            recordedActions.Add(action);
            lastRecordTime = Time.time;
            
            ActionRecordingEvents.OnActionRecorded?.Invoke(action);
        }
        
        // Rest of the methods remain the same...
    }
}
```

### Step 4: Create Event System

#### ActionRecordingEvents.cs
```csharp
using System.Collections.Generic;
using ActionRecording.Interfaces;

namespace ActionRecording.Events
{
    /// <summary>
    /// Centralized event system for action recording and clone management.
    /// Provides loose coupling between system components.
    /// </summary>
    public static class ActionRecordingEvents
    {
        // Recording Events
        public static event System.Action OnRecordingStarted;
        public static event System.Action OnRecordingStopped;
        public static event System.Action<PlayerAction> OnActionRecorded;
        
        // Clone Lifecycle Events
        public static event System.Action<IActionReplayable> OnCloneCreated;
        public static event System.Action<IActionReplayable> OnCloneDestroyed;
        public static event System.Action<IActionReplayable> OnCloneStuck;
        public static event System.Action<IActionReplayable> OnCloneUnstuck;
        
        // Loop Management Events
        public static event System.Action OnLoopStarted;
        public static event System.Action OnLoopEnded;
        public static event System.Action<float> OnLoopProgress; // 0-1 progress
        
        // System Events
        public static event System.Action<int> OnMaxClonesReached;
        public static event System.Action<IActionReplayable> OnOldestCloneRemoved;
        
        // Performance Events
        public static event System.Action<int> OnCloneCountChanged;
        public static event System.Action<float> OnRecordingMemoryUsage;
    }
}
```

### Step 5: Create Configuration System

#### ActionRecordingConfig.cs
```csharp
using UnityEngine;

namespace ActionRecording.Configuration
{
    /// <summary>
    /// Configuration settings for the action recording system.
    /// Provides centralized configuration with validation and defaults.
    /// </summary>
    [CreateAssetMenu(fileName = "ActionRecordingConfig", menuName = "Action Recording/Configuration")]
    public class ActionRecordingConfig : ScriptableObject
    {
        [Header("Recording Settings")]
        [Tooltip("Recording frequency in seconds (0.02 = 50 FPS)")]
        [Range(0.01f, 0.1f)]
        public float recordingInterval = 0.02f;
        
        [Tooltip("Maximum recording duration to prevent memory issues")]
        [Range(10f, 120f)]
        public float maxRecordingTime = 30f;
        
        [Tooltip("Automatically start recording when ActionRecorder initializes")]
        public bool autoStartRecording = true;
        
        [Header("Clone Management")]
        [Tooltip("Maximum number of simultaneous clones")]
        [Range(1, 50)]
        public int maxClones = 10;
        
        [Tooltip("Duration of automatic loops in seconds")]
        [Range(5f, 60f)]
        public float loopDuration = 15f;
        
        [Tooltip("Enable manual clone creation (L key by default)")]
        public bool enableManualCloning = true;
        
        [Tooltip("Key for manual clone creation")]
        public KeyCode manualCloneKey = KeyCode.L;
        
        [Header("Performance")]
        [Tooltip("Enable object pooling for clones")]
        public bool enableObjectPooling = true;
        
        [Tooltip("Size of clone object pool")]
        [Range(5, 100)]
        public int poolSize = 20;
        
        [Tooltip("Automatically cleanup null clone references")]
        public bool autoCleanupNullReferences = true;
        
        [Header("Visual Effects")]
        [Tooltip("Enable particle effects for clone lifecycle")]
        public bool enableParticleEffects = true;
        
        [Tooltip("Enable audio effects for clone events")]
        public bool enableAudioEffects = true;
        
        [Tooltip("Enable ghost trail effects for clones")]
        public bool enableGhostTrails = false;
        
        [Header("Debug")]
        [Tooltip("Enable debug logging for development")]
        public bool enableDebugLogging = false;
        
        [Tooltip("Show debug gizmos in scene view")]
        public bool showDebugGizmos = true;
        
        private void OnValidate()
        {
            // Validate settings and provide warnings
            if (recordingInterval * maxRecordingTime > 6000) // 6000 frames
            {
                Debug.LogWarning("ActionRecordingConfig: Very high frame count detected. Consider reducing maxRecordingTime or increasing recordingInterval.");
            }
            
            if (maxClones > 20)
            {
                Debug.LogWarning("ActionRecordingConfig: High clone count may impact performance.");
            }
        }
    }
}
```

### Step 6: Create Package Manifest

#### package.json
```json
{
  "name": "com.yourdomain.action-recording",
  "version": "1.0.0",
  "displayName": "Action Recording System",
  "description": "Physics-perfect action recording and replay system for Unity. Create time-loop mechanics, ghost players, AI demonstrations, and replay systems with frame-accurate recording.",
  "unity": "2022.3",
  "keywords": [
    "action-recording",
    "replay-system", 
    "time-loop",
    "clone-system",
    "physics-replay",
    "ghost-player"
  ],
  "author": {
    "name": "Your Name",
    "email": "your.email@domain.com",
    "url": "https://your-website.com"
  },
  "dependencies": {
    "com.unity.inputsystem": "1.7.0",
    "com.unity.mathematics": "1.2.6"
  },
  "samples": [
    {
      "displayName": "Basic Example",
      "description": "Simple scene demonstrating basic action recording and clone replay",
      "path": "Samples~/BasicExample"
    },
    {
      "displayName": "Advanced Integration",
      "description": "Complex example showing integration with custom character controllers and goal systems",
      "path": "Samples~/AdvancedIntegration"
    }
  ]
}
```

### Step 7: Create Assembly Definitions

#### Runtime/ActionRecording.asmdef
```json
{
    "name": "ActionRecording",
    "rootNamespace": "ActionRecording",
    "references": [
        "Unity.InputSystem",
        "Unity.Mathematics"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [
        {
            "name": "com.unity.inputsystem",
            "expression": "1.7.0",
            "define": "ACTION_RECORDING_INPUT_SYSTEM"
        }
    ],
    "noEngineReferences": false
}
```

## 🧪 Testing Strategy

### Create Unit Tests

#### Tests/Runtime/ActionRecordingTests.cs
```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using ActionRecording.Core;
using ActionRecording.Interfaces;

namespace ActionRecording.Tests
{
    public class ActionRecordingTests
    {
        private GameObject testObject;
        private ActionRecorder recorder;
        private MockCharacterController mockController;
        private MockInputProvider mockInput;
        
        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestActionRecorder");
            recorder = testObject.AddComponent<ActionRecorder>();
            mockController = testObject.AddComponent<MockCharacterController>();
            mockInput = testObject.AddComponent<MockInputProvider>();
            
            recorder.SetCharacterController(mockController);
            recorder.SetInputProvider(mockInput);
        }
        
        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(testObject);
        }
        
        [Test]
        public void StartRecording_ShouldInitializeCorrectly()
        {
            recorder.StartRecording();
            
            Assert.IsTrue(recorder.IsRecording);
            Assert.AreEqual(0, recorder.ActionCount);
        }
        
        [UnityTest]
        public IEnumerator RecordActions_ShouldCaptureInputOverTime()
        {
            recorder.StartRecording();
            
            // Simulate some input
            mockInput.SetMovement(1.0f);
            mockController.SetPosition(Vector3.right);
            
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            
            Assert.Greater(recorder.ActionCount, 0);
        }
        
        // Additional tests for edge cases, performance, etc.
    }
    
    // Mock implementations for testing
    public class MockCharacterController : MonoBehaviour, ICharacterController
    {
        public Vector3 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        // ... implement interface
        
        public void SetPosition(Vector3 position) => Position = position;
        // ... rest of mock implementation
    }
}
```

## 📚 Documentation Templates

### README.md Template
```markdown
# Action Recording System

A physics-perfect action recording and replay system for Unity that enables time-loop mechanics, ghost players, AI demonstrations, and replay systems with frame-accurate recording.

## Features

- **Physics-Perfect Replay**: 50Hz recording matches Unity's FixedUpdate for deterministic behavior
- **Interface-Based Design**: Easily integrate with any character controller or input system
- **Event-Driven Architecture**: Loose coupling through comprehensive event system
- **Memory Efficient**: Optimized data structures with automatic cleanup
- **Configurable**: Extensive configuration options for different use cases

## Quick Start

1. Install via Package Manager: `com.yourdomain.action-recording`
2. Add ActionRecorder to your player GameObject
3. Implement ICharacterController and IInputProvider interfaces
4. Add CloneManager to your scene
5. Configure settings and play!

[View Full Documentation](Documentation~/setup-guide.md)

## Requirements

- Unity 2022.3 or later
- Unity Input System package
- Character controller that implements ICharacterController
- Input provider that implements IInputProvider

## License

MIT License - see LICENSE.md for details
```

## 🚀 Implementation Checklist

### Core Extraction
- [ ] Copy core system files to package structure
- [ ] Create interface abstractions for dependencies
- [ ] Implement event system for loose coupling
- [ ] Create configuration system with validation
- [ ] Set up proper assembly definitions

### Testing and Validation
- [ ] Create unit tests for core functionality
- [ ] Create integration tests with mock implementations
- [ ] Test performance with multiple clones
- [ ] Validate memory usage and cleanup
- [ ] Test cross-platform compatibility

### Documentation
- [ ] Write comprehensive API documentation
- [ ] Create setup and integration guides
- [ ] Build example scenes and sample code
- [ ] Document common use cases and patterns
- [ ] Create troubleshooting guides

### Package Preparation
- [ ] Create proper package.json manifest
- [ ] Set up semantic versioning
- [ ] Prepare sample projects
- [ ] Create changelog and license files
- [ ] Test package installation process

### Distribution
- [ ] Test package via Package Manager
- [ ] Create GitHub releases with .unitypackage files
- [ ] Submit to Unity Asset Store (optional)
- [ ] Set up community support channels
- [ ] Create showcase projects

This extraction guide provides a complete roadmap for transforming the Clone/Action Recording System into a professional, reusable Unity package while maintaining all its functionality and improving its flexibility through interface-based design.