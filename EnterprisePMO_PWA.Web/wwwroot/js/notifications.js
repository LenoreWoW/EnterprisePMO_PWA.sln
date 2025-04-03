// Service Worker Registration
async function registerServiceWorker() {
    if ('serviceWorker' in navigator && 'PushManager' in window) {
        try {
            const registration = await navigator.serviceWorker.register('/service-worker.js');
            console.log('Service Worker registered with scope:', registration.scope);
            return registration;
        } catch (error) {
            console.error('Service Worker registration failed:', error);
            throw error;
        }
    }
    throw new Error('Push notifications are not supported in this browser');
}

// Push Notification Subscription
async function subscribeToPushNotifications() {
    try {
        const registration = await registerServiceWorker();
        
        // Fetch VAPID public key from server
        const response = await fetch('/api/notification/vapid-public-key');
        const { publicKey } = await response.json();
        
        const subscription = await registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: urlBase64ToUint8Array(publicKey)
        });

        // Send subscription to server
        await fetch('/api/notification/subscribe', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                endpoint: subscription.endpoint,
                p256dh: btoa(String.fromCharCode.apply(null, new Uint8Array(subscription.getKey('p256dh')))),
                auth: btoa(String.fromCharCode.apply(null, new Uint8Array(subscription.getKey('auth'))))
            })
        });

        console.log('Successfully subscribed to push notifications');
        return subscription;
    } catch (error) {
        console.error('Failed to subscribe to push notifications:', error);
        throw error;
    }
}

// Push Notification Unsubscription
async function unsubscribeFromPushNotifications() {
    try {
        const registration = await navigator.serviceWorker.ready;
        const subscription = await registration.pushManager.getSubscription();
        
        if (subscription) {
            await subscription.unsubscribe();
            await fetch('/api/notification/unsubscribe', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    endpoint: subscription.endpoint
                })
            });
            console.log('Successfully unsubscribed from push notifications');
        }
    } catch (error) {
        console.error('Failed to unsubscribe from push notifications:', error);
        throw error;
    }
}

// Notification Preferences
async function getNotificationPreferences() {
    try {
        const response = await fetch('/api/notification/preferences');
        if (!response.ok) throw new Error('Failed to get notification preferences');
        return await response.json();
    } catch (error) {
        console.error('Error getting notification preferences:', error);
        throw error;
    }
}

async function updateNotificationPreferences(preferences) {
    try {
        const response = await fetch('/api/notification/preferences', {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(preferences)
        });
        if (!response.ok) throw new Error('Failed to update notification preferences');
        return await response.json();
    } catch (error) {
        console.error('Error updating notification preferences:', error);
        throw error;
    }
}

// Get User Notifications
async function getUserNotifications() {
    try {
        const response = await fetch('/api/notification/notifications');
        if (!response.ok) throw new Error('Failed to get notifications');
        return await response.json();
    } catch (error) {
        console.error('Error getting notifications:', error);
        throw error;
    }
}

// Mark Notification as Read
async function markNotificationAsRead(notificationId) {
    try {
        const response = await fetch(`/api/notification/notifications/${notificationId}/read`, {
            method: 'PUT'
        });
        if (!response.ok) throw new Error('Failed to mark notification as read');
        return await response.json();
    } catch (error) {
        console.error('Error marking notification as read:', error);
        throw error;
    }
}

// Utility function to convert VAPID key
function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
        .replace(/\-/g, '+')
        .replace(/_/g, '/');

    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

// Initialize notification UI
function initializeNotificationUI() {
    const notificationToggle = document.getElementById('notification-toggle');
    if (notificationToggle) {
        notificationToggle.addEventListener('change', async (e) => {
            try {
                if (e.target.checked) {
                    await subscribeToPushNotifications();
                } else {
                    await unsubscribeFromPushNotifications();
                }
            } catch (error) {
                console.error('Error toggling notifications:', error);
                e.target.checked = !e.target.checked;
            }
        });
    }

    // Load initial state
    navigator.serviceWorker.ready.then(async (registration) => {
        const subscription = await registration.pushManager.getSubscription();
        if (notificationToggle) {
            notificationToggle.checked = !!subscription;
        }
    });
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    if ('serviceWorker' in navigator && 'PushManager' in window) {
        initializeNotificationUI();
    }
});

// Test email configuration
async function testEmailConfiguration() {
    try {
        const response = await fetch('/api/notification/test-email', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(error);
        }

        const result = await response.text();
        console.log('Test email result:', result);
        alert('Test email sent successfully! Please check your inbox.');
    } catch (error) {
        console.error('Failed to send test email:', error);
        alert('Failed to send test email: ' + error.message);
    }
}

// Export functions for use in other modules
export {
    registerServiceWorker,
    subscribeToPushNotifications,
    unsubscribeFromPushNotifications,
    getNotificationPreferences,
    updateNotificationPreferences,
    getUserNotifications,
    markNotificationAsRead,
    initializeNotificationUI,
    testEmailConfiguration
}; 