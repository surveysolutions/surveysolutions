import { notify } from '@kyvg/vue3-notification';

export function notice(text, title) {
    notify({
        title: title,
        text: text
    });
}

export function error(text, title) {
    notify({
        title: title,
        text: text,
        type: 'error'
    });
}
