<template>
    <section class="questionnaire  details-interview">
    <div class="unit-section complete-section" v-if="hasCompleteInfo">
        <div class="unit-title">
            <h3>Finish questionnaire</h3>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <h2>You are about to complete this questionnaire</h2>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <h4 class="gray-uppercase">Questions status:</h4>
                <!--ul  class="list-unstyled value-is-absent">
                    <li>Interview not started,</li>
                    <li>no questions are answered,</li>
                    <li>no data collected</li>
                </ul-->
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
                    <!--p class="gray-uppercase">Time spent: 04 hours 22 minutes</p>
                    <p class="gray-uppercase">Data size: 4312KB</p-->
                </div>
            </div>
        </div>
        <div class="wrapper-info" v-if="completeInfo.entitiesWithError.length > 0">
            <div class="container-info">
            <h4 class="gray-uppercase">Questions with errors:</h4>
            <ul class="list-unstyled marked-questions" v-for="entitiy in completeInfo.entitiesWithError">
                <li><a href="#" @click="navigate(entity)">{{ entitiy.title }}}</a></li>
            </ul>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <!--p class="gray-uppercase">No notes for supervisor</p-->
                <label class="gray-uppercase" for="comment-for-supervisor">Note for supervisor</label>
                <div class="field">
                    <textarea class="field-to-fill" id="comment-for-supervisor" placeholder="Tap to enter text"></textarea>
                    <button type="submit" class="btn btn-link btn-clear">
                        <span></span>
                    </button>
                </div>


            </div>
        </div>
        <!--div class="wrapper-info">
            <div class="container-info">
                <h4 class="gray-uppercase">Supervisor's note:</h4>
                <p><b>Lorem ipsum dolor sit amet, pro ne dicta saepe albucius, habemus fabellas luptatum eam in. Eam eu delectus accusata theophrastus, quaeque gubergren assentior id mel, sed ad feugiat repudiandae mediocritatem. Saepe quaerendum te sit, no appellantur suscipiantur sea, clita noluisse in est. Persius denique appellantur nam eu. Epicurei invenire sit no, his aeterno expetenda no, vix eu erant lucilius.</b></p>
                <h4 class="gray-uppercase">Questions with supervisor's comments:</h4>
                <ul class="list-unstyled marked-questions">
                    <li><a href="#">How much quantity in total did your hoesehold consume in the past 7 days?</a></li>
                    <li><a href="#">What is your relations status?</a></li>
                </ul>
            </div>
        </div-->
        <div class="wrapper-info">
            <div class="container-info">
                <a href="#" class="btn btn-lg btn-success" @click="completeInterview">Complete</a>
                <!--p class="gray-uppercase">After you finish this questionnaire it will become read only and will be uploaded to hq during your next synchronization</p-->
            </div>
        </div>
        <!--div class="wrapper-info">
            <div class="container-info">
                <p class="gray-uppercase">this questionnaire is complete, it is in read-only mode. you can access chapters from top-right menu</p>
                <a href="#" class="btn btn-lg btn-success">view</a>
                <p class="gray-uppercase">if you need to change some data you need to reinitialize it</p>
                <a href="#" class="btn btn-lg btn-primary">reinitialize</a>
            </div>
        </div-->
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
            navigate(entityWithError) {
                const navigateToEntity = {
                    name: 'section',
                    params: {
                        sectionId: entityWithError.parentId,
                        interviewId: this.$route.params.interviewId
                    }
                }
                this.$store.dispatch("sectionRequireScroll", { id: entityWithError.id })
                this.$router.push(navigateToEntity)
            }
        }
    }
</script>
