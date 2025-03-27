document.addEventListener('DOMContentLoaded', function() {
    const resetPasswordForm = document.getElementById('resetPasswordForm');
    const passwordInput = document.getElementById('password');
    const confirmPasswordInput = document.getElementById('confirmPassword');
    const messageContainer = document.getElementById('message-container');
    const passwordStrengthContainer = document.getElementById('password-strength-container');

    // Password strength and validation function
    function checkPasswordStrength(password) {
        // Minimum requirements
        const minLength = 8;
        const hasUpperCase = /[A-Z]/.test(password);
        const hasLowerCase = /[a-z]/.test(password);
        const hasNumbers = /\d/.test(password);
        const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(password);

        // Create strength indicators
        passwordStrengthContainer.innerHTML = `
            <div class="progress mt-2" style="height: 5px;">
                <div id="passwordStrengthBar" class="progress-bar" role="progressbar" style="width: 0%"></div>
            </div>
            <small id="passwordStrengthText" class="text-muted"></small>
            <ul class="list-unstyled mt-2 small">
                <li id="lengthCheck" class="text-danger">
                    <i class="bi ${password.length >= minLength ? 'bi-check-circle text-success' : 'bi-x-circle'}"></i>
                    At least 8 characters long
                </li>
                <li id="uppercaseCheck" class="text-danger">
                    <i class="bi ${hasUpperCase ? 'bi-check-circle text-success' : 'bi-x-circle'}"></i>
                    Contains uppercase letter
                </li>
                <li id="lowercaseCheck" class="text-danger">
                    <i class="bi ${hasLowerCase ? 'bi-check-circle text-success' : 'bi-x-circle'}"></i>
                    Contains lowercase letter
                </li>
                <li id="numberCheck" class="text-danger">
                    <i class="bi ${hasNumbers ? 'bi-check-circle text-success' : 'bi-x-circle'}"></i>
                    Contains a number
                </li>
                <li id="specialCharCheck" class="text-danger">
                    <i class="bi ${hasSpecialChar ? 'bi-check-circle text-success' : 'bi-x-circle'}"></i>
                    Contains a special character
                </li>
            </ul>
        `;

        // Determine strength
        const strengthIndicators = [
            password.length >= minLength,
            hasUpperCase,
            hasLowerCase,
            hasNumbers,
            hasSpecialChar
        ];

        const passedCount = strengthIndicators.filter(Boolean).length;
        const passwordStrengthBar = document.getElementById('passwordStrengthBar');
        const passwordStrengthText = document.getElementById('passwordStrengthText');

        if (passedCount < 2) {
            passwordStrengthBar.style.width = '20%';
            passwordStrengthBar.classList.remove('bg-warning', 'bg-success');
            passwordStrengthBar.classList.add('bg-danger');
            passwordStrengthText.textContent = 'Weak';
        } else if (passedCount < 4) {
            passwordStrengthBar.style.width = '50%';
            passwordStrengthBar.classList.remove('bg-danger', 'bg-success');
            passwordStrengthBar.classList.add('bg-warning');
            passwordStrengthText.textContent = 'Medium';
        } else {
            passwordStrengthBar.style.width = '100%';
            passwordStrengthBar.classList.remove('bg-danger', 'bg-warning');
            passwordStrengthBar.classList.add('bg-success');
            passwordStrengthText.textContent = 'Strong';
        }
    }

    // Password visibility toggle
    const togglePassword = document.getElementById('togglePassword');
    const toggleConfirmPassword = document.getElementById('toggleConfirmPassword');

    function togglePasswordVisibility(input, toggleBtn) {
        const type = input.type === 'password' ? 'text' : 'password';
        input.type = type;
        const icon = toggleBtn.querySelector('i');
        icon.classList.toggle('bi-eye');
        icon.classList.toggle('bi-eye-slash');
    }

    togglePassword.addEventListener('click', () => togglePasswordVisibility(passwordInput, togglePassword));
    toggleConfirmPassword.addEventListener('click', () => togglePasswordVisibility(confirmPasswordInput, toggleConfirmPassword));

    // Password strength checking
    passwordInput.addEventListener('input', function() {
        checkPasswordStrength(this.value);
    });

    // Form submission handler
    resetPasswordForm.addEventListener('submit', async function(event) {
        event.preventDefault();
        event.stopPropagation();

        // Validate form
        if (!resetPasswordForm.checkValidity()) {
            resetPasswordForm.classList.add('was-validated');
            return;
        }

        const password = passwordInput.value;
        const confirmPassword = confirmPasswordInput.value;
        const token = document.getElementById('token').value;
        const email = document.getElementById('email').value;

        // Additional password validation
        if (password !== confirmPassword) {
            messageContainer.innerHTML = `
                <div class="alert alert-danger">
                    <i class="bi bi-exclamation-triangle-fill me-2"></i>
                    Passwords do not match
                </div>
            `;
            return;
        }

        try {
            const response = await fetch('/api/auth/confirm-reset-password', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    email,
                    token,
                    newPassword: password
                })
            });

            const result = await response.json();

            if (response.ok) {
                // Success: Show success message and redirect
                messageContainer.innerHTML = `
                    <div class="alert alert-success">
                        <i class="bi bi-check-circle-fill me-2"></i>
                        ${result.message || 'Password reset successfully.'}
                    </div>
                `;

                // Redirect to login after a short delay
                setTimeout(() => {
                    window.location.href = '/Account/Login';
                }, 2000);
            } else {
                // Error: Show error message
                messageContainer.innerHTML = `
                    <div class="alert alert-danger">
                        <i class="bi bi-exclamation-triangle-fill me-2"></i>
                        ${result.message || 'Failed to reset password.'}
                    </div>
                `;
            }
        } catch (error) {
            console.error('Password reset error:', error);
            
            // Network or unexpected error
            messageContainer.innerHTML = `
                <div class="alert alert-danger">
                    <i class="bi bi-exclamation-triangle-fill me-2"></i>
                    Network error. Please try again later.
                </div>
            `;
        }
    });
});