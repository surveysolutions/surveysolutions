import dayjs from 'dayjs'
import 'dayjs/locale/ar.js'
import 'dayjs/locale/cs.js'
import 'dayjs/locale/en.js'
import 'dayjs/locale/es.js'
import 'dayjs/locale/fr.js'
import 'dayjs/locale/id.js'
import 'dayjs/locale/ka.js'
import 'dayjs/locale/km.js'
import 'dayjs/locale/ro.js'
import 'dayjs/locale/ru.js'
import 'dayjs/locale/sq.js'
import 'dayjs/locale/th.js'
import 'dayjs/locale/uk.js'
import 'dayjs/locale/vi.js'
import customParseFormat from 'dayjs/plugin/customParseFormat.js'
import duration from 'dayjs/plugin/duration.js'
import isoWeek from 'dayjs/plugin/isoWeek.js'
import relativeTime from 'dayjs/plugin/relativeTime.js'
import timezone from 'dayjs/plugin/timezone.js'
import utc from 'dayjs/plugin/utc.js'

dayjs.extend(customParseFormat)
dayjs.extend(duration)
dayjs.extend(isoWeek)
dayjs.extend(relativeTime)
dayjs.extend(utc)
dayjs.extend(timezone)

function moment(...args) {
    return dayjs(...args)
}

function normalizeLocale(locale, fallbackLocale = 'en') {
    if (typeof locale !== 'string') {
        return locale
    }

    const normalizedLocale = locale.toLowerCase().replace(/_/g, '-')
    if (dayjs.Ls[normalizedLocale] != null) {
        return normalizedLocale
    }

    const language = normalizedLocale.split('-')[0]
    if (dayjs.Ls[language] != null) {
        return language
    }

    return fallbackLocale
}

moment.utc = function (...args) { return dayjs.utc(...args) }
moment.unix = function (...args) { return dayjs.unix(...args) }
moment.duration = function (...args) { return dayjs.duration(...args) }
moment.locale = function (locale, ...args) {
    if (Array.isArray(locale)) {
        const matchingLocale = locale
            .map(candidate => normalizeLocale(candidate, null))
            .find(candidate => typeof candidate === 'string' && dayjs.Ls[candidate] != null)

        return dayjs.locale(matchingLocale ?? 'en', ...args)
    }

    return dayjs.locale(normalizeLocale(locale), ...args)
}
moment.isMoment = (value) => dayjs.isDayjs(value)

moment.tz = function (...args) { return dayjs.tz(...args) }
moment.tz.guess = () => dayjs.tz.guess()

moment.fn = dayjs.prototype

export default moment
