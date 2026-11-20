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
import isSameOrAfter from 'dayjs/plugin/isSameOrAfter.js'
import isSameOrBefore from 'dayjs/plugin/isSameOrBefore.js'
import isoWeek from 'dayjs/plugin/isoWeek.js'
import relativeTime from 'dayjs/plugin/relativeTime.js'
import timezone from 'dayjs/plugin/timezone.js'
import utc from 'dayjs/plugin/utc.js'

dayjs.extend(customParseFormat)
dayjs.extend(duration)
dayjs.extend(isSameOrAfter)
dayjs.extend(isSameOrBefore)
dayjs.extend(isoWeek)
dayjs.extend(relativeTime)
dayjs.extend(utc)
dayjs.extend(timezone)

function moment(...args) {
    return dayjs(...args)
}

// dayjs' built-in Duration#humanize() ignores any custom thresholds passed to it and
// always compares against "now" via fromNow(), which also means it can only honour the
// relativeTime strings of whichever locale happens to be active for unrelated reasons.
// Reimplement it so the resulting bucket (seconds/minutes/.../years) is derived purely
// from the duration value itself, matching moment.js' original behaviour and producing
// the same result regardless of the active UI language.
const defaultRelativeTimeThresholds = { ss: 44, s: 45, m: 45, h: 22, d: 26, M: 11 }

function roundAbs(value) {
    return Math.round(Math.abs(value))
}

function getRelativeTimeConfig(durationInstance) {
    const localeName = durationInstance.$l || dayjs.locale()
    const localeConfig = dayjs.Ls[localeName] || dayjs.Ls.en
    return localeConfig.relativeTime || dayjs.Ls.en.relativeTime
}

function formatRelativeTimeComponent(relativeTimeConfig, key, value, withoutSuffix, isFuture) {
    const template = relativeTimeConfig[key]
    if (template == null) {
        return String(value)
    }
    if (typeof template === 'function') {
        return template(value, withoutSuffix, key, isFuture)
    }
    return String(template).replace('%d', value)
}

function applySuffix(relativeTimeConfig, text, isFuture) {
    const template = isFuture ? relativeTimeConfig.future : relativeTimeConfig.past
    if (template == null) {
        return text
    }
    if (typeof template === 'function') {
        return template(text)
    }
    return String(template).replace('%s', text)
}

function humanizeDuration(durationInstance, argWithSuffix, argThresholds) {
    let withSuffix = false
    let overrideThresholds = argThresholds

    if (typeof argWithSuffix === 'boolean') {
        withSuffix = argWithSuffix
    } else if (argWithSuffix != null) {
        overrideThresholds = argWithSuffix
    }

    const thresholds = overrideThresholds
        ? Object.assign({}, defaultRelativeTimeThresholds, overrideThresholds)
        : defaultRelativeTimeThresholds

    const seconds = roundAbs(durationInstance.asSeconds())
    const minutes = roundAbs(durationInstance.asMinutes())
    const hours = roundAbs(durationInstance.asHours())
    const days = roundAbs(durationInstance.asDays())
    const months = roundAbs(durationInstance.asMonths())
    const years = roundAbs(durationInstance.asYears())

    const relativeTimeConfig = getRelativeTimeConfig(durationInstance)

    let key
    let value
    if (seconds <= thresholds.ss) { key = 's'; value = seconds }
    else if (seconds < thresholds.s) { key = 'ss'; value = seconds }
    else if (minutes <= 1) { key = 'm'; value = 1 }
    else if (minutes < thresholds.m) { key = 'mm'; value = minutes }
    else if (hours <= 1) { key = 'h'; value = 1 }
    else if (hours < thresholds.h) { key = 'hh'; value = hours }
    else if (days <= 1) { key = 'd'; value = 1 }
    else if (days < thresholds.d) { key = 'dd'; value = days }

    // Weeks only have dedicated wording in a handful of locales. When the active
    // locale doesn't define it, fall through to months so every language behaves
    // consistently instead of rendering an empty/undefined fragment.
    if (key === undefined && thresholds.w != null && relativeTimeConfig.w != null && relativeTimeConfig.ww != null) {
        const weeks = roundAbs(durationInstance.asWeeks())
        if (weeks <= 1) { key = 'w'; value = 1 }
        else if (weeks < thresholds.w) { key = 'ww'; value = weeks }
    }

    if (key === undefined) {
        if (months <= 1) { key = 'M'; value = 1 }
        else if (months < thresholds.M) { key = 'MM'; value = months }
        else if (years <= 1) { key = 'y'; value = 1 }
        else { key = 'yy'; value = years }
    }

    const isFuture = durationInstance.asMilliseconds() > 0
    let output = formatRelativeTimeComponent(relativeTimeConfig, key, value, !withSuffix, isFuture)

    if (withSuffix) {
        output = applySuffix(relativeTimeConfig, output, isFuture)
    }

    return output
}

const DurationPrototype = Object.getPrototypeOf(dayjs.duration(0))
DurationPrototype.humanize = function (argWithSuffix, argThresholds) {
    return humanizeDuration(this, argWithSuffix, argThresholds)
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
