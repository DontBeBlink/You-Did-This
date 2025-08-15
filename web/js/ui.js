/**
 * You Did This - Command Palette UI
 * Implements keyboard shortcuts and command palette functionality
 */

class CommandPalette {
    constructor() {
        this.isOpen = false;
        this.selectedIndex = 0;
        this.commands = [];
        this.filteredCommands = [];
        this.keySequence = [];
        this.keySequenceTimeout = null;
        
        this.init();
    }

    init() {
        this.initCommands();
        this.bindEvents();
        this.setupAccessibility();
    }

    initCommands() {
        this.commands = [
            {
                id: 'capture-memory',
                title: 'Capture Memory',
                description: 'Create a new memory entry',
                icon: '📝',
                shortcut: ['n'],
                keywords: ['memory', 'note', 'capture', 'new'],
                action: () => this.executeAction('capture-memory')
            },
            {
                id: 'new-task',
                title: 'New Task',
                description: 'Add a new task to your agenda',
                icon: '✓',
                shortcut: [],
                keywords: ['task', 'todo', 'agenda', 'new'],
                action: () => this.executeAction('new-task')
            },
            {
                id: 'go-memories',
                title: 'Go to Memories',
                description: 'Navigate to memories page',
                icon: '📖',
                shortcut: ['g', 'm'],
                keywords: ['memories', 'go', 'navigate'],
                action: () => window.location.href = 'memories.html'
            },
            {
                id: 'go-agenda',
                title: 'Go to Agenda',
                description: 'Navigate to agenda page',
                icon: '📅',
                shortcut: ['g', 'a'],
                keywords: ['agenda', 'tasks', 'go', 'navigate'],
                action: () => window.location.href = 'agenda.html'
            },
            {
                id: 'go-backup',
                title: 'Go to Backup',
                description: 'Navigate to backup page',
                icon: '💾',
                shortcut: [],
                keywords: ['backup', 'sync', 'go', 'navigate'],
                action: () => window.location.href = 'backup.html'
            },
            {
                id: 'go-settings',
                title: 'Go to Settings',
                description: 'Navigate to settings page',
                icon: '⚙️',
                shortcut: [],
                keywords: ['settings', 'preferences', 'go', 'navigate'],
                action: () => window.location.href = 'settings.html'
            },
            {
                id: 'go-home',
                title: 'Go to Hub',
                description: 'Navigate to home page',
                icon: '🏠',
                shortcut: [],
                keywords: ['home', 'hub', 'go', 'navigate'],
                action: () => window.location.href = 'index.html'
            },
            {
                id: 'search',
                title: 'Search',
                description: 'Search across all content',
                icon: '🔍',
                shortcut: ['/'],
                keywords: ['search', 'find', 'lookup'],
                action: () => this.executeAction('search')
            },
            {
                id: 'help',
                title: 'Show Keyboard Shortcuts',
                description: 'Display available keyboard shortcuts',
                icon: '❓',
                shortcut: ['?'],
                keywords: ['help', 'shortcuts', 'hotkeys'],
                action: () => this.showShortcuts()
            }
        ];
    }

