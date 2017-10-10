export default class BaseFormatter {
    constructor() {
        this._caches = Object.create(null)
    }

    interpolate(message, values) {
        let tokens = this._caches[message]
        if (!tokens) {
            tokens = parse(message)
            this._caches[message] = tokens
        }
        return compile(tokens, values)
    }
}

const RE_TOKEN_NAMED_VALUE = /^(\w)+/
const RE_TOKEN_OPEN = "{{"
const RE_TOKEN_CLOSE = "}}"

export function parse(format) {
    const tokens = []
    let start = 0, end = 0;

    while (end != -1 && start != -1) {
        start = format.indexOf(RE_TOKEN_OPEN, end);

        if (start == -1) break;

        if (start - end > 0) {
            tokens.push({ type: 'text', value: format.substring(end, start) });
        }

        end = format.indexOf(RE_TOKEN_CLOSE, start);

        if (end != -1) {
            const text = format.substring(start + RE_TOKEN_OPEN.length, end);

            const type = RE_TOKEN_NAMED_VALUE.test(text)
                ? 'named'
                : 'unknown'

            tokens.push({ value: text, type })

            end += RE_TOKEN_CLOSE.length; // advancing end pointer at the end of closing braces
        }
    }

    // append last part of text
    if (end != -1) {
        const text = format.substring(end);
        if(text.length > 0)
            tokens.push({ type: 'text', value: text });
    }

    return tokens
}

export function compile(tokens, values) {
    const compiled = [];
    let index = 0

    const mode = Array.isArray(values)
        ? 'list'
        : _.isObject(values)
            ? 'named'
            : 'unknown'
    if (mode === 'unknown') { return compiled }

    while (index < tokens.length) {
        const token = tokens[index]
        switch (token.type) {
            case 'text':
                compiled.push(token.value)
                break
            case 'list':
                compiled.push(values[parseInt(token.value, 10)])
                break
            case 'named':
                if (mode === 'named') {
                    compiled.push((values)[token.value])
                } else {
                    if (process.env.NODE_ENV !== 'production') {
                        console.warn(`Type of token '${token.type}' and format of value '${mode}' don't match!`)
                    }
                }
                break
            case 'unknown':
                if (process.env.NODE_ENV !== 'production') {
                    console.warn(`Detect 'unknown' type of token!`, token.value)
                }
                break
        }
        index++
    }

    return compiled
}