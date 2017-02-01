<template>
    <wb-question :question="$me" :questionCssClassName="$me.ordered ? 'ordered-question' : 'multiselect-question'">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group" v-for="option in $me.options">
                    <input class="wb-checkbox" type="checkbox" :id="$me.id + '_' + option.value" :name="$me.id" :value="option.value" v-model="answer"
                        v-disabledWhenUnchecked="allAnswersGiven">
                    <label :for="$me.id + '_' + option.value">
                        <span class="tick"></span> {{option.title}}
                    </label>
                    <div class="badge" v-if="$me.ordered">{{getAnswerOrder(option.value)}}</div>
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"
    import * as $ from "jquery"

    export default {
        name: 'LinkedMulti',
        computed: {
            answer: {
                get() {
                    return this.$me.answer
                },
                set(value) {
                    if (this.$me.isLinkedToList){
                        this.$store.dispatch("answerLinkedToListMultiQuestion", { answer: value, questionIdentity: this.$me.id })
                    }
                    this.$store.dispatch("answerMultiOptionQuestion", { answer: value, questionIdentity: this.$me.id })
                    return;
                }
            },
            allAnswersGiven() {
                return this.$me.maxSelectedAnswersCount && this.$me.answer.length >= this.$me.maxSelectedAnswersCount
            }
        },
        methods: {
            getAnswerOrder(answerValue){
                var answerIndex = this.$me.answer.indexOf(answerValue)
                return  answerIndex > -1 ? answerIndex + 1 : ""
            }
        },
        directives: {
            disabledWhenUnchecked: {
                update: (el, binding) => {
                    $(el).prop("disabled", binding.value && !el.checked)
                }
            }
        },
        mixins: [entityDetails]
    }
</script>
