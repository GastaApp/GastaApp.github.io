let db;

export function openDb() {
    return new Promise((resolve, reject) => {
        // Version bumped 1 -> 2 to add the categoryBudgets store. onupgradeneeded only
        // fires when the requested version is HIGHER than what's already stored, so
        // this bump is what makes the new store actually get created for existing
        // users who already have a version-1 database — without it, this store would
        // silently never exist for anyone who installed the app before this change.
        const request = indexedDB.open('gasta-db', 2);
        request.onupgradeneeded = (event) => {
            db = event.target.result;
            ['expenses', 'categories', 'paymentMethods', 'budgets', 'categoryBudgets'].forEach(store => {
                if (!db.objectStoreNames.contains(store)) {
                    db.createObjectStore(store, { keyPath: 'id', autoIncrement: true });
                }
            });
        };
        request.onsuccess = (event) => { db = event.target.result; resolve(); };
        request.onerror = (event) => reject(event.target.error);
    });
}

export function getAll(storeName) {
    return new Promise((resolve, reject) => {
        const request = db.transaction(storeName, 'readonly').objectStore(storeName).getAll();
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

export function put(storeName, item) {
    return new Promise((resolve, reject) => {
        // C# int Id defaults to 0, which IndexedDB treats as a REAL explicit key —
        // not "no key". Without this, every new record with Id=0 overwrites the
        // previous one at key 0 instead of getting a fresh autoIncrement key.
        // Strip the key entirely when it's the C# default so autoIncrement kicks in;
        // only an explicit, already-assigned (non-zero) id is used to update a row.
        const toStore = { ...item };
        if (!toStore.id) {
            delete toStore.id;
        }
        const request = db.transaction(storeName, 'readwrite').objectStore(storeName).put(toStore);
        request.onsuccess = () => resolve(request.result); // resolves with the generated (or existing) key
        request.onerror = () => reject(request.error);
    });
}

export function remove(storeName, id) {
    return new Promise((resolve, reject) => {
        const request = db.transaction(storeName, 'readwrite').objectStore(storeName).delete(id);
        request.onsuccess = () => resolve();
        request.onerror = () => reject(request.error);
    });
}

export function clearAll() {
    return Promise.all(['expenses', 'categories', 'paymentMethods', 'budgets', 'categoryBudgets'].map(storeName =>
        new Promise((resolve, reject) => {
            const request = db.transaction(storeName, 'readwrite').objectStore(storeName).clear();
            request.onsuccess = () => resolve();
            request.onerror = () => reject(request.error);
        })));
}