    bindEvents() {
        // Global keyboard shortcuts
        document.addEventListener('keydown', (e) => this.handleGlobalKeydown(e));
        
        // Command palette events
        const palette = document.getElementById('command-palette');
        const input = document.getElementById('command-palette-input');
        const backdrop = palette?.querySelector('.command-palette-backdrop');

        if (input) {
            input.addEventListener('input', (e) => this.handleSearch(e.target.value));
            input.addEventListener('keydown', (e) => this.handlePaletteKeydown(e));
        }

        if (backdrop) {
            backdrop.addEventListener('click', () => this.closePalette());
        }

        // Action button events
        document.addEventListener('click', (e) => {
            const actionBtn = e.target.closest('[data-action]');
            if (actionBtn) {
                e.preventDefault();
                const action = actionBtn.getAttribute('data-action');
                this.executeAction(action);
            }
        });

        // Modal close events
        const modalCloses = document.querySelectorAll('.modal-close');
        modalCloses.forEach(closeBtn => {
            closeBtn.addEventListener('click', () => this.closeModals());
        });

        // Modal backdrop events
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('modal-backdrop')) {
                this.closeModals();
            }
        });
    }

    setupAccessibility() {
        // Ensure proper ARIA attributes are set
        const palette = document.getElementById('command-palette');
        if (palette) {
            palette.setAttribute('aria-hidden', 'true');
        }

        // Set up focus trap for modals
        this.setupFocusTrap();
    }

    setupFocusTrap() {
        const modals = document.querySelectorAll('[role="dialog"]');
        modals.forEach(modal => {
            modal.addEventListener('keydown', (e) => {
                if (e.key === 'Tab') {
                    this.trapFocus(e, modal);
                }
            });
        });
    }

    trapFocus(e, container) {
        const focusableElements = container.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];

        if (e.shiftKey) {
            if (document.activeElement === firstElement) {
                lastElement.focus();
                e.preventDefault();
            }
        } else {
            if (document.activeElement === lastElement) {
                firstElement.focus();
                e.preventDefault();
            }
        }
    }

    handleGlobalKeydown(e) {
        // Command palette trigger
        if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
            e.preventDefault();
            this.togglePalette();
            return;
        }

        // Don't handle shortcuts when palette is open or in input fields
        if (this.isOpen || this.isInInputField(e.target)) {
            return;
        }

        // Escape key
        if (e.key === 'Escape') {
            this.closeModals();
            return;
        }

        // Handle key sequences
        this.handleKeySequence(e);
    }

    handleKeySequence(e) {
        const key = e.key.toLowerCase();
        
        // Clear timeout if exists
        if (this.keySequenceTimeout) {
            clearTimeout(this.keySequenceTimeout);
        }

        // Add key to sequence
        this.keySequence.push(key);

        // Set timeout to clear sequence
        this.keySequenceTimeout = setTimeout(() => {
            this.keySequence = [];
        }, 1000);

        // Check for matching commands
        const matchingCommand = this.commands.find(cmd => 
            cmd.shortcut.length > 0 && 
            this.arraysEqual(cmd.shortcut, this.keySequence.slice(-cmd.shortcut.length))
        );

        if (matchingCommand) {
            e.preventDefault();
            this.keySequence = [];
            matchingCommand.action();
        }
    }

    handlePaletteKeydown(e) {
        switch (e.key) {
            case 'Escape':
                e.preventDefault();
                this.closePalette();
                break;
            case 'ArrowDown':
                e.preventDefault();
                this.selectNext();
                break;
            case 'ArrowUp':
                e.preventDefault();
                this.selectPrevious();
                break;
            case 'Enter':
                e.preventDefault();
                this.executeSelected();
                break;
        }
    }

    handleSearch(query) {
        const normalizedQuery = query.toLowerCase().trim();
        
        if (!normalizedQuery) {
            this.filteredCommands = [...this.commands];
        } else {
            this.filteredCommands = this.commands.filter(cmd => {
                const titleMatch = cmd.title.toLowerCase().includes(normalizedQuery);
                const descriptionMatch = cmd.description.toLowerCase().includes(normalizedQuery);
                const keywordMatch = cmd.keywords.some(keyword => 
                    keyword.toLowerCase().includes(normalizedQuery)
                );
                return titleMatch || descriptionMatch || keywordMatch;
            });
        }

        this.selectedIndex = 0;
        this.renderResults();
    }

    togglePalette() {
        if (this.isOpen) {
            this.closePalette();
        } else {
            this.openPalette();
        }
    }

    openPalette() {
        const palette = document.getElementById('command-palette');
        const input = document.getElementById('command-palette-input');
        
        if (!palette || !input) return;

        this.isOpen = true;
        this.filteredCommands = [...this.commands];
        this.selectedIndex = 0;

        palette.classList.remove('hidden');
        palette.setAttribute('aria-hidden', 'false');
        
        // Focus the input
        setTimeout(() => {
            input.focus();
            input.value = '';
        }, 100);

        this.renderResults();
    }

    closePalette() {
        const palette = document.getElementById('command-palette');
        
        if (!palette) return;

        this.isOpen = false;
        palette.classList.add('hidden');
        palette.setAttribute('aria-hidden', 'true');
        
        // Clear search
        const input = document.getElementById('command-palette-input');
        if (input) {
            input.value = '';
        }
    }

    selectNext() {
        if (this.filteredCommands.length === 0) return;
        
        this.selectedIndex = (this.selectedIndex + 1) % this.filteredCommands.length;
        this.updateSelection();
    }

    selectPrevious() {
        if (this.filteredCommands.length === 0) return;
        
        this.selectedIndex = this.selectedIndex === 0 
            ? this.filteredCommands.length - 1 
            : this.selectedIndex - 1;
        this.updateSelection();
    }

    updateSelection() {
        const results = document.querySelectorAll('.command-item');
        results.forEach((item, index) => {
            if (index === this.selectedIndex) {
                item.classList.add('selected');
                item.setAttribute('aria-selected', 'true');
                item.scrollIntoView({ block: 'nearest' });
            } else {
                item.classList.remove('selected');
                item.setAttribute('aria-selected', 'false');
            }
        });
    }

    executeSelected() {
        if (this.filteredCommands.length === 0) return;
        
        const selectedCommand = this.filteredCommands[this.selectedIndex];
        if (selectedCommand) {
            this.closePalette();
            selectedCommand.action();
        }
    }

    renderResults() {
        const resultsContainer = document.querySelector('.command-results');
        if (!resultsContainer) return;

        if (this.filteredCommands.length === 0) {
            resultsContainer.innerHTML = `
                <div class="command-item">
                    <div class="command-icon">🚫</div>
                    <div class="command-content">
                        <div class="command-title">No commands found</div>
                        <div class="command-description">Try a different search term</div>
                    </div>
                </div>
            `;
            return;
        }

        resultsContainer.innerHTML = this.filteredCommands
            .map((cmd, index) => `
                <button 
                    class="command-item${index === this.selectedIndex ? ' selected' : ''}"
                    role="option"
                    aria-selected="${index === this.selectedIndex}"
                    data-command-id="${cmd.id}"
                >
                    <div class="command-icon">${cmd.icon}</div>
                    <div class="command-content">
                        <div class="command-title">${cmd.title}</div>
                        <div class="command-description">${cmd.description}</div>
                    </div>
                    ${cmd.shortcut.length > 0 ? `
                        <div class="command-shortcut">
                            ${cmd.shortcut.map(key => `<kbd>${key}</kbd>`).join('')}
                        </div>
                    ` : ''}
                </button>
            `).join('');

        // Add click listeners to command items
        resultsContainer.querySelectorAll('.command-item').forEach((item, index) => {
            item.addEventListener('click', () => {
                this.selectedIndex = index;
                this.executeSelected();
            });
        });
    }

    executeAction(action) {
        switch (action) {
            case 'capture-memory':
                this.captureMemory();
                break;
            case 'new-task':
                this.newTask();
                break;
            case 'search':
                this.search();
                break;
            case 'help':
                this.showShortcuts();
                break;
            default:
                console.log(`Action not implemented: ${action}`);
        }
    }

    captureMemory() {
        // Simulate capturing a memory
        const title = prompt('Memory title:');
        if (title) {
            const content = prompt('Memory content:');
            if (content) {
                // In a real app, this would save to storage
                alert(`Memory "${title}" captured!`);
                
                // If on memories page, could add to the list
                if (window.location.pathname.includes('memories.html')) {
                    this.addMemoryToPage(title, content);
                }
            }
        }
    }

    newTask() {
        // Simulate creating a new task
        const title = prompt('Task title:');
        if (title) {
            const description = prompt('Task description (optional):');
            // In a real app, this would save to storage
            alert(`Task "${title}" created!`);
            
            // If on agenda page, could add to the list
            if (window.location.pathname.includes('agenda.html')) {
                this.addTaskToPage(title, description);
            }
        }
    }

    search() {
        // Simulate search functionality
        const query = prompt('What are you looking for?');
        if (query) {
            alert(`Searching for: "${query}"`);
            // In a real app, this would perform actual search
        }
    }

    showShortcuts() {
        const modal = document.getElementById('shortcuts-modal');
        if (modal) {
            modal.classList.remove('hidden');
            modal.setAttribute('aria-hidden', 'false');
            
            // Focus the first focusable element
            const closeBtn = modal.querySelector('.modal-close');
            if (closeBtn) {
                closeBtn.focus();
            }
        }
    }

    closeModals() {
        const modals = document.querySelectorAll('.modal');
        modals.forEach(modal => {
            modal.classList.add('hidden');
            modal.setAttribute('aria-hidden', 'true');
        });
    }

    addMemoryToPage(title, content) {
        const memoriesGrid = document.querySelector('.memories-grid');
        if (memoriesGrid) {
            const memoryCard = document.createElement('div');
            memoryCard.className = 'memory-card';
            memoryCard.innerHTML = `
                <div class="memory-date">Just now</div>
                <div class="memory-content">
                    <h3>${this.escapeHtml(title)}</h3>
                    <p>${this.escapeHtml(content)}</p>
                </div>
                <div class="memory-tags">
                    <span class="tag">new</span>
                </div>
            `;
            memoriesGrid.insertBefore(memoryCard, memoriesGrid.firstChild);
        }
    }

    addTaskToPage(title, description) {
        const taskList = document.querySelector('.task-list');
        if (taskList) {
            const taskId = `task-${Date.now()}`;
            const taskItem = document.createElement('div');
            taskItem.className = 'task-item';
            taskItem.innerHTML = `
                <input type="checkbox" id="${taskId}" class="task-checkbox">
                <label for="${taskId}" class="task-content">
                    <span class="task-title">${this.escapeHtml(title)}</span>
                    ${description ? `<span class="task-description">${this.escapeHtml(description)}</span>` : ''}
                </label>
                <span class="task-priority medium">Medium</span>
            `;
            taskList.insertBefore(taskItem, taskList.firstChild);
        }
    }

    // Utility methods
    isInInputField(element) {
        const inputTypes = ['input', 'textarea', 'select'];
        return inputTypes.includes(element.tagName.toLowerCase()) ||
               element.contentEditable === 'true';
    }

    arraysEqual(a, b) {
        if (a.length !== b.length) return false;
        return a.every((val, index) => val === b[index]);
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
}

// Initialize the command palette when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new CommandPalette();
});

// Show loading indicator for development
console.log('🎮 You Did This - Command Palette loaded');
console.log('📝 Press Ctrl/⌘+K to open command palette');
console.log('⌨️  Try shortcuts: g+m (Memories), g+a (Agenda), n (New Memory), / (Search), ? (Help)');