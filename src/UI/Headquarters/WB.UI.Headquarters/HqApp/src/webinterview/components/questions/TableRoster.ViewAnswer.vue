<template>
    <span :id='questionId'>{{ getAnswer() }}</span>
</template>

<script lang="js">
    export default {
        name: 'TableRoster_ViewAnswer',
        methods: {
            getAnswer() {
                return this.params.value.answer
            },
            questionId()  {
                return this.params.value.id;
            }
        }
    }
</script>











