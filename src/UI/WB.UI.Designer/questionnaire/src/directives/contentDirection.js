import { getTextDirection } from '../plugins/uiLocale';

function updateDirection(element, value) {
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
