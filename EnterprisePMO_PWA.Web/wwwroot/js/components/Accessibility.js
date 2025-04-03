// Accessibility enhancements
class Accessibility {
    constructor() {
        this.init();
    }

    init() {
        this.addAriaAttributes();
        this.setupKeyboardNavigation();
        this.setupSkipLinks();
        this.setupFocusManagement();
    }

    addAriaAttributes() {
        // Add ARIA labels to interactive elements
        document.querySelectorAll('button:not([aria-label])').forEach(button => {
            if (button.textContent.trim()) {
                button.setAttribute('aria-label', button.textContent.trim());
            }
        });

        // Add ARIA labels to icons
        document.querySelectorAll('.bi').forEach(icon => {
            if (!icon.getAttribute('aria-label')) {
                const iconClass = Array.from(icon.classList)
                    .find(cls => cls.startsWith('bi-'))
                    ?.replace('bi-', '')
                    ?.replace(/-/g, ' ');
                if (iconClass) {
                    icon.setAttribute('aria-label', iconClass);
                    icon.setAttribute('role', 'img');
                }
            }
        });

        // Add ARIA labels to navigation
        document.querySelectorAll('nav').forEach(nav => {
            if (!nav.getAttribute('aria-label')) {
                nav.setAttribute('aria-label', 'Main navigation');
            }
        });

        // Add ARIA labels to search
        const searchInput = document.getElementById('search');
        if (searchInput && !searchInput.getAttribute('aria-label')) {
            searchInput.setAttribute('aria-label', 'Search');
        }

        // Add ARIA labels to dropdowns
        document.querySelectorAll('[role="menu"]').forEach(menu => {
            if (!menu.getAttribute('aria-label')) {
                menu.setAttribute('aria-label', 'Menu');
            }
        });
    }

    setupKeyboardNavigation() {
        // Add keyboard navigation to dropdowns
        document.querySelectorAll('[role="menu"]').forEach(menu => {
            const items = menu.querySelectorAll('[role="menuitem"]');
            let currentIndex = -1;

            menu.addEventListener('keydown', (event) => {
                switch (event.key) {
                    case 'ArrowDown':
                        event.preventDefault();
                        currentIndex = Math.min(currentIndex + 1, items.length - 1);
                        items[currentIndex]?.focus();
                        break;
                    case 'ArrowUp':
                        event.preventDefault();
                        currentIndex = Math.max(currentIndex - 1, 0);
                        items[currentIndex]?.focus();
                        break;
                    case 'Home':
                        event.preventDefault();
                        currentIndex = 0;
                        items[currentIndex]?.focus();
                        break;
                    case 'End':
                        event.preventDefault();
                        currentIndex = items.length - 1;
                        items[currentIndex]?.focus();
                        break;
                    case 'Escape':
                        event.preventDefault();
                        menu.setAttribute('aria-expanded', 'false');
                        menu.classList.add('hidden');
                        break;
                }
            });
        });

        // Add keyboard navigation to modals
        document.querySelectorAll('[role="dialog"]').forEach(modal => {
            const focusableElements = modal.querySelectorAll(
                'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
            );
            const firstFocusable = focusableElements[0];
            const lastFocusable = focusableElements[focusableElements.length - 1];

            modal.addEventListener('keydown', (event) => {
                if (event.key === 'Tab') {
                    if (event.shiftKey) {
                        if (document.activeElement === firstFocusable) {
                            event.preventDefault();
                            lastFocusable.focus();
                        }
                    } else {
                        if (document.activeElement === lastFocusable) {
                            event.preventDefault();
                            firstFocusable.focus();
                        }
                    }
                }
                if (event.key === 'Escape') {
                    modal.setAttribute('aria-hidden', 'true');
                    modal.classList.add('hidden');
                }
            });
        });
    }

    setupSkipLinks() {
        // Add skip links for keyboard users
        const skipLinks = document.createElement('div');
        skipLinks.className = 'sr-only focus:not-sr-only focus:absolute focus:top-0 focus:left-0 focus:z-50 focus:p-4 focus:bg-white focus:text-black';
        skipLinks.innerHTML = `
            <a href="#main-content" class="block mb-2">Skip to main content</a>
            <a href="#sidebar" class="block mb-2">Skip to navigation</a>
        `;
        document.body.insertBefore(skipLinks, document.body.firstChild);
    }

    setupFocusManagement() {
        // Store the last focused element when opening modals
        let lastFocusedElement = null;

        document.querySelectorAll('[role="dialog"]').forEach(modal => {
            const openButton = document.querySelector(`[aria-controls="${modal.id}"]`);
            if (openButton) {
                openButton.addEventListener('click', () => {
                    lastFocusedElement = document.activeElement;
                });
            }

            modal.addEventListener('keydown', (event) => {
                if (event.key === 'Escape') {
                    modal.setAttribute('aria-hidden', 'true');
                    modal.classList.add('hidden');
                    lastFocusedElement?.focus();
                }
            });
        });

        // Add focus trap to modals
        document.querySelectorAll('[role="dialog"]').forEach(modal => {
            const focusableElements = modal.querySelectorAll(
                'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
            );
            const firstFocusable = focusableElements[0];
            const lastFocusable = focusableElements[focusableElements.length - 1];

            modal.addEventListener('keydown', (event) => {
                if (event.key === 'Tab') {
                    if (event.shiftKey) {
                        if (document.activeElement === firstFocusable) {
                            event.preventDefault();
                            lastFocusable.focus();
                        }
                    } else {
                        if (document.activeElement === lastFocusable) {
                            event.preventDefault();
                            firstFocusable.focus();
                        }
                    }
                }
            });
        });
    }
}

// Initialize accessibility enhancements
const accessibility = new Accessibility(); 