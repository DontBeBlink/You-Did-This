/**
 * API helper functions for the Memories frontend
 */

class MemoriesAPI {
    constructor(baseURL = '') {
        this.baseURL = baseURL;
    }

    /**
     * Get memories with filtering and pagination
     */
    async getMemories(params = {}) {
        const queryParams = new URLSearchParams();
        
        if (params.q) queryParams.append('q', params.q);
        if (params.tags && params.tags.length > 0) {
            queryParams.append('tags', params.tags.join(','));
        }
        if (params.people && params.people.length > 0) {
            queryParams.append('people', params.people.join(','));
        }
        if (params.limit) queryParams.append('limit', params.limit);
        if (params.offset) queryParams.append('offset', params.offset);

        const response = await fetch(`${this.baseURL}/memories?${queryParams}`);
        if (!response.ok) {
            throw new Error(`Failed to fetch memories: ${response.statusText}`);
        }
        return await response.json();
    }

    /**
     * Get a specific memory by ID
     */
    async getMemory(id) {
        const response = await fetch(`${this.baseURL}/memories/${id}`);
        if (!response.ok) {
            throw new Error(`Failed to fetch memory: ${response.statusText}`);
        }
        return await response.json();
    }

    /**
     * Create a new memory
     */
    async createMemory(memory) {
        const response = await fetch(`${this.baseURL}/memories`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(memory),
        });
        if (!response.ok) {
            throw new Error(`Failed to create memory: ${response.statusText}`);
        }
        return await response.json();
    }

    /**
     * Update a memory
     */
    async updateMemory(id, updates) {
        const response = await fetch(`${this.baseURL}/memories/${id}`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(updates),
        });
        if (!response.ok) {
            throw new Error(`Failed to update memory: ${response.statusText}`);
        }
        return await response.json();
    }

    /**
     * Delete a memory
     */
    async deleteMemory(id) {
        const response = await fetch(`${this.baseURL}/memories/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) {
            throw new Error(`Failed to delete memory: ${response.statusText}`);
        }
        return await response.json();
    }

    /**
     * Delete multiple memories
     */
    async deleteMemories(ids) {
        const response = await fetch(`${this.baseURL}/memories`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(ids),
        });
        if (!response.ok) {
            throw new Error(`Failed to delete memories: ${response.statusText}`);
        }
        return await response.json();
    }
}

/**
 * Utility functions for the UI
 */
class MemoriesUI {
    constructor(api) {
        this.api = api;
        this.currentPage = 0;
        this.pageSize = 10;
        this.selectedMemories = new Set();
        this.filters = {
            q: '',
            tags: [],
            people: []
        };
    }

    /**
     * Initialize the UI
     */
    init() {
        this.setupEventListeners();
        this.loadMemories();
    }

    /**
     * Setup event listeners
     */
    setupEventListeners() {
        // Search input
        document.getElementById('searchInput').addEventListener('input', 
            this.debounce(() => this.handleSearch(), 300));

        // Filter clear buttons
        document.getElementById('clearFilters').addEventListener('click', () => this.clearFilters());

        // Bulk actions
        document.getElementById('bulkDelete').addEventListener('click', () => this.handleBulkDelete());
        document.getElementById('selectAll').addEventListener('click', () => this.handleSelectAll());

        // Pagination
        document.getElementById('prevPage').addEventListener('click', () => this.prevPage());
        document.getElementById('nextPage').addEventListener('click', () => this.nextPage());
    }

    /**
     * Debounce function for search input
     */
    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    /**
     * Handle search input
     */
    handleSearch() {
        this.filters.q = document.getElementById('searchInput').value.trim();
        this.currentPage = 0;
        this.loadMemories();
    }

    /**
     * Clear all filters
     */
    clearFilters() {
        this.filters = { q: '', tags: [], people: [] };
        document.getElementById('searchInput').value = '';
        document.getElementById('tagsFilter').innerHTML = '';
        document.getElementById('peopleFilter').innerHTML = '';
        this.currentPage = 0;
        this.loadMemories();
    }

    /**
     * Load memories from API
     */
    async loadMemories() {
        try {
            this.showLoading(true);
            const params = {
                ...this.filters,
                limit: this.pageSize,
                offset: this.currentPage * this.pageSize
            };
            
            const response = await this.api.getMemories(params);
            this.renderMemories(response.items);
            this.renderPagination(response.total);
            this.hideError();
        } catch (error) {
            this.showError(`Failed to load memories: ${error.message}`);
        } finally {
            this.showLoading(false);
        }
    }

    /**
     * Render memories list
     */
    renderMemories(memories) {
        const container = document.getElementById('memoriesContainer');
        container.innerHTML = '';

        if (memories.length === 0) {
            container.innerHTML = '<div class="no-memories">No memories found</div>';
            return;
        }

        memories.forEach(memory => {
            const memoryElement = this.createMemoryElement(memory);
            container.appendChild(memoryElement);
        });
    }

    /**
     * Create a memory element
     */
    createMemoryElement(memory) {
        const div = document.createElement('div');
        div.className = 'memory-item';
        div.dataset.id = memory.id;

        const tagsHtml = memory.tags.map(tag => 
            `<span class="tag" onclick="memoriesUI.addTagFilter('${tag}')">${tag}</span>`
        ).join('');

        const peopleHtml = memory.people.map(person => 
            `<span class="person" onclick="memoriesUI.addPersonFilter('${person}')">${person}</span>`
        ).join('');

        div.innerHTML = `
            <div class="memory-header">
                <input type="checkbox" class="memory-select" data-id="${memory.id}" 
                       onchange="memoriesUI.handleMemorySelect(${memory.id}, this.checked)">
                <h3 class="memory-title" onclick="memoriesUI.toggleEditMode(${memory.id})">${memory.title}</h3>
                <div class="memory-actions">
                    <button onclick="memoriesUI.editMemory(${memory.id})" class="btn-edit">Edit</button>
                    <button onclick="memoriesUI.deleteMemory(${memory.id})" class="btn-delete">Delete</button>
                </div>
            </div>
            <div class="memory-content">${memory.content}</div>
            <div class="memory-meta">
                <div class="tags">${tagsHtml}</div>
                <div class="people">${peopleHtml}</div>
                <div class="timestamp">Updated: ${new Date(memory.updated_at).toLocaleDateString()}</div>
            </div>
        `;

        return div;
    }

    /**
     * Handle memory selection for bulk operations
     */
    handleMemorySelect(id, selected) {
        if (selected) {
            this.selectedMemories.add(id);
        } else {
            this.selectedMemories.delete(id);
        }
        this.updateBulkActions();
    }

    /**
     * Handle select all checkbox
     */
    handleSelectAll() {
        const selectAll = document.getElementById('selectAll');
        const checkboxes = document.querySelectorAll('.memory-select');
        
        checkboxes.forEach(checkbox => {
            checkbox.checked = selectAll.checked;
            this.handleMemorySelect(parseInt(checkbox.dataset.id), selectAll.checked);
        });
    }

    /**
     * Update bulk actions visibility
     */
    updateBulkActions() {
        const bulkActions = document.getElementById('bulkActions');
        const count = this.selectedMemories.size;
        
        if (count > 0) {
            bulkActions.style.display = 'block';
            document.getElementById('selectedCount').textContent = count;
        } else {
            bulkActions.style.display = 'none';
        }
    }

    /**
     * Handle bulk delete
     */
    async handleBulkDelete() {
        if (this.selectedMemories.size === 0) return;

        if (!confirm(`Delete ${this.selectedMemories.size} selected memories?`)) {
            return;
        }

        try {
            await this.api.deleteMemories(Array.from(this.selectedMemories));
            this.selectedMemories.clear();
            this.updateBulkActions();
            this.loadMemories();
            this.showSuccess('Memories deleted successfully');
        } catch (error) {
            this.showError(`Failed to delete memories: ${error.message}`);
        }
    }

    /**
     * Delete a single memory
     */
    async deleteMemory(id) {
        if (!confirm('Delete this memory?')) return;

        try {
            await this.api.deleteMemory(id);
            this.loadMemories();
            this.showSuccess('Memory deleted successfully');
        } catch (error) {
            this.showError(`Failed to delete memory: ${error.message}`);
        }
    }

    /**
     * Edit memory (inline editing)
     */
    editMemory(id) {
        // This would open an inline edit form
        // For now, just show an alert - full implementation would be more complex
        alert(`Edit memory ${id} - inline editing would be implemented here`);
    }

    /**
     * Add tag filter
     */
    addTagFilter(tag) {
        if (!this.filters.tags.includes(tag)) {
            this.filters.tags.push(tag);
            this.renderTagFilters();
            this.currentPage = 0;
            this.loadMemories();
        }
    }

    /**
     * Add person filter
     */
    addPersonFilter(person) {
        if (!this.filters.people.includes(person)) {
            this.filters.people.push(person);
            this.renderPeopleFilters();
            this.currentPage = 0;
            this.loadMemories();
        }
    }

    /**
     * Render tag filters
     */
    renderTagFilters() {
        const container = document.getElementById('tagsFilter');
        container.innerHTML = this.filters.tags.map(tag => 
            `<span class="filter-chip">${tag} <span onclick="memoriesUI.removeTagFilter('${tag}')">&times;</span></span>`
        ).join('');
    }

    /**
     * Render people filters
     */
    renderPeopleFilters() {
        const container = document.getElementById('peopleFilter');
        container.innerHTML = this.filters.people.map(person => 
            `<span class="filter-chip">${person} <span onclick="memoriesUI.removePersonFilter('${person}')">&times;</span></span>`
        ).join('');
    }

    /**
     * Remove tag filter
     */
    removeTagFilter(tag) {
        this.filters.tags = this.filters.tags.filter(t => t !== tag);
        this.renderTagFilters();
        this.currentPage = 0;
        this.loadMemories();
    }

    /**
     * Remove person filter
     */
    removePersonFilter(person) {
        this.filters.people = this.filters.people.filter(p => p !== person);
        this.renderPeopleFilters();
        this.currentPage = 0;
        this.loadMemories();
    }

    /**
     * Render pagination
     */
    renderPagination(total) {
        const totalPages = Math.ceil(total / this.pageSize);
        const currentPage = this.currentPage + 1;
        
        document.getElementById('pageInfo').textContent = 
            `Page ${currentPage} of ${totalPages} (${total} total)`;
        
        document.getElementById('prevPage').disabled = this.currentPage === 0;
        document.getElementById('nextPage').disabled = this.currentPage >= totalPages - 1;
    }

    /**
     * Go to previous page
     */
    prevPage() {
        if (this.currentPage > 0) {
            this.currentPage--;
            this.loadMemories();
        }
    }

    /**
     * Go to next page
     */
    nextPage() {
        this.currentPage++;
        this.loadMemories();
    }

    /**
     * Show loading indicator
     */
    showLoading(show) {
        const loader = document.getElementById('loading');
        loader.style.display = show ? 'block' : 'none';
    }

    /**
     * Show error message
     */
    showError(message) {
        const errorDiv = document.getElementById('errorMessage');
        errorDiv.textContent = message;
        errorDiv.style.display = 'block';
    }

    /**
     * Hide error message
     */
    hideError() {
        document.getElementById('errorMessage').style.display = 'none';
    }

    /**
     * Show success message
     */
    showSuccess(message) {
        const successDiv = document.getElementById('successMessage');
        successDiv.textContent = message;
        successDiv.style.display = 'block';
        setTimeout(() => {
            successDiv.style.display = 'none';
        }, 3000);
    }
}

// Global instances
const memoriesAPI = new MemoriesAPI();
const memoriesUI = new MemoriesUI(memoriesAPI);