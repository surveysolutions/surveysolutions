export function formatNumber(value) {
    if (value == null || value == undefined)
        return value
    var language = navigator.languages && navigator.languages[0] || 
       navigator.language || 
       navigator.userLanguage 
    return value.toLocaleString(language)
}
