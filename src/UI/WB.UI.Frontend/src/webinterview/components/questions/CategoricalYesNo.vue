<template>
    <wb-question
        :question="$me"
        :questionCssClassName="$me.ordered ? 'yes-no-question ordered' : 'yes-no-question'"
        :no-comments="noComments">
        <div class="question-unit">
            <div class="yes-no-mark">
                {{ $t("WebInterviewUI.Yes") }}
                <b>/</b>
                {{ $t("WebInterviewUI.No")}}
            </div>
            <div class="options-group">
                <div
                    class="radio"
                    v-for="option in answeredOrAllOptions"
                    :key="$me.id + '_' + option.value">
                    <div
                        class="field"
                        v-bind:class="{ 'unavailable-option locked-option': isProtected(option.value) }">
                        <input
                            class="wb-radio"
                            type="radio"
                            :name="$me.id + '_' + option.value"
                            :id="$me.id + '_' + option.value + '_yes'"
                            :checked="isYesChecked(option.value)"
                            :disabled="!$me.acceptAnswer"
                            value="true"
                            @change="answerYes(option.value, $event.target)"
                            v-disabledWhenUnchecked="{maxAnswerReached: allAnswersGiven, answerNotAllowed: !$me.acceptAnswer, forceDisabled: !$me.acceptAnswer || isProtected(option.value)}"/>
                        <label :for="$me.id + '_' + option.value + '_yes'">
                            <span class="tick"></span>
                        </label>
                        <b>/</b>
                        <input
                            class="wb-radio"
                            type="radio"
                            :name="$me.id + '_' + option.value"
                            :id="$me.id + '_' + option.value + '_no'"
                            :checked="isNoChecked(option.value)"
                            :disabled="!$me.acceptAnswer || isProtected(option.value)"
                            value="false"
                            @change="answerNo(option.value, $event.target)"/>
                        <label :for="$me.id + '_' + option.value + '_no'">
                            <span class="tick"></span>
                        </label>
                        <span>{{option.title}}</span>
                        <button
                            type="submit"
                            v-if="$me.acceptAnswer && !isProtected(option.value)"
                            class="btn btn-link btn-clear"
                            @click="clearAnswer(option.value)"
                            :id="`btn_${$me.id}_removeAnswer_opt_${option.value}`">
                            <span></span>
                        </button>
                        <div class="badge"
                            v-if="$me.ordered">
                            {{ getAnswerOrder(option.value)}}
                        </div>
                        <div class="lock"></div>
                    </div>
                    <wb-attachment :attachmentName="option.attachmentName"
                        :interviewId="interviewId"
                        v-if="option.attachmentName" />

                </div>
                <button
                    type="button"
                    class="btn btn-link btn-horizontal-hamburger"
                    @click="toggleOptions"
                    v-if="shouldShowAnsweredOptionsOnly && !showAllOptions"
                    :id="`btn_${$me.id}_ShowAllOptions`">
                    <span></span>
                </button>
                <wb-lock />
            </div>
        </div>
        <div
            v-if="allAnswersGiven"
            class="information-block text-info"><h6>{{ $t("WebInterviewUI.MaxAnswersCountSelected", {value: $me.maxSelectedAnswersCount }) }}</h6></div>
    </wb-question>
</template>
<script lang="js">
import { entityDetails } from '../mixins'
import Vue from 'vue'
import * as $ from 'jquery'
import modal from '@/shared/modal'
import { find, findIndex, filter } from 'lodash'
import { shouldShowAnsweredOptionsOnlyForMulti } from './question_helpers'

