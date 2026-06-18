let formatUtcDate
let DateFormats
let originalWindowConfig

beforeAll(async () => {
    originalWindowConfig = window.CONFIG
    window.CONFIG = {
        ...window.CONFIG,
        locale: {
            locale: 'en',
        },
    }

    const helpers = await import('../../../src/shared/helpers.js')
    formatUtcDate = helpers.formatUtcDate
    DateFormats = helpers.DateFormats
})

afterAll(() => {
    if (originalWindowConfig === undefined)
        delete window.CONFIG
    else
        window.CONFIG = originalWindowConfig
})

const pad = value => value.toString().padStart(2, '0')

const toLocalDateTimeString = value => {
    const date = new Date(`${value}Z`)
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`
}

describe('formatUtcDate', () => {
    test('converts utc timestamp to local time', () => {
        const utcValue = '2024-05-07T16:30:00'

        expect(formatUtcDate(utcValue, DateFormats.dateTime)).toBe(toLocalDateTimeString(utcValue))
    })

    test('returns empty string for empty values', () => {
        expect(formatUtcDate(null, DateFormats.dateTime)).toBe('')
        expect(formatUtcDate('', DateFormats.dateTime)).toBe('')
    })
})
