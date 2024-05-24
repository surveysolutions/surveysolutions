<template>
    <div class="unit-section" :class="coverStatusClass">
        <div class="unit-title">
            <wb-humburger
                :showFoldbackButtonAsHamburger="showHumburger"
            ></wb-humburger>
            <h3 id="cover-title">
                {{
                    this.$store.state.webinterview.breadcrumbs.title ||
                    this.$store.state.webinterview.coverInfo.title ||
                    $t('WebInterviewUI.Cover')
                }}
            </h3>
        </div>

        <div class="wrapper-info error" v-if="hasBrokenPackage">
            <div class="container-info">
                <h4 class="error-text">
                    {{ $t('WebInterviewUI.CoverBrokenPackegeTitle') }}
                </h4>
                <p class="error-text">
                    <i v-html="$t('WebInterviewUI.CoverBrokenPackegeText')"></i>
                </p>
            </div>
        </div>

        <div class="wrapper-info" v-if="hasSupervisorComment">
            <div class="container-info">
                <h4 class="info-block gray-uppercase">
                    {{ $t('WebInterviewUI.CoverSupervisorNote') }}
                </h4>
                <p>
                    <b>{{ supervisorComment }}</b>
                </p>
            </div>
        </div>

        <div class="wrapper-info" v-if="commentedQuestions.length > 0">
            <div class="container-info">
                <h4 class="info-block gray-uppercase">
                    {{ commentsTitle }}
                </h4>
                <ul class="list-unstyled marked-questions">
                    <li
                        v-for="commentedQuestion in commentedQuestions"
                        :key="commentedQuestion.id"
                    >
                        <a
                            href="javascript:void(0);"
                            @click="navigateTo(commentedQuestion)"
                            >{{ commentedQuestion.title }}</a
                        >
                    </li>
                </ul>
            </div>
        </div>

        <template v-for="entity in entities">
            <ReadonlyQuestion v-if="entity.isReadonly" :key="entity.identity" :id="entity.identity">
            </ReadonlyQuestion>

            <component v-else :key="`${entity.identity}-${entity.entityType}`" :is="entity.entityType"
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

    watch: {
        ['$route.hash'](to) {
            if (to != null) {
                this.$store.dispatch('sectionRequireScroll', { id: to })
            }
        },
    },

    beforeMount() {
        this.fetch()
    },

    mounted() {
        if (this.$route.hash) {
            this.$store.dispatch('sectionRequireScroll', {
                id: this.$route.hash,
            })
        }
    },

    computed: {
        title() {
            return this.$store.state.webinterview.questionnaireTitle
        },
        interviewId() {
            return this.$route.params.interviewId
        },
        commentsTitle() {
            return this.$store.state.webinterview.coverInfo.entitiesWithComments
                .length <
                this.$store.state.webinterview.coverInfo.commentedQuestionsCount
                ? this.$t('WebInterviewUI.CoverFirstComments', {
                      count: this.$store.state.webinterview.coverInfo
                          .entitiesWithComments.length,
                  })
                : this.$t('WebInterviewUI.CoverComments')
        },
        entities() {
            return this.$store.state.webinterview.entities
        },
        commentedQuestions() {
            return (
                this.$store.state.webinterview.coverInfo.entitiesWithComments ||
                []
            )
        },
        supervisorComment() {
            return this.$store.state.webinterview.coverInfo
                .supervisorRejectComment
        },
        hasSupervisorComment() {
            return !isEmpty(
                this.$store.state.webinterview.coverInfo
                    .supervisorRejectComment,
            )
        },
        hasBrokenPackage() {
            return this.$store.state.webinterview.doesBrokenPackageExist ==
                undefined
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
            const coverPageId =
                this.$config.coverPageId == undefined
                    ? this.$config.model.coverPageId
                    : this.$config.coverPageId
            if (coverPageId) {
                return [
                    {
                        'complete-section':
                            !this.hasBrokenPackage &&
                            this.info.status == GroupStatus.Completed &&
                            !this.hasError,
                        'section-with-error':
                            this.hasBrokenPackage || this.hasError,
                    },
                ]
            }

            return [
                {
                    'complete-section': !this.hasBrokenPackage,
                    'section-with-error': this.hasBrokenPackage,
                },
            ]
        },
    },
    methods: {
        fetch() {
            this.$store.dispatch('fetchCoverInfo')
            this.$store.dispatch('fetchBreadcrumbs')
            this.$store.dispatch('fetchSectionEntities')
        },
        getGpsUrl(question) {
            return `http://maps.google.com/maps?q=${question.answer}`
        },
        getAttachment(question) {
            if (!question.answer) return null

            const details =
                this.$store.state.webinterview.entityDetails[question.identity]
            if (details && details.options) {
                const option = details.options.find(
                    (o) => o.value === details.answer,
                )
                if (option) return option.attachmentName
            }

            return null
        },
        navigateTo(commentedQuestion) {
            if (commentedQuestion.isPrefilled && !this.navigateToPrefilled) {
                this.$router.push({ name: 'prefilled' })
                return
            }

            const routeName = this.$route.params.sectionId == commentedQuestion.parentId
                ? 'cover'
                : 'section'

            const navigateToEntity = {
                name: routeName,
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
