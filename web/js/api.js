/**
 * Task Management API Client
 * Provides methods to interact with the task management backend API
 */

class TaskAPI {
    static baseURL = '';

    /**
     * Make an HTTP request to the API
     * @param {string} endpoint - API endpoint
     * @param {Object} options - Request options
     * @returns {Promise<any>} - Response data
     */
    static async request(endpoint, options = {}) {
        const url = `${this.baseURL}${endpoint}`;
        const config = {
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            },
            ...options
        };

        if (config.body && typeof config.body === 'object') {
            config.body = JSON.stringify(config.body);
        }

        try {
            const response = await fetch(url, config);
            
            if (!response.ok) {
                let errorMessage = `HTTP ${response.status}`;
                
                try {
                    const errorData = await response.json();
                    if (Array.isArray(errorData.detail)) {
                        // Handle Pydantic validation errors
                        errorMessage = errorData.detail.map(err => `${err.loc.join('.')}: ${err.msg}`).join(', ');
                    } else {
                        errorMessage = errorData.detail || errorData.message || errorMessage;
                    }
                } catch (e) {
                    // If we can't parse the error response, use the status text
                    errorMessage = response.statusText || errorMessage;
                }
                
                throw new Error(errorMessage);
            }

            // Handle responses that may not have content
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            } else {
                return null;
            }
        } catch (error) {
            // Re-throw with a more user-friendly message if it's a network error
            if (error.name === 'TypeError' && error.message.includes('fetch')) {
                throw new Error('Unable to connect to the server. Please check your connection.');
            }
            throw error;
        }
    }

    /**
     * Get tasks with optional filtering
     * @param {Object} params - Query parameters
     * @param {string} params.start_date - ISO date string for start date
     * @param {string} params.end_date - ISO date string for end date
     * @param {string} params.status - Task status filter
     * @param {string} params.priority - Task priority filter
     * @returns {Promise<Array>} - Array of tasks
     */
    static async getTasks(params = {}) {
        const queryParams = new URLSearchParams();
        
        for (const [key, value] of Object.entries(params)) {
            if (value !== null && value !== undefined && value !== '') {
                queryParams.append(key, value);
            }
        }

        const endpoint = `/tasks${queryParams.toString() ? '?' + queryParams.toString() : ''}`;
        return await this.request(endpoint);
    }

    /**
     * Get a specific task by ID
     * @param {number} taskId - Task ID
     * @returns {Promise<Object>} - Task object
     */
    static async getTask(taskId) {
        return await this.request(`/tasks/${taskId}`);
    }

    /**
     * Create a new task
     * @param {Object} taskData - Task data
     * @param {string} taskData.title - Task title
     * @param {string} taskData.description - Task description (optional)
     * @param {string} taskData.due_date - ISO date string (optional)
     * @param {string} taskData.priority - Task priority (low, medium, high, urgent)
     * @returns {Promise<Object>} - Created task object
     */
    static async createTask(taskData) {
        return await this.request('/tasks', {
            method: 'POST',
            body: taskData
        });
    }

    /**
     * Update a task
     * @param {number} taskId - Task ID
     * @param {Object} updateData - Fields to update
     * @returns {Promise<Object>} - Updated task object
     */
    static async updateTask(taskId, updateData) {
        return await this.request(`/tasks/${taskId}`, {
            method: 'PATCH',
            body: updateData
        });
    }

    /**
     * Delete a task
     * @param {number} taskId - Task ID
     * @returns {Promise<Object>} - Success message
     */
    static async deleteTask(taskId) {
        return await this.request(`/tasks/${taskId}`, {
            method: 'DELETE'
        });
    }

    /**
     * Reschedule a task using quick actions
     * @param {number} taskId - Task ID
     * @param {Object} rescheduleData - Reschedule options
     * @param {string} rescheduleData.action - Action type (+1h, +1d, next_week, pick_date)
     * @param {string} rescheduleData.new_date - New date for pick_date action (optional)
     * @returns {Promise<Object>} - Updated task object
     */
    static async rescheduleTask(taskId, rescheduleData) {
        return await this.request(`/tasks/${taskId}/reschedule`, {
            method: 'PATCH',
            body: rescheduleData
        });
    }

    /**
     * Mark a task as done
     * @param {number} taskId - Task ID
     * @returns {Promise<Object>} - Updated task object
     */
    static async markTaskDone(taskId) {
        return await this.request(`/tasks/${taskId}/done`, {
            method: 'PATCH'
        });
    }

    /**
     * Parse a natural language date string
     * @param {string} dateString - Natural language date string
     * @returns {Promise<Object>} - Parse result with original, parsed, and success fields
     */
    static async parseDate(dateString) {
        return await this.request('/tasks/parse-date', {
            method: 'POST',
            body: { date_string: dateString }
        });
    }

    /**
     * Check API health
     * @returns {Promise<Object>} - Health status
     */
    static async healthCheck() {
        return await this.request('/health');
    }
}

