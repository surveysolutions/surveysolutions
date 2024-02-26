import { formatDateTime } from '../services/utilityService';

const dateTime = app => {
    app.directive('dateTime', {
        beforeMount(el, binding) {
            init(el, binding);
        },
        updated(el, binding) {
            init(el, binding);
        }
    });
};

function init(el, binding) {
    const dateTime = new Date(binding.value);
    el.textContent = formatDateTime(dateTime);
}

export default dateTime;
