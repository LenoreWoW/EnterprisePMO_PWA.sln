/**
 * Auth handler for client-side authentication state management
 */
document.addEventListener('DOMContentLoaded', function() {
    // Check if user is authenticated
    const token = localStorage.getItem('auth_token');
    const userInfo = localStorage.getItem('user_info');
    
    // Set authentication state on body for CSS targeting
    if (token && userInfo) {
        document.body.setAttribute('data-authenticated', 'true');
        
        // Parse user info
        try {
            const user = JSON.parse(userInfo);
            document.body.setAttribute('data-user-role', user.role || '');
            console.log("Authenticated as:", user.username, "Role:", user.role);
        } catch (error) {
            console.error('Error parsing user info:', error);
        }
        
        // Add auth token to all API requests
        addAuthHeaderToRequests();
        
        // Initialize logout handler
        initLogoutHandler();
        
        // Show/hide elements based on auth state
        updateUIForAuthState(true);
        
        // Update notification badge if present
        updateNotificationBadge();
    } else {
        document.body.setAttribute('data-authenticated', 'false');
        updateUIForAuthState(false);
    }
});

/**
 * Adds Authorization header to all API requests
 */
function addAuthHeaderToRequests() {
    // Use fetch API's request interceptor pattern
    const originalFetch = window.fetch;
    
    window.fetch = function(url, options = {}) {
        // Only add auth header for our API requests
        if (typeof url === 'string' && url.startsWith('/api')) {
            const token = localStorage.getItem('auth_token');
            
            if (token) {
                options = options || {};
                options.headers = options.headers || {};
                
                // Don't override if Authorization is already set
                if (!options.headers.Authorization && !options.headers.authorization) {
                    options.headers.Authorization = `Bearer ${token}`;
                }
            }
        }
        
        return originalFetch(url, options);
    };
}

/**
 * Initializes the logout handler
 */
function initLogoutHandler() {
    const logoutForm = document.getElementById('logoutForm');
    
    if (logoutForm) {
        logoutForm.addEventListener('submit', function(event) {
            event.preventDefault();
            
            const token = localStorage.getItem('auth_token');
            
            fetch('/api/auth/logout', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            })
            .then(() => {
                // Clear auth data regardless of server response
                localStorage.removeItem('auth_token');
                localStorage.removeItem('refresh_token');
                localStorage.removeItem('user_info');
                
                // Redirect to login page
                window.location.href = '/Account/Login';
            })
            .catch(error => {
                console.error('Logout error:', error);
                
                // Still clear auth data and redirect on error
                localStorage.removeItem('auth_token');
                localStorage.removeItem('refresh_token');
                localStorage.removeItem('user_info');
                
                window.location.href = '/Account/Login';
            });
        });
    }
}

/**
 * Updates UI elements based on authentication state
 */
function updateUIForAuthState(isAuthenticated) {
    // Show/hide auth-dependent elements
    const authOnlyElements = document.querySelectorAll('[data-auth-only]');
    const noAuthElements = document.querySelectorAll('[data-no-auth]');
    const adminOnlyElements = document.querySelectorAll('[data-admin-only]');
    
    // Get user role
    let userRole = '';
    if (isAuthenticated) {
        try {
            const userInfo = localStorage.getItem('user_info');
            if (userInfo) {
                const user = JSON.parse(userInfo);
                userRole = user.role || '';
            }
        } catch (error) {
            console.error('Error parsing user info:', error);
        }
    }
    
    // Handle auth-only elements
    authOnlyElements.forEach(element => {
        element.style.display = isAuthenticated ? '' : 'none';
    });
    
    // Handle no-auth elements
    noAuthElements.forEach(element => {
        element.style.display = !isAuthenticated ? '' : 'none';
    });
    
    // Handle admin-only elements
    adminOnlyElements.forEach(element => {
        element.style.display = (isAuthenticated && userRole === 'Admin') ? '' : 'none';
    });
}

/**
 * Updates notification badge
 */
function updateNotificationBadge() {
    const token = localStorage.getItem('auth_token');
    if (!token) return;
    
    fetch('/api/notifications/unread/count', {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    })
    .then(response => {
        if (response.ok) {
            return response.json();
        }
        throw new Error('Failed to fetch notification count');
    })
    .then(data => {
        const badge = document.getElementById('notificationCount');
        if (badge && data.count > 0) {
            badge.textContent = data.count;
            badge.classList.remove('d-none');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        // Don't show error message for notification count - fail silently
    });
}