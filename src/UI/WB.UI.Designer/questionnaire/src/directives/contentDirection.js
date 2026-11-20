import { getTextDirection } from '../plugins/uiLocale';

function updateDirection(element, value) {
    // Alignment follows the UI direction in rtl.less; only the bidi order follows the content.
    element.dataset.contentDirection = '';
    if (value === undefined || value === null || String(value).trim() === '') {
        // Empty fields show a localized placeholder, so they keep the UI direction.
        element.removeAttribute('dir');
        return;
    }
    element.dir = getTextDirection(value);
}

const contentDirection = app => {
    app.directive('content-direction', {
        mounted(element, binding) {
            updateDirection(element, binding.value ?? element.value ?? element.textContent);
        },
        updated(element, binding) {
            updateDirection(element, binding.value ?? element.value ?? element.textContent);
        },
    });
};

export default contentDirection;
