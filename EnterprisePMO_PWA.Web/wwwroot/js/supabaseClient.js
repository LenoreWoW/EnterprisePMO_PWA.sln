import { createClient } from 'https://cdn.jsdelivr.net/npm/@supabase/supabase-js/+esm';

// Get the Supabase URL and anon key from the environment
const supabaseUrl = 'https://pffjdvahsgmtybxrhnla.supabase.co';
const supabaseAnonKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InBmZmpkdmFoc2dtdHlieHJobmxhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDI2ODk3NDYsImV4cCI6MjA1ODI2NTc0Nn0.6OPXiW2QwxvA42F1XcN83bHmtdM7NhulvDaqXxIE9hk';

// Initialize the Supabase client
export const supabase = createClient(supabaseUrl, supabaseAnonKey);

/**
 * Represents the authentication service for the application
 */
class AuthService {
    /**
     * Sign in to the application with email and password
     * This uses our custom backend API which handles both Supabase auth and our own user DB
     * @param {string} email - The user's email
     * @param {string} password - The user's password
     * @returns {Promise<Object>} - The authentication result including tokens and user info
     */
    async signIn(email, password) {
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
                throw new Error(error.message || 'Failed to sign in');
            }
            
            const data = await response.json();
            this.saveAuthData(data);
            return data;
        } catch (error) {
            console.error('Sign in error:', error);
            throw error;
        }
    }
    
    /**
     * Sign up for a new account
     * This uses our custom backend API which handles both Supabase auth and our own user DB
     * @param {Object} userData - The user data for registration
     * @returns {Promise<Object>} - The registration result
     */
    async signUp(userData) {
        try {
            const response = await fetch('/api/auth/signup', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(userData)
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to sign up');
            }
            
            const data = await response.json();
            
            // If we get tokens back (auto sign-in after registration), save them
            if (data.token) {
                this.saveAuthData(data);
            }
            
            return data;
        } catch (error) {
            console.error('Sign up error:', error);
            throw error;
        }
    }
    
    /**
     * Sign out from the application
     * This uses our custom backend API which handles both Supabase auth and our own user DB
     * @returns {Promise<void>}
     */
    async signOut() {
        try {
            const token = this.getToken();
            if (!token) {
                this.clearAuthData();
                return;
            }
            
            const response = await fetch('/api/auth/logout', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
            
            // Clear local auth data regardless of server response
            this.clearAuthData();
            
            if (!response.ok) {
                console.warn('Sign out not completed on server side');
            }
        } catch (error) {
            console.error('Sign out error:', error);
            // Still clear local auth data even on error
            this.clearAuthData();
        }
    }
    
    /**
     * Refresh the authentication token
     * @returns {Promise<Object>} - The new token and refresh token
     */
    async refreshToken() {
        try {
            const refreshToken = localStorage.getItem('refresh_token');
            if (!refreshToken) {
                throw new Error('No refresh token available');
            }
            
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
            this.saveAuthData(data);
            return data;
        } catch (error) {
            console.error('Token refresh error:', error);
            // If we can't refresh, sign the user out
            this.clearAuthData();
            throw error;
        }
    }
    
    /**
     * Request a password reset email
     * @param {string} email - The user's email address
     * @returns {Promise<Object>} - The response from the server
     */
    async resetPassword(email) {
        try {
            const response = await fetch('/api/auth/reset-password', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email })
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to send password reset email');
            }
            
            return await response.json();
        } catch (error) {
            console.error('Password reset error:', error);
            throw error;
        }
    }
    
    /**
     * Save authentication data to local storage
     * @param {Object} data - The authentication data
     * @private
     */
    saveAuthData(data) {
        if (data.token) {
            localStorage.setItem('auth_token', data.token);
        }
        
        if (data.refreshToken) {
            localStorage.setItem('refresh_token', data.refreshToken);
        }
        
        if (data.user) {
            localStorage.setItem('user_info', JSON.stringify(data.user));
        }
        
        if (data.expiresIn) {
            const expiresAt = new Date();
            expiresAt.setSeconds(expiresAt.getSeconds() + data.expiresIn);
            localStorage.setItem('expires_at', expiresAt.toISOString());
        }
    }
    
    /**
     * Clear all authentication data from local storage
     * @private
     */
    clearAuthData() {
        localStorage.removeItem('auth_token');
        localStorage.removeItem('refresh_token');
        localStorage.removeItem('user_info');
        localStorage.removeItem('expires_at');
    }
    
    /**
     * Get the current authentication token
     * @returns {string|null} - The authentication token or null if not available
     */
    getToken() {
        return localStorage.getItem('auth_token');
    }
    
    /**
     * Get the current user info
     * @returns {Object|null} - The user info or null if not logged in
     */
    getCurrentUser() {
        const userInfo = localStorage.getItem('user_info');
        return userInfo ? JSON.parse(userInfo) : null;
    }
    
    /**
     * Check if the user is currently authenticated
     * @returns {boolean} - True if authenticated, false otherwise
     */
    isAuthenticated() {
        const token = this.getToken();
        if (!token) {
            return false;
        }
        
        // Check if token is expired
        const expiresAt = localStorage.getItem('expires_at');
        if (expiresAt) {
            const expiryDate = new Date(expiresAt);
            if (expiryDate < new Date()) {
                // Token is expired
                return false;
            }
        }
        
        return true;
    }
    
    /**
     * Check if the current user has a specific role
     * @param {string} role - The role to check
     * @returns {boolean} - True if the user has the role, false otherwise
     */
    hasRole(role) {
        const user = this.getCurrentUser();
        return user && user.role === role;
    }
    
    /**
     * Get an authenticated fetch configuration with the Authorization header
     * @param {Object} options - Additional fetch options
     * @returns {Object} - The fetch options with Authorization header
     */
    getAuthFetchOptions(options = {}) {
        const token = this.getToken();
        const headers = { 
            ...options.headers,
            'Authorization': `Bearer ${token}`
        };
        
        return {
            ...options,
            headers
        };
    }
    
    /**
     * Initialize authentication from session storage
     * This should be called on application startup
     * @returns {Promise<void>}
     */
    async initAuth() {
        try {
            // Check if we have a token but it's expired
            if (this.getToken() && !this.isAuthenticated()) {
                // Try to refresh the token
                await this.refreshToken();
            }
        } catch (error) {
            console.error('Error initializing authentication:', error);
            this.clearAuthData();
        }
    }
}

// Create and export the auth service instance
export const authService = new AuthService();

// Initialize authentication when this module is loaded
authService.initAuth().catch(console.error);