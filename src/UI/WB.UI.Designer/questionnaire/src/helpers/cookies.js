export function getCookie(name) {
    const match = document.cookie.match(new RegExp('(?:^|; )' + name + '=([^;]*)'));
    if (!match) return null;

    try {
        return JSON.parse(decodeURIComponent(match[1]));
    } catch {
        removeCookie(name);
        return null;
    }
}

export function setCookie(name, value, days) {
    const expires = new Date(Date.now() + days * 864e5).toUTCString();
    document.cookie = `${name}=${encodeURIComponent(JSON.stringify(value))}; expires=${expires}; path=/`;
}

export function removeCookie(name) {
    document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/`;
}

export function hasCookie(name) {
    return document.cookie.split(';').some(c => c.trim().startsWith(name + '='));
}
