<template>
    <wb-question
        :question="$me"
        :questionCssClassName="$me.ordered ? 'ordered-question' : 'multiselect-question'"
        :noAnswer="noOptions">
        <button class="section-blocker"
            disabled="disabled"
            v-if="inFetchState"></button>
        <div class="question-unit">
            <div class="options-group"
                v-bind:class="{ 'dotted': noOptions }">
                <div
                    class="form-group"
                    v-for="option in answeredOrAllOptions"
                    :key="$me.id + '_' + option.value"
                    v-bind:class="{ 'unavailable-option locked-option': isProtected(option.value) }">
                    <input
                        class="wb-checkbox"
                        type="checkbox"
                        :id="$me.id + '_' + option.value"
                        :name="$me.id"
                        :value="option.value"
                        :disabled="!$me.acceptAnswer"
                        v-model="answer"
                        @change="change"
                        v-disabledWhenUnchecked="{
                            maxAnswerReached: allAnswersGiven,
                            answerNotAllowed: !$me.acceptAnswer,
                            forceDisabled: isProtected(option.value) }"/>
                    <label :for="$me.id + '_' + option.value">
                        <span class="tick"></span>
                        {{option.title}}
                    </label>
                    <div class="badge"
                        v-if="$me.ordered">
                        {{ getAnswerOrder(option.value) }}
                    </div>
                    <div class="lock"></div>
                    <wb-attachment :attachmentName="option.attachmentName"
                        :interviewId="interviewId"
                        customCssClass="static-text-image"
                        v-if="option.attachmentName" />

                </div>
                <button
                    type="button"
                    class="btn btn-link btn-horizontal-hamburger"
                    @click="toggleOptions"
                    :id="`btn_${$me.id}_ShowAllOptions`"
                    v-if="shouldShowAnsweredOptionsOnly && !showAllOptions">
                    <span></span>
                </button>
                <div
                    v-if="noOptions"
                    class="options-not-available">{{ $t("WebInterviewUI.OptionsAvailableAfterAnswer") }}</div>
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
import modal from '@/shared/modal'
import { filter, find, difference, join } from 'lodash'
import { shouldShowAnsweredOptionsOnlyForMulti } from './question_helpers'

export default {
    name: 'CategoricalMulti',
    data(){
        return {
            showAllOptions: false,
            answer: [],
        }
    },
    created(){
        this.$watch('$me.answer', (to, from) => {
            this.answer = to
        }, { immediate: true })
    },

    computed: {
        shouldShowAnsweredOptionsOnly(){
            return shouldShowAnsweredOptionsOnlyForMulti(this)
        },
        answeredOrAllOptions(){
            if(!this.shouldShowAnsweredOptionsOnly){
                return this.$me.options
            }

            var self = this
            return filter(this.$me.options, function(o) {
                return self.$me.answer.indexOf(o.value) >= 0
            })
        },

        noOptions() {
            return this.$me.options == null || this.$me.options.length == 0
        },
        allAnswersGiven() {
            return this.$me.maxSelectedAnswersCount && this.$me.answer.length >= this.$me.maxSelectedAnswersCount
        },
    },
    methods: {
        change() {
            this.sendAnswer(() => {
                this.answerMulti(this.answer)
            })
        },
        toggleOptions(){
            this.showAllOptions = !this.showAllOptions
        },
        isProtected(answerValue) {
            if (!this.$me.protectedAnswer) return false

            var answerIndex = this.$me.protectedAnswer.indexOf(answerValue)
            return answerIndex > -1
        },
        getAnswerOrder(answerValue) {
            var answerIndex = this.$me.answer.indexOf(answerValue)
            return answerIndex > -1 ? answerIndex + 1 : ''
        },
        answerMulti(value) {
            if (!this.$me.isRosterSize) {
                this.$store.dispatch('answerMultiOptionQuestion', { answer: value, identity: this.$me.id })
                return
            }

            const currentAnswerCount = value.length
            const previousAnswersCount = this.$me.answer.length
            const isNeedRemoveRosters = currentAnswerCount < previousAnswersCount

            if (!isNeedRemoveRosters) {
                this.$store.dispatch('answerMultiOptionQuestion', { answer: value, identity: this.$me.id })
                return
            }

            const diff = difference(this.$me.answer, value)
            const rosterTitle = join(diff.map(v => {
                return find(this.answeredOrAllOptions, { value: v }).title
            }), ', ')

            const confirmMessage = this.$t('WebInterviewUI.Interview_Questions_RemoveRowFromRosterMessage', {
                rosterTitle,
            } )

            modal.confirm(confirmMessage, result => {
                if (result) {
                    this.$store.dispatch('answerMultiOptionQuestion', { answer: value, identity: this.$me.id })
                    return
                } else {
                    this.fetch()
                    return
                }
            })
        },
    },
    mounted() {
        this.answer = this.$me.answer
    },
    mixins: [entityDetails],
}

</script>
