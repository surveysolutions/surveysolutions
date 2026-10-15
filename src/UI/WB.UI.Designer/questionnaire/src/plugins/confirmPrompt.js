import ConfirmPrompt from '../views/App/components/ConfirmPrompt.vue';
import event from './events';

export default {
    install: (app, args = {}) => {
        app.component(
            args.componentName || 'confirm-prompt-dialog',
            ConfirmPrompt,
        );

        const confirmPrompt = (params) => {
            if (typeof params != 'object' || Array.isArray(params)) {
                let caughtType = typeof params;
                if (Array.isArray(params)) caughtType = 'array';

                throw new Error(
                    `Options type must be an object. Caught: ${caughtType}. Expected: object`,
                );
            }

            if (typeof params === 'object') {
                if (
                    params.hasOwnProperty('callback') &&
                    typeof params.callback != 'function'
                ) {
                    let callbackType = typeof params.callback;
                    throw new Error(
                        `Callback type must be a function. Caught: ${callbackType}. Expected: function`,
                    );
                }
                event.emit('openPrompt', params);
            }
        };

        confirmPrompt.close = () => {
            event.emit('closePrompt');
        };

        app.config.globalProperties.$confirmPrompt = confirmPrompt;
        app['$confirmPrompt'] = confirmPrompt;
    },
};
