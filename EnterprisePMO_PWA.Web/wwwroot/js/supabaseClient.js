// supabaseClient.js - Enhanced Supabase Authentication Client

import { createClient } from 'https://cdn.jsdelivr.net/npm/@supabase/supabase-js/+esm';

// Supabase connection details
const supabaseUrl = 'https://pffjdvahsgmtybxrhnla.supabase.co';
const supabaseAnonKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InBmZmpkdmFoc2dtdHlieHJobmxhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDI2ODk3NDYsImV4cCI6MjA1ODI2NTc0Nn0.6OPXiW2QwxvA42F1XcN83bHmtdM7NhulvDaqXxIE9hk';

// Initialize the Supabase client
export const supabase = createClient(supabaseUrl, supabaseAnonKey);

/**
 * Authentication service using Supabase
 */
class SupabaseAuthService {
  /**
   * Sign in with email and password
   * @param {string} email - User email
   * @param {string} password - User password
   * @returns {Promise<Object>} Auth session data
   */
  async signIn(email, password) {
    try {
      const { data, error } = await supabase.auth.signInWithPassword({
        email,
        password
      });
      
      if (error) throw error;
      
      // Save auth data to local storage
      this._saveAuthData(data);
      
      // Sync with backend for custom user data
      await this._syncUserWithBackend(data.session);
      
      return data;
    } catch (error) {
      console.error('Sign in error:', error);
      throw error;
    }
  }
  
  /**
   * Sign up with email and password
   * @param {Object} userData - User registration data
   * @returns {Promise<Object>} Auth session data
   */
  async signUp(userData) {
    try {
      const { email, password, firstName, lastName, departmentId } = userData;
      
      // Register with Supabase
      const { data: authData, error } = await supabase.auth.signUp({
        email,
        password,
        options: {
          data: {
            first_name: firstName,
            last_name: lastName
          }
        }
      });
      
      if (error) throw error;
      
      // Register with our backend to store additional user info
      const backendResponse = await fetch('/api/auth/register-user', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authData.session?.access_token || ''}`
        },
        body: JSON.stringify({
          email,
          firstName,
          lastName,
          departmentId,
          supabaseId: authData.user.id
        })
      });
      
      if (!backendResponse.ok) {
        const backendError = await backendResponse.json();
        throw new Error(backendError.message || 'Failed to register user with backend');
      }
      
      // Save auth data if we have a session already (auto confirmation is enabled)
      if (authData.session) {
        this._saveAuthData(authData);
      }
      
      return authData;
    } catch (error) {
      console.error('Sign up error:', error);
      throw error;
    }
  }
  
  /**
   * Sign out the current user
   * @returns {Promise<void>}
   */
  async signOut() {
    try {
      const { error } = await supabase.auth.signOut();
      if (error) throw error;
      
      // Clear local auth data
      this._clearAuthData();
      
      // Notify our backend about the sign out
      try {
        await fetch('/api/auth/logout', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          }
        });
      } catch (backendError) {
        console.warn('Backend logout notification failed:', backendError);
        // Continue with sign out even if backend notification fails
      }
    } catch (error) {
      console.error('Sign out error:', error);
      // Still clear local data even if Supabase sign out fails
      this._clearAuthData();
      throw error;
    }
  }
  
  /**
   * Request password reset email
   * @param {string} email - User email
   * @returns {Promise<Object>} Result of the password reset request
   */
  async resetPassword(email) {
    try {
      const { data, error } = await supabase.auth.resetPasswordForEmail(email, {
        redirectTo: `${window.location.origin}/account/reset-password`
      });
      
      if (error) throw error;
      return { success: true, message: 'Password reset email sent' };
    } catch (error) {
      console.error('Password reset error:', error);
      throw error;
    }
  }
  
  /**
   * Update user password
   * @param {string} newPassword - New password
   * @returns {Promise<Object>} Result of password update
   */
  async updatePassword(newPassword) {
    try {
      const { data, error } = await supabase.auth.updateUser({
        password: newPassword
      });
      
      if (error) throw error;
      return { success: true, message: 'Password updated successfully' };
    } catch (error) {
      console.error('Update password error:', error);
      throw error;
    }
  }
  
  /**
   * Get the current user session
   * @returns {Promise<Object|null>} Current session or null
   */
  async getSession() {
    try {
      const { data, error } = await supabase.auth.getSession();
      if (error) throw error;
      
      return data.session;
    } catch (error) {
      console.error('Get session error:', error);
      return null;
    }
  }
  
  /**
   * Check if user is authenticated
   * @returns {Promise<boolean>} Authentication status
   */
  async isAuthenticated() {
    const session = await this.getSession();
    return !!session;
  }
  
  /**
   * Get the current user
   * @returns {Promise<Object|null>} User data or null
   */
  async getCurrentUser() {
    try {
      const { data, error } = await supabase.auth.getUser();
      if (error) throw error;
      
      // Get additional user data from our backend
      try {
        const response = await fetch('/api/users/me', {
          headers: {
            'Authorization': `Bearer ${data.user.aud}`
          }
        });
        
        if (response.ok) {
          const userData = await response.json();
          return { ...data.user, ...userData };
        }
      } catch (backendError) {
        console.warn('Failed to fetch extended user data:', backendError);
      }
      
      return data.user;
    } catch (error) {
      console.error('Get current user error:', error);
      return null;
    }
  }
  
  /**
   * Setup auth state change handler
   * @param {Function} callback - Function to call on auth state change
   * @returns {Function} Unsubscribe function
   */
  onAuthStateChange(callback) {
    const { data } = supabase.auth.onAuthStateChange((event, session) => {
      callback(event, session);
      
      // Sync with backend on auth changes
      if (event === 'SIGNED_IN' && session) {
        this._syncUserWithBackend(session).catch(console.error);
      }
    });
    
    return data.subscription.unsubscribe;
  }
  
  /**
   * Save authentication data to local storage
   * @param {Object} data - Auth data from Supabase
   * @private
   */
  _saveAuthData(data) {
    if (data.session) {
      localStorage.setItem('auth_token', data.session.access_token);
      localStorage.setItem('refresh_token', data.session.refresh_token);
      
      // Store expiry time
      const expiresAt = new Date();
      expiresAt.setSeconds(expiresAt.getSeconds() + data.session.expires_in);
      localStorage.setItem('expires_at', expiresAt.toISOString());
    }
    
    if (data.user) {
      localStorage.setItem('user_info', JSON.stringify(data.user));
    }
  }
  
  /**
   * Clear all authentication data
   * @private
   */
  _clearAuthData() {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('user_info');
    localStorage.removeItem('expires_at');
  }
  
  /**
   * Sync Supabase user with our backend
   * @param {Object} session - Supabase session
   * @private
   */
  async _syncUserWithBackend(session) {
    if (!session?.access_token) return;
    
    try {
      const response = await fetch('/api/auth/sync-user', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${session.access_token}`
        },
        body: JSON.stringify({
          supabaseId: session.user.id
        })
      });
      
      if (!response.ok) {
        throw new Error('Failed to sync user with backend');
      }
      
      return await response.json();
    } catch (error) {
      console.error('Error syncing user with backend:', error);
      throw error;
    }
  }
}

