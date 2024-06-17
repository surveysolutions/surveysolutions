import Confirm from '../views/App/components/Confirm.vue';
import event from './events';

export default {
    install: (app, args = {}) => {
        app.component(args.componentName || 'confirm-dialog', Confirm);

        const confirm = params => {
            if (typeof params != 'object' || Array.isArray(params)) {
                let caughtType = typeof params;
                if (Array.isArray(params)) caughtType = 'array';

                throw new Error(
                    `Options type must be an object. Caught: ${caughtType}. Expected: object`
                );
            }

            if (typeof params === 'object') {
                if (
                    params.hasOwnProperty('callback') &&
                    typeof params.callback != 'function'
                ) {
                    let callbackType = typeof params.callback;
                    throw new Error(
                        `Callback type must be an function. Caught: ${callbackType}. Expected: function`
                    );
                }
                event.emit('open', params);
            }
        };

        confirm.close = () => {
            event.emit('close');
        };

        app.config.globalProperties.$confirm = confirm;

        app['$confirm'] = confirm;
    }
};
