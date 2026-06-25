import moment from 'moment'

export const browserLanguage = window.CONFIG.locale.locale

export function getLocationHash(questionid) {
    return 'loc_' + questionid
}

export const DateFormats = {
    dateTime: 'YYYY-MM-DD HH:mm:ss',
    date: 'YYYY-MM-DD',
    dateTimeInList: 'MMM DD, YYYY HH:mm',
}

export function humanFileSize(bytes, si) {
    var thresh = si ? 1000 : 1024
    if (Math.abs(bytes) < thresh) {
        return bytes + ' B'
    }
    var units = si
        ? ['kB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB']
        : ['KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB']
    var u = -1
    do {
        bytes /= thresh
        ++u
    } while (Math.abs(bytes) >= thresh && u < units.length - 1)
    return bytes.toFixed(1) + ' ' + units[u]
}

export function convertToLocal(startUtc, startTimezone) {
    if (startUtc == null) {
        return null
    }

    const startTime = moment.utc(startUtc)

    if (startTimezone) {
        try {
            return startTime
                .tz(startTimezone)
                .local()
                .format(DateFormats.dateTimeInList)
        } catch { /* fall back to UTC->local conversion when timezone is invalid */ }
    }

    return startTime
        .local()
        .format(DateFormats.dateTimeInList)
}