window.gastaPointer = {
    capture(el, pointerId) {
        if (el && el.setPointerCapture) {
            try {
                el.setPointerCapture(pointerId);
            } catch (e) {
                // Pointer may already be released/invalid by the time this runs — safe to ignore.
            }
        }
    }
};
