import { ref, onMounted, onUnmounted } from 'vue';

/**
 * Returns a boolean ref that becomes true while the given key combination is held.
 * @param {(e: KeyboardEvent) => boolean} check - returns true if the event matches the shortcut
 */
export function useKeyShortcut(check) {
    const active = ref(false);

    function onKeydown(e) {
        if (e.type === 'keydown' && check(e)) {
            e.preventDefault();
            active.value = true;
        }
    }

    function resetActive() {
        active.value = false;
    }

    function onKeyup() {
        resetActive();
    }

    function onVisibilityChange() {
        if (document.visibilityState !== 'visible') {
            resetActive();
        }
    }

    onMounted(() => {
        window.addEventListener('keydown', onKeydown, { passive: false });
        window.addEventListener('keyup', onKeyup);
        window.addEventListener('blur', resetActive);
        document.addEventListener('visibilitychange', onVisibilityChange);
    });

    onUnmounted(() => {
        window.removeEventListener('keydown', onKeydown);
        window.removeEventListener('keyup', onKeyup);
        window.removeEventListener('blur', resetActive);
        document.removeEventListener('visibilitychange', onVisibilityChange);
    });

    return active;
}
