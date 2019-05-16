<template>
    <div :class="questionStyle" :id='questionId'>{{ answer }}</div>
</template>

<script lang="js">
    export default {
        name: 'TableRoster_ViewAnswer',

        data() {
            return {
                questionId: null
            }
        }, 
        computed: {
            $me() {
                return this.$store.state.webinterview.entityDetails[questionId] 
            },
            answer() {
                return this.$me.answer
            },
            questionStyle() {
                return {
                    'disabled-question' : this.$me.isDisabled,
                    'has-error' : this.$me.validity.isValid,
                    'has-warnings' : this.$me.validity.warnings.length > 0,
                    'not-applicable' : this.$me.isLocked
                }
            }
        },
        methods: {

        },
        created() {
            this.questionId = this.params.value.identity;
        }
    }
</script>











