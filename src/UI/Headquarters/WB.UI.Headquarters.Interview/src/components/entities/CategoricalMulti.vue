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
    import modal from "../Modal"

    export default {
        name: 'CategoricalMulti',
        computed: {
            answer: {
                get() {
                    return this.$me.answer
                },
                set(value) {

                    if (!this.$me.isRosterSize)
                    {
                        this.$store.dispatch("answerMutliOptionQuestion", { answer: value, questionId: this.$me.id })
                        return;
                    }

                    var currentAnswerCount = value.length;
                    var previousAnswersCount = this.$me.answer.length;
                    var isNeedRemoveRosters = currentAnswerCount < previousAnswersCount;

                    if (!isNeedRemoveRosters)
                    {
                        this.$store.dispatch('answerMutliOptionQuestion', { answer: value, questionId: this.$me.id });
                        return;
                    }

                    var confirmMessage = 'Are you sure you want to remove related roster?';
                    var oThis = this;

                    modal.methods.confirm(confirmMessage,  function (result) {
                        if (result) {
                            oThis.$store.dispatch("answerMutliOptionQuestion", { answer: value, questionId: oThis.$me.id })
                            return;
                        } else {
                            oThis.fetch();
                            return;
                        }
                    } );
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
