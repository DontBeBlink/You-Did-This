import sqlite3
import json
import uuid
from datetime import datetime
from pathlib import Path

class SettingsStorage:
    def __init__(self, db_path="settings.db"):
        self.db_path = Path(db_path)
        self.init_database()
    
    def init_database(self):
        """Initialize the SQLite database with required tables."""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        # Create settings table
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS settings (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                key TEXT UNIQUE NOT NULL,
                value TEXT NOT NULL,
                updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )
        ''')
        
        # Create tokens table for ICS tokens
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS tokens (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                token TEXT UNIQUE NOT NULL,
                type TEXT NOT NULL DEFAULT 'ics',
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                description TEXT
            )
        ''')
        
        conn.commit()
        conn.close()
    
    def get_setting(self, key, default=None):
        """Get a setting value by key."""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('SELECT value FROM settings WHERE key = ?', (key,))
        result = cursor.fetchone()
        conn.close()
        
        if result:
            try:
                return json.loads(result[0])
            except json.JSONDecodeError:
                return result[0]
        return default
    
    def set_setting(self, key, value):
        """Set a setting value by key."""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        json_value = json.dumps(value) if not isinstance(value, str) else value
        
        cursor.execute('''
            INSERT OR REPLACE INTO settings (key, value, updated_at)
            VALUES (?, ?, ?)
        ''', (key, json_value, datetime.now().isoformat()))
        
        conn.commit()
        conn.close()
    
    def get_all_settings(self):
        """Get all settings as a dictionary."""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('SELECT key, value FROM settings')
        results = cursor.fetchall()
        conn.close()
        
        settings = {}
        for key, value in results:
            try:
                settings[key] = json.loads(value)
            except json.JSONDecodeError:
                settings[key] = value
        
        return settings
    
    def update_settings(self, settings_dict):
        """Update multiple settings at once."""
        for key, value in settings_dict.items():
            self.set_setting(key, value)
    
    def generate_ics_token(self, description="Generated ICS token"):
        """Generate a new ICS calendar token."""
        token = str(uuid.uuid4())
        
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            INSERT INTO tokens (token, type, created_at, description)
            VALUES (?, ?, ?, ?)
        ''', (token, 'ics', datetime.now().isoformat(), description))
        
        conn.commit()
        conn.close()
        
        return token
    
    def get_ics_token(self):
        """Get the most recent ICS token, or generate one if none exists."""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('SELECT token FROM tokens WHERE type = ? ORDER BY created_at DESC LIMIT 1', ('ics',))
        result = cursor.fetchone()
        conn.close()
        
        if result:
            return result[0]
        else:
            return self.generate_ics_token()
    
    def delete_setting(self, key):
        """Delete a setting by key."""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('DELETE FROM settings WHERE key = ?', (key,))
        conn.commit()
        conn.close()