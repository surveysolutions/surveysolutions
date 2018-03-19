<template>
    <div class="unit-section" :class="coverStatusClass">
        <div class="unit-title">
            <wb-humburger :showFoldbackButtonAsHamburger="showHumburger"></wb-humburger>
            <h3>{{ $t("WebInterviewUI.Cover")}}</h3>
        </div>

        <div class="wrapper-info error">
            <div class="container-info" v-if="hasBrokenPackage">
                <h4 class="error-text">{{ $t("WebInterviewUI.CoverBrokenPackegeTitle")}}</h4>
                <p class="error-text"><i v-html="$t('WebInterviewUI.CoverBrokenPackegeText')"></i></p>
            </div>
            <div class="container-info">
                <h2>{{title}}</h2>
            </div>
        </div>

        <div class="wrapper-info" v-if="hasSupervisorComment">
            <div class="container-info">
                <h4 class="gray-uppercase">{{ $t("WebInterviewUI.CoverSupervisorNote")}}</h4>
                <p>
                    <b>{{supervisorComment}}</b>
                </p>
            </div>
        </div>

        <div class="wrapper-info" v-if="commentedQuestions.length > 0">
            <div class="container-info">
                <h4 class="gray-uppercase">{{commentsTitle}}</h4>
                <ul class="list-unstyled marked-questions">
                    <li v-for="commentedQuestion in commentedQuestions" :key="commentedQuestion.id">
                        <a href="javascript:void(0);" @click="navigateTo(commentedQuestion)">{{ commentedQuestion.title }}</a>
                    </li>
                </ul>
            </div>
        </div>

        <template v-for="question in questions">
            <div class="wrapper-info" v-if="question.isReadonly" :key="question.id">
                <div class="container-info" :id="question.identity">
                    <h5 v-html="question.title"></h5>
                    <p>
                        <b v-if="question.type == 'Gps'">
                            <a :href="getGpsUrl(question)" target="_blank">{{question.answer}}</a>
                            <br/>
                            <img v-bind:src="googleMapPosition(question.answer)" draggable="false" />
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

<script lang="js">
import { isEmpty } from "lodash"

export default {
    name: "cover-readonly-view",
    props: {
        navigateToPrefilled: {
            type: Boolean,
            required: false,
            default: false
        },
        showHumburger: {
            type: Boolean,
            default: true
        }
    },
    beforeMount() {
        this.fetch()
    },
    mounted() {
        window.scroll({ top: 0, behavior: "smooth" })
    },
    computed: {
        title() {
            return this.$store.state.webinterview.questionnaireTitle
        },
        commentsTitle() {
            return this.$store.state.webinterview.coverInfo.entitiesWithComments.length < this.$store.state.webinterview.coverInfo.commentedQuestionsCount
                ? this.$t("WebInterviewUI.CoverFirstComments", { count: this.$store.state.webinterview.coverInfo.entitiesWithComments.length})
                : this.$t("WebInterviewUI.CoverComments");
        },
        questions() {
            return this.$store.state.webinterview.coverInfo.identifyingQuestions
        },
        commentedQuestions() {
            return this.$store.state.webinterview.coverInfo.entitiesWithComments || []
        },
        firstSectionId() {
            return this.$store.state.webinterview.firstSectionId
        },
        supervisorComment() {
            return this.$store.state.webinterview.coverInfo.supervisorRejectComment
        },
        hasSupervisorComment() {
            return !isEmpty(this.$store.state.webinterview.coverInfo.supervisorRejectComment)
        },
        hasBrokenPackage() {
            return this.$store.state.webinterview.doesBrokenPackageExist == undefined 
                ? false
                : this.$store.state.webinterview.doesBrokenPackageExist;
        },
        coverStatusClass() {
            return [
                {
                    'complete-section'  : !this.hasBrokenPackage,
                    'section-with-error': this.hasBrokenPackage
                }
            ]
        }
    },
    methods: {
        googleMapPosition(answer) {
            return `${this.$config.googleMapsApiBaseUrl}/maps/api/staticmap?center=${answer}`
                + `&zoom=14&scale=0&size=385x200&markers=color:blue|label:O|${answer}`
                + `&key=${this.$config.googleApiKey}`
        },
        fetch() {
            this.$store.dispatch("fetchCoverInfo")
        },
        getGpsUrl(question) {
            return `http://maps.google.com/maps?q=${question.answer}`
        },
        navigateTo(commentedQuestion) {
            if (commentedQuestion.isPrefilled && !this.navigateToPrefilled) {
                this.$router.push({ name: "prefilled" })
                return;
            }

            const navigateToEntity = {
                name: 'section',
                params: {
                    sectionId: commentedQuestion.parentId,
                    interviewId: this.$route.params.interviewId
                },
                hash: '#' + commentedQuestion.id 
            }

            this.$router.push(navigateToEntity)
        }
    }
}
</script>
