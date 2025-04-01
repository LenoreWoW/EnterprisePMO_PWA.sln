document.addEventListener('DOMContentLoaded', function() {
    // Load departments
    fetchDepartmentsForDropdown();
    
    // Toggle password visibility
    setupPasswordToggles();
    
    // Initialize Tailwind form validation
    window.TailwindUI.Form.initValidation('signupForm', {
        errorClass: 'border-red-500',
        successClass: 'border-green-500',
        errorMessageClass: 'text-sm text-red-500 mt-1'
    });
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
            
            // Use Tailwind toast for error notification
            window.TailwindUI.Toast.show({
                message: 'Could not load departments. Please try refreshing the page.',
                type: 'warning'
            });
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
            const type = passwordInput.type === 'password' ? 'text' : 'password';
            passwordInput.type = type;
            
            // Toggle visibility icon (assuming you're using Lucide icons or similar)
            this.innerHTML = type === 'password' 
                ? '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-eye"><path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7z"/><circle cx="12" cy="12" r="3"/></svg>'
                : '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-eye-off"><path d="M9.88 9.88a3 3 0 104.24 4.24"/><path d="M10.73 5.08A10.43 10.43 0 0112 5c7 0 10 7 10 7a13.16 13.16 0 01-1.67 2.68"/><path d="M6.61 6.61A13.526 13.526 0 002 12s3 7 10 7a9.74 9.74 0 005.39-1.61"/><line x1="2" x2="22" y1="2" y2="22"/></svg>';
        });
    }
    
    const toggleConfirmPassword = document.getElementById('toggleConfirmPassword');
    const confirmPasswordInput = document.getElementById('confirmPassword');
    
    if (toggleConfirmPassword && confirmPasswordInput) {
        toggleConfirmPassword.addEventListener('click', function() {
            const type = confirmPasswordInput.type === 'password' ? 'text' : 'password';
            confirmPasswordInput.type = type;
            
            // Toggle visibility icon
            this.innerHTML = type === 'password' 
                ? '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-eye"><path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7z"/><circle cx="12" cy="12" r="3"/></svg>'
                : '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-eye-off"><path d="M9.88 9.88a3 3 0 104.24 4.24"/><path d="M10.73 5.08A10.43 10.43 0 0112 5c7 0 10 7 10 7a13.16 13.16 0 01-1.67 2.68"/><path d="M6.61 6.61A13.526 13.526 0 002 12s3 7 10 7a9.74 9.74 0 005.39-1.61"/><line x1="2" x2="22" y1="2" y2="22"/></svg>';
        });
    }
}

// Form submission logic (slightly modified to use Tailwind UI)
document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('signupForm');
    
    if (form) {
        form.addEventListener('submit', function(event) {
            event.preventDefault();
            
            // Validate form using Tailwind UI validation
            if (window.TailwindUI.Form.validateForm(form)) {
                // Prepare form data
                const formData = new FormData(form);
                const jsonData = {};
                
                formData.forEach((value, key) => {
                    jsonData[key] = value;
                });
                
                // Disable submit button
                const submitButton = form.querySelector('button[type="submit"]');
                submitButton.disabled = true;
                submitButton.classList.add('opacity-50', 'cursor-not-allowed');
                
                // Send signup request
                fetch('/api/auth/signup', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(jsonData)
                })
                .then(response => {
                    if (response.ok) {
                        return response.json();
                    }
                    throw new Error('Signup failed');
                })
                .then(data => {
                    // Use Tailwind toast for success
                    window.TailwindUI.Toast.show({
                        message: 'Account created successfully!',
                        type: 'success'
                    });
                    
                    // Redirect to login
                    setTimeout(() => {
                        window.location.href = '/Account/Login';
                    }, 2000);
                })
                .catch(error => {
                    // Use Tailwind toast for error
                    window.TailwindUI.Toast.show({
                        message: error.message || 'Signup failed. Please try again.',
                        type: 'error'
                    });
                    
                    // Re-enable submit button
                    submitButton.disabled = false;
                    submitButton.classList.remove('opacity-50', 'cursor-not-allowed');
                });
            }
        });
    }
});