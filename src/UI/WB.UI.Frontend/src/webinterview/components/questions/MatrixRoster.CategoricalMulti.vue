<template>
    <div :class="questionStyle"
        :id="`mr_view_${questionId}`">
        <popover
            class="tooltip-wrapper"
            trigger="hover-focus"
            append-to="body"
            :enable="!question.isDisabled && (question.validity.messages.length > 0 || question.validity.warnings.length > 0)">
            <a class="cell-content has-tooltip"
                type="primary"
                data-role="trigger"></a>
            <template slot="popover">
                <div class="error-tooltip"
                    v-if="!question.validity.isValid">
                    <h6
                        style="text-transform:uppercase;"
                        v-if="question.validity.errorMessage">{{ $t("WebInterviewUI.AnswerWasNotSaved") }}</h6>
                    <template v-for="message in question.validity.messages">
                        <span v-dateTimeFormatting
                            v-html="message"
                            :key="message"></span>
                    </template>
                </div>
                <div class="warning-tooltip"
                    v-else-if="question.validity.warnings.length > 0">
                    <template v-for="message in question.validity.warnings">
                        <span v-dateTimeFormatting
                            v-html="message"
                            :key="message"></span>
                    </template>
                </div>
            </template>
        </popover>
        <div
            class="cell-bordered d-flex"
            style="align-items:center;width:180px !important;max-width:180px;"
            v-for="option in editorParams.question.options"
            :key="$me.id + '_' + option.value"
            v-bind:class="{ 'unavailable-option locked-option': isProtected(option.value) }">
            <div class="field"
                style="width:180px;">
                <input
                    class="wb-checkbox"
                    type="checkbox"
                    :id="$me.id + '_' + option.value"
                    :name="$me.id"
                    :value="option.value"
                    v-model="answer"
                    @change="change"
                    v-disabledWhenUnchecked="{
                        maxAnswerReached: allAnswersGiven,
                        answerNotAllowed: disabled,
                        forceDisabled: isProtected(option.value) }"/>
                <label :for="$me.id + '_' + option.value">
                    <span class="tick"></span>
                </label>
            </div>
        </div>
    </div>
</template>

<script lang="js">
import Vue from 'vue'
import { filter, find, difference, join} from 'lodash'
import { entityDetails, tableCellEditor } from '../mixins'
import modal from '@/shared/modal'

export default {
    name: 'MatrixRoster_CategoricalMulti',
    mixins: [entityDetails, tableCellEditor],

    data() {
        return {
            showAllOptions: false,
            question: null,
            answer: [],
            lastUpdate: null,
            questionId: null,
        }
    },
    watch: {
        ['$watchedQuestion'](watchedQuestion) {
            if (watchedQuestion.updatedAt != this.lastUpdate) {
                this.question = watchedQuestion
                this.answer = watchedQuestion.answer
                this.cacheQuestionData()
            }
        },
    },
    computed: {
        $watchedQuestion() {
            return this.$store.state.webinterview.entityDetails[this.questionId]
        },

        disabled() {
            if (this.$me.isDisabled || this.$me.isLocked || !this.$me.acceptAnswer)
                return true
            return false
        },
        noOptions() {
            return this.$me.options == null || this.$me.options.length == 0
        },
        answeredOrAllOptions() {
            return this.$me.options
        },
        allAnswersGiven() {
            return this.$me.maxSelectedAnswersCount && this.$me.answer.length >= this.$me.maxSelectedAnswersCount
        },
        questionStyle() {
            return [{
                'disabled-element' : this.question.isDisabled,
                'has-error' : !this.question.isDisabled && !this.question.validity.isValid,
                'has-warnings' : !this.question.isDisabled && this.question.validity.warnings.length > 0,
                'not-applicable' : this.question.isLocked,
                'syncing': this.isFetchInProgress,
            }, 'cell-unit', 'options-group', ' h-100',' d-flex']
        },
    },
    methods: {
        cacheQuestionData() {
            this.lastUpdate = this.question.updatedAt
        },
        change() {
            this.sendAnswer(() => {
                this.answerMulti(this.answer)
            })
        },
        //questionId()  {
        //    return this.params.value.identity
        //},
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
    },
    created() {
        this.questionId = this.editorParams.value.identity
        this.question = this.$watchedQuestion
        this.cacheQuestionData()
    },
    mounted() {
        this.answer = this.$me.answer
    },
}
</script>
