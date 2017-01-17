<template>
    <div class="question" v-if="!$me.isLoading && !($me.isDisabled && $me.hideIfDisabled)" :class="[{'hidden-question': $me.isDisabled}]" :id="hash">
        <div class="question-editor" :class="[{'answered': !isNotStarted}]">
            <div class="question-unit">
                <div class="options-group">
                    <div class="form-group">
                        <div class="field" :class="[{'complete-section': !hasInvalidAnswers && isCompleted}, {'section-with-error': hasInvalidAnswers}]">
                            <router-link :to="navigateTo" class="btn btn-block unit-title">
                                <div class="text-left text-capitalize">
                                    {{this.$me.title}}<span v-if="this.$me.rosterTitle != null"> - <i>{{this.$me.rosterTitle}}</i></span>
                                </div>
                            </router-link>
                        </div>
                    </div>
                </div>
                <h5>
                    <small>
                        <span :class="[{'text-primary': isStarted}, {'text-success': isCompleted}]">{{this.$me.statisticsByAnswersAndSubsections}}</span>
                        <span v-if="hasInvalidAnswers">, &nbsp;<strong class="text-danger">{{this.$me.statisticsByInvalidAnswers}}</strong></span>
                    </small>
                </h5>
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
            }
        }
    }
</script>
