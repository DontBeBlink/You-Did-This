# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-01-XX

### Added

- **Core Action Recording System**: Complete physics-perfect action recording and replay
  - `ActionRecorder` component for capturing player input and physics states
  - `PlayerAction` struct for frame-perfect action data storage
  - 50 FPS recording rate for optimal accuracy and performance

- **Clone Replay System**: Advanced clone creation and management
  - `ActionReplayClone` component for physics-perfect action reproduction
  - `ActionRecordingManager` for coordinating recording loops and clone lifecycle
  - Automatic and manual loop creation with configurable duration

- **Interface Abstractions**: Flexible system design for maximum reusability
  - `ICharacterController` interface for character controller abstraction
  - `IInteractionSystem` interface for interaction system abstraction
  - Support for any character controller implementation

- **Adapter Components**: Seamless integration with existing systems
  - `CharacterController2DAdapter` for GMTK2025 CharacterController2D
  - `InteractSystemAdapter` for GMTK2025 InteractSystem
  - Reflection-based access to private fields for complete state capture

- **Visual Effects System**: Comprehensive clone visual feedback
  - `ActionReplayEffects` component for unified effects management
  - Ghost trail system with configurable transparency and lifetime
  - Particle effects for spawn, ambient, and stuck states
  - Glow effects using Unity lighting system
  - Sound effects for clone actions and state changes

- **Event System**: Complete integration hooks for external systems
  - Clone creation, completion, and stuck events
  - Loop start and end events
  - Progress tracking and state monitoring

- **Unity Package Manager Support**: Professional package structure
  - Proper assembly definitions for runtime, editor, and tests
  - Comprehensive documentation and examples
  - Unity 2022.3 LTS compatibility

### Features

- **Physics-Perfect Recording**: Captures input states, position, velocity, and external forces
- **Loop Management**: Automatic duration-based loops or manual triggering
- **Memory Optimization**: Configurable recording limits and automatic cleanup
- **Visual Customization**: Configurable clone appearance, effects, and transparency
- **Audio Integration**: Sound effects for clone actions and state transitions
- **Error Handling**: Comprehensive validation and graceful degradation
- **Performance Optimization**: Efficient update loops and memory management

### Technical Details

- **Recording Rate**: 50 FPS (0.02s intervals) for physics alignment
- **Memory Usage**: ~50 bytes per action frame
- **Dependencies**: Unity Input System only
- **Platform Support**: All Unity-supported platforms
- **Architecture**: Interface-first design with adapter pattern

### Documentation

- Comprehensive README with quick start guide
- API reference for all public components
- Integration examples for common use cases
- Performance considerations and best practices
- Custom implementation guides for interfaces

---

## Extraction Notes

This package was extracted from the GMTK2025 "You Did This" project, where it was successfully used to implement sophisticated time-loop puzzle mechanics. The system has been battle-tested in a complete game and refactored for maximum reusability.

### Original System Features Preserved

- Physics-perfect action recording with frame-accurate timing
- Sophisticated clone management with visual and audio effects
- Integration with character controllers and interaction systems
- Loop-based gameplay mechanics with automatic and manual triggers
- Visual effects including ghost trails, particles, and glow effects

### Improvements in Package Version

- Interface abstractions eliminate hard dependencies on specific controllers
- Adapter pattern allows integration with existing systems without modification
- Unified effects management simplifies customization
- Professional package structure with proper assembly definitions
- Comprehensive documentation and examples
- Improved error handling and validation
- Performance optimizations and memory management

### Migration from Original

For projects using the original GMTK2025 implementation:

1. Install the package through Unity Package Manager
2. Add adapter components to existing GameObjects
3. Replace `CloneManager` with `ActionRecordingManager`
4. Replace `Clone` with `ActionReplayClone`
5. Update event subscriptions to use new event names
6. Configure visual effects through the new unified system

The adapters ensure 100% compatibility with existing character controllers and interaction systems.