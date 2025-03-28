document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('loginForm');
    const messageContainer = document.getElementById('message-container');
    
    if (form) {
        // Toggle password visibility
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
            const loadingIndicator = document.createElement('div');
            loadingIndicator.className = 'alert alert-info';
            loadingIndicator.innerHTML = '<i class="bi bi-hourglass-split me-2"></i>Signing in, please wait...';
            if (messageContainer) {
                messageContainer.appendChild(loadingIndicator);
            }
            
            // Disable the submit button
            const submitButton = form.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.disabled = true;
            }
            
            // Get form data
            const formData = new FormData(form);
            const jsonData = {
                email: formData.get('email'),
                password: formData.get('password'),
                rememberMe: formData.get('rememberMe') === 'on'
            };
            
            console.log('Login attempt with email:', jsonData.email);
            
            // Special handling for test admin account
            if (jsonData.email === "admin@test.com" && jsonData.password === "Password123!") {
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
                            // Store auth tokens
                            if (data.token) {
                                localStorage.setItem('auth_token', data.token);
                            }
                            if (data.refreshToken) {
                                localStorage.setItem('refresh_token', data.refreshToken);
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
                            
                            return data;
                        });
                    } else {
                        // Still failed with direct login
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
                
                return; // Skip the normal login flow
            }
            
            // Normal login flow for other accounts
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
                        // Store auth tokens
                        if (data.token) {
                            localStorage.setItem('auth_token', data.token);
                        }
                        if (data.refreshToken) {
                            localStorage.setItem('refresh_token', data.refreshToken);
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
        });
        
        // Helper function to handle login errors
        function handleLoginError(error, jsonData) {
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
            const submitButton = form.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.disabled = false;
            }
        }
    }
});