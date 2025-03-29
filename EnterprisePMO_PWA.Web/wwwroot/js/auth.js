// auth.js - Place this in your wwwroot/js folder
(function() {
    // Auth Token Management
    const TOKEN_KEY = 'auth_token';
    const USER_KEY = 'auth_user';
  
    // Get token from localStorage or query string
    function getToken() {
      // First try from localStorage
      let token = localStorage.getItem(TOKEN_KEY);
      
      // If not in localStorage, check query string
      if (!token) {
        const urlParams = new URLSearchParams(window.location.search);
        token = urlParams.get('auth_token');
        
        // If found in query string, save to localStorage
        if (token) {
          localStorage.setItem(TOKEN_KEY, token);
          
          // Clean up URL by removing token parameter
          const newUrl = window.location.pathname + 
            window.location.search.replace(/[\?&]auth_token=[^&]+(&|$)/, '$1');
          window.history.replaceState({}, document.title, newUrl);
        }
      }
      
      return token;
    }
  
    // Set token with optional expiration
    function setToken(token, user) {
      localStorage.setItem(TOKEN_KEY, token);
      
      if (user) {
        localStorage.setItem(USER_KEY, JSON.stringify(user));
      }
      
      // Update UI state
      updateAuthUI(true);
    }
  
    // Remove token and user info (logout)
    function clearToken() {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(USER_KEY);
      
      // Update UI state
      updateAuthUI(false);
    }
  
    // Get current user info
    function getCurrentUser() {
      const userJson = localStorage.getItem(USER_KEY);
      return userJson ? JSON.parse(userJson) : null;
    }
  
    // Check if user is authenticated
    function isAuthenticated() {
      return !!getToken();
    }
  
    // Update UI based on authentication state
    function updateAuthUI(isAuth) {
      document.body.dataset.authenticated = isAuth ? 'true' : 'false';
      
      // Hide/show login/signup buttons and profile menu
      const authButtons = document.querySelectorAll('.auth-buttons');
      const profileMenu = document.querySelectorAll('.profile-menu');
      
      if (authButtons.length) {
        authButtons.forEach(el => {
          el.style.display = isAuth ? 'none' : 'flex';
        });
      }
      
      if (profileMenu.length) {
        profileMenu.forEach(el => {
          el.style.display = isAuth ? 'flex' : 'none';
        });
      }
      
      // Update username display if available
      const user = getCurrentUser();
      if (user) {
        const usernameDivs = document.querySelectorAll('.current-username');
        if (usernameDivs.length) {
          usernameDivs.forEach(el => {
            el.textContent = user.username;
          });
        }
        
        const roleDivs = document.querySelectorAll('.current-role');
        if (roleDivs.length) {
          roleDivs.forEach(el => {
            el.textContent = user.role;
          });
        }
        
        // Show/hide role-specific elements
        if (user.role === 'Admin' || user.role === 'MainPMO') {
          document.querySelectorAll('.admin-only').forEach(el => {
            el.style.display = '';
          });
        } else {
          document.querySelectorAll('.admin-only').forEach(el => {
            el.style.display = 'none';
          });
        }
      }
      
      // Add auth token to all protected links
      if (isAuth) {
        const token = getToken();
        if (token) {
          document.querySelectorAll('[data-requires-auth="true"]').forEach(link => {
            if (!link.href.includes('auth_token=')) {
              const url = new URL(link.href, window.location.origin);
              url.searchParams.set('auth_token', token);
              link.href = url.toString();
            }
          });
        }
      }
    }
  
    // Setup fetch interceptor to add authorization header
    function setupFetchInterceptor() {
      const originalFetch = window.fetch;
      
      window.fetch = function(url, options = {}) {
        // Don't add auth header for login/signup requests
        if (typeof url === 'string' && (url.includes('/api/auth/login') || url.includes('/api/auth/signup') || url.includes('/api/auth/direct-login'))) {
          return originalFetch(url, options);
        }
        
        // Clone the options to avoid modifying the original object
        const newOptions = { ...options };
        newOptions.headers = newOptions.headers || {};
        
        // Add authorization header if token exists
        const token = getToken();
        if (token && !newOptions.headers.Authorization) {
          newOptions.headers.Authorization = `Bearer ${token}`;
        }
        
        return originalFetch(url, newOptions)
          .then(response => {
            // Handle 401 Unauthorized
            if (response.status === 401) {
              clearToken();
              window.location.href = '/Account/Login';
              return Promise.reject(new Error('Unauthorized'));
            }
            return response;
          });
      };
    }
  
    // Initialize auth system
    function init() {
      // Setup fetch interceptor
      setupFetchInterceptor();
      
      // Wait for DOM to be ready before setting up form handlers and UI
      document.addEventListener('DOMContentLoaded', function() {
        // Setup login form handler
        const loginForm = document.getElementById('loginForm');
        if (loginForm) {
          loginForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const emailField = document.getElementById('Email');
            const passwordField = document.getElementById('Password');
            
            if (!emailField || !passwordField) {
              console.error('Email or password field not found');
              return;
            }
            
            const email = emailField.value;
            const password = passwordField.value;
            
            // Use direct login for admin@test.com
            const endpoint = email === 'admin@test.com' 
              ? '/api/auth/direct-login'
              : '/api/auth/login';
            
            fetch(endpoint, {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json'
              },
              body: JSON.stringify({
                email: email,
                password: password
              })
            })
            .then(response => {
              if (!response.ok) {
                throw new Error('Login failed');
              }
              return response.json();
            })
            .then(data => {
              // Save token and user info
              setToken(data.token, data.user);
              
              // Redirect to dashboard
              window.location.href = '/Dashboard';
            })
            .catch(error => {
              console.error('Login error:', error);
              const errorMsg = document.getElementById('loginErrorMessage');
              if (errorMsg) {
                errorMsg.textContent = 'Invalid email or password. Please try again.';
                errorMsg.style.display = 'block';
              }
            });
          });
        }
        
        // Initial UI update based on authentication state
        updateAuthUI(isAuthenticated());
      });
    }
  
    // Export auth functions to window
    window.authManager = {
      getToken,
      setToken,
      clearToken,
      getCurrentUser,
      isAuthenticated,
      updateAuthUI
    };
    
    // Initialize
    init();
  })();