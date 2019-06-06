<template>
    <div :class="questionStyle" :id='questionId'>
        <popover :enable="question.validity.messages.length > 0 || question.validity.warnings.length > 0" trigger="hover-focus" append-to="body">
            <a class="cell-content has-tooltip" type="primary" data-role="trigger">
                <span v-if="(questionType == 'Integer' || questionType == 'Double') && question.useFormatting">
                    {{question.answer | formatNumber}}
                </span>
                <span v-else>{{question.answer}}</span>
            </a>
            <template slot="popover">
                <div class="popover-content error-tooltip" v-if="!question.validity.isValid">        
                    <h6 style="text-transform:uppercase;" v-if="question.validity.errorMessage">{{ $t("WebInterviewUI.AnswerWasNotSaved") }}</h6>
                    <template v-for="message in question.validity.messages">
                        <span v-dateTimeFormatting v-html="message" :key="message"></span>
                    </template>
                </div>
                <div class="popover-content warning-tooltip" v-else-if="question.validity.warnings.length > 0">        
                    <template v-for="message in question.validity.warnings">
                        <span v-dateTimeFormatting v-html="message" :key="message"></span>
                    </template>
                </div>
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
            ["$watchedQuestion"](watchedQuestion) {
                if (watchedQuestion.updatedAt != this.lastUpdate) {
                    this.question = watchedQuestion
                    this.cacheQuestionData()
                }
            }
        },
        computed: {
            $watchedQuestion() {
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
            doesExistValidationMessage() {
                const validity = this.question.validity
                if (validity.messages && validity.messages.length > 0)
                    return true
                if (validity.warnings && validity.warnings.length > 0)
                    return true
                return false
            },
            isFetchInProgress() {
                return this.question.fetching
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
            }
        },
        created() {
            this.questionId = this.params.value.identity
            this.questionType = this.params.value.type
            this.question = this.$watchedQuestion
            this.cacheQuestionData()
        },
        filters: {
            formatNumber (value) {
                if (value == null || value == undefined || value == NaN)
                    return ''
                
                return value.toLocaleString()    
            }
        }
    }
</script>











