<template>
    <div class="input-group">
        <input class="form-control with-clear-btn" :placeholder="placeholder" :type="type" :value="value"
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
        value: { type: String },
        type: { type: String, default: 'text' },
    },

    emits: ['input'],

    methods: {
        remove() {
            this.$emit('input', null)
        },

        input(event) {
            this.$emit('input', event.target.value)
        },
    },
}
</script>