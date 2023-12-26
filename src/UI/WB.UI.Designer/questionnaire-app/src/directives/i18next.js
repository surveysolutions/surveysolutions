import { i18n } from '../plugins/localization';

const i18next = app => {
    app.directive('i18next', {
        mounted(el, binding) {
            const key = binding.value
                ? binding.value
                : el.text;
            i18n.t('$'+ key);
        }
    });
};

export default i18next;
