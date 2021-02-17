<script>
export default {
    props: {
        value: {
            type: Number,
            default: 0,
        },
    },

    computed: {
        model: {
            get() {
                return this.value
            },
            set(value) {
                if (typeof vlaue === 'number') {
                    this.$emit('input', value)
                }
            },
        },
    },
}
</script>

<template>
    <input type="number"
        v-model.number="model"
        placeholder="dummy number">
</template>
