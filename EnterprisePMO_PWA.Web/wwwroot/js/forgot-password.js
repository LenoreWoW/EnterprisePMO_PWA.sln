document.addEventListener('DOMContentLoaded', function() {
    // Initialize Tailwind form validation
    window.TailwindUI.Form.initValidation('forgotPasswordForm', {
        errorClass: 'border-red-500',
        successClass: 'border-green-500',
        errorMessageClass: 'text-sm text-red-500 mt-1'
    });

    const forgotPasswordForm = document.getElementById('forgotPasswordForm');
    const emailInput = document.getElementById('email');
    const messageContainer = document.getElementById('message-container');

    forgotPasswordForm.addEventListener('submit', async function(event) {
        event.preventDefault();

        // Validate form using Tailwind UI validation
        if (window.TailwindUI.Form.validateForm(forgotPasswordForm)) {
            const resetRequestData = {
                email: emailInput.value
            };

            try {
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
            }
        }
    });
});