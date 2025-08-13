# Build Instructions

This guide explains how to build **You Did This** for different platforms.

## 🎯 Supported Platforms

The game is currently configured and tested for:
- **Windows** (Primary platform)
- **macOS** (Intel and Apple Silicon)
- **Linux** (Ubuntu/SteamOS)
- **WebGL** (Browser play)

## 🛠️ Build Requirements

### Unity Version
- **Required**: Unity 6000.2.0b9 or later
- **Recommended**: Use Unity Hub for consistent version management

### Platform Dependencies
- **Windows**: Windows 10 SDK (installed with Unity)
- **macOS**: Xcode (for code signing and native libraries)
- **Linux**: No additional requirements
- **WebGL**: Modern browser with WebAssembly support

## 📦 Creating Builds

### Quick Build Process

1. **Open Project**
   ```
   - Launch Unity Hub
   - Open the GMTK2025 project
   - Ensure all assets are imported correctly
   ```

2. **Build Settings**
   ```
   - File → Build Settings
   - Select target platform
   - Add scenes if not already present
   - Configure platform-specific settings
   ```

3. **Build**
   ```
   - Click "Build" or "Build and Run"
   - Choose output directory
   - Wait for build completion
   ```

### Platform-Specific Instructions

#### Windows Build
```
1. File → Build Settings
2. Select "Windows, Mac, Linux" platform
3. Target Platform: Windows
4. Architecture: x86_64 (recommended)
5. Set compression: LZ4HC (good balance of size/speed)
6. Click "Build"
```

**Output**: Standalone .exe file with data folder

#### macOS Build
```
1. File → Build Settings
2. Select "Windows, Mac, Linux" platform  
3. Target Platform: macOS
4. Architecture: Universal (Intel + Apple Silicon)
5. Click "Build"
```

**Output**: .app bundle for macOS

#### Linux Build
```
1. File → Build Settings
2. Select "Windows, Mac, Linux" platform
3. Target Platform: Linux
4. Architecture: x86_64
5. Click "Build"
```

**Output**: Executable binary with data folder

#### WebGL Build
```
1. File → Build Settings
2. Select "WebGL" platform
3. Compression Format: Gzip (best browser compatibility)
4. Click "Build"
5. Host resulting files on web server
```

**Output**: Web-ready files for browser deployment

## ⚙️ Build Configuration

### Player Settings
Key settings in Edit → Project Settings → Player:

#### General Settings
- **Product Name**: You Did This
- **Version**: 1.0.0 (or current version)
- **Company Name**: blink

#### Resolution Settings
- **Default Resolution**: 1920x1080 (16:9)
- **Windowed Mode**: Enabled
- **Resizable Window**: Enabled
- **Fullscreen Mode**: Windowed (allows easy testing)

#### Input Settings
- **Active Input Handling**: Input System Package (New)
- **Legacy Input Support**: Disabled

### Quality Settings
Recommended settings (Edit → Project Settings → Quality):

#### Performance Profile
- **Rendering Pipeline**: Universal Render Pipeline
- **Texture Quality**: Full Res
- **Anisotropic Textures**: Per Texture
- **Anti Aliasing**: 2x Multi Sampling

## 🔧 Optimization for Distribution

### Build Size Optimization
```
1. Player Settings → Publishing Settings
2. Enable "Strip Engine Code"
3. Scripting Backend: IL2CPP (better performance)
4. Api Compatibility Level: .NET Standard 2.1
5. Remove unused assets before building
```

### Performance Optimization
```
1. Quality Settings → Use appropriate quality level
2. Player Settings → Scripting Backend: IL2CPP
3. Player Settings → Configuration: Release
4. Remove debug and development features
```

### Asset Optimization
```
1. Compress textures appropriately
2. Optimize audio files (Ogg Vorbis for most)
3. Remove development-only assets
4. Use asset bundles for large content (if needed)
```

## 📋 Build Verification Checklist

### Pre-Build Testing
- [ ] Game runs correctly in Unity editor
- [ ] No console errors or warnings
- [ ] All scenes load properly
- [ ] Clone system functions correctly
- [ ] Audio plays correctly
- [ ] Input system responds to all controls

### Post-Build Testing
- [ ] Game launches without errors
- [ ] Main menu appears (if present)
- [ ] All levels accessible
- [ ] Clone creation and replay working
- [ ] Goal system functioning
- [ ] Audio and visual feedback present
- [ ] Performance acceptable on target hardware

### Distribution Checklist
- [ ] Include README or quick start guide
- [ ] Test on clean machine without Unity
- [ ] Verify all dependencies included
- [ ] Check file size is reasonable
- [ ] Test installer (if using one)

## 🚀 Deployment Options

### Direct Distribution
- **Standalone**: Distribute executable + data folder
- **Archive**: ZIP/TAR the entire build folder
- **Installer**: Use tools like NSIS (Windows) or create DMG (macOS)

### Platform Stores
- **Steam**: Requires Steamworks SDK integration
- **Itch.io**: Direct upload of build files
- **Game Jam Platforms**: Usually ZIP upload

### Web Deployment
For WebGL builds:
```
1. Upload build files to web server
2. Ensure proper MIME types set
3. Configure HTTPS for modern browsers
4. Test in multiple browsers
```

## 🐛 Common Build Issues

### Build Errors
**"Scripts have compile errors"**
- Fix all compiler errors before building
- Check console for specific error messages

**"Build target not supported"**
- Install required platform support modules
- Unity Hub → Installs → Add Modules

**"Out of memory during build"**
- Close other applications
- Increase Unity's memory allocation
- Build with fewer assets if possible

### Runtime Issues
**"DLL missing" errors**
- Ensure all required libraries included
- Check Player Settings → Configuration

**Input not working**
- Verify Input System package installed
- Check Player Settings → Active Input Handling

**Audio not playing**
- Confirm audio files included in build
- Check Audio Mixer settings

## 📊 Build Performance

### Typical Build Times
- **Development Build**: 2-5 minutes
- **Release Build**: 5-15 minutes
- **WebGL Build**: 10-30 minutes (due to compression)

### Build Size Expectations
- **Windows**: 50-100 MB
- **WebGL**: 20-50 MB compressed
- **macOS**: 60-120 MB
- **Linux**: 50-100 MB

## 🔄 Continuous Integration

For automated builds (advanced):
```
1. Use Unity Cloud Build or GitHub Actions
2. Configure build triggers on commit/tag
3. Automate testing with Unity Test Framework
4. Deploy builds automatically to distribution platforms
```

---

**Need Help?** Check the documentation or create an issue on GitHub for build-specific problems.