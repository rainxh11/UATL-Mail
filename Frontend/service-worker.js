
const cacheName = '#CACHENAME';
const appShellFiles = '#FILES';

self.addEventListener('install', (e) => {
  console.log('[Service Worker] Install');
  e.waitUntil((async () => {
    const cache = await caches.open(cacheName);
    console.log('[Service Worker] Caching all: app shell and content');
    await cache.addAll(appShellFiles);
  })());
});

self.addEventListener('fetch', (e) => {
  e.respondWith((async () => {
    const r = await caches.match(e.request);
    if (r) { 
      console.log(`[Service Worker] Fetching cached resource: ${e.request.url}`);
      return r; 
    }
    const response = await fetch(e.request);
    try {
      if (e.request.match(/(.js|.png|.otf|.woff2|.ttf|.html|.css|.svg|.jpg)$/gim) !== null) {
        const cache = await caches.open(cacheName);
        console.log(`[Service Worker] Caching new resource: ${e.request.url}`);
        cache.put(e.request, response.clone());
      }
    } catch{
      
    }   
    return response;
  })());
});

self.addEventListener('activate', (e) => {
  e.waitUntil(caches.keys().then((keyList) => {
    return Promise.all(keyList.map((key) => {
      if (key === cacheName) { return; }
      return caches.delete(key);
    }))
  }));
});