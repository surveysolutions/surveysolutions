import { mande } from 'mande';

const api = mande('/error/report');

export function setupErrorHandler(app) {
    app.config.errorHandler = (err, vm, info) => {
        var errorDetails = {
            message: err.message,
            additionalData: {
                component: vm?.$options?.name,
                route: vm?.$route.fullPath,
                stack: err.stack,
                info
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

    window.addEventListener('error', function(e) {
        var errorDetails = {
            message: e.error.message,

            additionalData: {
                stack: e.error.stack
            }
        };

        api.post('', errorDetails).catch(error => {
            console.error('Error sending error details to server:', error);
        });

        return false;
    });

    window.addEventListener('unhandledrejection', function(e) {
        var errorDetails = {
            message: e.reason?.body?.message ?? e.reason,

            additionalData: {
                stack: e.reason?.stack
            }
        };

        api.post('', errorDetails).catch(error => {
            console.error('Error sending error details to server:', error);
        });

        return false;
    });
}
