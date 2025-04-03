// Keyboard shortcuts for power users
class KeyboardShortcuts {
    constructor() {
        this.shortcuts = new Map();
        this.isHelpVisible = false;
        this.helpOverlay = null;
        this.init();
    }

    init() {
        // Define default shortcuts
        this.registerShortcut('?', 'Show keyboard shortcuts', () => this.toggleHelp());
        this.registerShortcut('g d', 'Go to Dashboard', () => window.location.href = '/');
        this.registerShortcut('g p', 'Go to Projects', () => window.location.href = '/Project');
        this.registerShortcut('g t', 'Go to Tasks', () => window.location.href = '/Tasks');
        this.registerShortcut('g r', 'Go to Reports', () => window.location.href = '/Reports');
        this.registerShortcut('g k', 'Go to Calendar', () => window.location.href = '/Kanban');
        this.registerShortcut('g s', 'Go to Settings', () => window.location.href = '/Settings');
        this.registerShortcut('n p', 'New Project', () => window.location.href = '/Project/Create');
        this.registerShortcut('n t', 'New Task', () => window.location.href = '/Tasks/Create');
        this.registerShortcut('/', 'Focus search', () => document.getElementById('search')?.focus());
        this.registerShortcut('d', 'Toggle dark mode', () => {
            const darkModeToggle = document.querySelector('[data-action="toggle-dark-mode"]');
            if (darkModeToggle) darkModeToggle.click();
        });
        this.registerShortcut('s', 'Toggle sidebar', () => {
            const sidebarToggle = document.getElementById('sidebar-toggle');
            if (sidebarToggle) sidebarToggle.click();
        });

        // Create help overlay
        this.createHelpOverlay();

        // Add event listeners
        document.addEventListener('keydown', this.handleKeyPress.bind(this));
    }

    registerShortcut(key, description, action) {
        this.shortcuts.set(key, { description, action });
    }

    handleKeyPress(event) {
        // Ignore if typing in input fields
        if (event.target.tagName === 'INPUT' || event.target.tagName === 'TEXTAREA') {
            return;
        }

        const key = event.key.toLowerCase();
        const ctrlKey = event.ctrlKey || event.metaKey;

        // Handle single key shortcuts
        if (!ctrlKey && this.shortcuts.has(key)) {
            event.preventDefault();
            this.shortcuts.get(key).action();
            return;
        }

        // Handle two-key combinations
        if (ctrlKey) {
            const combo = `ctrl+${key}`;
            if (this.shortcuts.has(combo)) {
                event.preventDefault();
                this.shortcuts.get(combo).action();
                return;
            }
        }
    }

    createHelpOverlay() {
        this.helpOverlay = document.createElement('div');
        this.helpOverlay.className = 'fixed inset-0 bg-black bg-opacity-50 z-50 hidden';
        this.helpOverlay.innerHTML = `
            <div class="fixed inset-0 overflow-y-auto">
                <div class="flex min-h-full items-center justify-center p-4">
                    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-2xl w-full">
                        <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
                            <div class="flex items-center justify-between">
                                <h3 class="text-lg font-medium text-gray-900 dark:text-white">Keyboard Shortcuts</h3>
                                <button class="text-gray-400 hover:text-gray-500 dark:hover:text-gray-300" onclick="keyboardShortcuts.toggleHelp()">
                                    <i class="bi bi-x-lg"></i>
                                </button>
                            </div>
                        </div>
                        <div class="px-6 py-4">
                            <div class="grid grid-cols-2 gap-4">
                                ${Array.from(this.shortcuts.entries()).map(([key, { description }]) => `
                                    <div class="flex items-center justify-between">
                                        <span class="text-gray-600 dark:text-gray-300">${description}</span>
                                        <kbd class="px-2 py-1 text-sm font-semibold text-gray-800 dark:text-gray-200 bg-gray-100 dark:bg-gray-700 rounded">${key}</kbd>
                                    </div>
                                `).join('')}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
        document.body.appendChild(this.helpOverlay);
    }

    toggleHelp() {
        this.isHelpVisible = !this.isHelpVisible;
        this.helpOverlay.classList.toggle('hidden', !this.isHelpVisible);
    }
}

// Initialize keyboard shortcuts
const keyboardShortcuts = new KeyboardShortcuts(); 