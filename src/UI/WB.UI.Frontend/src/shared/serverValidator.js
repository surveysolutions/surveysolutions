import modal from '~/shared/modal'

const EXPECTED_HEADER_VALUE = '773994826649214'

let shownOnce = false

export function validateServerHeader(response) {
    if (!response) return

    // Avoid recursive modal when the error-reporting endpoint itself fails validation
    const url = response.config && response.config.url
    if (url && url.includes('/error/report')) return

    const actual = response.headers && response.headers['x-survey-solutions']
    if (actual === EXPECTED_HEADER_VALUE) return

    if (shownOnce) return
    shownOnce = true

    modal.dialog({
        title: 'Invalid server response',
        message: '<p>The server did not return the expected identification header.</p>' +
            '<p>You may be connected to a proxy or an incorrect server. Please reload the page.</p>',
        onEscape: false,
        closeButton: false,
        buttons: {
            reload: {
                label: 'Reload',
                className: 'btn-danger',
                callback: () => { location.reload() },
            },
        },
    })
}
