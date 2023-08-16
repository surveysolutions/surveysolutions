import Vue from 'vue'

Vue.directive('maskedText', {
    bind: (el, binding) => {
        if (binding.value) {
            const input = $(el)
            input.mask(binding.value, {
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
        }
    },
})


