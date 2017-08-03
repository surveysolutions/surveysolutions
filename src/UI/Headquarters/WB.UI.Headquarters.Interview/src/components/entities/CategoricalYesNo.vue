<template>
    <wb-question :question="$me" :questionCssClassName="$me.ordered ? 'yes-no-question ordered' : 'yes-no-question'">
        <div class="question-unit">
            <div class="yes-no-mark">Yes <b>/</b> No</div>
            <div class="options-group">
                <div class="radio" v-for="option in $me.options">
                    <div class="field">
                        <input class="wb-radio" type="radio" :name="$me.id + '_' + option.value" :id="$me.id + '_' + option.value + '_yes'" :checked="isYesChecked(option.value)" value="true"
                            @click="answerYes(option.value)" v-disabledWhenUnchecked="allAnswersGiven" />
                        <label :for="$me.id + '_' + option.value + '_yes'">
                            <span class="tick"></span>
                        </label>
                        <b>/</b>
                        <input class="wb-radio" type="radio" :name="$me.id + '_' + option.value" :id="$me.id + '_' + option.value + '_no'" :checked="isNoChecked(option.value)" value="false"
                            @click="answerNo(option.value)" />
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
<script lang="js">
    import { entityDetails } from "components/mixins"

    import * as $ from "jquery"
    import modal from "../../modal"
    import * as findIndex from "lodash/findIndex"

    export default {
        name: 'CategoricalYesNo',
        computed: {
            allAnswersGiven() {
                const yesAnswers = $.grep(this.$me.answer, function(e){ return e.yes; });
                const isMaxLimitReached = this.$me.maxSelectedAnswersCount && yesAnswers.length >= this.$me.maxSelectedAnswersCount;
                return isMaxLimitReached;
            }
        },
        methods: {
            answerYes(optionValue){
                 this.sendAnswer(optionValue, true);
                 return true;
            },
            answerNo(optionValue){
                 this.sendAnswer(optionValue, false);
                 return true;
            },
            clearAnswer(optionValue){
                 this.sendAnswer(optionValue, null);
                 return true;
            },
            isCanBeenAnswered(optionValue){
                const yesAnswers = $.grep(this.$me.answer, function(e){ return e.yes; });
                const isMaxLimitReached = this.$me.maxSelectedAnswersCount && yesAnswers.length >= this.$me.maxSelectedAnswersCount;
                return isMaxLimitReached;
            },
            getAnswerOrder(optionValue){
                const yesAnswers = $.grep(this.$me.answer, function(e){ return e.yes; });
                const answerIndex = findIndex(yesAnswers, function(x) { return x.value == optionValue })
                return  answerIndex > -1 ? answerIndex + 1 : ""
            },
            isYesChecked(optionValue) {
                const answerObj = $.grep(this.$me.answer, function(e){ return e.value == optionValue; });
                return answerObj.length == 0 ? false : answerObj[0].yes;
            },
            isNoChecked(optionValue) {
                const answerObj = $.grep(this.$me.answer, function(e){ return e.value == optionValue; });
                return answerObj.length == 0 ? false : !answerObj[0].yes;
            },
            sendAnswer(optionValue, answerValue) {
                const previousAnswer = this.$me.answer;
                const isChangedAnswerOnOption = findIndex(previousAnswer, function(x) { return x.value == optionValue && x.yes == answerValue}) < 0;
                if (!isChangedAnswerOnOption)
                    return;

                const previousAnswerWithoutCurrecntAnswered = $.grep(this.$me.answer, function(e){ return e.value != optionValue; });
                const newAnswer = answerValue == null
                    ? previousAnswerWithoutCurrecntAnswered
                    : previousAnswerWithoutCurrecntAnswered.concat([{ value: optionValue, yes: answerValue }]);

                if (!this.$me.isRosterSize)
                {
                    this.$store.dispatch("answerYesNoQuestion", { questionId: this.$me.id, answer: newAnswer })
                    return;
                }

                const currentYesAnswerCount = $.grep(newAnswer, function(e){ return e.yes; }).length;
                const previousYesAnswersCount = $.grep(this.$me.answer, function(e){ return e.yes; }).length;
                const isNeedRemoveRosters = currentYesAnswerCount < previousYesAnswersCount;

                if (!isNeedRemoveRosters)
                {
                    this.$store.dispatch('answerYesNoQuestion', { questionId: this.$me.id, answer: newAnswer });
                    return;
                }

                const confirmMessage = 'Are you sure you want to remove related roster?';

                modal.confirm(confirmMessage,  result => {
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
        mixins: [entityDetails]
    }
</script>
