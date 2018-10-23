<template>
    <wb-question :question="$me" :questionCssClassName="$me.ordered ? 'yes-no-question ordered' : 'yes-no-question'"  :no-comments="noComments">
        <div class="question-unit">
            <div class="yes-no-mark">{{ $t("WebInterviewUI.Yes") }} <b>/</b> {{ $t("WebInterviewUI.No")}}</div>
            <div class="options-group">
                <div class="radio" v-for="option in answeredOrAllOptions" :key="$me.id + '_' + option.value">
                    <div class="field" v-bind:class="{ 'unavailable-option locked-option': isProtected(option.value) }">
                        <input class="wb-radio" type="radio" 
                            :name="$me.id + '_' + option.value" 
                            :id="$me.id + '_' + option.value + '_yes'" 
                            :checked="isYesChecked(option.value)"
                            :disabled="!$me.acceptAnswer"
                            value="true"                            
                            @click="answerYes(option.value)" 
                            v-disabledWhenUnchecked="{maxAnswerReached: allAnswersGiven, answerNotAllowed: !$me.acceptAnswer, forceDisabled: !$me.acceptAnswer || isProtected(option.value)}" />
                        <label :for="$me.id + '_' + option.value + '_yes'">
                            <span class="tick"></span>
                        </label>
                        <b>/</b>
                        <input class="wb-radio" type="radio" 
                            :name="$me.id + '_' + option.value" 
                            :id="$me.id + '_' + option.value + '_no'" 
                            :checked="isNoChecked(option.value)"
                            :disabled="!$me.acceptAnswer || isProtected(option.value)"
                            value="false"
                            @click="answerNo(option.value)" />
                        <label :for="$me.id + '_' + option.value + '_no'">
                            <span class="tick"></span>
                        </label>
                        <span>{{option.title}}</span>
                        <button type="submit" v-if="$me.acceptAnswer && !isProtected(option.value)" class="btn btn-link btn-clear" @click="clearAnswer(option.value)">
                            <span></span>
                        </button>
                        <div class="badge" v-if="$me.ordered">{{ getAnswerOrder(option.value)}}</div>
                        <div class="lock"></div>
                    </div>
                </div>
                <button type="button" class="btn btn-link btn-horizontal-hamburger" @click="toggleOptions" v-if="shouldShowAnsweredOptionsOnly && !showAllOptions">
                    <span></span>
                </button>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "../mixins"

    import * as $ from "jquery"
    import modal from "../modal"
    import { findIndex, filter } from "lodash"
    import { shouldShowAnsweredOptionsOnlyForMulti } from "./question_helpers"
    
    export default {
        name: 'CategoricalYesNo',
        data(){
            return {
                showAllOptions: false
            }
        },
        props: ['noComments'],
        computed: {
            allAnswersGiven() {
                const yesAnswers = $.grep(this.$me.answer, function(e){ return e.yes; });
                const isMaxLimitReached = this.$me.maxSelectedAnswersCount && yesAnswers.length >= this.$me.maxSelectedAnswersCount;
                return isMaxLimitReached;
            },
            shouldShowAnsweredOptionsOnly(){
                 return shouldShowAnsweredOptionsOnlyForMulti(this);
            },
            answeredOrAllOptions(){
                if(!this.shouldShowAnsweredOptionsOnly)
                    return this.$me.options;
                
                var self = this;
                return filter(this.$me.options, function(option) {
                    return $.grep(self.$me.answer, (e) => { return e.value == option.value; }).length != 0 
                    });
            },
            noOptions() {
                return this.$me.options == null || this.$me.options.length == 0;
            }
        },
        methods: {
            toggleOptions(){
                this.showAllOptions = !this.showAllOptions;
            },
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
                if(!this.$me.acceptAnswer) return false;

                const yesAnswers = $.grep(this.$me.answer, (e) => { return e.yes; });
                const isMaxLimitReached = this.$me.maxSelectedAnswersCount && yesAnswers.length >= this.$me.maxSelectedAnswersCount;
                return isMaxLimitReached;
            },
            getAnswerOrder(optionValue){
                const yesAnswers = $.grep(this.$me.answer, (e) => { return e.yes; });
                const answerIndex = findIndex(yesAnswers, (x) =>  { return x.value == optionValue })
                return  answerIndex > -1 ? answerIndex + 1 : ""
            },
            isYesChecked(optionValue) {
                const answerObj = $.grep(this.$me.answer, (e) => { return e.value == optionValue; });
                return answerObj.length == 0 ? false : answerObj[0].yes;
            },
            isNoChecked(optionValue) {
                const answerObj = $.grep(this.$me.answer, (e) => { return e.value == optionValue; });
                return answerObj.length == 0 ? false : !answerObj[0].yes;
            },
            isProtected(optionValue) {
                const answerObj = $.grep(this.$me.answer, (e) => { return e.value == optionValue; });
                return answerObj.length == 0 ? false : answerObj[0].isProtected;
            },
            sendAnswer(optionValue, answerValue) {
                const previousAnswer = this.$me.answer;
                const isChangedAnswerOnOption = findIndex(previousAnswer, (x) => { return x.value == optionValue && x.yes == answerValue}) < 0;
                if (!isChangedAnswerOnOption)
                    return;

                const previousAnswerWithoutCurrecntAnswered = $.grep(this.$me.answer, (e) =>{ return e.value != optionValue; });
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

                const confirmMessage = this.$t("WebInterviewUI.ConfirmRosterRemove");

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
