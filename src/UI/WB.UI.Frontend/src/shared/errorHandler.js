import axios from 'axios';
import { getCurrentVersion } from './headquartersService';
import * as toastr from 'toastr'

const errorUrl = '/error/report';


export function errorHandler(err, vm, info) {

    const ignoreStatusCodes = [];

    const ignoredMessages = [
        'NetworkError when attempting to fetch resource.',
        'Failed to fetch',
        'Load failed'
    ];

    tryToShowErrorTooltip(err)

    const status = err.response?.status;

    if (status && ignoreStatusCodes.includes(status)) return false;
    if (err.message && ignoredMessages.includes(err.message)) {
        return false;
    }

    var errorDetails = {
        message: err.message,
        additionalData: {
            source: 'vue-error-handler',
            component: vm?.$options?.name || 'unknown',
            route: vm?.$route.fullPath,
            info,
            stack: err.stack ? getNestedErrorDetails(err) : '',
            version: getCurrentVersion()
        }
    };

    if (err.response) {
        errorDetails.additionalData.resposeStatus = err.response.status;
        errorDetails.additionalData.requestedUrl = err.response.url;
    }

    axios.post(errorUrl, errorDetails)
        .catch(function (error) {
            console.error('Error sending error details to server:', error);
        });

    console.error('Error Handler:', err);


    function tryToShowErrorTooltip(error) {
        if (error.response && error.response.data != null) {
            var data = error.response.data

            // handling asp net core validation errors
            if (data.Type === 'https://tools.ietf.org/html/rfc7231#section-6.5.1'
                || data.Type === 'https://tools.ietf.org/html/rfc9110#section-15.5.1'
            ) {
                let message = ''
                Object.keys(data.Errors).forEach(k => {
                    //message += k + ':\r\n'
                    data.Errors[k].forEach(errMessage => message += '  ' + errMessage + '\r\n')
                })

                //console.error(data)
                toastr.error(message, data.Title)
                return
            }

            if (data.errors && data.errors.length > 0) {
                let message = ''
                data.errors.forEach(errMessage => message += '  ' + errMessage + '\r\n')

                //console.error(data)
                toastr.error(message)
                return
            }

            const errorMessage = data.error || data.errorMessage
            if (errorMessage) {
                //console.error(data)
                toastr.error(errorMessage)
                return
            }
        }
    }
}

function getNestedErrorDetails(error) {
    let errorDetails = '';
    while (error) {
        errorDetails += `Message: ${error.message}\nStack: ${error.stack}\n\n`;
        error = error.cause;
    }
    return errorDetails;
}


export function setupErrorHandler(app) {

    app.config.errorHandler = errorHandler;

    window.addEventListener('error', function (e) {
        if (e.error) return false;

        var errorDetails = {
            message: e.error.message,

            additionalData: {
                source: 'error event',
                stack: getNestedErrorDetails(e.error),
                version: getCurrentVersion()
            }
        };

        axios.post(errorUrl, errorDetails)
            .catch(function (error) {
                console.error('Error sending error details to server:', error);
            });

        return false;
    });

    window.addEventListener('unhandledrejection', function (e) {
        const status = e.reason?.response?.status;
        const message = e.reason?.body?.message ?? e.reason?.message;

        if (status && ignoreStatusCodes.includes(status)) return false;
        if (message && ignoredMessages.includes(message)) {
            return false;
        }

        var errorDetails = {
            message: message,

            additionalData: {
                source: 'unhandledrejection event',
                responseStatusCode: status || null,
                stack:
                    e.reason instanceof Error
                        ? getNestedErrorDetails(e.reason)
                        : e.reason?.stack,
                version: getCurrentVersion()
            }
        };

        axios.post(errorUrl, errorDetails)
            .catch(function (error) {
                console.error('Error sending error details to server:', error);
            });

        return false;
    });

    app.config.globalProperties.$errorHandler = app.config.errorHandler;
}
