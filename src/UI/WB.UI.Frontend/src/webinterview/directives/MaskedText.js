let maskedTextDependenciesPromise

async function loadMaskedTextDependencies() {
    if (!maskedTextDependenciesPromise) {
        maskedTextDependenciesPromise = Promise.all([
            import('jquery'),
            import('jquery-mask-plugin'),
        ]).then(([jqueryModule]) => {
            const jquery = jqueryModule.default
            window.jQuery = jquery
            window.$ = jquery

            return jquery
        })
    }

    return maskedTextDependenciesPromise
}

function applyMask($, el, mask) {
    const input = $(el)
    input.mask(mask, {
        onChange: function () {
            input.data('maskCompleted', false)
        },
        onComplete: function () {
            input.data('maskCompleted', true)
        },
        translation: {
            '0': { pattern: /0/, fallback: '0' },
            '~': { pattern: /[a-zA-Z]/ },
            '#': { pattern: /\d/ },
            '*': { pattern: /[a-zA-Z0-9]/ },
            '9': { pattern: /9/, fallback: '9' },
            'A': { pattern: /A/, fallback: 'A' },
            'S': { pattern: /S/, fallback: 'S' },
        },
    })

    el.__maskedTextInput = input
    el.__maskedTextMask = mask
}

export function registerMaskedText(vue) {
    vue.directive('maskedText', {
        async mounted(el, binding) {
            if (!binding.value) return

            const $ = await loadMaskedTextDependencies()
            if (el.__maskedTextDisposed || !binding.value) return

            applyMask($, el, binding.value)
        },
        async updated(el, binding) {
            if (binding.value === binding.oldValue) return

            const $ = await loadMaskedTextDependencies()
            if (el.__maskedTextDisposed) return

            if (el.__maskedTextInput) {
                el.__maskedTextInput.unmask()
                delete el.__maskedTextInput
            }

            if (binding.value) {
                applyMask($, el, binding.value)
            }
            else {
                delete el.__maskedTextMask
            }
        },
        beforeUnmount(el) {
            el.__maskedTextDisposed = true

            if (el.__maskedTextInput) {
                el.__maskedTextInput.unmask()
                delete el.__maskedTextInput
            }

            delete el.__maskedTextMask
        },
    })
}
