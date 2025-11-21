<template>
    <div class="unit-section first-last-chapter complete-section" v-if="hasCompleteInfo"
        v-bind:class="{ 'section-with-error': hasErrors, 'complete-section': isAllAnswered }">
        <div class="unit-title">
            <wb-humburger></wb-humburger>
            <h3>{{ competeButtonTitle }}</h3>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <h2
                    v-dompurify-html="$t('WebInterviewUI.CompleteReviewSubmit', { key: $store.state.webinterview.interviewKey })">
                </h2>
            </div>
        </div>


        <div class="wrapper-info" v-if="!hasAnyIssue">
            <div class="container-info info-block">
                <div class="gray-uppercase" v-dompurify-html="$t('WebInterviewUI.Complete_AllGood')">
                </div>
            </div>
        </div>

        <ul class="wrapper-info complete-tabs" v-else role="tablist">
            <li v-for="(completeGroup, idx) in completeGroups" :key="idx"
                :class="['tab-item', completeGroup.cssClass, { active: idx === activeCompleteGroupIndex, disabled: !(completeGroup.items?.length > 0) }]"
                role="presentation" @click.stop="setActive(idx)">
                <div class="tab-count">{{ completeGroup.total }}</div>
                <div class="tab-title" v-dompurify-html="completeGroup.title"></div>
            </li>
        </ul>

        <div class="tab-content wrapper-info list-unstyled marked-questions" :class="activeGroup.cssClass">
            <div class="tab-content-item" v-for="item in activeGroup.items" :key="item.id" @click="navigateTo(item)">
                <a class="item-title" v-if="item.parentId || item.isPrefilled" href="javascript:void(0);"
                    v-dompurify-html="item.title"></a>
                <div class="item-title" v-else v-dompurify-html="item.title"></div>

                <div class="item-error" v-if="item.error"
                    v-dompurify-html="$t('WebInterviewUI.Complete_Error') + ' ' + item.error"></div>
                <div class="item-comment" v-if="item.comment"
                    v-dompurify-html="$t('WebInterviewUI.Complete_LastComment') + ' ' + item.comment">
                </div>
            </div>

            <div class="and-more" v-if="moreCount > 0">{{ $t('WebInterviewUI.Complete_AndMore', { count: moreCount }) }}</div>
        </div>

        <div class="wrapper-info">
            <div class="container-info">
                <label class="info-block gray-uppercase" for="comment-for-supervisor">
                    {{ noteToSupervisor }} <template v-if="hasCriticalIssues && criticalityLevel == 'Warn'">
                        ({{ $t('WebInterviewUI.Complete_Required') }})
                    </template>
                </label>
                <div class="field">
                    <textarea class="field-to-fill" id="comment-for-supervisor" v-autosize
                        :placeholder="$t('WebInterviewUI.TextEnter')" v-model="comment" maxlength="750"></textarea>
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
            <div class="submit-info" v-dompurify-html="$t('WebInterviewUI.Complete_SubmitInfo')"></div>
            <div class="container-info">
                <a href="javascript:void(0);" id="btnComplete" class="btn btn-lg btn-with-icon" v-bind:class="{
                    'btn-success': isAllAnswered,
                    'btn-primary': hasUnansweredQuestions,
                    'btn-danger': hasErrors,
                    'disabled': !isCompletionPermitted,
                }" @click="completeInterview">
                    <span class="btn-icon"></span>
                    {{ competeButtonTitle }}
                </a>
                <div class="info-block gray-uppercase" v-if="doesShowCompleteComment" style="margin-top:10px;">{{
                    completeButtionComment }}
                </div>
            </div>
        </div>
        <SectionLoadingProgress />
    </div>
</template>

<style scoped>
.submit-info {
    color: #343434;
    font-family: RobotoRegular;
    font-size: 13px;
    opacity: 1;
    padding-bottom: 16px;
}

.wrapper-info {
    border-bottom: none;
}

.complete-tabs {
    display: flex;
    flex-wrap: wrap;
    padding-top: 0px;
    padding-bottom: 0px;
    list-style: none;
    border-bottom: 1px solid #000000;
    margin-bottom: 0px;
}

.tab-item {
    width: 159px;
    height: 75px;
    opacity: 1;
    overflow: hidden;
    margin-top: 18px;

    position: relative;
    cursor: pointer;
    padding: 8px 14px 10px;
    font-size: 14px;
    line-height: 1.2;
    border: 1px solid #000000;
    border-bottom: none;
    border-radius: 6px 6px 0 0;
    color: #555;
    align-items: center;
    gap: 6px;
    transition: background .15s ease, color .15s ease;
}


