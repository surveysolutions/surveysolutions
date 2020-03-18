import { isNumber, parseInt } from 'lodash'

export function formatNumber(value) {
    if (value == null || value == undefined)
        return value
    if (!isNumber(value)) {
        const parseRes = parseInt(value)
        if (isNaN(parseRes)) {
            return value
        }
        value = parseRes
    }

    var language = navigator.languages && navigator.languages[0] ||
        navigator.language ||
        navigator.userLanguage
    return value.toLocaleString(language)
}
