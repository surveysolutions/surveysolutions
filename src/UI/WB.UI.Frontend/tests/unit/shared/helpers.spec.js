process.env.TZ = 'America/New_York'

let formatUtcDate
let DateFormats

beforeAll(async () => {
    global.window = {
        CONFIG: {
            locale: {
                locale: 'en',
            },
        },
    }

    const helpers = await import('../../../src/shared/helpers.js')
    formatUtcDate = helpers.formatUtcDate
    DateFormats = helpers.DateFormats
})

describe('formatUtcDate', () => {
    test('converts utc timestamp to local time', () => {
        expect(formatUtcDate('2024-05-07T16:30:00', DateFormats.dateTime)).toBe('2024-05-07 12:30:00')
    })

    test('returns empty string for empty values', () => {
        expect(formatUtcDate(null, DateFormats.dateTime)).toBe('')
        expect(formatUtcDate('', DateFormats.dateTime)).toBe('')
    })
})
