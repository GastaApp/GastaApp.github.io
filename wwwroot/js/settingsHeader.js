window.gastaSettingsHeader = {
    init(headerEl, sentinelEl) {
        if (!headerEl || !sentinelEl) return;
        const observer = new IntersectionObserver(
            ([entry]) => headerEl.classList.toggle('is-collapsed', !entry.isIntersecting),
            { threshold: 0 }
        );
        observer.observe(sentinelEl);
        headerEl._gastaObserver = observer; // stashed for cleanup on dispose
    },
    dispose(headerEl) {
        headerEl?._gastaObserver?.disconnect();
    }
};