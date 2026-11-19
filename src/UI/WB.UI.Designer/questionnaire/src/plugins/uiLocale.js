const RTL_LOCALES = new Set(['ar']);

export function normalizeUiLocale(locale) {
    const normalized = String(locale || 'en').trim().replace('_', '-');
    return (normalized.split('-')[0] || 'en').toLowerCase();
}

export function isRtlLocale(locale) {
    return RTL_LOCALES.has(normalizeUiLocale(locale));
}

export function getTextDirection(value) {
    const text = String(value ?? '');
    const firstStrongCharacter = text.match(/\p{L}/u)?.[0];

    return firstStrongCharacter && /[\u0590-\u05ff\u0600-\u06ff\u0750-\u077f\u08a0-\u08ff]/u.test(firstStrongCharacter)
        ? 'rtl'
        : 'ltr';
}

export function getUiLocale(root = globalThis.document?.documentElement) {
    return normalizeUiLocale(root?.lang || 'en');
}

export function getUiDirection(locale = getUiLocale()) {
    return isRtlLocale(locale) ? 'rtl' : 'ltr';
}

export function mirrorPlacement(placement, direction = getUiDirection()) {
    if (direction !== 'rtl') return placement;
    if (placement === 'left') return 'right';
    if (placement === 'right') return 'left';
    return placement;
}

export function synchronizeUiDirection(root = globalThis.document?.documentElement) {
    if (!root) return 'ltr';

    const locale = getUiLocale(root);
    const direction = getUiDirection(locale);

    root.lang = locale;
    root.dir = direction;
    root.dataset.uiLocale = locale;

    return direction;
}
