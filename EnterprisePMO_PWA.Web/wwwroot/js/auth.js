/**
 * Authentication handler for the EnterprisePMO application
 * Consolidates functionality from login.js and direct-login.js
 */
document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('loginForm');
    const messageContainer = document.getElementById('message-container');
    
    if (form) {
        // Toggle password visibility
        setupPasswordToggle();
        
        // Set up form submission
        form.addEventListener('submit', function(event) {
            event.preventDefault();
            
            if (!form.checkValidity()) {
                event.stopPropagation();
                form.classList.add('was-validated');
                return;
            }
            
            // Clear previous messages
            if (messageContainer) {
                messageContainer.innerHTML = '';
            }
            
            // Show loading indicator
            showLoadingIndicator();
            
            // Get form data
            const formData = new FormData(form);
            const jsonData = {
                email: formData.get('email'),
                password: formData.get('password'),
                rememberMe: formData.get('rememberMe') === 'on'
            };
            
            console.log('Login attempt with email:', jsonData.email);
            
            // Check if this is a test account
            if (jsonData.email === "admin@test.com" && jsonData.password === "Password123!") {
                handleDirectLogin(jsonData);
            } else {
                handleStandardLogin(jsonData);
            }
        });
    }
});

/**
 * Sets up password visibility toggle functionality
 */
function setupPasswordToggle() {
    const togglePassword = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('password');
    
    if (togglePassword && passwordInput) {
        togglePassword.addEventListener('click', function() {
            const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
            passwordInput.setAttribute('type', type);
            
            // Toggle eye icon
            const eyeIcon = this.querySelector('i');
            if (eyeIcon) {
                eyeIcon.classList.toggle('bi-eye');
                eyeIcon.classList.toggle('bi-eye-slash');
            }
        });
    }
}

/**
 * Shows a loading indicator in the message container
 */
function showLoadingIndicator() {
    const messageContainer = document.getElementById('message-container');
    const submitButton = document.querySelector('button[type="submit"]');
    
    // Show loading state
    if (submitButton) {
        submitButton.originalInnerHTML = submitButton.innerHTML;
        submitButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> Logging in...';
        submitButton.disabled = true;
    }
    
    // Add loading message
    if (messageContainer) {
        const loadingIndicator = document.createElement('div');
        loadingIndicator.className = 'alert alert-info';
        loadingIndicator.innerHTML = '<i class="bi bi-hourglass-split me-2"></i>Signing in, please wait...';
        messageContainer.appendChild(loadingIndicator);
    }
}

/**
 * Handles login for the test admin account using simplified direct login
 * @param {Object} jsonData - The login form data
 */
function handleDirectLogin(jsonData) {
    const messageContainer = document.getElementById('message-container');
    
    console.log("Using simplified login for test admin");
    
    // Use the simpler direct-login endpoint that bypasses complex authentication
    fetch('/api/auth/direct-login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(jsonData)
    })
    .then(response => {
        // Remove loading indicator
        if (messageContainer) {
            messageContainer.innerHTML = '';
        }
        
        if (response.ok) {
            return response.json().then(data => {
                handleLoginSuccess(data);
                return data;
            });
        } else {
            return response.json().then(errorData => {
                throw new Error(errorData.message || 'Test login failed');
            }).catch(error => {
                if (error instanceof SyntaxError) {
                    throw new Error('Test login failed');
                }
                throw error;
            });
        }
    })
    .catch(error => {
        handleLoginError(error, jsonData);
    });
}

/**
 * Handles standard login through the normal auth flow
 * @param {Object} jsonData - The login form data
 */
function handleStandardLogin(jsonData) {
    const messageContainer = document.getElementById('message-container');
    
    fetch('/api/auth/login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(jsonData)
    })
    .then(response => {
        // Remove loading indicator
        if (messageContainer) {
            messageContainer.innerHTML = '';
        }
        
        if (response.ok) {
            return response.json().then(data => {
                handleLoginSuccess(data);
                return data;
            });
        } else {
            return response.json().then(errorData => {
                throw new Error(errorData.message || 'Invalid email or password');
            }).catch(error => {
                // If response is not valid JSON
                if (error instanceof SyntaxError) {
                    throw new Error('Invalid email or password');
                }
                throw error;
            });
        }
    })
    .catch(error => {
        handleLoginError(error, jsonData);
    });
}

/**
 * Handles a successful login response
 * @param {Object} data - The successful login response data
 */
function handleLoginSuccess(data) {
    const messageContainer = document.getElementById('message-container');
    
    // Store auth tokens
    if (data.token) {
        localStorage.setItem('auth_token', data.token);
    }
    if (data.refreshToken) {
        localStorage.setItem('refresh_token', data.refreshToken);
    }
    if (data.user) {
        localStorage.setItem('user_info', JSON.stringify(data.user));
    }
    
    // Show success message
    const successDiv = document.createElement('div');
    successDiv.className = 'alert alert-success';
    successDiv.innerHTML = '<i class="bi bi-check-circle-fill me-2"></i>Login successful! Redirecting...';
    if (messageContainer) {
        messageContainer.appendChild(successDiv);
    }
    
    // Redirect to home page or dashboard
    setTimeout(() => {
        window.location.href = '/Dashboard/Index';
    }, 1000);
}

/**
 * Handles login errors and displays appropriate messages
 * @param {Error} error - The error that occurred
 * @param {Object} jsonData - The login form data
 */
function handleLoginError(error, jsonData) {
    const messageContainer = document.getElementById('message-container');
    const submitButton = document.querySelector('button[type="submit"]');
    
    console.error('Login error:', error);
    
    // Show error message
    const errorDiv = document.createElement('div');
    errorDiv.className = 'alert alert-danger';
    
    // Try using the admin test account as a fallback suggestion
    if (jsonData.email !== "admin@test.com") {
        errorDiv.innerHTML = `<i class="bi bi-exclamation-triangle-fill me-2"></i>${error.message || 'An error occurred during login.'} <br><small>Try using the test account: admin@test.com / Password123!</small>`;
    } else {
        errorDiv.innerHTML = `<i class="bi bi-exclamation-triangle-fill me-2"></i>${error.message || 'An error occurred during login. Please try again.'}`;
    }
    
    if (messageContainer) {
        messageContainer.appendChild(errorDiv);
    }
    
    // Re-enable the submit button
    if (submitButton) {
        if (submitButton.originalInnerHTML) {
            submitButton.innerHTML = submitButton.originalInnerHTML;
        }
        submitButton.disabled = false;
    }
}