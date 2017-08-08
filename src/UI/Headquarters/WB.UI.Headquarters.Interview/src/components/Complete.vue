<template>
    <div class="unit-section first-last-chapter" v-if="hasCompleteInfo" v-bind:class="{'section-with-error' : hasInvalidQuestions, 'complete-section' : isAllAnswered  }" >
        <div class="unit-title">
            <wb-humburger></wb-humburger>
            <h3>Complete</h3>
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
                <h4 class="gray-uppercase">{{doesShowErrorsCommentWithCount ? 'First ' + completeInfo.entitiesWithError.length + ' entities with errors:' : 'Questions with errors:'}}</h4>
                <ul class="list-unstyled marked-questions">
                    <li v-for="entity in completeInfo.entitiesWithError">
                        <a href="javascript:void(0);" @click="navigateTo(entity)">{{ entity.title }}</a>
                    </li>
                </ul>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <label class="gray-uppercase" for="comment-for-supervisor">Note for supervisor</label>
                <div class="field">
                    <textarea class="field-to-fill" id="comment-for-supervisor" placeholder="Enter text" v-model="comment"></textarea>
                    <button type="submit" class="btn btn-link btn-clear">
                        <span></span>
                    </button>
                </div>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <a href="javascript:void(0);" class="btn btn-lg" v-bind:class="{ 'btn-success': isAllAnswered, 'btn-primary' : hasUnansweredQuestions, 'btn-danger' : hasInvalidQuestions }" @click="completeInterview">Complete</a>
            </div>
        </div>
    </div>
</template>

<script lang="js">
    export default {
        name: 'complete-view',
        beforeMount() {
            this.fetchCompleteInfo()
        },
        watch: {
            $route(to, from) {
                this.fetchCompleteInfo()
            }
        },
        computed: {
            completeInfo() {
                return this.$store.state.completeInfo;
            },
            hasCompleteInfo() {
                return this.completeInfo != undefined
            },
            hasAnsweredQuestions() {
                return this.completeInfo.answeredCount > 0
            },
            isAllAnswered() {
                return this.completeInfo.unansweredCount == 0 && this.completeInfo.errorsCount == 0
            },
            answeredQuestionsCountString() {
                return this.hasAnsweredQuestions ? this.completeInfo.answeredCount : "No";
            },
            hasUnansweredQuestions() {
                return this.completeInfo.unansweredCount > 0
            },
            unansweredQuestionsCountString() {
                return this.hasUnansweredQuestions ? this.completeInfo.unansweredCount : "No";
            },
            hasInvalidQuestions() {
                return this.completeInfo.errorsCount > 0
            },
            invalidQuestionsCountString() {
                return this.hasInvalidQuestions ? this.completeInfo.errorsCount : "No";
            },
            doesShowErrorsCommentWithCount() {
                return this.completeInfo.entitiesWithError.length < this.completeInfo.errorsCount
            }
        },
        data () {
            return {
                comment: ''
            }
        },
        methods: {
            fetchCompleteInfo() {
                this.$store.dispatch("fetchCompleteInfo")
            },
            completeInterview() {
                this.$store.dispatch('completeInterview', { comment: this.comment });
            },
            navigateTo(entityWithError) {
                if(entityWithError.isPrefilled){
                    this.$router.push({ name: "prefilled" })
                    return;
                }

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
