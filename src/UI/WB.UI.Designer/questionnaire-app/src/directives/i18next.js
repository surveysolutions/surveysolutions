import { i18n } from '../plugins/localization';

const i18next = app => {
    app.directive('i18next', {
        beforeMount(el, binding) {
            init(el, binding);
        },
        updated(el, binding) {
            init(el, binding);
        }
    });
};

function init(el, binding) {
    el.innerHTML = '<b style="color:red">Need migrate translation</b>';
    /*let key = binding.value || el.getAttribute('v-i18next');
    if (key) {
        const translatedText = i18n.t('QuestionnaireEditor.' + key);
        el.innerText = translatedText;
    }*/
}

export default i18next;
