window.gastaTheme = {
    apply(option) {
        const root = document.documentElement;
        // System option removed — only Light and Dark are supported now.
        // Anything unexpected falls back to light rather than checking matchMedia.
        root.setAttribute('data-theme', option === 'Dark' ? 'dark' : 'light');
    }
};