/**
 * Global site JavaScript for the EnterprisePMO application
 * Migrated to use Tailwind UI components
 */

document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips using Tailwind UI
    window.TailwindUI.Tooltip.init();
    
    // Initialize date pickers
    initDatePickers();
    
    // Register service worker for PWA
    registerServiceWorker();
    
    // Initialize header notification badge (only if authenticated)
    initNotificationBadge();
});

/**
 * Initializes date picker elements
 */
function initDatePickers() {
    // For simple date inputs, set default min date to today
    const dateInputs = document.querySelectorAll('input[type="date"]:not([min])');
    const today = new Date().toISOString().split('T')[0];
    
    dateInputs.forEach(input => {
        if (!input.hasAttribute('data-no-min')) {
            input.setAttribute('min', today);
        }
    });
}

/**
 * Registers the service worker for PWA functionality
 */
function registerServiceWorker() {
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register('/service-worker.js')
            .then(function(registration) {
                console.log('ServiceWorker registration successful with scope: ', registration.scope);
            })
            .catch(function(error) {
                console.log('ServiceWorker registration failed: ', error);
            });
    }
}

/**
 * Initializes the notification badge in the header
 */
function initNotificationBadge() {
    // Check if user is authenticated
    const isAuthenticated = 
        (document.body.getAttribute('data-authenticated') === 'true') || 
        (localStorage.getItem('auth_token') !== null);
    
    // Only fetch notifications if the user is authenticated
    if (isAuthenticated) {
        fetch('/api/notifications/unread/count')
            .then(response => {
                if (response.ok) {
                    return response.json();
                }
                throw new Error('Failed to fetch notification count');
            })
            .then(data => {
                const badge = document.getElementById('notificationCount');
                if (badge && data.count > 0) {
                    badge.textContent = data.count;
                    badge.classList.remove('d-none');
                }
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }
}

/**
 * Shows a toast notification using Tailwind UI
 * @param {string} message - The message to display
 * @param {string} type - The type of message (success, warning, error, info)
 */
function showToast(message, type = 'info') {
    window.TailwindUI.Toast.show({ 
        message, 
        type 
    });
}

/**
 * Confirms an action with the user
 * @param {string} message - Confirmation message
 * @param {function} callback - Function to call if confirmed
 */
function confirmAction(message, callback) {
    // Use browser's native confirm, or create a Tailwind modal for more styling
    if (confirm(message)) {
        callback();
    }
}

/**
 * Handles AJAX form submission
 * @param {HTMLFormElement} form - The form to submit
 * @param {function} successCallback - Function to call on success
 * @param {function} errorCallback - Function to call on error
 */
function submitFormAjax(form, successCallback, errorCallback) {
    // Use Tailwind's form validation
    if (!window.TailwindUI.Form.validateForm(form)) {
        return;
    }
    
    const formData = new FormData(form);
    
    fetch(form.action, {
        method: form.method,
        body: formData,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => {
        if (!response.ok) {
            return response.json().then(data => {
                throw new Error(data.message || 'Server error');
            });
        }
        return response.json();
    })
    .then(data => {
        if (successCallback) {
            successCallback(data);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        if (errorCallback) {
            errorCallback(error);
        }
    });
}

// Export utility functions
window.SiteUtils = {
    showToast,
    confirmAction,
    submitFormAjax,
    formatDate,
    formatCurrency
};