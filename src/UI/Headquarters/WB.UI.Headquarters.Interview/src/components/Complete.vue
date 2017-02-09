<template>
    <section class="questionnaire  details-interview">
    <div class="unit-section complete-section" v-if="hasCompleteInfo">
        <div class="unit-title">
            <h3>Complete interview</h3>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <h2>You are about to complete this interview</h2>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <h4 class="gray-uppercase">Questions status:</h4>
                <div class="question-status">
                    <ul class="list-inline clearfix">
                        <li class="answered" v-bind:class="{'has-value' : hasAnsweredQuestions }">{{ answeredQuestionsCountString }}
                            <span>Answered</span>
                        </li>
                        <li class="unanswered" v-bind:class="{'has-value' : hasUnansweredQuestions }">{{ unansweredQuestionsCountString }}
                            <span>Unanswered</span>
                        </li>
                        <li class="errors" v-bind:class="{'has-value' : hasInvalidQuestions }">{{ invalidQuestionsCountString }}
                            <span>Error(s)</span>
                        </li>
                    </ul>
                </div>
            </div>
        </div>
        <div class="wrapper-info" v-if="completeInfo.entitiesWithError.length > 0">
            <div class="container-info">
            <h4 class="gray-uppercase">Questions with errors:</h4>
            <ul class="list-unstyled marked-questions" v-for="entity in completeInfo.entitiesWithError">
                <li>
                    <router-link :to="navigateTo(entity)">{{entity.title}}</router-link>
                </li>
            </ul>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <label class="gray-uppercase" for="comment-for-supervisor">Note for supervisor</label>
                <div class="field">
                    <textarea class="field-to-fill" id="comment-for-supervisor" placeholder="Tap to enter text"></textarea>
                    <button type="submit" class="btn btn-link btn-clear">
                        <span></span>
                    </button>
                </div>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <a href="#" class="btn btn-lg btn-success" @click="completeInterview">Complete</a>
            </div>
        </div>
    </div>
    </section>
</template>

<script lang="ts">
    import * as Vue from 'vue'
    import * as $ from "jquery"

    export default {
        name: 'complete-view',
        beforeMount() {
            this.loadComplete()
        },
        computed: {
            completeInfo() {
                return this.$store.state.completeInfo;
            },
            hasCompleteInfo() {
                return this.$store.state.completeInfo != undefined
            },
            hasAnsweredQuestions() {
                return this.$store.state.completeInfo.answeredCount > 0
            },
            answeredQuestionsCountString() {
                return this.hasAnsweredQuestions ? this.$store.state.completeInfo.answeredCount : "No";
            },
            hasUnansweredQuestions() {
                return this.$store.state.completeInfo.unansweredCount > 0
            },
            unansweredQuestionsCountString() {
                return this.hasUnansweredQuestions ? this.$store.state.completeInfo.unansweredCount : "No";
            },
            hasInvalidQuestions() {
                return this.$store.state.completeInfo.errorsCount > 0
            },
            invalidQuestionsCountString() {
                return this.hasInvalidQuestions ? this.$store.state.completeInfo.errorsCount : "No";
            }
        },
        methods: {
            loadComplete() {
                this.$store.dispatch("fetchCompleteInfo")
            },
            completeInterview() {
                const comment = $('#comment-for-supervisor').val();
                this.$store.dispatch('completeInterview', { comment: comment });
            },
            navigateTo(entityWithError) {
                return {
                    name: 'section',
                    params: {
                        sectionId: entityWithError.parentId,
                        interviewId: this.$route.params.interviewId
                    }
                }
            }
        }
    }
</script>
