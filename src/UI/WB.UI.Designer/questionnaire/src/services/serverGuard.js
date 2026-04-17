// Ensure localization is initialized before we need to use i18next
import '../plugins/localization';
import i18next from 'i18next';

// Capture the native fetch at module load time, before any patching by installFetchGuard.
const _nativeFetch = window.fetch;

const OVERLAY_ID = '__srv-guard-overlay';
const EXPECTED_SERVER_TOKEN = '773994826649214';

function getMessage() {
    if (i18next.isInitialized) {
        return i18next.t('QuestionnaireEditor.ApplicationNotAvailable');
    }
    return "";
}

export function checkServerHeader(headerValue) {
    if (headerValue !== EXPECTED_SERVER_TOKEN) {
        blockUIForever();
    }
}

export function blockUIForever() {
    if (document.getElementById(OVERLAY_ID)) return;
    const overlay = document.createElement('div');
    overlay.id = OVERLAY_ID;
    overlay.style.cssText =
        'position:fixed;top:0;left:0;width:100%;height:100%;' +
        'background:rgba(0,0,0,0.75);z-index:99999;display:flex;' +
        'align-items:center;justify-content:center;';
    const box = document.createElement('div');
    box.style.cssText =
        'color:#fff;font-size:1.25rem;font-family:sans-serif;' +
        'padding:2rem 3rem;text-align:center;border-radius:8px;' +
        'background:rgba(0,0,0,0.5);white-space:pre-line;max-width:600px;';
    box.textContent = getMessage();
    overlay.appendChild(box);
    document.body.appendChild(overlay);

    if (!i18next.isInitialized) {
        i18next.on('initialized', () => {
            box.textContent = i18next.t('QuestionnaireEditor.ApplicationNotAvailable');
        });
    }
}

export function installFetchGuard() {
    if (installFetchGuard.installed) return;
    installFetchGuard.installed = true;

    const originalFetch = window.fetch;
    window.fetch = async function (...args) {
        const response = await originalFetch.apply(this, args);
        const url = args[0] instanceof Request ? args[0].url : String(args[0]);
        try {
            if (new URL(url, window.location.href).origin === window.location.origin) {
                checkServerHeader(response.headers.get('X-Survey-Solutions'));
            }
        } catch {
            // Unparseable URL — skip the guard check.
        }
        return response;
    };
}

installFetchGuard.installed = false;

let pageGuardInstalled = false;

export function installPageGuard() {
    if (pageGuardInstalled) return;
    pageGuardInstalled = true;

    const run = async () => {
        try {
            const headResponse = await _nativeFetch(window.location.href, { method: 'HEAD', cache: 'no-store' });
            if (headResponse.status === 405) {
                // Some endpoints do not expose custom headers on HEAD; retry once with GET.
                const getResponse = await _nativeFetch(window.location.href, { method: 'GET', cache: 'no-store' });
                checkServerHeader(getResponse.headers.get('X-Survey-Solutions'));
                return;
            }

            checkServerHeader(headResponse.headers.get('X-Survey-Solutions'));
        } catch {
            // network error — do not block
        }
    };
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', run, { once: true });
    } else {
        run();
    }
}

export function installServerGuards({ page = true, fetch = false } = {}) {
    if (page) installPageGuard();
    if (fetch) installFetchGuard();
}
