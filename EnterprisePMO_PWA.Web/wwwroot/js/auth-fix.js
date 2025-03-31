// auth-fix.js - Consolidated authentication handler
(function() {
    // Auth Configuration
    const TOKEN_KEY = 'auth_token';
    const USER_KEY = 'auth_user';
    
    // Try to recover user info from token if needed
    function tryRecoverUserInfo() {
        const token = localStorage.getItem(TOKEN_KEY);
        if (token && !localStorage.getItem(USER_KEY)) {
            try {
                const payload = JSON.parse(atob(token.split('.')[1]));
                
                // Create user info object
                const userInfo = {
                    id: payload.nameid,
                    username: payload.unique_name,
                    role: payload.role
                };
                
                // Store it
                localStorage.setItem(USER_KEY, JSON.stringify(userInfo));
                console.log('User info recovered from token:', userInfo);
                return true;
            } catch (e) {
                console.error('Failed to recover user info from token:', e);
            }
        }
        return false;
    }
    
    // Apply auth token to all navigation and API links
    function applyAuthToAllLinks() {
        const token = localStorage.getItem(TOKEN_KEY);
        if (!token) return;
        
        // Get all links that might need auth
        const links = document.querySelectorAll('a:not([href^="/Account"]), button[formaction]');
        
        links.forEach(link => {
            // Skip logout, already auth'd links, and non-HTTP links
            if (link.href && (
                link.href.includes('/api/auth/logout') || 
                link.href.includes('auth_token=') ||
                !link.href.startsWith('http')
            )) {
                return;
            }
            
            // For link elements with href
            if (link.tagName === 'A' && link.href) {
                try {
                    const url = new URL(link.href);
                    url.searchParams.set('auth_token', token);
                    link.href = url.toString();
                } catch (e) {
                    console.warn('Error updating link:', link, e);
                }
            }
            
            // For form submit buttons
            if (link.tagName === 'BUTTON' && link.formAction) {
                try {
                    const url = new URL(link.formAction);
                    url.searchParams.set('auth_token', token);
                    link.formAction = url.toString();
                } catch (e) {
                    console.warn('Error updating button formAction:', link, e);
                }
            }
        });
        
        console.log('Applied auth token to all applicable links');
    }
    
    // Setup fetch interceptor to add auth header to all API requests
    function setupFetchInterceptor() {
        const originalFetch = window.fetch;
        
        window.fetch = function(url, options = {}) {
            // Skip for auth endpoints to avoid loops
            if (typeof url === 'string' && (
                url.includes('/api/auth/login') || 
                url.includes('/api/auth/signup') || 
                url.includes('/api/auth/direct-login')
            )) {
                return originalFetch(url, options);
            }
            
            // Clone options to avoid modifying the original
            const newOptions = {...options};
            newOptions.headers = newOptions.headers || {};
            
            // Get auth token
            const token = localStorage.getItem(TOKEN_KEY);
            
            // Add Authorization header if not already present
            if (token && !newOptions.headers.Authorization && !newOptions.headers.authorization) {
                newOptions.headers.Authorization = `Bearer ${token}`;
                console.log(`Added Authorization header to request: ${typeof url === 'string' ? url : 'Request'}`);
            }
            
            // Make the request with auth header
            return originalFetch(url, newOptions)
                .then(response => {
                    // Handle 401 Unauthorized by redirecting to login
                    if (response.status === 401) {
                        console.error('401 Unauthorized response received');
                        localStorage.removeItem(TOKEN_KEY);
                        localStorage.removeItem(USER_KEY);
                        
                        // Only redirect if not already on login page
                        if (!window.location.pathname.includes('/Account/Login')) {
                            window.location.href = '/Account/Login';
                        }
                        
                        return Promise.reject(new Error('Unauthorized'));
                    }
                    return response;
                });
        };
    }
    
    // Hide navigation elements on login pages
    function hideNavOnLoginPage() {
        // Check if on login/account page
        const isLoginPage = window.location.pathname.includes('/Account/Login') || 
                          window.location.pathname.includes('/Account/Signup') ||
                          window.location.pathname === '/';
        
        if (isLoginPage) {
            // Hide navigation buttons
            document.querySelectorAll('nav a:not([href="/"]), nav button').forEach(el => {
                el.style.display = 'none';
            });
            
            // Add class to body for CSS targeting
            document.body.classList.add('auth-page');
            
            console.log('Navigation hidden on login page');
        }
    }
    
    // Main initialization
    function initialize() {
        console.log('Auth fix initialization started');
        
        // Try to recover user info from token
        const recovered = tryRecoverUserInfo();
        
        // Setup fetch interceptor
        setupFetchInterceptor();
        
        // On DOM ready, update UI and apply auth to links
        document.addEventListener('DOMContentLoaded', function() {
            // Hide navigation on login pages
            hideNavOnLoginPage();
            
            // Check authentication status
            const hasToken = !!localStorage.getItem(TOKEN_KEY);
            
            // Set body attribute for CSS
            document.body.setAttribute('data-authenticated', hasToken ? 'true' : 'false');
            
            // If authenticated, apply auth to all links
            if (hasToken) {
                applyAuthToAllLinks();
                
                // If we recovered user info, reload page to apply UI changes
                if (recovered) {
                    window.location.reload();
                }
            }
            
            console.log('Auth fix initialization completed');
        });
    }
    
    // Initialize the fix
    initialize();
})();