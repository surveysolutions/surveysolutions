import { sanitize } from '../services/utilityService';

const sanitizeText = app => {
    app.directive('sanitize-text', {
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
    el.textContent = sanitizedValue;
}

export default sanitizeText;
