<template>
    <div class="question" v-if="!$me.isLoading && !($me.isDisabled && $me.hideIfDisabled)" :class="[{'hidden-question': $me.isDisabled}]" :id="hash">
        <div class="question-editor" :class="[{'answered': !isNotStarted}]">
            <div class="question-unit">
                <div class="options-group">
                    <div class="form-group">
                        <div class="field">
                            <router-link :to="navigateTo" :class="[{'btn-primary': isNotStarted || isStarted}, {'btn-success': isCompleted}, {'btn-danger': hasInvalidAnswers}]"
                                class="btn btn-block">
                                {{this.$me.title}} <span v-if="this.$me.rosterTitle != null"> - <i>{{this.$me.rosterTitle}}</i></span>
                            </router-link>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="question-editor ">{{this.$me.statistics}}</div>
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
                return this.$me.status == "NotStarted";
            },
            isStarted() {
                return this.$me.status == "Started";
            },
            isCompleted() {
                return this.$me.status == "Completed";
            },
            hasInvalidAnswers() {
                return this.$me.status == "Invalid";
            }
        }
    }
</script>
