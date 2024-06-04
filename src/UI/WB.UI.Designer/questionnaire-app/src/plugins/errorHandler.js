import { mande } from 'mande';
import {
    getOnlineVersion,
    getCurrentVersion
} from '../services/designerService';
import { i18n } from './localization';

const api = mande('/error/report');

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
            checkIsNeedUpdate();
            return false;
        }

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
        if (e.error) return false;

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

    window.addEventListener('unhandledrejection', function(e) {
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
                        : e.reason?.stack
            }
        };

        api.post('', errorDetails).catch(error => {
            console.error('Error sending error details to server:', error);
        });

        return false;
    });

    window.addEventListener('vite:preloadError', event => {
        checkIsNeedUpdate();
    });

    async function checkIsNeedUpdate() {
        try {
            const versionFromInternet = await getOnlineVersion();
            const currentVersion = getCurrentVersion();

            if (versionFromInternet != currentVersion) {
                const params = {
                    title: i18n.t('QuestionnaireEditor.RefreshPageConfirm'),
                    okButtonTitle: i18n.t('QuestionnaireEditor.Refresh'),
                    cancelButtonTitle: i18n.t('QuestionnaireEditor.Cancel'),
                    isReadOnly: false,
                    callback: async confirm => {
                        if (confirm) {
                            location.reload();
                        }
                    }
                };

                app.config.globalProperties.$confirm(params);
            }
        } catch {
            // ignore
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
}
