import modal from '~/shared/modal'
import { $t } from '~/shared/plugins/locale'

const EXPECTED_HEADER_VALUE = '773994826649214'

let shownOnce = false

function validateHeaderValues(headerValue, url) {
    const urlStr = url && String(url)
    if (urlStr && urlStr.includes('/error/report')) return

    // Skip validation for cross-origin requests (e.g. Designer API calls from Web-tester).
    // Those responses come from a different server and are not expected to carry the header.
    if (urlStr) {
        try {
            const requestOrigin = new URL(urlStr, window.location.href).origin
            if (requestOrigin !== window.location.origin) return
        } catch { /* ignore malformed URLs */ }
    }

    if (headerValue === EXPECTED_HEADER_VALUE) return

    if (shownOnce) return
    shownOnce = true

    // $t may be undefined on plain (non-Vue) pages that never call Vuei18n.initialize().
    // Fall back to hard-coded English strings so the modal still renders correctly.
    const translate = (key, fallback) => {
        try { return $t(key) } catch { return fallback }
    }

    modal.dialog({
        title: translate('WebInterviewUI.InvalidServerResponseTitle', 'Invalid Server Response'),
        message: '<p>' + translate('WebInterviewUI.InvalidServerResponseMessage', 'The server returned an unexpected response. Please reload the page.') + '</p>',
        onEscape: false,
        closeButton: false,
        buttons: {
            reload: {
                label: translate('WebInterviewUI.Reload', 'Reload'),
                className: 'btn-danger',
                callback: () => { location.reload() },
            },
        },
    })
}

export function validateServerHeader(response) {
    if (!response) return

    // Avoid recursive modal when the error-reporting endpoint itself fails validation
    const url = response.config && response.config.url
    const actual = response.headers && response.headers['x-survey-solutions']
    validateHeaderValues(actual, url)
}

export function validateJQueryXhr(jqXHR, settings) {
    if (!jqXHR) return

    const url = settings && settings.url
    const actual = jqXHR.getResponseHeader('x-survey-solutions')
    validateHeaderValues(actual, url)
}

export function validatePageLoad() {
    // Probe the current page on the SAME origin using a RELATIVE url (path + query only).
    // The destination host is therefore always the current document origin - it is never taken
    // from window.location and placed into the request url - so the request cannot be pointed at
    // another server. mode: 'same-origin' additionally rejects any cross-origin redirect.
    const url = window.location.pathname + window.location.search

    fetch(url, { method: 'HEAD', mode: 'same-origin', credentials: 'same-origin' })
        .then(response => {
            if (!response.ok) {
                return fetch(url, { method: 'GET', mode: 'same-origin', credentials: 'same-origin' })
            }
            return response
        })
        .then(response => {
            validateHeaderValues(
                response.headers.get('x-survey-solutions'),
                url
            )
        })
        .catch(() => { })
}

export function installAxiosInterceptors(axiosInstance) {
    axiosInstance.interceptors.response.use(
        function (response) {
            validateServerHeader(response)
            return response
        },
        function (error) {
            if (error.response) validateServerHeader(error.response)
            return Promise.reject(error)
        }
    )
}

/**
 * Validates the x-survey-solutions header on a native Fetch API Response.
 * Use this instead of validateServerHeader when working outside of axios.
 */
export function validateFetchResponse(response) {
    const url = response.url
    const actual = response.headers.get('x-survey-solutions')
    validateHeaderValues(actual, url)
}