export default {
    name: 'CategoricalYesNo',
    data(){
        return {
            showAllOptions: false,
            answer: [],
        }
    },
    props: ['noComments'],
    watch: {
        '$me.answer'(to) {
            Vue.set(this, 'answer', to)
        },
    },
    computed: {
        allAnswersGiven() {
            const yesAnswers = $.grep(this.$me.answer, function(e){ return e.yes })
            const isMaxLimitReached = this.$me.maxSelectedAnswersCount && yesAnswers.length >= this.$me.maxSelectedAnswersCount
            return isMaxLimitReached
        },
        shouldShowAnsweredOptionsOnly(){
            return shouldShowAnsweredOptionsOnlyForMulti(this)
        },
        answeredOrAllOptions(){
            if(!this.shouldShowAnsweredOptionsOnly)
                return this.$me.options

            var self = this
            return filter(this.$me.options, function(option) {
                return $.grep(self.answer, (e) => { return e.value == option.value }).length != 0
            })
        },
        noOptions() {
            return this.$me.options == null || this.$me.options.length == 0
        },
    },
    methods: {
        toggleOptions(){
            this.showAllOptions = !this.showAllOptions
        },
        answerYes(optionValue){
            this.sendAnswer(optionValue, true)
            return true
        },
        answerNo(optionValue){
            this.sendAnswer(optionValue, false)
            return true
        },
        clearAnswer(optionValue){
            this.sendAnswer(optionValue, null)
            return true
        },
        isCanBeenAnswered(){
            if(!this.$me.acceptAnswer) return false

            const yesAnswers = $.grep(this.$me.answer, (e) => { return e.yes })
            const isMaxLimitReached = this.$me.maxSelectedAnswersCount && yesAnswers.length >= this.$me.maxSelectedAnswersCount
            return isMaxLimitReached
        },
        getAnswerOrder(optionValue){
            const yesAnswers = $.grep(this.$me.answer, (e) => { return e.yes })
            const answerIndex = findIndex(yesAnswers, (x) =>  { return x.value == optionValue })
            return  answerIndex > -1 ? answerIndex + 1 : ''
        },
        isYesChecked(optionValue) {
            const answerObj = $.grep(this.$me.answer, (e) => { return e.value == optionValue })
            return answerObj.length == 0 ? false : answerObj[0].yes
        },
        isNoChecked(optionValue) {
            const answerObj = $.grep(this.$me.answer, (e) => { return e.value == optionValue })
            return answerObj.length == 0 ? false : !answerObj[0].yes
        },
        isProtected(optionValue) {
            const answerObj = $.grep(this.$me.answer, (e) => { return e.value == optionValue })
            return answerObj.length == 0 ? false : answerObj[0].isProtected
        },
        sendAnswer(optionValue, answerValue) {
            const previousAnswer = this.$me.answer
            const isChangedAnswerOnOption = findIndex(previousAnswer, (x) => { return x.value == optionValue && x.yes == answerValue}) < 0
            if (!isChangedAnswerOnOption)
                return

            const previousAnswerWithoutCurrecntAnswered = $.grep(this.$me.answer, (e) =>{ return e.value != optionValue })
            const newAnswer = answerValue == null
                ? previousAnswerWithoutCurrecntAnswered
                : previousAnswerWithoutCurrecntAnswered.concat([{ value: optionValue, yes: answerValue }])

            if (!this.$me.isRosterSize)
            {
                this.$store.dispatch('answerYesNoQuestion', { identity: this.$me.id, answer: newAnswer })
                return
            }

            const currentYesAnswerCount = $.grep(newAnswer, function(e){ return e.yes }).length
            const previousYesAnswersCount = $.grep(this.$me.answer, function(e){ return e.yes }).length
            const isNeedRemoveRosters = currentYesAnswerCount < previousYesAnswersCount

            if (!isNeedRemoveRosters)
            {
                this.$store.dispatch('answerYesNoQuestion', { identity: this.$me.id, answer: newAnswer })
                return
            }
            const rosterTitle = find(this.answeredOrAllOptions, { value: optionValue}).title

            const confirmMessage = this.$t('WebInterviewUI.Interview_Questions_RemoveRowFromRosterMessage', {
                rosterTitle,
            } )

            modal.confirm(confirmMessage,  result => {
                if (result) {
                    this.$store.dispatch('answerYesNoQuestion', { identity: this.$me.id, answer: newAnswer })
                    return
                } else {
                    // trigger update for checkboxes to override some vue optimizations
                    Vue.nextTick(() => {
                        const opt = find(this.answer, { 'value': optionValue })
                        opt.yes = answerValue
                        Vue.nextTick( () => {
                            opt.yes = !answerValue
                        })
                    })
                    return
                }
            })
        },
    },
    mixins: [entityDetails],
}
</script>
