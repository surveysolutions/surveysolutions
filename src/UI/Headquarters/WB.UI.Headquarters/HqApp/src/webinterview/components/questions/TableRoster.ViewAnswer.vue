<template>
    <div :class="questionStyle" :id='questionId'>
        <popover  :title="validationTitle" :enable="doesExistValidationMessage" trigger="hover-focus" append-to="body">
            <a class="cell-content has-tooltip" type="primary" data-role="trigger">
                <span v-if="(questionType == 'Integer' || questionType == 'Double') && question.useFormatting">
                    {{question.answer | formatNumber}}
                </span>
                <span v-else>{{question.answer}}</span>
            </a>
            <template slot="popover">
                <div class="popover-content error-tooltip" v-html="validationMessage"></div>
            </template>
        </popover>

        <wb-progress :visible="isFetchInProgress" :valuenow="valuenow" :valuemax="valuemax" />
    </div>
</template>

<script lang="js">
    export default {
        name: 'TableRoster_ViewAnswer',

        data() {
            return {
                questionId: null,
                questionType: null,
                question : null,
                lastUpdate: null
            }
        }, 
        watch: {
            ["$me"](watchedQuestion) {
                if (watchedQuestion.updatedAt != this.lastUpdate) {
                    this.question = watchedQuestion
                    this.cacheQuestionData()
                }
            }
        },
        computed: {
            $me() {
                return this.$store.state.webinterview.entityDetails[this.questionId] 
            },

            questionStyle() {
                return [{
                    'disabled-question' : this.question.isDisabled,
                    'has-error' : !this.question.validity.isValid,
                    'has-warnings' : this.question.validity.warnings.length > 0,
                    'not-applicable' : this.question.isLocked,
                    'syncing': this.question.fetching
                }, 'cell-unit']
            },
            isFetchInProgress() {
                return this.question.fetching
            },
            doesExistValidationMessage() {
                const validity = this.question.validity
                if (validity.messages && validity.messages.length > 0)
                    return true
                if (validity.warnings && validity.warnings.length > 0)
                    return true
                return false
            },
            validationTitle() {
                const validity = this.question.validity
                if (validity.messages && validity.messages.length > 0)
                    return 'Error'
                if (validity.warnings && validity.warnings.length > 0)
                    return 'Warning'
                return null
            },
            validationMessage() {
                let message = ''
                const validity = this.question.validity
                for (let index = 0; index < validity.messages.length; index++) {
                    const errorMessage = validity.messages[index];
                    message += errorMessage + '<br />'
                }
                for (let index = 0; index < validity.warnings.length; index++) {
                    const errorMessage = validity.warnings[index];
                    message += errorMessage + '<br />'
                }
                return message;
            },
            valuenow() {
                if (this.question.fetchState) {
                    return this.question.fetchState.uploaded
                }
                return 100
            },
            valuemax() {
                if (this.question.fetchState) {
                    return this.question.fetchState.total
                }
                return 100
            }
        },
        methods: {
            cacheQuestionData() {
                this.lastUpdate = this.question.updatedAt
                //this.hasTooltip = 
                //this.isValid =
            }
        },
        created() {
            this.questionId = this.params.value.identity
            this.questionType = this.params.value.type
            this.question = this.$store.state.webinterview.entityDetails[this.questionId]
            this.cacheQuestionData()
        },
        filters: {
            formatNumber (value) {
                if (value)
                    return value.toLocaleString()
                return ''
            }
        }
    }
</script>











