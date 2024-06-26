<template>
    <div :class="questionStyle" :id="`tr_view_${questionId}`">
        <popover trigger="hover-focus" append-to="body"
            :enable="!question.isDisabled && (question.validity.messages.length > 0 || question.validity.warnings.length > 0)"
            v-if="!question.isDisabled">
            <a class="cell-content has-tooltip" type="primary" data-role="trigger">
                <span
                    v-if="(questionType == 'Integer' || questionType == 'Double') && question.useFormatting">{{ question.answer
                    | formatNumber}}</span>
                <span v-else>{{ question.answer }}</span>
            </a>

            <template v-slot:popover>
                <div class="error-tooltip" v-if="!question.validity.isValid">
                    <h6 style="text-transform:uppercase;" v-if="question.validity.errorMessage">{{
                        $t("WebInterviewUI.AnswerWasNotSaved") }}</h6>
                    <template v-for="message in question.validity.messages">
                        <div v-dateTimeFormatting v-html="message" :key="message"></div>
                    </template>
                </div>
                <div class="warning-tooltip" v-else-if="question.validity.warnings.length > 0">
                    <template v-for="message in question.validity.warnings">
                        <div v-dateTimeFormatting v-html="message" :key="message"></div>
                    </template>
                </div>
            </template>
        </popover>
        <wb-progress :visible="isFetchInProgress" />
    </div>
</template>

<script lang="js">
export default {
    name: 'TableRoster_ViewAnswer',

    data() {
        return {
            questionId: null,
            questionType: null,
            question: null,
            lastUpdate: null,
        }
    },
    watch: {
        ['$watchedQuestion'](watchedQuestion) {
            if (watchedQuestion.updatedAt != this.lastUpdate) {
                this.question = watchedQuestion
                this.cacheQuestionData()
            }
        },
    },
    computed: {
        $watchedQuestion() {
            return this.$store.state.webinterview.entityDetails[this.questionId]
        },
        questionStyle() {
            return [{
                'disabled-element': this.question.isDisabled,
                'has-error': !this.question.isDisabled && !this.question.validity.isValid,
                'has-warnings': !this.question.isDisabled && this.question.validity.warnings.length > 0,
                'not-applicable': this.question.isLocked,
                'syncing': this.isFetchInProgress,
            }, 'cell-unit']
        },
        doesExistValidationMessage() {
            const validity = this.question.validity
            if (validity.messages && validity.messages.length > 0)
                return true
            if (validity.warnings && validity.warnings.length > 0)
                return true
            return false
        },
        isFetchInProgress() {
            const result = this.$store.state.webinterview.fetch.state[this.questionId]
            return result
        },
    },
    methods: {
        cacheQuestionData() {
            this.lastUpdate = this.question.updatedAt
        },
    },
    created() {
        this.questionId = this.params.value.identity
        this.questionType = this.params.value.type
        this.question = this.$watchedQuestion
        this.cacheQuestionData()
    },
    filters: {
        formatNumber(value) {
            if (value == null || value == undefined || Number.isNaN(value))
                return ''

            return value.toLocaleString(undefined, { style: 'decimal', maximumFractionDigits: 15, minimumFractionDigits: 0 })
        },
    },
}
</script>