// Create and export the auth service instance
export const authService = new SupabaseAuthService();

// Initialize auth on module load
document.addEventListener('DOMContentLoaded', async () => {
  try {
    // Setup auth state change listener
    authService.onAuthStateChange((event, session) => {
      console.log('Auth state changed:', event);
      
      // Update UI based on auth state
      if (event === 'SIGNED_IN' || event === 'TOKEN_REFRESHED') {
        document.body.setAttribute('data-authenticated', 'true');
        // Show authenticated UI elements
        document.querySelectorAll('.nav-items, .nav-auth-item').forEach(el => {
          el.style.display = el.tagName === 'NAV' ? 'flex' : 'block';
        });
        document.querySelectorAll('.nav-no-auth-item').forEach(el => {
          el.style.display = 'none';
        });
        
        // Update username display
        if (session?.user) {
          document.querySelectorAll('.current-username').forEach(el => {
            el.textContent = session.user.email;
          });
        }
      } else if (event === 'SIGNED_OUT') {
        document.body.setAttribute('data-authenticated', 'false');
        // Show non-authenticated UI elements
        document.querySelectorAll('.nav-items, .nav-auth-item').forEach(el => {
          el.style.display = 'none';
        });
        document.querySelectorAll('.nav-no-auth-item').forEach(el => {
          el.style.display = 'flex';
        });
      }
    });
    
    // Check initial auth state
    const isAuthenticated = await authService.isAuthenticated();
    document.body.setAttribute('data-authenticated', isAuthenticated ? 'true' : 'false');
    
  } catch (error) {
    console.error('Error initializing auth:', error);
  }
});