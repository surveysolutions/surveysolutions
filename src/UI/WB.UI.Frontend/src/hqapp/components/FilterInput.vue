<template>
    <div class="input-group">
        <input class="form-control with-clear-btn" :placeholder="placeholder" :type="type" :value="modelValue"
            @input.self="input" />
        <div class="input-group-btn" @click="remove">
            <div class="btn btn-default">
                <span class="glyphicon glyphicon-remove" aria-hidden="true"></span>
            </div>
        </div>
    </div>
</template>

<script>

export default {
    props: {
        placeholder: { type: String },
        modelValue: { type: String },
        type: { type: String, default: 'text' },
    },

    emits: ['input', 'update:modelValue'],

    methods: {
        remove() {
            this.$emit('update:modelValue', null)
            this.$emit('input', null)
        },

        input(event) {
            this.$emit('update:modelValue', event.target.value)
            this.$emit('input', event.target.value)
        },
    },
}
</script>