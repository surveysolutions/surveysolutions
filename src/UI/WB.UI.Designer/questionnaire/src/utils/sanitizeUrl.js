const BLANK_URL = 'about:blank';
const INVALID_PROTOCOL = /^([^\w]*)(javascript|vbscript|data):/i;

export function sanitizeUrl(url = BLANK_URL) {
    try {
        const decoded = decodeURIComponent(url.replace(/\0/g, '')).replace(/\s/g, '');
        return INVALID_PROTOCOL.test(decoded) ? BLANK_URL : url;
    } catch {
        return BLANK_URL;
    }
}
