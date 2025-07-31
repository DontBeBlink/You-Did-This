# Contributing to You Did This

Thank you for your interest in contributing to **You Did This**! This document provides guidelines and information for contributors to help maintain code quality and project consistency.

## 🎯 Ways to Contribute

### 🐛 Bug Reports
- Use GitHub Issues to report bugs
- Include detailed reproduction steps
- Specify Unity version and platform
- Attach relevant console logs or screenshots

### 💡 Feature Suggestions
- Propose new puzzle mechanics or game features
- Suggest improvements to existing systems
- Discuss ideas in GitHub Discussions before implementation

### 🔧 Code Contributions
- Bug fixes and performance improvements
- New puzzle elements and mechanics
- Quality of life improvements
- Documentation enhancements

### 🎨 Level Design
- Create new puzzle levels
- Design innovative clone-based challenges
- Test and balance existing levels

## 🚀 Getting Started

### Development Environment Setup

1. **Unity Installation**
   ```
   Required: Unity 6000.2.0b9 or later
   Recommended: Unity Hub for version management
   ```

2. **Fork and Clone**
   ```bash
   # Fork the repository on GitHub
   git clone https://github.com/YOUR_USERNAME/GMTK2025.git
   cd GMTK2025
   ```

3. **Branch Strategy**
   ```bash
   # Create feature branch from main
   git checkout main
   git pull origin main
   git checkout -b feature/your-feature-name
   ```

### First-Time Setup

1. Open the project in Unity
2. Let Unity import all assets (may take a few minutes)
3. Open `Assets/Scenes/DemoScene.unity` to test basic functionality
4. Press F1 in Play mode to view debug information
5. Verify clone system works by pressing L to create manual clones

## 📋 Development Guidelines

### Code Style

#### C# Conventions
```csharp
// Use PascalCase for public members
public class CloneManager : MonoBehaviour
{
    // Use camelCase for private fields
    private float loopDuration = 15f;
    
    // Use descriptive names and clear documentation
    /// <summary>
    /// Creates a new clone from recorded player actions
    /// </summary>
    public void CreateClone()
    {
        // Implementation here
    }
}
```

#### Unity-Specific Guidelines
- **SerializeField**: Use `[SerializeField]` for private fields that need inspector access
- **Headers**: Group related inspector fields with `[Header("Section Name")]`
- **Tooltips**: Add helpful tooltips for designer-facing parameters
- **Components**: Use `RequireComponent` attribute when dependencies exist

### System Design Principles

#### Clone System Integration
- All new mechanics should work with the clone replay system
- Consider how recorded actions will be replayed by clones
- Test interactions between multiple clones and new features

#### Performance Considerations
- Optimize for multiple simultaneous clones (up to 10 by default)
- Use object pooling for frequently created/destroyed objects
- Profile physics interactions with multiple characters

#### Modularity
- Keep systems loosely coupled
- Use events for cross-system communication
- Make components easily configurable in the inspector

### Testing Requirements

#### Manual Testing Checklist
- [ ] Basic player movement and controls work
- [ ] Clone creation and replay functions correctly
- [ ] New features work with existing clone system
- [ ] No console errors or warnings
- [ ] Performance remains stable with 10 active clones

#### Automated Testing
Currently, the project relies on manual testing. Contributions to add automated testing are welcome!

## 🎮 Level Design Guidelines

### Creating New Levels

1. **Scene Setup**
   ```
   - Copy existing scene as template
   - Add CloneManager component to scene
   - Place player spawn point
   - Configure camera bounds
   ```

2. **Puzzle Design Principles**
   - Start with simple clone coordination
   - Gradually introduce timing elements
   - Provide clear visual feedback for goals
   - Test with multiple solution paths

3. **Required Components**
   - **Player Spawn**: CloneManager position defines spawn point
   - **Goals**: At least one Goal component for puzzle completion
   - **Camera**: CameraController for player following

### Level Testing
- Test puzzle can be solved in intended way
- Verify no unintended solutions break the puzzle
- Confirm appropriate difficulty progression
- Check clone replay works consistently

## 🔧 Core Systems Documentation

### Clone System Architecture

#### Key Components
- **CloneManager**: Central controller for clone lifecycle
- **ActionRecorder**: Records player input and physics state
- **Clone**: Individual clone behavior and replay logic

#### Recording System
```csharp
// Actions recorded every FixedUpdate (50Hz)
public struct PlayerAction
{
    public float timestamp;
    public Vector3 position;
    public Vector2 speed;
    public bool isJumping;
    public bool isDashing;
    // ... other input states
}
```

#### Integration Points
New systems should integrate at these points:
- **PlayerController**: Record new input types
- **CharacterController2D**: Ensure physics state recording
- **ActionRecorder**: Add new action types to PlayerAction struct
- **Clone**: Add replay logic for new actions

### Goal System
Goals trigger puzzle completion and clone sticking:
```csharp
// Goals can be general or clone-specific
[SerializeField] private bool requiresSpecificClone = false;
[SerializeField] private int requiredCloneIndex = -1;
```

### Audio Integration
Use AudioManager for consistent sound design:
```csharp
// Play sounds through centralized manager
AudioManager.Instance.PlaySound(audioClip);
```

## 📝 Pull Request Process

### Before Submitting

1. **Code Quality**
   - Follow established code style
   - Add appropriate comments and documentation
   - Remove debug code and temporary assets

2. **Testing**
   - Test changes in multiple scenes
   - Verify clone system integration
   - Check for console errors/warnings

3. **Documentation**
   - Update relevant documentation files
   - Add comments for complex systems
   - Include usage examples for new features

### PR Template

```markdown
## Description
Brief description of changes and motivation

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Level design
- [ ] Documentation update
- [ ] Performance improvement

## Testing
- [ ] Tested in DemoScene
- [ ] Verified clone system compatibility
- [ ] No console errors
- [ ] Performance impact assessed

## Screenshots/Videos
Include visual changes or new level gameplay

## Additional Notes
Any special considerations or known limitations
```

### Review Process

1. **Automated Checks**: Code formatting and basic validation
2. **Manual Review**: Code quality, design consistency, documentation
3. **Testing**: Functionality verification in Unity
4. **Approval**: Merge after review completion

## 🎯 Priority Areas for Contribution

### High Priority
- **Performance Optimization**: Multi-clone scenarios
- **Level Design**: Creative puzzle mechanics
- **Polish**: Visual and audio improvements
- **Documentation**: Code comments and system guides

### Medium Priority
- **Quality of Life**: Better debug tools, editor improvements
- **Extended Features**: New puzzle elements, mechanics
- **Platform Support**: Different input devices, platforms

### Future Considerations
- **Automated Testing**: Unit tests for core systems
- **Level Editor**: In-game level creation tools
- **Community Features**: Level sharing, leaderboards

## 📞 Communication

### Getting Help
- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: Design discussions and questions
- **Code Comments**: Ask questions directly in PR reviews

### Community Guidelines
- Be respectful and constructive in all interactions
- Provide detailed feedback and suggestions
- Help others learn and improve their contributions
- Keep discussions focused and productive

## 📄 License and Legal

By contributing to this project, you agree that your contributions will be licensed under the same license as the project. See [LICENSE.md](LICENSE.md) for details.

### Attribution
Contributors will be acknowledged in project documentation and release notes.

---

Thank you for contributing to **You Did This**! Your efforts help make this puzzle platformer even better for everyone. 🎮✨