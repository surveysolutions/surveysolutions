import { sanitize } from '../services/utilityService';

const sanitizeHtml = app => {
    app.directive('sanitize-html', {
        beforeMount(el, binding) {
            init(el, binding);
        },
        updated(el, binding) {
            init(el, binding);
        }
    });
};

function init(el, binding) {
    const sanitizedValue = sanitize(binding.value);
    el.innerHTML = sanitizedValue;
}

export default sanitizeHtml;
