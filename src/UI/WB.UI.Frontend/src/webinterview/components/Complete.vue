<template>
    <div class="unit-section first-last-chapter complete" v-if="hasCompleteInfo"
        v-bind:class="{ 'section-with-error': hasErrors, 'complete-section': isAllAnswered }">
        <div class="unit-title">
            <wb-humburger></wb-humburger>
            <h3>{{ competeButtonTitle }}</h3>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <h2>{{ $t('WebInterviewUI.CompleteAbout') }}</h2>
                <h2>{{ $store.state.webinterview.interviewKey }}</h2>
            </div>
        </div>
        <div class="wrapper-info" v-if="completeGroups.length > 0">
            <div class="container-info info-block">
                <div class="gray-uppercase">
                    {{ $t('WebInterviewUI.CompleteInterviewStatus') }}
                </div>
                <div class="gray-uppercase" v-if="hasCriticalIssues">
                    {{ $t('WebInterviewUI.CompleteNoteCommentCriticality') }}
                </div>
            </div>
        </div>

        <template v-for="group in completeGroups">
            <ExpandableList :title="group.title" :cssClass="group.cssClass">
                <ul class="list-unstyled marked-questions">
                    <li v-for="item in group.items" :key="item.id">
                        <a v-if="item.parentId || item.isPrefilled" href="javascript:void(0);" @click="navigateTo(item)"
                            v-html="item.title"></a>
                        <span v-else v-html="item.title"></span>
                    </li>
                </ul>
            </ExpandableList>
        </template>

        <div class="wrapper-info">
            <div class="container-info">
                <label class="info-block gray-uppercase" for="comment-for-supervisor">
                    {{ noteToSupervisor }}
                </label>
                <div class="field">
                    <textarea-autosize class="field-to-fill" id="comment-for-supervisor"
                        :placeholder="$t('WebInterviewUI.TextEnter')" v-model="comment"
                        maxlength="750"></textarea-autosize>
                    <button type="submit" class="btn btn-link btn-clear">
                        <span></span>
                    </button>
                </div>
            </div>
        </div>
        <div class="wrapper-info" v-if="mayBeSwitchedToWebMode">
            <div class="container-info">
                <input v-if="mayBeSwitchedToWebMode" class="wb-checkbox" type="checkbox" id="switchToWeb_id"
                    name="switchToWeb" v-model="switchToWeb" />
                <label for="switchToWeb_id" class="font-bold" v-if="mayBeSwitchedToWebMode">
                    <span class="tick"></span>
                    {{ $t('WebInterviewUI.SwitchToWebMode') }}
                </label>
            </div>
            <div class="container-info action-block" v-if="switchToWeb">
                <p calss="info-block gray-uppercase">
                    {{ $t('WebInterviewUI.SwitchToWebMode_LinkDescription') }}
                </p>
                <p class="font-bold">
                    {{ webLink }}
                </p>
            </div>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <a href="javascript:void(0);" id="btnComplete" class="btn btn-lg" v-bind:class="{
        'btn-success': isAllAnswered,
        'btn-primary': hasUnansweredQuestions,
        'btn-danger': hasErrors,
        'disabled': !isCompletionPermitted,
    }" @click="completeInterview">{{ competeButtonTitle }}</a>
                <div class="info-block gray-uppercase" style="margin-top:10px;">{{ completeButtionComment }}</div>
            </div>
        </div>
        <SectionLoadingProgress />
    </div>
</template>

<script lang="js">
import modal from '@/shared/modal'
import SectionProgress from './SectionLoadProgress'
import ExpandableList from '../../hqapp/components/ExpandableList.vue';