.tab-item.active {
    height: 85px;
    margin-top: 8px;
    padding-top: 18px;
}

.tab-item:hover:not(.active) {
    background: #ebebee;
}

.tab-item.active {
    background: #fff;
    color: #222;
    font-weight: 600;
    box-shadow: 0 -2px 6px rgba(0, 0, 0, 0.06);
    bottom: -1px;
}

.tab-item.active::before {
    content: '';
    margin: 10px 5px;
    position: absolute;
    top: -5px;
    left: 0;
    right: 0;
    height: 5px;
    background: currentColor;
    border-radius: 5px;
}

.tab-item.errors {
    color: #DB3913;
}

.tab-item.unanswered {
    color: #2878BE;
}

.tab-item.critical-rule-errors {
    color: #DB3913;
}

.tab-item.disabled {
    color: #949494;
    cursor: initial;
    pointer-events: none;
}

.tab-item .tab-title {
    display: inline-block;
    width: 130px;
    font-family: RobotoLight;
    font-weight: Medium;
    font-size: 14px;
    opacity: 1;
    text-align: left;
}

.tab-item .tab-count {
    font-family: RobotoRegular;
    font-weight: Black;
    font-size: 18px;
    opacity: 1;
    text-align: left;
}

.tab-content {
    margin-top: 0px;
    margin-bottom: 20px;
    background-color: #fff;
    border-bottom: 1px solid #000000;
}

.tab-content .tab-content-item {
    padding: 10px 15px;
    margin: 4px;
    opacity: 1;
    border-top-left-radius: 4px;
    border-top-right-radius: 4px;
    border-bottom-left-radius: 4px;
    border-bottom-right-radius: 4px;
    box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.25);
    overflow: hidden;
    cursor: pointer;
}

.tab-content-item .item-title {
    color: rgba(0, 0, 0, 1);
    font-family: RobotoRegular;
    font-weight: Bold;
    font-size: 14px;
    opacity: 1;
    text-align: left;
}

.tab-content-item .item-error {
    color: rgba(219, 57, 18, 1);
    font-family: RobotoRegular;
    font-size: 14px;
    opacity: 1;
    text-align: left;
}

.tab-content-item .item-comment {
    color: rgba(0, 0, 0, 1);
    font-family: RobotoRegular;
    font-size: 14px;
    opacity: 1;
    text-align: left;
}

/* Left colored stripe based on active group type */
.tab-content-item {
    position: relative;
    padding-left: 20px;
    max-width: 540px;
}

.tab-content.errors .tab-content-item::before,
.tab-content.critical-rule-errors .tab-content-item::before {
    background: #DB3913;
}

.tab-content.unanswered .tab-content-item::before {
    background: #2878BE;
}

.tab-content .tab-content-item::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 5px;
    height: 100%;
    border-radius: 4px 0 0 4px;
}

/* Button with icon styles */
.btn-with-icon {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    position: relative;
}

.btn-icon {
    width: 18px;
    height: 18px;
    display: inline-block;
    flex-shrink: 0;
}

.btn-with-icon:not(.disabled) .btn-icon {
    background-image: url("data:image/svg+xml,%3Csvg width='17' height='17' viewBox='0 0 17 17' fill='none' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath d='M15.5 3.5L8.74999 9.75L5.5 6.5' stroke='white' stroke-width='2' stroke-miterlimit='10' stroke-linecap='round' stroke-linejoin='round'/%3E%3Cpath d='M15.9819 9.02093C15.873 10.5842 15.2773 12.0742 14.2782 13.2815C13.2791 14.4888 11.927 15.3528 10.4117 15.7523C8.89637 16.1517 7.294 16.0665 5.82962 15.5085C4.36524 14.9505 3.11241 13.9479 2.2471 12.6414C1.38178 11.3349 0.947437 9.79016 1.00507 8.22415C1.06271 6.65813 1.60943 5.1495 2.56845 3.91015C3.52747 2.67079 4.85062 1.76296 6.35205 1.31416C7.85348 0.865359 9.45779 0.898134 10.9396 1.40788' stroke='white' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/%3E%3C/svg%3E");
    background-size: contain;
    background-repeat: no-repeat;
    background-position: center;
}

