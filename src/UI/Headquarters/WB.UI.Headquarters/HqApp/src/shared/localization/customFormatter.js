export default class BaseFormatter {
    constructor() {
        this._caches = Object.create(null)
    }

    interpolate(message, values) {
        let tokens = this._caches[message]
        if (!tokens) {
            tokens = tokenize(message)
            this._caches[message] = tokens
        }
        return compile(tokens, values)
    }
}

export function tokenize(str) {
    let pos = -1;
    let state = 'text';
    let tokenValue = '';

    const tokens = [];

    function addToken(type, value, rx = null){
        const valid = rx == null || rx.test(value);
        tokens.push({type: valid ? type : 'unknown', value});
    }

    while (++pos < str.length) {
        const char = str[pos];

        switch (state) {
            case 'text':
                if (char == "{") {
                    state = 'TOKEN_START';
                } else {
                    tokenValue += char;
                }
                break;
            case 'TOKEN_START':
                if (tokenValue != '') {
                    addToken('text', tokenValue)
                    tokenValue = '';
                }

                if (char == "{") {
                    state = 'NAMED_TOKEN_START';
                } else {
                    state = 'LIST_TOKEN';
                    if(char != ' ') tokenValue += char
                }
                break;
            case 'NAMED_TOKEN_START':
                if (char == "}") {
                    if (pos + 1 < str.length && str[pos + 1] == "}") {
                        addToken('named', tokenValue, /^(\w)+/,)
                        pos += 1;
                        state = "text";
                        tokenValue = '';
                    }
                } else {
                    if(char != ' ')
                        tokenValue += char;
                }
                break;
            case 'LIST_TOKEN':
                if (char == "}") {
                    addToken('list', tokenValue, /^(\d)+/)
                    state = "text";
                    tokenValue = '';
                } else {
                    if(char != ' ') tokenValue += char;
                }
                break;
        }
        //console.log(state, pos, tokenValue, "'" + char + "'");
    }

    if (tokenValue != '') {
        addToken('text', tokenValue)
    }

    return tokens;
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