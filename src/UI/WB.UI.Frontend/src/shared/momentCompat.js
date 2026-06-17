import dayjs from 'dayjs'
import customParseFormat from 'dayjs/plugin/customParseFormat'
import duration from 'dayjs/plugin/duration'
import isoWeek from 'dayjs/plugin/isoWeek'
import relativeTime from 'dayjs/plugin/relativeTime'
import timezone from 'dayjs/plugin/timezone'
import utc from 'dayjs/plugin/utc'

dayjs.extend(customParseFormat)
dayjs.extend(duration)
dayjs.extend(isoWeek)
dayjs.extend(relativeTime)
dayjs.extend(utc)
dayjs.extend(timezone)

function moment(...args) {
    return dayjs(...args)
}

moment.utc = function (...args) { return dayjs.utc(...args) }
moment.unix = function (...args) { return dayjs.unix(...args) }
moment.duration = function (...args) { return dayjs.duration(...args) }
moment.locale = function (...args) { return dayjs.locale(...args) }
moment.isMoment = (value) => dayjs.isDayjs(value)

moment.tz = function (...args) { return dayjs.tz(...args) }
moment.tz.guess = () => dayjs.tz.guess()

moment.fn = dayjs.prototype

export default moment
