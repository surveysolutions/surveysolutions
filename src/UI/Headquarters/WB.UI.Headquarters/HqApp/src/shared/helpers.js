export const browserLanguage = window.CONFIG.locale.locale;

export function getLocationHash(questionid) {
    return "loc_" + questionid
}

export const DateFormats = {
    dateTime: 'YYYY-MM-DD HH:mm:ss',
    date: 'YYYY-MM-DD',
    dateTimeInList: 'MMM DD, YYYY HH:mm'
}