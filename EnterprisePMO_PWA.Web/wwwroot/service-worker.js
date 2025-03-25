const CACHE_NAME = 'enterprise-pmo-cache-v2';
const urlsToCache = [
  '/',
  '/css/site.css',
  '/js/supabaseClient.js',
  '/manifest.json'
];

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME).then(cache => cache.addAll(urlsToCache))
  );
});

self.addEventListener('fetch', event => {
  event.respondWith(
    caches.match(event.request).then(response => response || fetch(event.request))
  );
});

self.addEventListener('sync', event => {
  if (event.tag === 'sync-notifications') {
    event.waitUntil(syncNotifications());
  }
});

async function syncNotifications() {
  console.log('Background sync triggered');
  try {
    const response = await fetch('/api/notifications/sync');
    const result = await response.json();
    console.log('Sync result:', result);
  } catch (err) {
    console.error('Error during sync:', err);
  }
}
