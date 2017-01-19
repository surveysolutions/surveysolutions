<template>
    <div class="question" v-if="!$me.isLoading && !($me.isDisabled && $me.hideIfDisabled)" :class="[{'hidden-question': $me.isDisabled}]" :id="hash">
        <div class="question-editor roster-section-block" :class="[{'has-error': hasInvalidAnswers && !isCompleted}, {'answered': isCompleted}]">
            <div class="question-unit">
                <div class="options-group">
                    <router-link :to="navigateTo" class="btn btn-roster-section" :class="statusClass">
                        {{this.$me.title}}<span v-if="this.$me.rosterTitle != null"> - <i>{{this.$me.rosterTitle}}</i></span>
                    </router-link>
                </div>
                <div class="information-block roster-section-info">
                    {{this.$me.statisticsByAnswersAndSubsections}}<span v-if="hasInvalidAnswers">, </span> 
                    <span class="error-text" v-if="hasInvalidAnswers">{{this.$me.statisticsByInvalidAnswers}}</span>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import { entityDetails } from "components/mixins"

    export default {
        name: 'Group',
        mixins: [entityDetails],
        computed: {
            navigateTo() {
                return {
                    name: 'section', params: {
                        sectionId: this.id,
                        interviewId: this.$route.params.interviewId
                    }
                }
            },
            isNotStarted() {
                return this.$me.status == "NotStarted"
            },
            isStarted() {
                return this.$me.status == "Started"
            },
            isCompleted() {
                return this.$me.status == "Completed"
            },
            hasInvalidAnswers() {
                return this.$me.hasInvalidAnswers
            },
            statusClass() {
                return [{
                    'btn-success': !this.hasInvalidAnswers && this.isCompleted,
                    'btn-danger': this.hasInvalidAnswers,
                    'btn-primary': !this.hasInvalidAnswers && !this.isCompleted
                }]
            }
        }
    }
</script>