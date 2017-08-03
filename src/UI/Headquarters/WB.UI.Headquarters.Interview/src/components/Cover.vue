<template>
    <div class="unit-section complete-section">
        <div class="unit-title">
            <wb-humburger></wb-humburger>
            <h3>Cover</h3>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <h2>{{title}}</h2>
                <p>
                    <b>Please provide answers to all questions to the extent possible. Any answers you provide are sent to our system right away. If you experience a communication disruption you can return to the questionnaire by following the same link you provided so far retained for you. You can then continue by submitting new answers or revising earlier answers.
                    </b>
                </p>
            </div>
        </div>

        <div class="wrapper-info" v-if="hasSupervisorComment">
            <div class="container-info">
                <h4 class="gray-uppercase">Supervisor note:</h4>
                <p>
                    <b>{{supervisorComment}}</b>
                </p>
            </div>
        </div>

        <div class="wrapper-info" v-if="commentedQuestions.length > 0">
            <div class="container-info">
                <h4 class="gray-uppercase">{{commentsTitle}}</h4>
                <ul class="list-unstyled marked-questions">
                    <li v-for="commentedQuestion in commentedQuestions">
                        <a href="#" @click="navigateTo(commentedQuestion)">{{ commentedQuestion.title }}</a>
                    </li>
                </ul>
            </div>
        </div>

        <template v-for="question in questions">
            <div class="wrapper-info" v-if="question.isReadonly">
                <div class="container-info" :id="question.identity">
                    <h5 v-html="question.title"></h5>
                    <p>
                        <b v-if="question.type == 'Gps'">
                            <a :href="getGpsUrl(question)" target="_blank">{{question.answer}}</a>
                        </b>
                        <b v-else-if="question.type == 'DateTime'" v-dateTimeFormatting v-html="question.answer">
                        </b>
                        <b v-else>{{question.answer}}</b>
                    </p>
                </div>
            </div>
            <component v-else :key="question.identity" v-bind:is="question.type" v-bind:id="question.identity"></component>

        </template>

        <NavigationButton id="NavigationButton" :target="firstSectionId"></NavigationButton>
    </div>
</template>

<script lang="ts">
import * as isEmpty from "lodash/isempty"

export default {
    name: "cover-readonly-view",
    beforeMount() {
        this.fetch()
    },
    computed: {
        title() {
            return this.$store.state.questionnaireTitle
        },
        commentsTitle() {
            return this.$store.state.coverInfo.entitiesWithComments.length < this.$store.state.coverInfo.commentedQuestionsCount
                ? 'First ' + this.$store.state.coverInfo.entitiesWithComments.length + ' entities with comments:'
                : 'Questions with comments:';
        },
        questions() {
            return this.$store.state.coverInfo.identifyingQuestions
        },
        commentedQuestions() {
            return this.$store.state.coverInfo.entitiesWithComments
        },
        firstSectionId() {
            return this.$store.state.firstSectionId
        },
        supervisorComment() {
            return this.$store.state.coverInfo.supervisorRejectComment
        },
        hasSupervisorComment() {
            return !isEmpty(this.$store.state.coverInfo.supervisorRejectComment)
        }
    },
    methods: {
        fetch() {
            this.$store.dispatch("fetchCoverInfo")
        },
        getGpsUrl(question: IReadonlyPrefilledQuestion) {
            return `http://maps.google.com/maps?q=${question.answer}`
        },
        navigateTo(commentedQuestion) {
            if (commentedQuestion.isPrefilled) {
                this.$router.push({ name: "prefilled" })
                return;
            }

            const navigateToEntity = {
                name: 'section',
                params: {
                    sectionId: commentedQuestion.parentId,
                    interviewId: this.$route.params.interviewId
                }
            }

            this.$store.dispatch("sectionRequireScroll", { id: commentedQuestion.id })
            this.$router.push(navigateToEntity)
        }
    }
}
</script>