// Utility functions for common operations
const TaskUtils = {
    /**
     * Format a task due date for display
     * @param {string} dueDateString - ISO date string
     * @returns {string} - Formatted date string
     */
    formatDueDate(dueDateString) {
        if (!dueDateString) return 'No due date';
        
        const date = new Date(dueDateString);
        const now = new Date();
        const diffMs = date.getTime() - now.getTime();
        const diffDays = Math.ceil(diffMs / (1000 * 60 * 60 * 24));
        
        if (diffDays === 0) {
            return 'Today at ' + date.toLocaleTimeString('en-US', { 
                hour: 'numeric', 
                minute: '2-digit' 
            });
        } else if (diffDays === 1) {
            return 'Tomorrow at ' + date.toLocaleTimeString('en-US', { 
                hour: 'numeric', 
                minute: '2-digit' 
            });
        } else if (diffDays === -1) {
            return 'Yesterday at ' + date.toLocaleTimeString('en-US', { 
                hour: 'numeric', 
                minute: '2-digit' 
            });
        } else if (diffDays > 1 && diffDays <= 7) {
            return `In ${diffDays} days at ` + date.toLocaleTimeString('en-US', { 
                hour: 'numeric', 
                minute: '2-digit' 
            });
        } else if (diffDays < -1 && diffDays >= -7) {
            return `${Math.abs(diffDays)} days ago at ` + date.toLocaleTimeString('en-US', { 
                hour: 'numeric', 
                minute: '2-digit' 
            });
        } else {
            return date.toLocaleDateString('en-US', {
                month: 'short',
                day: 'numeric',
                year: date.getFullYear() !== now.getFullYear() ? 'numeric' : undefined
            }) + ' at ' + date.toLocaleTimeString('en-US', { 
                hour: 'numeric', 
                minute: '2-digit' 
            });
        }
    },

    /**
     * Get priority color class
     * @param {string} priority - Priority level
     * @returns {string} - CSS class name
     */
    getPriorityClass(priority) {
        const classes = {
            low: 'priority-low',
            medium: 'priority-medium',
            high: 'priority-high',
            urgent: 'priority-urgent'
        };
        return classes[priority] || 'priority-medium';
    },

    /**
     * Get status color class
     * @param {string} status - Task status
     * @returns {string} - CSS class name
     */
    getStatusClass(status) {
        const classes = {
            pending: 'status-pending',
            done: 'status-done',
            snoozed: 'status-snoozed'
        };
        return classes[status] || 'status-pending';
    },

    /**
     * Check if a task is overdue
     * @param {string} dueDateString - ISO date string
     * @returns {boolean} - True if overdue
     */
    isOverdue(dueDateString) {
        if (!dueDateString) return false;
        return new Date(dueDateString) < new Date();
    },

    /**
     * Group tasks by day
     * @param {Array} tasks - Array of tasks
     * @returns {Object} - Tasks grouped by date key (YYYY-MM-DD)
     */
    groupTasksByDay(tasks) {
        const groups = {};
        
        tasks.forEach(task => {
            let dateKey;
            
            if (task.due_date) {
                dateKey = task.due_date.split('T')[0];
            } else {
                dateKey = 'unscheduled';
            }
            
            if (!groups[dateKey]) {
                groups[dateKey] = [];
            }
            groups[dateKey].push(task);
        });
        
        return groups;
    },

    /**
     * Group tasks by hour for day view
     * @param {Array} tasks - Array of tasks for a single day
     * @returns {Object} - Tasks grouped by hour (0-23)
     */
    groupTasksByHour(tasks) {
        const groups = {};
        
        tasks.forEach(task => {
            let hourKey;
            
            if (task.due_date) {
                hourKey = new Date(task.due_date).getHours();
            } else {
                hourKey = 'unscheduled';
            }
            
            if (!groups[hourKey]) {
                groups[hourKey] = [];
            }
            groups[hourKey].push(task);
        });
        
        return groups;
    },

    /**
     * Sort tasks by due date and priority
     * @param {Array} tasks - Array of tasks
     * @returns {Array} - Sorted tasks
     */
    sortTasks(tasks) {
        return tasks.sort((a, b) => {
            // First, sort by due date (null dates go last)
            if (a.due_date && !b.due_date) return -1;
            if (!a.due_date && b.due_date) return 1;
            if (a.due_date && b.due_date) {
                const dateA = new Date(a.due_date);
                const dateB = new Date(b.due_date);
                if (dateA.getTime() !== dateB.getTime()) {
                    return dateA.getTime() - dateB.getTime();
                }
            }
            
            // Then sort by priority
            const priorityOrder = { urgent: 0, high: 1, medium: 2, low: 3 };
            const priorityA = priorityOrder[a.priority] || 2;
            const priorityB = priorityOrder[b.priority] || 2;
            
            return priorityA - priorityB;
        });
    }
};

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { TaskAPI, TaskUtils };
}