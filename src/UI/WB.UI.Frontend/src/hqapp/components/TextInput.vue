<template>
    <div class="field" :class="{ 'answered': hasValue === true }">
        <input class="form-control with-clear-btn" :class="{ 'input-validation-error': haserror === true }"
            @input="$emit('update:modelValue', $event.target.value)" :value="modelValue" ref="input"
            autocomplete="new-password" />
        <button type="button" class="btn btn-link btn-clear" v-if="hasValue" @click="clearFilter" tabindex="-1">
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
        modelValue: {
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
        hasValue: function () {
            return this.type == 'text' && !this.disabled && this.canClear && this.modelValue !== null && this.modelValue !== ''
        }
    },
    methods: {
        clearFilter() {
            this.$emit('update:modelValue', null)
            this.$emit('onChanged')
        },
    },
    mounted() {
        this.disabled = this.$refs.input.disabled
        this.type = this.$refs.input.type
    },
}
</script>
