import axios from 'axios';

const errorUrl = '/error/report';

export function setupErrorHandler(app) {
    const ignoreStatusCodes = [401, 400, 403, 404, 406];

    const ignoredMessages = [
        'NetworkError when attempting to fetch resource.',
        'Failed to fetch',
        'Load failed'
    ];

    app.config.errorHandler = (err, vm, info) => {
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
    };

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

    function getNestedErrorDetails(error) {
        let errorDetails = '';
        while (error) {
            errorDetails += `Message: ${error.message}\nStack: ${error.stack}\n\n`;
            error = error.cause;
        }
        return errorDetails;
    }
}