export default {
    name: 'complete-view',
    components: {
        SectionLoadingProgress: SectionProgress,
        ExpandableList: ExpandableList,
    },
    data() {
        return {
            comment: '',
            switchToWeb: false,
            wasCriticalityInfoLoaded: false,
        }
    },
    beforeMount() {
        this.wasCriticalityInfoLoaded = false;
        this.fetchCompleteInfo()
    },
    watch: {
        $route(to, from) {
            this.fetchCompleteInfo()
        },
        shouldCloseWindow(to) {
            if (to === true) {
                this.completeInterview()
            }
        },
        criticalityInfo(to, from) {
            if (to) {
                this.wasCriticalityInfoLoaded = true;
            }
        }
    },
    computed: {
        completeInfo() {
            return this.$store.state.webinterview.completeInfo
        },
        criticalityInfo() {
            return this.$store.state.webinterview.criticalityInfo
        },
        completeGroups() {
            let groups = []

            if (this.criticalityInfo?.unansweredCriticalQuestions.length > 0) {
                groups.push({
                    title: this.$t('WebInterviewUI.Complete_CriticalUnansweredQuestions', { count: this.moreThen30(this.criticalityInfo.unansweredCriticalQuestions.length) }),
                    items: this.criticalityInfo.unansweredCriticalQuestions,
                    cssClass: 'errors'
                })
            }

            if (this.criticalityInfo?.failedCriticalRules.length > 0) {
                groups.push({
                    title: this.$t('WebInterviewUI.Complete_FailedCriticalRules', { count: this.moreThen30(this.criticalityInfo.failedCriticalRules.length) }),
                    items: this.criticalityInfo.failedCriticalRules,
                    cssClass: 'critical-rule-errors'
                })
            }

            if (this.completeInfo.unansweredCount > 0) {
                groups.push({
                    title: this.$t('WebInterviewUI.Complete_UnansweredQuestions', { count: this.moreThen30(this.completeInfo.unansweredCount) }),
                    items: this.completeInfo.unansweredQuestions,
                    cssClass: 'unanswered'
                })
            }

            if (this.completeInfo.errorsCount > 0) {
                groups.push({
                    title: this.$t('WebInterviewUI.Complete_QuestionsWithErrors', { count: this.moreThen30(this.completeInfo.errorsCount) }),
                    items: this.completeInfo.entitiesWithError,
                    cssClass: 'errors'
                })
            }

            return groups;
        },
        doesSupportCriticality() {
            return this.$store.state.webinterview.doesSupportCriticality == true
        },
        criticalityLevel() {
            return this.$store.state.webinterview.criticalityLevel
        },
        isCompletionPermitted() {
            if (this.mayBeSwitchedToWebMode && this.switchToWeb)
                return true;

            if (this.wasCriticalityInfoLoaded === false) {
                return false;
            }

            if (this.doesSupportCriticality !== false) {
                if (this.hasCriticalIssues) {
                    if (this.criticalityLevel == 'Block') {
                        return false;
                    }
                    if (this.criticalityLevel == 'Warn') {
                        return this.comment && this.comment.length > 0
                    }
                }
            }
            return true;
        },
        completeButtionComment() {
            if (this.hasCriticalIssues) {
                if (this.criticalityLevel == 'Block') {
                    return this.$t('WebInterviewUI.CompleteCommentCriticalityLevelBlock')
                } else if (this.criticalityLevel == 'Warn') {
                    return this.$t('WebInterviewUI.CompleteCommentCriticalityLevelWarn')
                }
            }
            return '';
        },
        shouldCloseWindow() {
            return this.$store.state.webinterview.interviewCompleted && this.$config.inWebTesterMode
        },
        competeButtonTitle() {
            return this.$config.customTexts.completeButton
        },
        noteToSupervisor() {
            return this.$config.customTexts.noteToSupervisor
        },
        hasCompleteInfo() {
            return this.completeInfo != undefined
        },
        hasAnsweredQuestions() {
            return this.completeInfo.answeredCount > 0
        },
        hasCriticalIssues() {
            return this.criticalityInfo?.unansweredCriticalQuestions?.length > 0 || this.criticalityInfo?.failedCriticalRules?.length > 0
        },
        isAllAnswered() {
            return this.completeInfo.unansweredCount == 0 && this.completeInfo.errorsCount == 0
        },
        answeredQuestionsCountString() {
            return this.hasAnsweredQuestions ? this.completeInfo.answeredCount : this.$t('WebInterviewUI.No')
        },
        hasUnansweredQuestions() {
            return this.completeInfo.unansweredCount > 0
        },
        unansweredQuestionsCountString() {
            return this.hasUnansweredQuestions ? this.completeInfo.unansweredCount : this.$t('WebInterviewUI.No')
        },
        hasErrors() {
            return this.completeInfo.errorsCount > 0 || this.hasCriticalIssues
        },
        errorsCount() {
            if (this.hasCriticalIssues)
                return this.completeInfo.errorsCount + this.criticalityInfo.unansweredCriticalQuestions.length + this.criticalityInfo.failedCriticalRules.length
            return this.completeInfo.errorsCount;
        },
        invalidQuestionsCountString() {
            return this.hasErrors ? this.errorsCount : this.$t('WebInterviewUI.No')
        },
        criticalErrorsCountString() {
            return this.hasCriticalIssues ? this.criticalityInfo.failChecks.length : this.$t('WebInterviewUI.No')
        },
        doesShowErrorsCommentWithCount() {
            return this.completeInfo.entitiesWithError.length < this.completeInfo.errorsCount
        },
        mayBeSwitchedToWebMode() {
            return this.$config.mayBeSwitchedToWebMode === true
        },
        webLink() {
            return this.$config.webInterviewUrl ?? ''
        },
    },
    methods: {
        fetchCompleteInfo() {
            this.$store.dispatch('fetchCompleteInfo')
        },

        completeInterview() {
            if (!this.isCompletionPermitted)
                return;

            if (this.shouldCloseWindow) {
                modal.dialog({
                    title: '<p style="text-align: center">' + this.$t('WebInterviewUI.WebTesterSessionOver') + '</p>',
                    message: `<p style="text-align: center">${this.$t('WebInterviewUI.WebTesterSessionOverMessage')}</p>`,
                    callback: () => { },
                    onEscape: false,
                    closeButton: false,
                    buttons: {},
                })

                return
            }
            if (this.switchToWeb)
                this.$store.dispatch('requestWebInterview', this.comment)
            else {
                if (this.criticalityLevel == 'Warn') {

                    modal.dialog({
                        title: `<h2> ${this.$t('WebInterviewUI.ConfirmationNeededTitle')}</h2>`,
                        message: `<p> ${this.$t('WebInterviewUI.CompleteCriticalityWarnConfirmation')}</p>`,
                        onEscape: true,
                        closeButton: true,
                        buttons: {
                            cancel: {
                                label: this.$t('Common.Cancel'),
                            },
                            success: {
                                label: this.$t('Common.Ok'),
                                callback: async () => {
                                    this.$store.dispatch('completeInterview', this.comment)
                                },
                            },
                        },
                    })

                    return
                }
                else {
                    this.$store.dispatch('completeInterview', this.comment)
                }
            }
        },

        navigateTo(entityWithError) {
            if (entityWithError.isPrefilled) {
                this.$router.push({ name: 'prefilled' })
                return
            }

            const navigateToEntity = {
                name: 'section',
                params: {
                    sectionId: entityWithError.parentId,
                    interviewId: this.$route.params.interviewId,
                },
                hash: '#' + entityWithError.id,
            }

            this.$router.push(navigateToEntity)
        },

        moreThen30(value) {
            if (value >= 30)
                return '30+'
            return value + ''
        }
    },
}

</script>
