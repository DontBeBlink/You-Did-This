/**
 * API client for You Did This settings interface
 * Handles communication with the backend server and localStorage persistence
 */

class SettingsAPI {
    constructor(baseUrl = '') {
        this.baseUrl = baseUrl;
        this.localStorageKey = 'youDidThis_settings';
    }

    /**
     * Get settings from server, falling back to localStorage
     */
    async getSettings() {
        try {
            const response = await fetch(`${this.baseUrl}/settings`);
            const data = await response.json();
            
            if (data.success) {
                // Store in localStorage as backup
                this.saveToLocalStorage(data.settings);
                return data.settings;
            } else {
                console.warn('Failed to load from server, falling back to localStorage:', data.error);
                return this.loadFromLocalStorage();
            }
        } catch (error) {
            console.warn('Server unavailable, using localStorage:', error);
            return this.loadFromLocalStorage();
        }
    }

    /**
     * Update settings on server and in localStorage
     */
    async updateSettings(settings) {
        // Always save to localStorage first
        this.saveToLocalStorage(settings);

        try {
            const response = await fetch(`${this.baseUrl}/settings`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(settings)
            });

            const data = await response.json();
            return data;
        } catch (error) {
            console.warn('Failed to save to server, settings saved locally:', error);
            return { 
                success: true, 
                message: 'Settings saved locally (server unavailable)',
                localStorage: true 
            };
        }
    }

    /**
     * Generate a new ICS token
     */
    async generateICSToken(description = 'Generated ICS token') {
        try {
            const response = await fetch(`${this.baseUrl}/settings/generate-ics-token`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ description })
            });

            const data = await response.json();
            
            if (data.success) {
                // Update localStorage with new token
                const currentSettings = this.loadFromLocalStorage();
                currentSettings.ics_token = data.token;
                this.saveToLocalStorage(currentSettings);
            }
            
            return data;
        } catch (error) {
            console.error('Failed to generate ICS token:', error);
            return { 
                success: false, 
                error: 'Failed to connect to server for token generation' 
            };
        }
    }

    /**
     * Send test notification via ntfy
     */
    async testNotification(ntfyUrl, ntfyTopic) {
        try {
            const response = await fetch(`${this.baseUrl}/settings/test-ntfy`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ 
                    ntfy_url: ntfyUrl, 
                    ntfy_topic: ntfyTopic 
                })
            });

            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Failed to send test notification:', error);
            return { 
                success: false, 
                error: 'Failed to connect to server for notification test' 
            };
        }
    }

    /**
     * Check server health
     */
    async checkHealth() {
        try {
            const response = await fetch(`${this.baseUrl}/health`);
            const data = await response.json();
            return data.success;
        } catch (error) {
            return false;
        }
    }

    /**
     * Save settings to localStorage
     */
    saveToLocalStorage(settings) {
        try {
            localStorage.setItem(this.localStorageKey, JSON.stringify(settings));
        } catch (error) {
            console.error('Failed to save to localStorage:', error);
        }
    }

    /**
     * Load settings from localStorage with defaults
     */
    loadFromLocalStorage() {
        const defaults = {
            ntfy_topic: '',
            ntfy_url: 'https://ntfy.sh',
            whisper_model: 'base',
            whisper_language: 'auto',
            whisper_device: 'cpu',
            ics_token: this.generateFallbackToken()
        };

        try {
            const stored = localStorage.getItem(this.localStorageKey);
            if (stored) {
                const parsed = JSON.parse(stored);
                return { ...defaults, ...parsed };
            }
        } catch (error) {
            console.error('Failed to load from localStorage:', error);
        }

        return defaults;
    }

    /**
     * Generate a fallback ICS token when server is unavailable
     */
    generateFallbackToken() {
        return 'offline-' + crypto.randomUUID();
    }

    /**
     * Clear all stored settings
     */
    clearSettings() {
        try {
            localStorage.removeItem(this.localStorageKey);
        } catch (error) {
            console.error('Failed to clear localStorage:', error);
        }
    }
}

// Export for use in settings page
window.SettingsAPI = SettingsAPI;