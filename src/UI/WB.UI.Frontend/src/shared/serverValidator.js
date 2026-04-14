import modal from '~/shared/modal'
import { $t } from '~/shared/plugins/locale'

const EXPECTED_HEADER_VALUE = '773994826649214'

let shownOnce = false

export function validateServerHeader(response) {
    if (!response) return

    // Avoid recursive modal when the error-reporting endpoint itself fails validation
    const url = response.config && response.config.url
    const urlStr = url && String(url)
    if (urlStr && urlStr.includes('/error/report')) return

    const actual = response.headers && response.headers['x-survey-solutions']
    if (actual === EXPECTED_HEADER_VALUE) return

    if (shownOnce) return
    shownOnce = true

    modal.dialog({
        title: $t('WebInterviewUI.InvalidServerResponseTitle'),
        message: '<p>' + $t('WebInterviewUI.InvalidServerResponseMessage') + '</p>',
        onEscape: false,
        closeButton: false,
        buttons: {
            reload: {
                label: $t('WebInterviewUI.Reload'),
                className: 'btn-danger',
                callback: () => { location.reload() },
            },
        },
    })
}
