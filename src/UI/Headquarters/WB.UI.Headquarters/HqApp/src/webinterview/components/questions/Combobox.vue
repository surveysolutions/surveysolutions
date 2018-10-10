<template>
    <wb-question :question="$me"
                 questionCssClassName="single-select-question"
                 :no-comments="noComments">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field"
                         :class="{answered: $me.isAnswered}">
                        <wb-typeahead :questionId="$me.id"
                                      :value="$me.answer"
                                      :disabled="!$me.acceptAnswer"
                                      :optionsSource="optionsSource"
                                      @input="answerComboboxQuestion" 
                                      :watermark="!$me.acceptAnswer && !$me.isAnswered ? $t('Details.NoAnswer') : null"/>
                        <wb-remove-answer />
                    </div>
                </div>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>

<script lang="js">

    import { entityDetails } from "../mixins"
    import Vue from 'vue'

    export default {
        name: 'ComboboxQuestion',
        mixins: [entityDetails],
        props: ['noComments'],
        data() {
            return {
                answer: null
            }
        },

        watch: {
            answer(newValue) {
                this.answerComboboxQuestion(newValue)
            }
        },
        
        methods: {
            answerComboboxQuestion(newValue) {
                this.sendAnswer(() => {
                    this.$store.dispatch("answerSingleOptionQuestion", { answer: newValue, questionId: this.$me.id })
                })
            },

            optionsSource(filter) {
                return Vue.$api.call(api => api.getTopFilteredOptionsForQuestion(this.$me.id, filter, 50))
            }
        }
    }

</script>
