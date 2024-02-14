import { mande } from 'mande';

const api = mande('/q/errors');

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
}
