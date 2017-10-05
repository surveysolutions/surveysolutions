<template>
    <wb-question :question="$me"
                 questionCssClassName="single-select-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field"
                         :class="{answered: $me.isAnswered}">
                        <wb-typeahead :questionId="$me.id"
                                      :value="$me.answer"
                                      :optionsSource="optionsSource"
                                      @input="answerComboboxQuestion" />
                        <wb-remove-answer />
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
</template>

<script lang="js">

    import { entityDetails } from "../mixins"
    
    export default {
        name: 'ComboboxQuestion',
        mixins: [entityDetails],

        data() {
            return {
                answer: null,
            }
        },

        watch: {
            answer(newValue) {
                this.answerComboboxQuestion(newValue)
            }
        },
        
        methods: {
            answerComboboxQuestion(newValue) {
                 this.$store.dispatch("answerSingleOptionQuestion", { answer: newValue, questionId: this.$me.id })
            },

            async optionsSource(filter) {
                return await this.$apiCaller(api => api.getTopFilteredOptionsForQuestion(this.$me.id, filter, 30))
            }
        }
    }

</script>
