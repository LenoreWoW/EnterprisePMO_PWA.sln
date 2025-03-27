document.addEventListener('DOMContentLoaded', function() {
    // Load departments
    fetchDepartmentsForDropdown();
    
    // Toggle password visibility functions
    setupPasswordToggles();
    
    // Form validation and submission
    setupFormValidation();
});

/**
 * Fetches departments from the API and populates the dropdown
 */
function fetchDepartmentsForDropdown() {
    fetch('/api/departments')
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to load departments');
            }
            return response.json();
        })
        .then(departments => {
            const departmentSelect = document.getElementById('department');
            
            if (departmentSelect) {
                departments.forEach(function(dept) {
                    const option = document.createElement('option');
                    option.value = dept.id;
                    option.textContent = dept.name;
                    departmentSelect.appendChild(option);
                });
            }
        })
        .catch(function(error) {
            console.error('Error loading departments:', error);
            const messageContainer = document.getElementById('message-container');
            if (messageContainer) {
                messageContainer.innerHTML = `
                    <div class="alert alert-warning" role="alert">
                        <i class="bi bi-exclamation-triangle-fill me-2"></i>
                        Could not load departments. Please try refreshing the page.
                    </div>
                `;
            }
        });
}

/**
 * Sets up password visibility toggle functionality
 */
function setupPasswordToggles() {
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
    
    const toggleConfirmPassword = document.getElementById('toggleConfirmPassword');
    const confirmPasswordInput = document.getElementById('confirmPassword');
    
    if (toggleConfirmPassword && confirmPasswordInput) {
        toggleConfirmPassword.addEventListener('click', function() {
            const type = confirmPasswordInput.getAttribute('type') === 'password' ? 'text' : 'password';
            confirmPasswordInput.setAttribute('type', type);
            
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
 * Sets up form validation and handles form submission
 */
function setupFormValidation() {
    const form = document.getElementById('signupForm');
    
    if (!form) return;
    
    // Real-time password confirmation validation
    const confirmPasswordInput = document.getElementById('confirmPassword');
    const passwordInput = document.getElementById('password');
    
    if (confirmPasswordInput && passwordInput) {
        confirmPasswordInput.addEventListener('input', function() {
            const password = passwordInput.value;
            const confirmPassword = this.value;
            
            if (password !== confirmPassword) {
                this.setCustomValidity('Passwords do not match');
            } else {
                this.setCustomValidity('');
            }
        });
    }
    
    form.addEventListener('submit', function(event) {
        event.preventDefault();
        event.stopPropagation();
        
        // Basic validation
        if (!form.checkValidity()) {
            form.classList.add('was-validated');
            return;
        }
        
        // Check if passwords match
        const password = document.getElementById('password').value;
        const confirmPassword = document.getElementById('confirmPassword').value;
        
        if (password !== confirmPassword) {
            document.getElementById('confirmPassword').setCustomValidity('Passwords do not match');
            form.classList.add('was-validated');
            return;
        }
        
        // Prepare form data for submission
        const formData = new FormData(form);
        const jsonData = {};
        
        formData.forEach(function(value, key) {
            jsonData[key] = value;
        });
        
        // Log the data being sent (for debugging)
        console.log("Sending signup data:", jsonData);
        
        // Clear any previous messages
        const messageContainer = document.getElementById('message-container');
        messageContainer.innerHTML = '';
        
        // Show loading indicator
        const loadingIndicator = document.createElement('div');
        loadingIndicator.className = 'alert alert-info';
        loadingIndicator.innerHTML = '<i class="bi bi-hourglass-split me-2"></i>Creating your account, please wait...';
        messageContainer.appendChild(loadingIndicator);
        
        // Disable the submit button to prevent multiple submissions
        const submitButton = form.querySelector('button[type="submit"]');
        if (submitButton) {
            submitButton.disabled = true;
        }
        
        // Send the request
        fetch('/api/auth/signup', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(jsonData)
        })
        .then(function(response) {
            // Remove loading indicator
            messageContainer.innerHTML = '';
            
            if (response.ok) {
                return response.json().then(function(data) {
                    // Show success message
                    const successDiv = document.createElement('div');
                    successDiv.className = 'alert alert-success';
                    successDiv.innerHTML = '<i class="bi bi-check-circle-fill me-2"></i>Account created successfully! Redirecting to login...';
                    messageContainer.appendChild(successDiv);
                    
                    // Store authentication token if provided
                    if (data.token) {
                        localStorage.setItem('auth_token', data.token);
                    }
                    if (data.refreshToken) {
                        localStorage.setItem('refresh_token', data.refreshToken);
                    }
                    
                    // Redirect after a delay
                    setTimeout(function() {
                        window.location.href = '/Account/Login';
                    }, 2000);
                    
                    return data;
                });
            } else {
                // Handle HTTP errors (400, 500, etc.)
                return response.json().then(function(errorData) {
                    throw new Error(errorData.message || 'Failed to create account');
                });
            }
        })
        .catch(function(error) {
            // Show error message
            const errorDiv = document.createElement('div');
            errorDiv.className = 'alert alert-danger';
            errorDiv.innerHTML = `<i class="bi bi-exclamation-triangle-fill me-2"></i>${error.message || 'An error occurred. Please try again.'}`;
            messageContainer.innerHTML = '';
            messageContainer.appendChild(errorDiv);
            
            // Re-enable the submit button
            if (submitButton) {
                submitButton.disabled = false;
            }
            
            // Log the error for debugging
            console.error('Signup error:', error);
        });
    });
}