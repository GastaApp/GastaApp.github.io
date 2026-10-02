window.gastaAccent = {
    apply(css) {
        let tag = document.getElementById('gasta-accent-style');
        if (!tag) {
            tag = document.createElement('style');
            tag.id = 'gasta-accent-style';
            document.head.appendChild(tag);
        }
        tag.textContent = css;
    }
};