# Changelog

All notable changes to the Interaction System package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-01-XX

### Added
- **Complete Interaction System Package** extracted from GMTK2025 project
- **Interface-based architecture** for maximum flexibility and reusability
- **Core Components**:
  - `InteractionController` - Main interaction system with proximity detection
  - `InteractableObject` - Base implementation for interactive objects  
  - `PickupableObject` - Physics-based pickup and carry mechanics
  - `InteractionFeedbackUI` - Visual feedback with input device detection
  - `CharacterController2DAdapter` - Seamless integration with existing controllers

- **Key Interfaces**:
  - `IInteractable` - Standard interface for interactive objects
  - `IPickupable` - Interface for objects that can be picked up
  - `IInteractionController` - Interface for interaction system controllers
  - `ICharacterVelocityProvider` - Interface for velocity information
  - `ICharacterFacingProvider` - Interface for facing direction information

- **Features**:
  - Proximity-based object detection with configurable range
  - Priority-based interaction selection for multiple objects
  - Visual feedback system with automatic input device icon switching
  - Complete pickup/carry/throw mechanics with physics integration
  - Event system for external integration
  - Debug visualization with Scene view gizmos
  - Performance optimization with configurable object limits

- **Sample Content**:
  - Complete example scene demonstrating all features
  - Example scripts showing custom interaction implementations
  - Pre-configured prefabs for common interaction types
  - Comprehensive documentation with integration examples

- **Editor Support**:
  - Assembly definitions for proper compilation order
  - Custom property names for adapter integration
  - Visual debugging tools and gizmos

- **Documentation**:
  - Comprehensive README with usage examples
  - Complete API reference for all interfaces and classes
  - Integration guides for existing projects
  - Migration guide from original GMTK2025 InteractSystem
  - Troubleshooting guide and performance tips

### Technical Details
- **Unity Version**: 2022.3 LTS compatibility
- **Dependencies**: Zero external dependencies (optional Input System for device detection)
- **Assembly Definitions**: Proper runtime and editor assembly separation
- **Package Structure**: Full Unity Package Manager compliance
- **Performance**: Optimized detection with configurable limits and caching

### Integration Support
- Works seamlessly with Action Recording System for replay functionality
- Compatible with Audio Management System for interaction sound effects
- Flexible adapter system for any existing character controller
- Event-driven architecture for loose coupling with other systems