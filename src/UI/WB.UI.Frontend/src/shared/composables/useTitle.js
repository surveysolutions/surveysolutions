import { watchEffect, toRef } from 'vue'

export function useTitle(title) {
    watchEffect(() => { document.title = toRef(title).value ?? document.title })
}
