document.addEventListener('DOMContentLoaded', function() {
    // Load departments
    fetch('/api/departments')
        .then(function(response) { return response.json(); })
        .then(function(departments) {
            const departmentSelect = document.getElementById('department');
            
            departments.forEach(function(dept) {
                const option = document.createElement('option');
                option.value = dept.id;
                option.textContent = dept.name;
                departmentSelect.appendChild(option);
            });
        })
        .catch(function(error) {
            console.error('Error loading departments:', error);
        });
    
    // Toggle password visibility functions
    const togglePassword = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('password');
    
    togglePassword.addEventListener('click', function() {
        const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
        passwordInput.setAttribute('type', type);
        
        // Toggle eye icon
        const eyeIcon = this.querySelector('i');
        eyeIcon.classList.toggle('bi-eye');
        eyeIcon.classList.toggle('bi-eye-slash');
    });
    
    const toggleConfirmPassword = document.getElementById('toggleConfirmPassword');
    const confirmPasswordInput = document.getElementById('confirmPassword');
    
    toggleConfirmPassword.addEventListener('click', function() {
        const type = confirmPasswordInput.getAttribute('type') === 'password' ? 'text' : 'password';
        confirmPasswordInput.setAttribute('type', type);
        
        // Toggle eye icon
        const eyeIcon = this.querySelector('i');
        eyeIcon.classList.toggle('bi-eye');
        eyeIcon.classList.toggle('bi-eye-slash');
    });
    
    // Form validation
    const form = document.getElementById('signupForm');
    
    form.addEventListener('submit', function(event) {
        if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
        }
        
        // Check if passwords match
        const password = document.getElementById('password').value;
        const confirmPassword = document.getElementById('confirmPassword').value;
        
        if (password !== confirmPassword) {
            document.getElementById('confirmPassword').setCustomValidity('Passwords do not match');
        } else {
            document.getElementById('confirmPassword').setCustomValidity('');
        }
        
        form.classList.add('was-validated');
        
        // If form is valid, submit via AJAX
        if (form.checkValidity()) {
            event.preventDefault();
            
            const formData = new FormData(form);
            const jsonData = {};
            
            formData.forEach(function(value, key) {
                jsonData[key] = value;
            });
            
            fetch('/api/auth/signup', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(jsonData)
            })
            .then(function(response) {
                if (response.ok) {
                    return response.json();
                } else {
                    return response.json().then(function(data) {
                        throw new Error(data.message || 'Error creating account');
                    });
                }
            })
            .then(function(data) {
                // Show success message and redirect
                const successDiv = document.createElement('div');
                successDiv.className = 'alert alert-success mt-3';
                successDiv.innerHTML = '<i class="bi bi-check-circle-fill me-2"></i>Account created successfully! Redirecting to login...';
                
                const messageContainer = document.getElementById('message-container');
                messageContainer.innerHTML = '';
                messageContainer.appendChild(successDiv);
                
                // Redirect after a delay
                setTimeout(function() {
                    window.location.href = '/Account/Login';
                }, 2000);
            })
            .catch(function(error) {
                // Show error message
                const errorDiv = document.createElement('div');
                errorDiv.className = 'alert alert-danger mt-3';
                errorDiv.innerHTML = '<i class="bi bi-exclamation-triangle-fill me-2"></i>' + error.message;
                
                const messageContainer = document.getElementById('message-container');
                messageContainer.innerHTML = '';
                messageContainer.appendChild(errorDiv);
            });
        }
    });
    
    // Real-time password confirmation validation
    document.getElementById('confirmPassword').addEventListener('input', function() {
        const password = document.getElementById('password').value;
        const confirmPassword = this.value;
        
        if (password !== confirmPassword) {
            this.setCustomValidity('Passwords do not match');
        } else {
            this.setCustomValidity('');
        }
    });
});