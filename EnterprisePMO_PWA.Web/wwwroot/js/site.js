/**
 * Global site JavaScript for the EnterprisePMO application
 */

document.addEventListener('DOMContentLoaded', function() {
    // Initialize any tooltips
    initTooltips();
    
    // Initialize any date pickers
    initDatePickers();
    
    // Register service worker for PWA
    registerServiceWorker();
    
    // Initialize header notification badge (only if authenticated)
    initNotificationBadge();
});

/**
 * Initializes Bootstrap tooltips
 */
function initTooltips() {
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    if (typeof bootstrap !== 'undefined') {
        const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
    }
}

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
    // Check if user is authenticated by looking for auth token in localStorage
    // or by checking a data attribute on the body element
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
                // Don't show error message to user for notification count - fail silently
            });
    }
}

/**
 * Global form validation helper
 * @param {HTMLFormElement} form - The form element to validate
 * @returns {boolean} - True if valid, false otherwise
 */
function validateForm(form) {
    if (!form.checkValidity()) {
        event.preventDefault();
        event.stopPropagation();
        form.classList.add('was-validated');
        return false;
    }
    return true;
}

/**
 * Formats a date in a user-friendly format
 * @param {string} dateString - ISO date string
 * @returns {string} - Formatted date string
 */
function formatDate(dateString) {
    const options = { year: 'numeric', month: 'short', day: 'numeric' };
    return new Date(dateString).toLocaleDateString(undefined, options);
}

/**
 * Formats currency values
 * @param {number} value - The value to format
 * @returns {string} - Formatted currency string
 */
function formatCurrency(value) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(value);
}

/**
 * Shows a toast notification
 * @param {string} message - The message to display
 * @param {string} type - The type of message (success, warning, error, info)
 */
function showToast(message, type = 'info') {
    // Create toast container if it doesn't exist
    let toastContainer = document.querySelector('.toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }
    
    // Map type to Bootstrap class
    const typeClass = {
        'success': 'bg-success',
        'warning': 'bg-warning',
        'error': 'bg-danger',
        'info': 'bg-info'
    }[type] || 'bg-info';
    
    // Create toast element
    const toastId = 'toast-' + Date.now();
    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center ${typeClass} text-white border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;
    
    // Add toast to container
    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    
    // Initialize and show the toast
    const toastElement = document.getElementById(toastId);
    if (typeof bootstrap !== 'undefined') {
        const toast = new bootstrap.Toast(toastElement, { autohide: true, delay: 5000 });
        toast.show();
    }
    
    // Remove toast from DOM after it's hidden
    toastElement.addEventListener('hidden.bs.toast', function() {
        toastElement.remove();
    });
}

/**
 * Confirms an action with the user
 * @param {string} message - Confirmation message
 * @param {function} callback - Function to call if confirmed
 */
function confirmAction(message, callback) {
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
    if (!validateForm(form)) {
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