<template>
    <div class="field"
        :class="{'answered': hasValue === true}">
        <input
            class="form-control with-clear-btn"
            :class="{'input-validation-error': haserror === true}"
            v-bind="$attrs"
            v-bind:value="value"
            v-on="inputListeners"
            ref="input"
            autocomplete="new-password"/>

        <button
            type="button"
            class="btn btn-link btn-clear"
            v-if="hasValue"
            @click="clearFilter"
            tabindex="-1">
            <span></span>
        </button>
    </div>
</template>

<script>
export default {
    data() {
        return {
            disabled: false,
            type: 'text',
        }
    },
    inheritAttrs: false,
    props: {
        value: {
            default: null,
        },
        haserror: {
            type: Boolean,
            default: false,
        },
        canClear: {
            type: Boolean,
            default: true,
        },
    },
    computed: {
        hasValue: function() {
            return this.type == 'text' && !this.disabled && this.canClear && this.value !== null && this.value !== ''
        },
        inputListeners: function() {
            var vm = this
            // `Object.assign` объединяет объекты вместе, чтобы получить новый объект
            return Object.assign(
                {},
                // Мы добавляем все слушатели из родителя
                this.$listeners,
                // Затем мы можем добавить собственные слушатели или
                // перезаписать поведение некоторых существующих.
                {
                    // Это обеспечит, что будет работать v-model на компоненте
                    input: function(event) {
                        vm.$emit('input', event.target.value)
                    },
                }
            )
        },
    },
    methods: {
        clearFilter() {
            this.$emit('input', null)
            this.$emit('changed')
        },
    },
    mounted() {
        this.disabled = this.$refs.input.disabled
        this.type = this.$refs.input.type
    },
}
</script>
