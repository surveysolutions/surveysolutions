<template>
    <wb-question :question="$me" :questionCssClassName="$me.ordered ? 'yes-no-question ordered' : 'yes-no-question'">
        <div class="question-unit">
            <div class="yes-no-mark">Yes <b>/</b> No</div>
            <div class="options-group">
                <div class="radio" v-for="option in $me.options">
                    <div class="field">
                        <input class="wb-radio" type="radio" :name="$me.id + '_' + option.value" :id="$me.id + '_' + option.value + '_yes'" :checked="isYesChecked(option.value)" value="true" @click="answerYes(option.value)" />
                        <label :for="$me.id + '_' + option.value + '_yes'">
                            <span class="tick"></span>
                        </label>
                        <b>/</b>
                        <input class="wb-radio" type="radio" :name="$me.id + '_' + option.value" :id="$me.id + '_' + option.value + '_no'" :checked="isNoChecked(option.value)" value="false" @click="answerNo(option.value)" />
                        <label :for="$me.id + '_' + option.value + '_no'">
                            <span class="tick"></span>
                        </label>
                        <span>{{option.title}}</span>
                        <button type="submit" class="btn btn-link btn-clear" @click="clearAnswer(option.value)">
                            <span></span>
                        </button>
                        <div class="badge" v-if="$me.ordered">{{ getAnswerOrder(option.value)}}</div>
                    </div>
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
        name: 'CategoricalYesNo',
        computed: {
            answer: {
                get() {
                    return this.$me.answer
                },
            },
            allAnswersGiven() {
                return this.$me.maxSelectedAnswersCount && this.$me.answer.length >= this.$me.maxSelectedAnswersCount
            }
        },
        methods: {
            answerYes(optionVlue){
                 this.sendAnswer(optionVlue, true);
                 return true;
            },
            answerNo(optionVlue){
                 this.sendAnswer(optionVlue, false);
                 return true;
            },
            clearAnswer(optionVlue){
                 this.sendAnswer(optionVlue, null);
                 return true;
            },
            getAnswerOrder(answerValue){
                const yesAnswers = $.grep(this.$me.answer, function(e){ return e.yes; });
                const answerIndex = yesAnswers.findIndex(x => x.value == answerValue)
                return  answerIndex > -1 ? answerIndex + 1 : ""
            },
            isYesChecked(answerValue) {
                const answerObj = $.grep(this.$me.answer, function(e){ return e.value == answerValue; });
                return answerObj.length == 0 ? false : answerObj[0].yes;
            },
            isNoChecked(answerValue) {
                const answerObj = $.grep(this.$me.answer, function(e){ return e.value == answerValue; });
                if (answerObj.length == 0) {
                    return false;
                } else {
                    return !answerObj[0].yes;
                }
            },
            sendAnswer(optionValue, answerValue) {
                const previousAnswer = this.$me.answer;
                const previousAnswerWithoutCurrecntAnswered = $.grep(this.$me.answer, function(e){ return e.value != optionValue; });
                const newAnswer = answerValue == null
                    ? previousAnswerWithoutCurrecntAnswered
                    : previousAnswerWithoutCurrecntAnswered.concat([{ value: optionValue, yes: answerValue }]);

                if (!this.$me.isRosterSize)
                {
                    this.$store.dispatch("answerYesNoQuestion", { questionId: this.$me.id, answer: newAnswer })
                    return;
                }

                const currentAnswerCount = newAnswer.length;
                const previousAnswersCount = this.$me.answer.length;
                const isNeedRemoveRosters = currentAnswerCount < previousAnswersCount;

                if (!isNeedRemoveRosters)
                {
                    this.$store.dispatch('answerYesNoQuestion', { questionId: this.$me.id, answer: newAnswer });
                    return;
                }

                const confirmMessage = 'Are you sure you want to remove related roster?';

                modal.methods.confirm(confirmMessage,  result => {
                    if (result) {
                        this.$store.dispatch("answerYesNoQuestion", { questionId: this.$me.id, answer: newAnswer })
                        return;
                    } else {
                        this.fetch()
                        return
                    }
                })
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
