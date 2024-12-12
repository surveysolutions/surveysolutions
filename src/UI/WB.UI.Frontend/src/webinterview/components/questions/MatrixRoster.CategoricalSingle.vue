<template>
    <div :class="questionStyle" :id='`mr_view_${questionId}`'>
        <popover class="tooltip-wrapper" trigger="hover-focus" append-to="body"
            :enable="!question.isDisabled && (question.validity.messages.length > 0 || question.validity.warnings.length > 0)">
            <a class="has-tooltip" type="primary" data-role="trigger"></a>
            <template v-slot:popover>
                <div class="error-tooltip" v-if="!question.validity.isValid || question.validity.errorMessage">
                    <h6 style="text-transform:uppercase;" v-if="question.validity.errorMessage">
                        {{ $t("WebInterviewUI.AnswerWasNotSaved") + (question.validity.notSavedAnswerValue ? ': "' +
                            question.validity.notSavedAnswerValue + '"' : '') }}
                    </h6>
                    <template v-for="message in question.validity.messages" :key="message">
                        <div v-dateTimeFormatting v-dompurify-html="message"></div>
                    </template>
                </div>
                <div class="warning-tooltip" v-else-if="question.validity.warnings.length > 0">
                    <template v-for="message in question.validity.warnings" :key="message">
                        <div v-dateTimeFormatting v-dompurify-html="message"></div>
                    </template>
                </div>
            </template>

        </popover>
        <div class="radio cell-bordered" style="width:180px !important;max-width:180px;"
            v-for="option in editorParams.question.options" :key="$me.id + '_' + option.value">
            <div class="field" :class="{ 'answered': $me.isAnswered }" style="width:180px;">
                <input class="wb-radio" type="radio" :id="`${$me.id}_${option.value}`" :name="$me.id"
                    :value="option.value" :disabled="disabled" v-model="answer" @change="change">
                <label :for="$me.id + '_' + option.value" style="padding-top:10px;padding-bottom: 10px;">
                    <span class="tick"></span>
                </label>

                <wb-remove-answer v-if="option.value === $me.answer" />
            </div>
        </div>
    </div>

</template>

<script lang="js">
import { entityDetails, tableCellEditor } from '../mixins'

export default {
    name: 'MatrixRoster_CategoricalSingle',
    mixins: [entityDetails, tableCellEditor],
    props: ['isDisabled'],
    data() {
        return {
            showAllOptions: false,
            answer: null,
            question: null,
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
        questionStyle() {
            return [{
                'disabled-element': this.question.isDisabled,
                'has-error': !this.question.isDisabled && !this.question.validity.isValid,
                'has-warnings': !this.question.isDisabled && this.question.validity.warnings.length > 0,
                'not-applicable': this.question.isLocked,
                'syncing': this.isFetchInProgress,
            }, 'cell-unit', 'options-group', ' h-100', ' d-flex']
        },
    },
    methods: {
        cacheQuestionData() {
            this.lastUpdate = this.question.updatedAt
        },
        change() {
            this.sendAnswer(() => {
                this.answerSingle(this.answer)
            })
        },
        answerSingle(value) {
            this.$store.dispatch('answerSingleOptionQuestion', { answer: value, identity: this.$me.id })
        },
        toggleOptions() {
            this.showAllOptions = !this.showAllOptions
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
