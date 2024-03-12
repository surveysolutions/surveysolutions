import { mande } from 'mande';

const api = mande('/error/report');

export function setupErrorHandler(app) {
    app.config.errorHandler = (err, vm, info) => {
        var errorDetails = {
            message: err.message,
            additionalData: {
                source: 'vue-error-handler',
                component: vm?.$options?.name || 'unknown',
                route: vm?.$route.fullPath,
                info,
                stack: err.stack ? getNestedErrorDetails(err) : ''
            }
        };

        if (err.response) {
            errorDetails.additionalData.resposeStatus = err.response.status;
            errorDetails.additionalData.requestedUrl = err.response.url;
        }

        api.post('', errorDetails).catch(error => {
            console.error('Error sending error details to server:', error);
        });

        console.error('Error Handler:', err);
    };

    /*window.onerror = function(message, source, lineno, colno, error) {
        var errorDetails = {
            message: message,
            source: source,
            line: lineno,
            column: colno,
            additionalData: {
                stack: error ? getNestedErrorDetails(error) : ''
            }
        };

        api.post('', errorDetails).catch(error => {
            console.error('Error sending error details to server:', error);
        });

        return false;
    };*/

    window.addEventListener('error', function(e) {
        var errorDetails = {
            message: e.error.message,

            additionalData: {
                source: 'error event',
                stack: getNestedErrorDetails(e.error)
            }
        };

        api.post('', errorDetails).catch(error => {
            console.error('Error sending error details to server:', error);
        });

        return false;
    });

    const ignoreStatusCodes = [400, 403, 404, 406];

    window.addEventListener('unhandledrejection', function(e) {
        const status = e.reason?.response?.status;

        if (status && ignoreStatusCodes.includes(status)) return false;

        var errorDetails = {
            message: e.reason?.body?.message ?? e.reason?.message,

            additionalData: {
                source: 'unhandledrejection event',
                stack:
                    e.reason instanceof Error
                        ? getNestedErrorDetails(e.reason)
                        : e.reason?.stack
            }
        };

        api.post('', errorDetails).catch(error => {
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
