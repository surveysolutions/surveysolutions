<template>
    <span :id='questionId'>{{ answer }}</span>
</template>

<script lang="js">
export default {
    name: 'MatrixRoster_ViewAnswer',
    computed: {
        $me() {
            return this.$store.state.webinterview.entityDetails[this.params.value.identity]
        },
        answer() {
            return this.$me.answer
        },
    },
    methods: {
        questionId()  {
            return this.params.value.identity
        },
    },
}
</script>
