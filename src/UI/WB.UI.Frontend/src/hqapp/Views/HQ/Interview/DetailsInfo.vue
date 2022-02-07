
<template>
    <div class="panel panel-details"
        :class="{ 'contains-action-buttons': showRejectButton || showUnapproveButton || showApproveButton, 'contains-tranlation' : canChangeLanguage}">
        <div class="panel-body clearfix">
            <div class="about-questionnaire clearfix">
                <div class="about-questionnaire-details clearfix">
                    <ul class="main-info-column list-unstyled pull-left">
                        <li id="detailsInfo_interviewKeyListItem">{{this.$t('Common.InterviewKey')}}: {{$config.model.key}}({{this.$t('Common.Assignment')}}
                            <a :href="this.$config.model.assignmentDetailsUrl">#{{this.$config.model.assignmentId}}</a>)</li>
                        <li id="detailsInfo_qusetionnaireTitleListItem"
                            class="questionnaire-title">[ver.{{this.$config.model.questionnaireVersion}}] {{this.$config.model.questionnaireTitle}}</li>

                        <li id="detailsInfo_interviewMode">
                            <span class="data-label">{{this.$t('Details.InterviewMode')}}: </span>
                            <span v-if="interviewinCawiMode">
                                <button type="button"
                                    class="btn btn-link gray-action-unit"
                                    @click="showModeDetails">{{this.$t('Common.Cawi')}}</button>
                            </span>
                            <span v-else>{{this.$t('Common.Capi')}}</span>
                        </li>
                    </ul>
                    <ul class="list-unstyled pull-left table-info">
                        <li id="detailsInfo_interviewDurationListItem"
                            v-if="this.$config.model.interviewDuration">
                            <span class="data-label">{{this.$t('Details.Duration')}}:</span>
                            <span class="data">{{this.$config.model.interviewDuration}}</span>
                        </li>
                        <li id="detailsInfo_responsibleListItem">
                            <span class="data-label">{{this.$t('Details.Responsible')}}: </span>
                            <span v-if="isInterviewerResponsible"
                                class="data">
                                <a :class="responsibleRole"
                                    :href="this.$config.model.responsibleProfileUrl">{{this.$config.model.responsible}}</a>
                            </span>
                            <span v-else
                                class="data supervisor">{{this.$config.model.responsible}}</span>
                        </li>
                        <li id="detailsInfo_supervisorListItem">
                            <span class="data-label">{{this.$t('Users.Supervisor')}}: </span>
                            <span class="data supervisor">{{this.$config.model.supervisor}}</span>
                        </li>
                    </ul>
                    <ul class="list-unstyled pull-left table-info">
                        <li id="detailsInfo_StatusListItem">
                            <span class="data-label">{{this.$t('Details.Status')}}</span>
                            <span class="data">{{this.$config.model.statusName}}</span>
                            <button type="button"
                                class="btn btn-link gray-action-unit"
                                @click="showStatusesHistory">{{$t("Common.ShowStatusHistory")}}</button>
                        </li>
                        <li id="detailsInfo_lastUpdatedListItem"><span class="data-label">{{this.$t('Details.LastUpdated')}}:</span>
                            <span class="data">{{lastUpdateDate}}</span>
                            <button id="btn_ShowOverview"
                                type="button"
                                class="btn btn-link gray-action-unit"
                                @click="showOverview">{{$t("Details.Overview")}}</button>
                        </li>
                        <li>
                            <span class="data-label">{{$t("Common.CalendarEvent")}}:</span>
                            <span class="data"
                                data-toggle="tooltip"
                                v-if="calendarEvent != null"
                                :title="((calendarEvent.comment == null || calendarEvent.comment == '') ? this.$t('Assignments.NoComment') : calendarEvent.comment)">
                                {{calendarEventTime}}
                            </span>

                            <span class="data"
                                v-if="calendarEvent == null"></span>
                            <a id="btn_Print"
                                class="btn btn-link gray-action-unit"
                                v-bind:href="this.$config.model.pdfUrl"
                                target="_blank"
                                :title="$t('WebInterview.DownloadAnswersHint')"
                                download>{{$t("WebInterview.DownloadAnswers")}}</a>
                        </li>
                    </ul>
                </div>
            </div>
            <div class="questionnaire-details-actions clearfix" >
                <div class="buttons-container">
                    <SwitchLanguage v-if="canChangeLanguage"
                        :disabled="changeLanguageDisabled"/>

                    <button id="btn_approve"
                        type="button"
                        class="btn btn-success marl"
                        v-if="showApproveButton"
                        @click="approve"
                        :disabled="changeStatusDisabled">
                        {{$t("Pages.ApproveRejectPartialView_ApproveAction")}}
                    </button>
                    <button id="btn_reject"
                        type="button"
                        class="btn btn-default btn-lg reject marl"
                        v-if="showRejectButton"
                        @click="reject"
                        :disabled="changeStatusDisabled">
                        {{$t("Pages.ApproveRejectPartialView_RejectAction")}}
                    </button>
                    <button  id="btn_unapprove"
                        type="button"
                        class="btn btn-default btn-lg reject marl"
                        v-if="showUnapproveButton"
                        @click="reject">
                        {{$t("Pages.ApproveRejectPartialView_UnapproveAction")}}
                    </button>
                </div>
            </div>
        </div>
        <OverviewModal ref="overview"
            id="overview"
            slot="modals"
            class="overviewModal" />
        <StatusesHistory ref="statusesHistory"
            id="statusesHistory"
            slot="modals"
            class="statusHistoryModal" />
        <Confirm ref="confirmApprove"
            id="confirmApprove"
            slot="modals"
            :title="$t('Pages.ApproveRejectPartialView_ApproveLabel')"
            :okTitle="$t('Common.Approve')"
            :disableOk="receivedByInterviewer && !doApproveReceivedByInterviewer">
            <div class="form-group"
                v-if="receivedByInterviewer">
                <input
                    type="checkbox"
                    id="reassignReceivedByInterviewer"
                    v-model="doApproveReceivedByInterviewer"
                    class="checkbox-filter"/>
                <label for="reassignReceivedByInterviewer"
                    style="font-weight: normal">
                    <span class="tick"></span>
                    {{$t("Pages.ApproveRejectPartialView_ApproveReceivedConfirm")}}
                </label>
                <br />
                <span v-if="doApproveReceivedByInterviewer"
                    class="text-danger">
                    {{$t("Pages.ApproveRejectPartialView_ApproveReceivedWarning")}}
                </span>
                <br />
            </div>

            <label for="txtApproveComment">
                {{$t("Pages.ApproveRejectPartialView_CommentLabel")}}:
            </label>
            <textarea class="form-control"
                rows="10"
                :maxlength="commentMaxLength"
                id="txtApproveComment"
                v-model="approveComment"></textarea>
            <span class="countDown">{{approveCharsLeft}}</span>
        </Confirm>

        <Confirm ref="rejectConfirm"
            id="rejectConfirm"
            slot="modals"
            :title="showUnapproveButton ? $t('Pages.ApproveRejectPartialView_UnapproveLabel') : $t('Pages.ApproveRejectPartialView_RejectLAbel')"
            :disableOk="(interviewerShouldbeSelected || rejectToNewResponsible) && newResponsibleId == null"
            :okTitle="showUnapproveButton ? $t('Common.Unapprove') : $t('Common.Reject')">
            <form v-if="!showUnapproveButton"
                onsubmit="return false;">
                <div class="form-group">
                    <Radio
                        v-if="!interviewerShouldbeSelected"
                        :label="$t('Interviews.RejectToOriginal')"
                        :radioGroup="false"
                        name="rejectToNewResponsible"
                        :value="rejectToNewResponsible"
                        @input="rejectToNewResponsible = false; newResponsibleId = null" />
                    <Radio
                        v-if="!interviewerShouldbeSelected"
                        :label="$t('Interviews.RejectToNewResponsible')"
                        :radioGroup="true"
                        name="rejectToNewResponsible"
                        :value="rejectToNewResponsible"
                        @input="rejectToNewResponsible = true" />
                    <p>
                        <Typeahead
                            v-if="rejectToNewResponsible == true || interviewerShouldbeSelected"
                            control-id="newResponsibleId"
                            :placeholder="$t('Common.Responsible')"
                            :value="newResponsibleId"
                            @selected="newResponsibleSelected"
                            :fetch-url="this.$config.model.approveReject.interviewersListUrl">
                        </Typeahead>
                    </p>
                </div>
            </form>

            <label for="txtApproveComment">
                {{$t("Pages.ApproveRejectPartialView_CommentLabel")}}:
            </label>
            <textarea class="form-control"
                rows="10"
                :maxlength="commentMaxLength"
                id="txtRejectComment"
                v-model="rejectComment"></textarea>
            <span class="countDown">{{rejectCharsLeft}}</span>
        </Confirm>

        <ModalFrame ref="modeDetails"
            id="modeDetails">
            <h3>{{$t("Details.InterviewMode")}}: {{interviewinCawiMode ? this.$t('Common.Cawi') : this.$t('Common.Capi')}}</h3>
            <div>
                <p>{{webLink}}</p>
            </div>
            <div slot="actions">
                <button type="button"
                    class="btn btn-link"
                    @click="hideModeDetails">
                    {{ $t("Pages.CloseLabel") }}
                </button>
            </div>
        </ModalFrame>
    </div>
</template>

<script>
import SwitchLanguage from './SwitchLanguage'
import StatusesHistory from './StatusesHistory'
import OverviewModal from './OverviewModal'
import Vue from 'vue'
import {DateFormats, convertToLocal} from '~/shared/helpers'
import moment from 'moment-timezone'

export default {
    data() {
        return {
            approveComment: '',
            rejectComment: '',
            commentMaxLength: 1500,
            newResponsibleId: null,
            rejectToNewResponsible: false,
            doApproveReceivedByInterviewer: false,
        }
    },
    methods: {
        approve() {
            this.$refs.confirmApprove.promt(ok => {
                if (ok) {
                    this.$store.dispatch('approve', this.approveComment).then(() => {
                        window.location = this.$config.model.interviewsUrl
                    })
                }
            })
        },
        reject() {
            this.$refs.rejectConfirm.promt(async ok => {
                if (ok) {
                    var newId = (this.newResponsibleId || {}).key
                    var dispatchResult = this.$store.dispatch('reject', {
                        comment: this.rejectComment,
                        assignTo: newId,
                    })
                    dispatchResult.then(() => {
                        window.location = this.$config.model.interviewsUrl
                    })
                }
            })
        },
        newResponsibleSelected(newValue) {
            this.newResponsibleId = newValue
        },
        showStatusesHistory() {
            this.$refs.statusesHistory.show()
        },
        showOverview() {
        //this.$router.push({name: "Overview"})
            this.$refs.overview.show()
        },
        showModeDetails(){
            this.$refs.modeDetails.modal('show')
        },
        hideModeDetails() {
            $(this.$refs.modeDetails).modal('hide')
        },
    },

    computed: {
        responsibleRole() {
            return this.$config.model.responsibleRole.toLowerCase()
        },
        interviewerShouldbeSelected() {
            return this.$config.model.approveReject.interviewerShouldbeSelected
        },
        approveCharsLeft() {
            return `${this.approveComment.length} / ${this.commentMaxLength}`
        },
        rejectCharsLeft() {
            return `${this.rejectComment.length} / ${this.commentMaxLength}`
        },
        showApproveButton() {
            return (
                this.$config.model.approveReject.supervisorApproveAllowed
                || this.$config.model.approveReject.hqOrAdminApproveAllowed
            )
        },
        receivedByInterviewer(){
            return this.$config.model.approveReject.receivedByInterviewer
        },
        showUnapproveButton() {
            return this.$config.model.approveReject.hqOrAdminUnapproveAllowed
        },
        showRejectButton() {
            return (
                this.$config.model.approveReject.supervisorRejectAllowed ||
                this.$config.model.approveReject.hqOrAdminRejectAllowed
            )
        },
        lastUpdateDate() {
            return moment.utc(this.$config.model.lastUpdatedAtUtc).fromNow()
        },
        canChangeLanguage() {
            return (
                this.$store.state.webinterview.languages != undefined &&
                this.$store.state.webinterview.languages.length > 0
            )
        },
        changeLanguageDisabled() {
            return this.$store.state.webinterview.interviewCannotBeChanged
        },
        changeStatusDisabled() {
            return this.$store.state.webinterview.isCurrentUserObserving
        },
        isInterviewerResponsible() {
            return this.$config.model.responsibleRole == 'Interviewer'
        },
        calendarEvent(){
            return this.$config.model.calendarEvent
        },

        calendarEventTime() {
            return this.calendarEvent != null
                ? convertToLocal(this.calendarEvent.startUtc, this.calendarEvent.startTimezone)
                : ''
        },
        interviewinCawiMode(){
            return this.$config.model.interviewMode === 2
        },
        webLink(){
            return this.$config.model.webInterviewUrl ?? ''
        },
    },

    components: {
        SwitchLanguage,
        StatusesHistory,
        OverviewModal,
    },
}
</script>

