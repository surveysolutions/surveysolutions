<template>
    <wb-question :question="$me" questionCssClassName="single-select-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field" :class="{answered: $me.isAnswered}">
                        <wb-typeahead :questionId="$me.id" :value="$me.answer" @input="answerComboboxQuestion" />
                        <wb-remove-answer />
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "components/mixins"

    export default {
        name: 'ComboboxQuestion',
        mixins: [entityDetails],
        data() {
            return {
                answer: null,
            }
        },
        computed: {

        },
        watch: {
            answer(newValue) {
                this.answerComboboxQuestion(newValue)
            }
        },
        methods: {
            answerComboboxQuestion(newValue) {
                 this.$store.dispatch("answerSingleOptionQuestion", { answer: newValue, questionId: this.$me.id })
            }
        }
    }
</script>
