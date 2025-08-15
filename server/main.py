#!/usr/bin/env python3
"""
Settings server for You Did This game configuration.
Provides REST API for managing ntfy notifications, Whisper model settings, and calendar tokens.
"""

from flask import Flask, request, jsonify, render_template, send_from_directory
from flask_cors import CORS
import requests
import os
from pathlib import Path

from storage import SettingsStorage

app = Flask(__name__)
CORS(app)  # Enable CORS for web frontend

# Initialize storage
storage = SettingsStorage()

# Default settings
DEFAULT_SETTINGS = {
    'ntfy_topic': '',
    'ntfy_url': 'https://ntfy.sh',
    'whisper_model': 'base',
    'whisper_language': 'auto',
    'whisper_device': 'cpu'
}

@app.route('/')
def index():
    """Serve the settings page."""
    return send_from_directory('../web', 'settings.html')

@app.route('/js/<path:filename>')
def serve_js(filename):
    """Serve JavaScript files."""
    return send_from_directory('../web/js', filename)

@app.route('/settings', methods=['GET'])
def get_settings():
    """Get all current settings."""
    try:
        settings = storage.get_all_settings()
        
        # Ensure all default settings are present
        for key, default_value in DEFAULT_SETTINGS.items():
            if key not in settings:
                settings[key] = default_value
        
        # Add ICS token
        settings['ics_token'] = storage.get_ics_token()
        
        return jsonify({
            'success': True,
            'settings': settings
        })
    except Exception as e:
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@app.route('/settings', methods=['POST'])
def update_settings():
    """Update settings from the frontend."""
    try:
        data = request.get_json()
        
        if not data:
            return jsonify({
                'success': False,
                'error': 'No data provided'
            }), 400
        
        # Filter out ICS token from updates (it's read-only via generation)
        settings_to_update = {k: v for k, v in data.items() if k != 'ics_token'}
        
        # Update settings in storage
        storage.update_settings(settings_to_update)
        
        return jsonify({
            'success': True,
            'message': 'Settings updated successfully'
        })
    except Exception as e:
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@app.route('/settings/generate-ics-token', methods=['POST'])
def generate_ics_token():
    """Generate a new ICS calendar token."""
    try:
        description = request.get_json().get('description', 'Generated ICS token') if request.get_json() else 'Generated ICS token'
        token = storage.generate_ics_token(description)
        
        return jsonify({
            'success': True,
            'token': token
        })
    except Exception as e:
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@app.route('/settings/test-ntfy', methods=['POST'])
def test_ntfy():
    """Send a test notification via ntfy."""
    try:
        data = request.get_json()
        ntfy_url = data.get('ntfy_url') or storage.get_setting('ntfy_url', DEFAULT_SETTINGS['ntfy_url'])
        ntfy_topic = data.get('ntfy_topic') or storage.get_setting('ntfy_topic')
        
        if not ntfy_topic:
            return jsonify({
                'success': False,
                'error': 'No ntfy topic configured'
            }), 400
        
        # Construct the full URL
        full_url = f"{ntfy_url.rstrip('/')}/{ntfy_topic}"
        
        # Send test notification
        response = requests.post(
            full_url,
            data='Test notification from You Did This settings interface! 🎮',
            headers={
                'Content-Type': 'text/plain',
                'Title': 'You Did This - Test Notification',
                'Tags': 'video_game,test'
            },
            timeout=10
        )
        
        if response.status_code == 200:
            return jsonify({
                'success': True,
                'message': 'Test notification sent successfully!'
            })
        else:
            return jsonify({
                'success': False,
                'error': f'Failed to send notification: HTTP {response.status_code}'
            }), 500
            
    except requests.exceptions.RequestException as e:
        return jsonify({
            'success': False,
            'error': f'Network error: {str(e)}'
        }), 500
    except Exception as e:
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@app.route('/health', methods=['GET'])
def health_check():
    """Health check endpoint."""
    return jsonify({
        'success': True,
        'status': 'healthy',
        'service': 'You Did This Settings Server'
    })

if __name__ == '__main__':
    port = int(os.environ.get('PORT', 5000))
    debug = os.environ.get('DEBUG', 'False').lower() == 'true'
    
    print(f"Starting You Did This Settings Server on port {port}")
    print(f"Access the settings interface at: http://localhost:{port}")
    
    app.run(host='0.0.0.0', port=port, debug=debug)