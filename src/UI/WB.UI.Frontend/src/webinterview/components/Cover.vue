<template>
    <div class="unit-section"
        :class="coverStatusClass">
        <div class="unit-title">
            <wb-humburger :showFoldbackButtonAsHamburger="showHumburger"></wb-humburger>
            <h3 id="cover-title">
                {{ this.$store.state.webinterview.breadcrumbs.title || this.$store.state.webinterview.coverInfo.title || $t("WebInterviewUI.Cover")}}
            </h3>
        </div>

        <div class="wrapper-info error">
            <div class="container-info"
                v-if="hasBrokenPackage">
                <h4 class="error-text">
                    {{ $t("WebInterviewUI.CoverBrokenPackegeTitle")}}
                </h4>
                <p class="error-text"><i v-html="$t('WebInterviewUI.CoverBrokenPackegeText')"></i></p>
            </div>
            <div class="container-info">
                <h2>{{title}}</h2>
            </div>
        </div>

        <div class="wrapper-info"
            v-if="hasSupervisorComment">
            <div class="container-info">
                <h4 class="gray-uppercase">
                    {{ $t("WebInterviewUI.CoverSupervisorNote")}}
                </h4>
                <p>
                    <b>{{supervisorComment}}</b>
                </p>
            </div>
        </div>

        <div class="wrapper-info"
            v-if="commentedQuestions.length > 0">
            <div class="container-info">
                <h4 class="gray-uppercase">
                    {{commentsTitle}}
                </h4>
                <ul class="list-unstyled marked-questions">
                    <li v-for="commentedQuestion in commentedQuestions"
                        :key="commentedQuestion.id">
                        <a href="javascript:void(0);"
                            @click="navigateTo(commentedQuestion)">{{ commentedQuestion.title }}</a>
                    </li>
                </ul>
            </div>
        </div>

        <template v-for="entity in entities">
            <div class="wrapper-info"
                v-if="entity.isReadonly"
                :key="entity.identity">
                <div class="container-info"
                    :id="entity.identity">
                    <h5 v-html="entity.title"></h5>
                    <p>
                        <b v-if="entity.entityType == 'Gps'">
                            <a :href="getGpsUrl(entity)"
                                target="_blank">{{entity.answer}}</a>
                            <br/>
                            <img v-bind:src="googleMapPosition(entity.answer)"
                                draggable="false" />
                        </b>
                        <b v-else-if="entity.entityType == 'DateTime'"
                            v-dateTimeFormatting
                            v-html="entity.answer">
                        </b>
                        <b v-else>{{entity.answer}}</b>
                    </p>
                </div>
            </div>
            <component v-else
                :key="`${entity.identity}-${entity.entityType}`"
                :is="entity.entityType"
                :id="entity.identity"></component>
        </template>
    </div>
</template>

<script lang="js">
import isEmpty from 'lodash/isEmpty'
import { GroupStatus } from './questions'

export default {
    name: 'cover-readonly-view',
    props: {
        navigateToPrefilled: {
            type: Boolean,
            required: false,
            default: false,
        },
        showHumburger: {
            type: Boolean,
            default: true,
        },
    },

    beforeMount() {
        this.fetch()
    },

    mounted() {
        if(this.$route.hash){
            this.$store.dispatch('sectionRequireScroll', { id: this.$route.hash })
        }
    },


    computed: {
        title() {
            return this.$store.state.webinterview.questionnaireTitle
        },
        commentsTitle() {
            return this.$store.state.webinterview.coverInfo.entitiesWithComments.length < this.$store.state.webinterview.coverInfo.commentedQuestionsCount
                ? this.$t('WebInterviewUI.CoverFirstComments', { count: this.$store.state.webinterview.coverInfo.entitiesWithComments.length})
                : this.$t('WebInterviewUI.CoverComments')
        },
        entities() {
            return this.$store.state.webinterview.entities
        },
        commentedQuestions() {
            return this.$store.state.webinterview.coverInfo.entitiesWithComments || []
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
                : this.$store.state.webinterview.doesBrokenPackageExist
        },
        info() {
            return this.$store.state.webinterview.breadcrumbs
        },
        hasError() {
            return this.info.validity && this.info.validity.isValid === false
        },
        coverStatusClass() {
            const coverPageId = this.$config.coverPageId == undefined ? this.$config.model.coverPageId : this.$config.coverPageId
            if (coverPageId) {
                return [
                    {
                        'complete-section'  : !this.hasBrokenPackage && this.info.status == GroupStatus.Completed && !this.hasError,
                        'section-with-error': this.hasBrokenPackage || this.hasError,
                    },
                ]
            }

            return [
                {
                    'complete-section'  : !this.hasBrokenPackage,
                    'section-with-error': this.hasBrokenPackage,
                },
            ]
        },
    },
    methods: {
        googleMapPosition(answer) {
            return `${this.$config.googleMapsApiBaseUrl}/maps/api/staticmap?center=${answer}`
                + `&zoom=14&scale=0&size=385x200&markers=color:blue|label:O|${answer}`
                + `&key=${this.$config.googleApiKey}`
        },
        fetch() {
            this.$store.dispatch('fetchCoverInfo')
            this.$store.dispatch('fetchBreadcrumbs')
            this.$store.dispatch('fetchSectionEntities')
        },
        getGpsUrl(question) {
            return `http://maps.google.com/maps?q=${question.answer}`
        },
        navigateTo(commentedQuestion) {
            if (commentedQuestion.isPrefilled && !this.navigateToPrefilled) {
                this.$router.push({ name: 'prefilled' })
                return
            }

            const navigateToEntity = {
                name: 'section',
                params: {
                    sectionId: commentedQuestion.parentId,
                    interviewId: this.$route.params.interviewId,
                },
                hash: '#' + commentedQuestion.id,
            }

            this.$router.push(navigateToEntity)
        },
    },
}
</script>
