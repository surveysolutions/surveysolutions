// Mirrors EmailTextDirection on the server: direction of an email follows the first letter
// of its authored text; substituted values (%PLACEHOLDERS%) do not define the language.
const placeholders = /%[A-Za-z0-9_]+(:[a-z]+)?%/g
const htmlTags = /<[^>]*>/g
const rightToLeftChars = /[\u0590-\u065F\u066A-\u06EF\u06FA-\u08FF\uFB1D-\uFDFF\uFE70-\uFEFF]/
const letters = /\p{L}/u

function decodeHtmlEntities(text) {
    return new DOMParser().parseFromString(text, 'text/html').body.textContent || ''
}

export function startsRightToLeft(text) {
    for (const char of text || '') {
        if (rightToLeftChars.test(char)) return true
        if (letters.test(char)) return false
    }
    return false
}

export function emailDirection(template) {
    const text = [template?.subject, template?.message, template?.linkText, template?.passwordDescription]
        .filter((part) => part != null && part.trim() !== '')
        .map((part) => decodeHtmlEntities(part.replace(placeholders, ' ').replace(htmlTags, ' ')))
        .join('\n')
    return startsRightToLeft(text) ? 'rtl' : 'ltr'
}
