/**
 * Consolidated authentication service for Enterprise PMO
 * Combines functionality from auth.js, auth-fix.js, and auth-handler.js
 */
(function() {
  // Constants
  const TOKEN_KEY = 'auth_token';
  const REFRESH_TOKEN_KEY = 'refresh_token';
  const USER_KEY = 'user_info';
  const EXPIRES_AT_KEY = 'expires_at';

  /**
   * Get token from localStorage or query string
   * @returns {string|null} Authentication token
   */
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

  /**
   * Store authentication data in localStorage
   * @param {string} token - JWT auth token
   * @param {string} refreshToken - Refresh token (optional)
   * @param {Object} user - User data (optional)
   * @param {number} expiresIn - Expiration time in seconds (optional)
   */
  function setTokens(token, refreshToken, user, expiresIn) {
    if (token) {
      localStorage.setItem(TOKEN_KEY, token);
    }
    
    if (refreshToken) {
      localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    }
    
    if (user) {
      localStorage.setItem(USER_KEY, JSON.stringify(user));
    } else if (token) {
      // Try to extract user info from token
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const userInfo = {
          id: payload.nameid || payload.sub,
          username: payload.unique_name || payload.email,
          role: payload.role
        };
        localStorage.setItem(USER_KEY, JSON.stringify(userInfo));
      } catch (e) {
        console.error('Failed to extract user info from token:', e);
      }
    }
    
    if (expiresIn) {
      const expiresAt = new Date();
      expiresAt.setSeconds(expiresAt.getSeconds() + expiresIn);
      localStorage.setItem(EXPIRES_AT_KEY, expiresAt.toISOString());
    }
  }

  /**
   * Clear all auth data from localStorage (logout)
   */
  function clearTokens() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(EXPIRES_AT_KEY);
  }

  /**
   * Get current authenticated user info
   * @returns {Object|null} User info object or null
   */
  function getCurrentUser() {
    const userJson = localStorage.getItem(USER_KEY);
    if (!userJson) return null;
    
    try {
      return JSON.parse(userJson);
    } catch (e) {
      console.error('Error parsing user info:', e);
      return null;
    }
  }

  /**
   * Check if user is authenticated
   * @returns {boolean} True if authenticated
   */
  function isAuthenticated() {
    const token = getToken();
    if (!token) return false;
    
    // Check if token is expired
    const expiresAt = localStorage.getItem(EXPIRES_AT_KEY);
    if (expiresAt) {
      const expiryDate = new Date(expiresAt);
      if (expiryDate < new Date()) {
        return false;
      }
    }
    
    return true;
  }

  /**
   * Login with email and password
   * @param {string} email - User email
   * @param {string} password - User password
   * @returns {Promise<Object>} Login result
   */
  async function login(email, password) {
    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ email, password })
      });
      
      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Login failed');
      }
      
      const data = await response.json();
      
      // Save authentication data
      setTokens(
        data.token, 
        data.refreshToken, 
        data.user, 
        data.expiresIn
      );
      
      // Update UI state
      updateAuthUI(true);
      
      return data;
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  }

  /**
   * Submit signup form data
   * @param {Object} formData - User signup data
   * @returns {Promise<Object>} Signup result
   */
  async function signup(formData) {
    try {
      const response = await fetch('/api/auth/signup', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(formData)
      });
      
      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Signup failed');
      }
      
      const data = await response.json();
      
      // Auto login if token returned
      if (data.token) {
        setTokens(
          data.token, 
          data.refreshToken, 
          data.user, 
          data.expiresIn
        );
        updateAuthUI(true);
      }
      
      return data;
    } catch (error) {
      console.error('Signup error:', error);
      throw error;
    }
  }

  /**
   * Logout user
   * @returns {Promise<void>}
   */
  async function logout() {
    const token = getToken();
    
    try {
      if (token) {
        await fetch('/api/auth/logout', {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });
      }
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      // Always clear tokens regardless of API success
      clearTokens();
      updateAuthUI(false);
      
      // Redirect to login
      window.location.href = '/Account/Login';
    }
  }

  /**
   * Refresh authentication token
   * @returns {Promise<Object>} New token data
   */
  async function refreshToken() {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }
    
    try {
      const response = await fetch('/api/auth/refresh-token', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ refreshToken })
      });
      
      if (!response.ok) {
        throw new Error('Failed to refresh token');
      }
      
      const data = await response.json();
      
      setTokens(
        data.token, 
        data.refreshToken, 
        data.user, 
        data.expiresIn
      );
      
      return data;
    } catch (error) {
      console.error('Token refresh error:', error);
      clearTokens();
      throw error;
    }
  }

  /**
   * Set up fetch interceptor to add auth token to requests
   */
  function setupFetchInterceptor() {
    const originalFetch = window.fetch;
    
    window.fetch = function(url, options = {}) {
      // Don't add auth header for login/signup requests
      if (typeof url === 'string' && 
          (url.includes('/api/auth/login') || 
           url.includes('/api/auth/signup') || 
           url.includes('/api/auth/direct-login'))) {
        return originalFetch(url, options);
      }
      
      // Clone options to avoid modification of original
      const newOptions = {...options};
      newOptions.headers = newOptions.headers || {};
      
      // Add auth token if available and not already added
      const token = getToken();
      if (token && !newOptions.headers.Authorization && !newOptions.headers.authorization) {
        newOptions.headers.Authorization = `Bearer ${token}`;
      }
      
      return originalFetch(url, newOptions)
        .then(response => {
          // Handle 401 Unauthorized
          if (response.status === 401) {
            console.error('401 Unauthorized response received');
            clearTokens();
            
            // Redirect to login if not already there
            if (!window.location.pathname.includes('/Account/Login')) {
              window.location.href = '/Account/Login';
            }
            
            return Promise.reject(new Error('Unauthorized'));
          }
          return response;
        });
    };
  }

  /**
   * Apply auth token to links with data-requires-auth attribute
   */
  function applyAuthToLinks() {
    const token = getToken();
    if (!token) return;
    
    // Add token to links
    document.querySelectorAll('[data-requires-auth="true"]').forEach(link => {
      if (!link.href.includes('auth_token=')) {
        try {
          const url = new URL(link.href);
          url.searchParams.set('auth_token', token);
          link.href = url.toString();
        } catch (e) {
          console.warn('Error updating auth link:', link, e);
        }
      }
    });
  }

  /**
   * Update UI elements based on auth state
   * @param {boolean} isAuth - Whether user is authenticated
   */
  function updateAuthUI(isAuth) {
    // Set data attribute on body
    document.body.setAttribute('data-authenticated', isAuth ? 'true' : 'false');
    
    // Show/hide navigation elements
    const navItems = document.querySelectorAll('.nav-items');
    navItems.forEach(el => {
      el.style.display = isAuth ? 'flex' : 'none';
    });
    
    // Show/hide auth-dependent items
    const authItems = document.querySelectorAll('.nav-auth-item');
    authItems.forEach(el => {
      el.style.display = isAuth ? 'block' : 'none';
    });
    
    // Show/hide non-auth items
    const noAuthItems = document.querySelectorAll('.nav-no-auth-item');
    noAuthItems.forEach(el => {
      el.style.display = isAuth ? 'none' : 'flex'; 
    });
    
    // Update username display if authenticated
    if (isAuth) {
      const user = getCurrentUser();
      if (user) {
        const usernameDivs = document.querySelectorAll('.current-username');
        usernameDivs.forEach(el => {
          el.textContent = user.username;
        });
        
        // Update admin-only elements visibility
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
      
      // Apply auth token to links
      applyAuthToLinks();
    }
  }

  /**
   * Initialize the auth system
   */
  function init() {
    console.log('Initializing auth system...');
    
    // Set up fetch interceptor
    setupFetchInterceptor();
    
    // Try to extract user info from token if needed
    const token = getToken();
    if (token && !localStorage.getItem(USER_KEY)) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const userInfo = {
          id: payload.nameid || payload.sub,
          username: payload.unique_name || payload.email,
          role: payload.role
        };
        localStorage.setItem(USER_KEY, JSON.stringify(userInfo));
      } catch (e) {
        console.error('Failed to extract user info from token:', e);
      }
    }
    
    // Wait for DOM to be loaded
    document.addEventListener('DOMContentLoaded', function() {
      // Check if we need to refresh the token
      const expiresAt = localStorage.getItem(EXPIRES_AT_KEY);
      if (expiresAt) {
        const expiresAtDate = new Date(expiresAt);
        const now = new Date();
        
        // If token expires within the next hour, refresh it
        if (expiresAtDate <= new Date(now.getTime() + 60 * 60 * 1000)) {
          refreshToken().catch(error => {
            console.warn('Token refresh failed:', error);
            // Continue with initialization anyway
            updateAuthUI(isAuthenticated());
          });
        } else {
          updateAuthUI(isAuthenticated());
        }
      } else {
        // No expiration info, just check if we have a token
        updateAuthUI(isAuthenticated());
      }
      
      // Set up login form handler
      const loginForm = document.getElementById('loginForm');
      if (loginForm) {
        loginForm.addEventListener('submit', function(e) {
          e.preventDefault();
          
          const emailField = document.getElementById('Email');
          const passwordField = document.getElementById('Password');
          const errorDiv = document.getElementById('loginErrorMessage');
          
          if (!emailField || !passwordField) {
            console.error('Email or password field not found');
            return;
          }
          
          // Show loading state
          if (errorDiv) {
            errorDiv.style.display = 'none';
          }
          
          const submitBtn = loginForm.querySelector('button[type="submit"]');
          if (submitBtn) {
            submitBtn.disabled = true;
            const originalText = submitBtn.innerHTML;
            submitBtn.innerHTML = '<svg class="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg> Signing in...';
          }
          
          // Use direct login for admin@test.com if needed
          const email = emailField.value;
          const password = passwordField.value;
          const endpoint = email === 'admin@test.com' 
            ? '/api/auth/direct-login'
            : '/api/auth/login';
          
          login(email, password)
            .then(() => {
              // Redirect to dashboard
              window.location.href = '/Dashboard';
            })
            .catch(error => {
              // Show error
              if (errorDiv) {
                errorDiv.textContent = error.message || 'Invalid email or password';
                errorDiv.style.display = 'block';
              }
              
              // Reset submit button
              if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = 'Sign in';
              }
            });
        });
      }
      
      // Set up logout form handler
      document.querySelectorAll('form#logoutForm').forEach(form => {
        form.addEventListener('submit', function(e) {
          e.preventDefault();
          logout();
        });
      });
    });
  }

  // Export the auth API
  window.authManager = {
    getToken,
    setTokens,
    clearTokens,
    getCurrentUser,
    isAuthenticated,
    login,
    signup,
    logout,
    refreshToken,
    updateAuthUI,
    applyAuthToLinks
  };
  
  // Initialize
  init();
})();