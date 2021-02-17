<script>
export default {
    props: {
        value: {
            type: String,
            default: '',
        },
    },

    computed: {
        model: {
            get() {
                return this.value
            },
            set(value) {
                this.$emit('input', value)
            },
        },
    },
}
</script>

<template>
    <input type="text"
        v-model="model"
        placeholder="dummy input">
</template>
