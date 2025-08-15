# You Did This - Settings Interface

A web-based configuration interface for managing game settings including ntfy notifications, Whisper model configuration, and calendar token management.

## Features

- **📢 Ntfy Notifications**: Configure notification server URL and topic for game notifications
- **🎤 Whisper Model Settings**: Configure speech recognition model, language, and processing device
- **📅 Calendar Integration**: Generate and manage ICS calendar tokens for game event feeds
- **💾 Persistent Storage**: Settings persist in both SQLite database and localStorage
- **🔄 Offline Support**: Works offline using localStorage when server is unavailable
- **🧪 Test Functions**: Test notifications and validate configurations

## Quick Start

### 1. Install Dependencies

```bash
cd server
pip install -r requirements.txt
```

### 2. Start the Server

```bash
cd server
python main.py
```

The server will start on `http://localhost:5000` by default.

### 3. Access Settings Interface

Open your browser and navigate to `http://localhost:5000` to access the settings interface.

## Configuration Options

### Ntfy Notifications
- **Server URL**: The ntfy server endpoint (default: https://ntfy.sh)
- **Topic**: Unique topic name for receiving notifications

### Whisper Model
- **Model Size**: Choose from tiny, base, small, medium, or large based on performance needs
- **Language**: Specify language for better accuracy or use auto-detect
- **Device**: Select CPU, CUDA (NVIDIA GPU), or MPS (Apple Silicon)

### Calendar Integration
- **ICS Token**: Unique token for accessing calendar feeds
- **Token Management**: Generate new tokens and copy existing ones

## API Endpoints

### GET /settings
Retrieve all current settings.

**Response:**
```json
{
  "success": true,
  "settings": {
    "ntfy_topic": "my-topic",
    "ntfy_url": "https://ntfy.sh",
    "whisper_model": "base",
    "whisper_language": "auto",
    "whisper_device": "cpu",
    "ics_token": "uuid-token-here"
  }
}
```

### POST /settings
Update multiple settings.

**Request:**
```json
{
  "ntfy_topic": "new-topic",
  "whisper_model": "small"
}
```

### POST /settings/generate-ics-token
Generate a new ICS calendar token.

**Request:**
```json
{
  "description": "New token for calendar access"
}
```

### POST /settings/test-ntfy
Send a test notification via ntfy.

**Request:**
```json
{
  "ntfy_url": "https://ntfy.sh",
  "ntfy_topic": "test-topic"
}
```

## Environment Variables

- `PORT`: Server port (default: 5000)
- `DEBUG`: Enable debug mode (default: False)

## Storage

Settings are stored in:
1. **SQLite Database**: `server/settings.db` for server-side persistence
2. **localStorage**: Browser local storage for offline access

## Offline Mode

When the server is unavailable, the interface automatically falls back to localStorage:
- Settings are saved locally
- ICS tokens use fallback generation
- Status indicator shows offline mode
- All functionality remains available

## Development

### Project Structure
```
server/
├── main.py           # Flask server with API endpoints
├── storage.py        # SQLite database management
└── requirements.txt  # Python dependencies

web/
├── settings.html     # Main settings interface
└── js/
    └── api.js       # Frontend API client
```

### Adding New Settings

1. Add default value to `DEFAULT_SETTINGS` in `main.py`
2. Add form field to `settings.html`
3. Update `loadSettings()` and `saveSettings()` JavaScript functions
4. Add validation if needed

## Integration with Unity Game

The settings interface is designed to work alongside the Unity game project. Settings can be:

- Read by Unity scripts via HTTP requests to the Flask server
- Shared through configuration files
- Integrated with game notification systems
- Used for voice recognition and calendar features

## Security Notes

- ICS tokens are unique UUIDs for access control
- No sensitive data is stored in localStorage
- Server runs locally by default for security
- CORS is enabled for localhost development