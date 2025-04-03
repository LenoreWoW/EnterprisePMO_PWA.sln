/**
 * EnterprisePMO Layout Script
 * Handles common UI elements like sidebar, navigation, and notifications
 */

document.addEventListener('DOMContentLoaded', function() {
    // Initialize sidebar toggle
    initSidebar();
    
    // Initialize dropdowns
    initDropdowns();
    
    // Initialize notifications
    initNotifications();
    
    // Initialize tooltips
    initTooltips();
    
    // Handle authentication-related UI
    updateAuthUI();
    
    // Initialize search functionality
    initSearch();
});

/**
 * Initialize sidebar functionality
 */
function initSidebar() {
    const sidebarToggle = document.getElementById('sidebar-toggle');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.getElementById('main-content');
    
    if (sidebarToggle && sidebar) {
        // Get sidebar state from localStorage
        const sidebarCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        
        // Apply initial state
        if (sidebarCollapsed) {
            sidebar.classList.add('collapsed');
            mainContent?.classList.add('expanded');
        }
        
        // Toggle sidebar on click
        sidebarToggle.addEventListener('click', function() {
            sidebar.classList.toggle('collapsed');
            mainContent?.classList.toggle('expanded');
            
            // Save state to localStorage
            localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
        });
    }
    
    // Highlight active menu item
    const currentPath = window.location.pathname;
    const menuItems = document.querySelectorAll('.clickup-sidebar-item');
    
    menuItems.forEach(item => {
        const href = item.getAttribute('href');
        if (href && currentPath.startsWith(href) && href !== '/') {
            item.classList.add('clickup-sidebar-item-active');
        }
    });
}

/**
 * Initialize dropdown functionality
 */
function initDropdowns() {
    const dropdownToggles = document.querySelectorAll('[data-dropdown-toggle]');
    
    dropdownToggles.forEach(toggle => {
        const targetId = toggle.getAttribute('data-dropdown-toggle');
        const target = document.getElementById(targetId);
        
        if (target) {
            // Toggle dropdown
            toggle.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                
                // Close all other dropdowns
                document.querySelectorAll('.clickup-dropdown').forEach(dropdown => {
                    if (dropdown !== target) {
                        dropdown.classList.add('hidden');
                    }
                });
                
                // Toggle current dropdown
                target.classList.toggle('hidden');
            });
            
            // Close dropdown when clicking outside
            document.addEventListener('click', (e) => {
                if (!target.contains(e.target) && !toggle.contains(e.target)) {
                    target.classList.add('hidden');
                }
            });
        }
    });
}

/**
 * Initialize notifications
 */
function initNotifications() {
    const notificationToggle = document.getElementById('notification-toggle');
    const notificationDropdown = document.getElementById('notification-dropdown');
    
    if (notificationToggle && notificationDropdown) {
        // Toggle notifications
        notificationToggle.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            
            notificationDropdown.classList.toggle('hidden');
            
            // Close other dropdowns
            document.querySelectorAll('.clickup-dropdown').forEach(dropdown => {
                if (dropdown !== notificationDropdown) {
                    dropdown.classList.add('hidden');
                }
            });
        });
        
        // Close notifications when clicking outside
        document.addEventListener('click', (e) => {
            if (!notificationDropdown.contains(e.target) && !notificationToggle.contains(e.target)) {
                notificationDropdown.classList.add('hidden');
            }
        });
        
        // Check for new notifications
        checkNewNotifications();
    }
}

/**
 * Check for new notifications
 */
async function checkNewNotifications() {
    try {
        // This would be replaced with an actual API call
        const response = await fetch('/api/notifications/unread');
        const data = await response.json();
        
        const unreadCount = data.length;
        const badge = document.getElementById('notification-badge');
        
        if (badge) {
            if (unreadCount > 0) {
                badge.textContent = unreadCount;
                badge.classList.remove('hidden');
            } else {
                badge.classList.add('hidden');
            }
        }
    } catch (error) {
        console.error('Error checking notifications:', error);
    }
}

/**
 * Mark notification as read
 */
async function markNotificationAsRead(notificationId) {
    try {
        // This would be replaced with an actual API call
        await fetch(`/api/notifications/${notificationId}/read`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': document.querySelector('meta[name="csrf-token"]').getAttribute('content')
            }
        });
        
        // Update UI
        const notification = document.getElementById(`notification-${notificationId}`);
        if (notification) {
            notification.classList.remove('bg-blue-50');
        }
        
        // Update badge count
        checkNewNotifications();
    } catch (error) {
        console.error('Error marking notification as read:', error);
    }
}

/**
 * Initialize tooltips
 */
function initTooltips() {
    const tooltipTriggers = document.querySelectorAll('[data-tooltip]');
    
    tooltipTriggers.forEach(trigger => {
        const tooltipText = trigger.getAttribute('data-tooltip');
        
        // Create tooltip element
        const tooltip = document.createElement('div');
        tooltip.className = 'tooltip';
        tooltip.textContent = tooltipText;
        
        // Add tooltip to trigger
        trigger.appendChild(tooltip);
        
        // Show tooltip on hover
        trigger.addEventListener('mouseenter', () => {
            tooltip.classList.add('tooltip-visible');
        });
        
        // Hide tooltip on mouse leave
        trigger.addEventListener('mouseleave', () => {
            tooltip.classList.remove('tooltip-visible');
        });
    });
}

/**
 * Update UI based on authentication state
 */
function updateAuthUI() {
    const isAuthenticated = document.body.classList.contains('authenticated');
    const authElements = document.querySelectorAll('[data-auth]');
    
    authElements.forEach(element => {
        const authType = element.getAttribute('data-auth');
        
        if (authType === 'authenticated' && !isAuthenticated) {
            element.classList.add('hidden');
        } else if (authType === 'unauthenticated' && isAuthenticated) {
            element.classList.add('hidden');
        } else {
            element.classList.remove('hidden');
        }
    });
}

/**
 * Initialize search functionality
 */
function initSearch() {
    const searchInput = document.querySelector('.clickup-search');
    
    if (searchInput) {
        searchInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                
                const query = searchInput.value.trim();
                if (query) {
                    // Determine the current page context
                    const currentPath = window.location.pathname;
                    
                    // Redirect to the appropriate search page
                    if (currentPath.includes('/Project')) {
                        window.location.href = `/Project/Search?q=${encodeURIComponent(query)}`;
                    } else if (currentPath.includes('/Kanban')) {
                        window.location.href = `/Kanban/Search?q=${encodeURIComponent(query)}`;
                    } else if (currentPath.includes('/ChangeRequests')) {
                        window.location.href = `/ChangeRequests/Search?q=${encodeURIComponent(query)}`;
                    } else {
                        // Default to global search
                        window.location.href = `/Search?q=${encodeURIComponent(query)}`;
                    }
                }
            }
        });
    }
}

/**
 * Show notification
 */
function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `fixed bottom-4 right-4 p-4 rounded-md shadow-lg z-50 animate-fadeIn ${
        type === 'success' ? 'bg-green-500 text-white' :
        type === 'error' ? 'bg-red-500 text-white' :
        type === 'warning' ? 'bg-yellow-500 text-white' :
        'bg-blue-500 text-white'
    }`;
    
    // Add message
    notification.textContent = message;
    
    // Add to DOM
    document.body.appendChild(notification);
    
    // Remove after 3 seconds
    setTimeout(() => {
        notification.classList.add('opacity-0');
        setTimeout(() => {
            notification.remove();
        }, 300);
    }, 3000);
}