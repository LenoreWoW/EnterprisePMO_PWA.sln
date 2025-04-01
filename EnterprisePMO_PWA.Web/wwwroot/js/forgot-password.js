document.addEventListener('DOMContentLoaded', function() {
    // Initialize Tailwind form validation
    window.TailwindUI.Form.initValidation('forgotPasswordForm', {
        errorClass: 'border-red-500',
        successClass: 'border-green-500',
        errorMessageClass: 'text-sm text-red-500 mt-1'
    });

    const forgotPasswordForm = document.getElementById('forgotPasswordForm');
    const emailInput = document.getElementById('email');
    const submitButton = forgotPasswordForm.querySelector('button[type="submit"]');

    forgotPasswordForm.addEventListener('submit', async function(event) {
        event.preventDefault();

        // Validate form using Tailwind UI validation
        if (window.TailwindUI.Form.validateForm(forgotPasswordForm)) {
            const originalButtonText = submitButton.textContent;

            try {
                // Disable submit button and show loading state
                submitButton.disabled = true;
                submitButton.innerHTML = `
                    <svg class="animate-spin h-5 w-5 mr-3 inline" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Sending Reset Link...
                `;

                const resetRequestData = {
                    email: emailInput.value
                };

                const response = await fetch('/api/auth/reset-password', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(resetRequestData)
                });

                const result = await response.json();

                if (response.ok) {
                    // Success: Show success toast
                    window.TailwindUI.Toast.show({
                        message: result.message || 'Password reset link sent to your email.',
                        type: 'success'
                    });

                    // Optionally disable form after successful submission
                    forgotPasswordForm.reset();
                    forgotPasswordForm.querySelectorAll('input, button').forEach(el => el.disabled = true);
                } else {
                    // Error: Show error toast
                    window.TailwindUI.Toast.show({
                        message: result.message || 'Failed to send password reset link.',
                        type: 'error'
                    });
                }
            } catch (error) {
                console.error('Password reset request error:', error);
                
                // Network or unexpected error
                window.TailwindUI.Toast.show({
                    message: 'Network error. Please try again later.',
                    type: 'error'
                });
            } finally {
                // Restore button state
                submitButton.disabled = false;
                submitButton.textContent = originalButtonText;
            }
        }
    });
});