.btn-with-icon.disabled .btn-icon {
    background-image: url("data:image/svg+xml,%3Csvg width='15' height='19' viewBox='0 0 15 19' fill='none' xmlns='http://www.w3.org/2000/svg'%3E%3Crect x='1' y='7' width='13' height='11' rx='2' stroke='white' stroke-width='2'/%3E%3Cpath d='M4 4.5C4 2.567 5.567 1 7.5 1V1C9.433 1 11 2.567 11 4.5V7H4V4.5Z' stroke='white' stroke-width='2'/%3E%3Crect x='6' y='11' width='3' height='3' rx='1.5' fill='white'/%3E%3C/svg%3E");
    background-size: contain;
    background-repeat: no-repeat;
    background-position: center;
}

.btn-success .btn-icon,
.btn-primary .btn-icon,
.btn-danger .btn-icon {
    filter: brightness(0) invert(1);
}

.btn-with-icon.disabled .btn-icon {
    filter: brightness(0) invert(0.4);
    opacity: 0.5;
}

.and-more {
    color: #333;
    font-family: RobotoRegular;
    font-size: 14px;
    opacity: 0.5;
    padding-top: 17px;
    padding-left: 19px;
}
</style>

<script lang="js">
import modal from '@/shared/modal'
import SectionProgress from './SectionLoadProgress'

export default {
    name: 'complete-view',
    components: {
        SectionLoadingProgress: SectionProgress,
    },
    data() {
        return {
            comment: '',
            switchToWeb: false,
            wasCriticalityInfoLoaded: false,
            activeCompleteGroupIndex: null,
        }
    },
    beforeMount() {
        this.wasCriticalityInfoLoaded = false;
        this.fetchCompleteInfo()
    },
    watch: {
        completeGroups(newVal) {
            if (Array.isArray(newVal) && newVal.length > 0) {
                let targetIndex = null;

                for (let i = 0; i < newVal.length; i++) {
                    const group = newVal[i];
                    if (Array.isArray(group.items) && group.items.length > 0) {
                        targetIndex = i;
                        break;
                    }
                }

                if (targetIndex !== null && (this.activeCompleteGroupIndex === null || this.shouldUpdateActiveTab(newVal))) {
                    this.activeCompleteGroupIndex = targetIndex;
                } else if (this.activeCompleteGroupIndex === null) {
                    this.activeCompleteGroupIndex = 0;
                }
            }
        },
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

            const criticalFailed = this.criticalityInfo?.failedCriticalRules || []
            const criticalUnanswered = this.criticalityInfo?.unansweredCriticalQuestions || []
            const critical = criticalFailed.concat(criticalUnanswered)
            groups.push({
                title: this.$t('WebInterviewUI.Complete_Tab_CriticalErrors'),
                items: critical,
                total: critical.length,
                cssClass: 'errors'
            })

            groups.push({
                title: this.$t('WebInterviewUI.Complete_Tab_QuestionsWithErrors'),
                items: this.completeInfo?.entitiesWithError,
                total: this.completeInfo?.errorsCount,
                cssClass: 'errors'
            })

            groups.push({
                title: this.$t('WebInterviewUI.Complete_Tab_UnansweredQuestions'),
                items: this.completeInfo?.unansweredQuestions,
                total: this.completeInfo?.unansweredCount,
                cssClass: 'unanswered'
            })

            return groups;
        },
        activeGroup() {
            const active = this.completeGroups[this.activeCompleteGroupIndex]
            return active || { items: [] }
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
        doesShowCompleteComment() {
            return !this.switchToWeb
        },
        overallProgressPercent() {
            const answered = this.completeInfo?.answeredCount || 0
            const unanswered = this.completeInfo?.unansweredCount || 0
            const total = answered + unanswered
            if (total === 0) return 0
            return Math.round((answered / total) * 100)
        },
        hasAnyIssue() {
            return this.hasErrors || this.hasUnansweredQuestions || this.hasCriticalIssues
        },
        moreCount() {
            const activeGroup = this.activeGroup;
            if (!activeGroup) return 0;
            const displayedItemsCount = activeGroup.items ? activeGroup.items.length : 0;
            return activeGroup.total - displayedItemsCount;
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
                if (this.hasCriticalIssues && this.criticalityLevel == 'Warn') {

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

        setActive(index) {
            const group = this.completeGroups[index]
            if (!group) return
            if (!Array.isArray(group.items) || group.items.length === 0) return
            this.activeCompleteGroupIndex = index
        },

        shouldUpdateActiveTab(newGroups) {
            if (!Array.isArray(newGroups) || newGroups.length === 0) return false;

            const criticalErrorsGroup = newGroups[0];

            if (Array.isArray(criticalErrorsGroup.items) &&
                criticalErrorsGroup.items.length > 0 &&
                this.activeCompleteGroupIndex !== 0) {
                return true;
            }

            return false;
        },
    },
}

</script>
