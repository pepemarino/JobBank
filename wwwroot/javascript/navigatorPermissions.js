(function () {
    if (!navigator.permissions) return;
    const original = navigator.permissions.query;

    navigator.permissions.query = function (desc) {
        // Only log in local/dev 
        const isDev = location.hostname === 'localhost' || location.hostname === '127.0.0.1';
        if (isDev) {
            console.warn('navigator.permissions.query called with:', desc);
            console.warn(new Error('permissions.query call stack').stack);
        }
        return original.call(navigator.permissions, desc);
    };
})();