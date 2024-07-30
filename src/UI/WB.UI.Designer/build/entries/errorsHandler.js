import { mande } from 'mande';

const api = mande('/error/report');

const ignoreStatusCodes = [401, 400, 403, 404, 406];

const ignoredMessages = [
    'NetworkError when attempting to fetch resource.',
    'Failed to fetch',
    'Load failed',
];

export function setupErrorHandler() {
    window.addEventListener('error', function (e) {
        if (e.error) return false;

        var errorDetails = {
            message: e.error.message,

            additionalData: {
                source: 'error event',
                stack: getNestedErrorDetails(e.error),
            },
        };

        api.post('', errorDetails).catch((error) => {
            console.error('Error sending error details to server:', error);
        });

        return false;
    });

    window.addEventListener('unhandledrejection', function (e) {
        const status = e.reason?.response?.status;
        const message = e.reason?.body?.message ?? e.reason?.message;

        if (status && ignoreStatusCodes.includes(status)) return false;
        if (message && ignoredMessages.includes(message)) {
            checkIsNeedUpdate();
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
            },
        };

        api.post('', errorDetails).catch((error) => {
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
