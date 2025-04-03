// Service Worker Version
const CACHE_VERSION = 'v1';
const CACHE_NAME = `pmo-cache-${CACHE_VERSION}`;

// Assets to cache
const CACHED_ASSETS = [
    '/',
    '/index.html',
    '/css/global.css',
    '/js/notifications.js',
    '/js/app.js',
    '/images/notification-icon.png',
    '/images/notification-badge.png'
];

// Install event - cache assets
self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then((cache) => {
                console.log('Opened cache');
                return cache.addAll(CACHED_ASSETS);
            })
    );
});

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.map((cacheName) => {
                    if (cacheName !== CACHE_NAME) {
                        console.log('Deleting old cache:', cacheName);
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});

// Fetch event - serve from cache or network
self.addEventListener('fetch', (event) => {
    event.respondWith(
        caches.match(event.request)
            .then((response) => {
                if (response) {
                    return response;
                }
                return fetch(event.request).then(
                    (response) => {
                        if (!response || response.status !== 200 || response.type !== 'basic') {
                            return response;
                        }
                        const responseToCache = response.clone();
                        caches.open(CACHE_NAME)
                            .then((cache) => {
                                cache.put(event.request, responseToCache);
                            });
                        return response;
                    }
                );
            })
    );
});

// Push event - handle push notifications
self.addEventListener('push', (event) => {
    if (!event.data) {
        console.log('Push event but no data');
        return;
    }

    try {
        const data = event.data.json();
        const options = {
            body: data.body,
            icon: data.icon,
            badge: data.badge,
            vibrate: data.vibrate,
            data: data.data,
            actions: data.actions
        };

        event.waitUntil(
            self.registration.showNotification(data.title, options)
        );
    } catch (error) {
        console.error('Error handling push event:', error);
    }
});

// Notification click event
self.addEventListener('notificationclick', (event) => {
    event.notification.close();

    if (event.action === 'open') {
        const urlToOpen = event.notification.data?.url || '/';
        event.waitUntil(
            clients.openWindow(urlToOpen)
        );
    }
});

// Background sync for offline actions
self.addEventListener('sync', (event) => {
    if (event.tag === 'sync-notifications') {
        event.waitUntil(syncNotifications());
    }
});

// Function to sync notifications when back online
async function syncNotifications() {
    try {
        const registration = await self.registration;
        const subscription = await registration.pushManager.getSubscription();
        
        if (subscription) {
            // Re-subscribe to push notifications
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
        }
    } catch (error) {
        console.error('Error syncing notifications:', error);
    }
}

// Handle offline/online status
self.addEventListener('online', () => {
    self.registration.sync.register('sync-notifications');
});
