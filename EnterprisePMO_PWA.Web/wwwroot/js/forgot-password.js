document.addEventListener('DOMContentLoaded', function() {
    const forgotPasswordForm = document.getElementById('forgotPasswordForm');
    const emailInput = document.getElementById('email');
    const messageContainer = document.getElementById('message-container');

    forgotPasswordForm.addEventListener('submit', async function(event) {
        event.preventDefault();
        event.stopPropagation();

        // Basic form validation
        if (!forgotPasswordForm.checkValidity()) {
            forgotPasswordForm.classList.add('was-validated');
            return;
        }

        // Prepare reset request data
        const resetRequestData = {
            email: emailInput.value
        };

        try {
            // Send password reset request
            const response = await fetch('/api/auth/reset-password', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(resetRequestData)
            });

            const result = await response.json();

            if (response.ok) {
                // Success: Show success message
                messageContainer.innerHTML = `
                    <div class="alert alert-success">
                        <i class="bi bi-check-circle-fill me-2"></i>
                        ${result.message || 'Password reset link sent to your email.'}
                    </div>
                `;

                // Optionally disable form after successful submission
                forgotPasswordForm.reset();
                forgotPasswordForm.querySelectorAll('input, button').forEach(el => el.disabled = true);
            } else {
                // Error: Show error message
                messageContainer.innerHTML = `
                    <div class="alert alert-danger">
                        <i class="bi bi-exclamation-triangle-fill me-2"></i>
                        ${result.message || 'Failed to send password reset link.'}
                    </div>
                `;
            }
        } catch (error) {
            console.error('Password reset request error:', error);
            
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