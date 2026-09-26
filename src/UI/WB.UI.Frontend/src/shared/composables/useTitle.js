import { watchEffect, unref } from 'vue'

export function useTitle(title) {
    watchEffect(() => { document.title = unref(title) ?? document.title })
}
