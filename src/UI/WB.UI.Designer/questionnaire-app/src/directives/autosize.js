import { attach } from '@frsource/autoresize-textarea';

const autosize = app => {
    app.directive('autosize', {
        mounted(el) {
            el.rows = 1;

            const { detach } = attach(el);
            el.detachAutosize = detach;
        },
        unmounted(el) {
            el.detachAutosize();
        }
    });
};

export default autosize;
