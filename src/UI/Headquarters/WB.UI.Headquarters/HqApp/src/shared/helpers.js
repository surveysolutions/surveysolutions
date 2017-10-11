var navigator = window.navigator;
var lang = navigator.languages ? navigator.languages[0] : (navigator.language || navigator.userLanguage)
export const browserLanguage = lang

export function getLocationHash(questionid) {
    return "loc_" + questionid
}

export const DateFormats = {
    dateTime: 'YYYY-MM-DD HH:mm:ss',
    date: 'YYYY-MM-DD'
}