<template>
    <div class="panel panel-details" :class="{
        'contains-action-buttons':
            showRejectButton ||
            showUnapproveButton ||
            showApproveButton ||
            showMoreButton,
        'contains-tranlation': canChangeLanguage,
    }">
        <div class="panel-body clearfix">
            <div class="about-questionnaire clearfix">
                <div class="about-questionnaire-details clearfix">
                    <ul class="main-info-column list-unstyled pull-left">
                        <li id="detailsInfo_interviewKeyListItem">
                            {{ $t('Common.InterviewKey') }}:
                            {{ $config.model.key }}({{
                                $t('Common.Assignment')
                            }}
                            <a :href="this.$config.model.assignmentDetailsUrl">#{{ this.$config.model.assignmentId
                                }}</a>)
                        </li>
                        <li id="detailsInfo_qusetionnaireTitleListItem" class="questionnaire-title">
                            [ver.{{ this.$config.model.questionnaireVersion }}]
                            {{ this.$config.model.questionnaireTitle }}
                        </li>

                        <li id="detailsInfo_interviewMode">
                            <span class="data-label">{{ $t('Details.InterviewMode') }}:
                            </span>
                            <span v-if="interviewinCawiMode">
                                <button type="button" class="btn btn-link gray-action-unit" @click="showModeDetails">
                                    {{ $t('Common.Cawi') }}
                                </button>
                            </span>
                            <span v-else>{{ $t('Common.Capi') }}</span>
                        </li>
                    </ul>
                    <ul class="list-unstyled pull-left table-info">
                        <li id="detailsInfo_interviewDurationListItem" v-if="this.$config.model.interviewDuration">
                            <span class="data-label">{{ $t('Details.Duration') }}:</span>
                            <span class="data">{{
                                this.$config.model.interviewDuration
                            }}</span>
                        </li>
                        <li id="detailsInfo_responsibleListItem">
                            <span class="data-label">{{ $t('Details.Responsible') }}:
                            </span>
                            <span v-if="isInterviewerResponsible" class="data">
                                <a :class="responsibleRole" :href="this.$config.model.responsibleProfileUrl
                                    ">{{ this.$config.model.responsible }}</a>
                            </span>
                            <span v-else class="data supervisor">{{
                                this.$config.model.responsible
                            }}</span>
                        </li>
                        <li id="detailsInfo_supervisorListItem">
                            <span class="data-label">{{ $t('Users.Supervisor') }}:
                            </span>
                            <span class="data supervisor">{{
                                this.$config.model.supervisor
                            }}</span>
                        </li>
                    </ul>
                    <ul class="list-unstyled pull-left table-info">
                        <li id="detailsInfo_StatusListItem">
                            <span class="data-label">{{
                                this.$t('Details.Status')
                            }}</span>
                            <span class="data">{{
                                this.$config.model.statusName
                            }}</span>
                            <button type="button" class="btn btn-link gray-action-unit" @click="showStatusesHistory">
                                {{ $t('Common.ShowStatusHistory') }}
                            </button>
                        </li>
                        <li id="detailsInfo_lastUpdatedListItem">
                            <span class="data-label">{{ $t('Details.LastUpdated') }}:</span>
                            <span class="data">{{ lastUpdateDate }}</span>
                            <button id="btn_ShowOverview" type="button" class="btn btn-link gray-action-unit"
                                @click="showOverview">
                                {{ $t('Details.Overview') }}
                            </button>
                        </li>
                        <li>
                            <span class="data-label">{{ $t('Common.CalendarEvent') }}:</span>
                            <span class="data" data-bs-toggle="tooltip" v-if="calendarEvent != null" :title="calendarEvent.comment == null ||
                                calendarEvent.comment == ''
                                ? this.$t('Assignments.NoComment')
                                : calendarEvent.comment
                                ">
                                {{ calendarEventTime }}
                            </span>

                            <span class="data" v-if="calendarEvent == null"></span>
                            <a id="btn_Print" class="btn btn-link gray-action-unit"
                                v-bind:href="this.$config.model.pdfUrl" target="_blank"
                                :title="$t('WebInterview.DownloadAnswersHint')" download>{{
                                    $t('WebInterview.DownloadAnswers') }}</a>
                        </li>
                    </ul>
                </div>
            </div>
            <div class="questionnaire-details-actions clearfix">
                <div class="buttons-container">
                    <SwitchLanguage v-if="canChangeLanguage" :isDisabled="changeLanguageDisabled" />
                    <button id="btn_approve" type="button" class="btn btn-success marl" v-if="showApproveButton"
                        @click="approve" :disabled="changeStatusDisabled">
                        {{ $t('Pages.ApproveRejectPartialView_ApproveAction') }}
                    </button>
                    <button id="btn_reject" type="button" class="btn btn-default btn-lg reject marl"
                        v-if="showRejectButton" @click="reject" :disabled="changeStatusDisabled">
                        {{ $t('Pages.ApproveRejectPartialView_RejectAction') }}
                    </button>
                    <button id="btn_unapprove" type="button" class="btn btn-default btn-lg reject marl"
                        v-if="showUnapproveButton" @click="reject">
                        {{
                            $t('Pages.ApproveRejectPartialView_UnapproveAction')
                        }}
                    </button>

                    <div class="dropdown aside-menu" :disabled="config.isObserving" v-if="showMoreButton">
                        <button type="button" data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false"
                            class="btn btn-link" :disabled="config.isObserving">
                            <span></span>
                        </button>
                        <ul class="dropdown-menu context-menu-list context-menu-root">
                            <li v-if="canBeReassigned">
                                <a href="#" class="primary-text" @click="assignSelected">
                                    {{ $t('Common.Assign') }}
                                </a>
                            </li>

                            <li v-if="canChangeToCawi">
                                <a href="#" @click="cahngeToCawiSelected">
                                    {{ $t('Common.ChangeToCAWI') }}
                                </a>
                            </li>
                            <li v-if="canChangeToCapi">
                                <a href="#" @click="changeToCapiSelected">
                                    {{ $t('Common.ChangeToCAPI') }}
                                </a>
                            </li>
                            <li v-if="canBeReassigned ||
                                canChangeToCawi ||
                                canChangeToCapi
                            " class="context-menu-separator context-menu-not-selectable"></li>
                            <li :class="canBeDeleted ? '' : 'disabled'">
                                <a href="#" :class="canBeDeleted ? 'error-text' : 'disabled'
                                    " @click="deleteSelected">
                                    {{ $t('Common.Delete') }}
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
        <OverviewModal ref="overview" id="overview" slot="modals" class="overviewModal" />
        <StatusesHistory ref="statusesHistory" id="statusesHistory" slot="modals" class="statusHistoryModal" />
        <Confirm ref="confirmApprove" id="confirmApprove" slot="modals"
            :title="$t('Pages.ApproveRejectPartialView_ApproveLabel')" :okTitle="$t('Common.Approve')" :disableOk="receivedByInterviewer && !doApproveReceivedByInterviewer
                ">
            <div class="form-group" v-if="receivedByInterviewer">
                <input type="checkbox" id="approveReceivedByInterviewer" v-model="doApproveReceivedByInterviewer"
                    class="checkbox-filter" />
                <label for="approveReceivedByInterviewer" style="font-weight: normal">
                    <span class="tick"></span>
                    {{
                        $t(
                            'Pages.ApproveRejectPartialView_ApproveReceivedConfirm',
                        )
                    }}
                </label>
                <br />
                <span v-if="doApproveReceivedByInterviewer" class="text-danger">
                    {{
                        $t(
                            'Pages.ApproveRejectPartialView_ApproveReceivedWarning',
                        )
                    }}
                </span>
                <br />
            </div>

            <label for="txtApproveComment">
                {{ $t('Pages.ApproveRejectPartialView_CommentLabel') }}:
            </label>
            <textarea class="form-control" rows="10" :maxlength="commentMaxLength" id="txtApproveComment"
                v-model="approveComment"></textarea>
            <span class="countDown">{{ approveCharsLeft }}</span>
        </Confirm>

        <Confirm ref="rejectConfirm" id="rejectConfirm" slot="modals" :title="showUnapproveButton
            ? $t('Pages.ApproveRejectPartialView_UnapproveLabel')
            : $t('Pages.ApproveRejectPartialView_RejectLAbel')
            " :disableOk="(interviewerShouldbeSelected || rejectToNewResponsible) &&
                newResponsibleId == null
                " :okTitle="showUnapproveButton
                    ? $t('Common.Unapprove')
                    : $t('Common.Reject')
                    " :okClass="btn - danger">
            <form v-if="!showUnapproveButton" onsubmit="return false;">
                <div class="form-group">
                    <Radio v-if="!interviewerShouldbeSelected" :label="$t('Interviews.RejectToOriginal')"
                        :radioGroup="false" name="rejectToNewResponsible" :value="rejectToNewResponsible"
                        @input="rejectToNewResponsible = false; newResponsibleId = null" />
                    <Radio v-if="!interviewerShouldbeSelected" :label="$t('Interviews.RejectToNewResponsible')"
                        :radioGroup="true" name="rejectToNewResponsible" :value="rejectToNewResponsible"
                        @input="rejectToNewResponsible = true" />
                    <p>
                        <Typeahead v-if="rejectToNewResponsible == true ||
                            interviewerShouldbeSelected
                        " control-id="rejectResponsibleId" :placeholder="$t('Common.Responsible')"
                            :value="newResponsibleId" @selected="newResponsibleSelected" :fetch-url="this.$config.model.approveReject
                                .interviewersListUrl
                                ">
                        </Typeahead>
                    </p>
                </div>
            </form>

            <label for="txtApproveComment">
                {{ $t('Pages.ApproveRejectPartialView_CommentLabel') }}:
            </label>
            <textarea class="form-control" rows="10" :maxlength="commentMaxLength" id="txtRejectComment"
                v-model="rejectComment"></textarea>
            <span class="countDown">{{ rejectCharsLeft }}</span>
        </Confirm>

        <ModalFrame ref="modeDetails" id="modeDetails">
            <h3>
                {{ $t('Details.InterviewMode') }}:
                {{
                    interviewinCawiMode
                        ? $t('Common.Cawi')
                        : $t('Common.Capi')
                }}
            </h3>
            <div>
                <p>{{ webLink }}</p>
            </div>
            <template v-slot:actions>
                <div>
                    <button type="button" class="btn btn-link" @click="hideModeDetails">
                        {{ $t('Pages.CloseLabel') }}
                    </button>
                </div>
            </template>
        </ModalFrame>

        <ModalFrame ref="deleteModal" :title="$t('Common.Delete')">
            <div class="action-container">
                <p v-dompurify-html="$t('Interviews.DeleteConfirmMessageHQ', {
                    count: 1,
                    status1: 'Supervisor assigned',
                    status2: 'Interviewer assigned',
                })
                    "></p>
            </div>
            <template v-slot:actions>
                <div>
                    <button type="button" class="btn btn-danger" role="confirm" @click="deleteInterviews">
                        {{ $t('Common.Delete') }}
                    </button>
                    <button type="button" class="btn btn-link" data-bs-dismiss="modal" role="cancel">
                        {{ $t('Common.Cancel') }}
                    </button>
                </div>
            </template>
        </ModalFrame>

        <ModalFrame ref="assignModal" :title="$t('Common.Assign')">
            <form onsubmit="return false;">
                <div class="form-group">
                    <label class="control-label" for="newResponsibleId">{{
                        $t('Assignments.SelectResponsible')
                    }}</label>
                    <Typeahead control-id="newResponsibleId" :placeholder="$t('Common.Responsible')"
                        :value="newResponsibleId" :ajax-params="{}" @selected="newResponsibleSelected"
                        :fetch-url="config.api.responsible"></Typeahead>
                </div>
                <div id="pnlAssignToOtherTeamConfirmMessage">
                    <p v-dompurify-html="this.config.isSupervisor
                        ? $t('Interviews.AssignConfirmMessage', {
                            count: 1,
                            status1: 'Supervisor assigned',
                            status2: 'Interviewer assigned',
                            status3: 'Rejected by Supervisor',
                        })
                        : $t(
                            'Interviews.AssignToOtherTeamConfirmMessage',
                            {
                                count: 1,
                                status1: 'Approved by Supervisor',
                                status2: 'Approved by Headquarters',
                            },
                        )
                        "></p>
                </div>

                <div v-if="isReceivedByInterviewerAtUtc">
                    <br />
                    <input type="checkbox" id="reassignReceivedByInterviewer" v-model="isReassignReceivedByInterviewer"
                        class="checkbox-filter" />
                    <label for="reassignReceivedByInterviewer" style="font-weight: normal">
                        <span class="tick"></span>
                        {{ $t('Interviews.AssignReceivedConfirm', 1) }}
                    </label>
                    <br />
                    <span v-if="isReassignReceivedByInterviewer" class="text-danger">
                        {{ $t('Interviews.AssignReceivedWarning') }}
                    </span>
                </div>
            </form>
            <template v-slot:actions>
                <div>
                    <button type="button" class="btn btn-primary" role="confirm" @click="assign"
                        :disabled="!newResponsibleId">
                        {{ $t('Common.Assign') }}
                    </button>
                    <button type="button" class="btn btn-link" data-bs-dismiss="modal" role="cancel">
                        {{ $t('Common.Cancel') }}
                    </button>
                </div>
            </template>
        </ModalFrame>

        <ChangeToCapi ref="modalChangeToCAWI" :modalId="'switchToCawi_id'" :title="$t('Common.ChangeToCAWI')"
            :confirmMessage="$t('Common.ChangeToCAWIConfirmHQ', { count: 1 })" :filteredCount="1"
            :receivedByInterviewerItemsCount="isReceivedByInterviewerAtUtc ? 1 : 0
                " @confirm="changeInterviewModeToCawi" />

        <ChangeToCapi ref="modalChangeToCAPI" :modalId="'switchToCapi_id'" :title="$t('Common.ChangeToCAPI')"
            :confirmMessage="$t('Common.ChangeToCAPIConfirmHQ', {
                count: 1,
            })
                " :filteredCount="1" :receivedByInterviewerItemsCount="isReceivedByInterviewerAtUtc ? 1 : 0
                    " @confirm="changeInterviewModeToCapi" />
    </div>
</template>

<script>
import SwitchLanguage from './SwitchLanguage'
import StatusesHistory from './StatusesHistory'
import OverviewModal from './OverviewModal'
import { convertToLocal } from '~/shared/helpers'
import moment from 'moment-timezone'
import ChangeToCapi from '../Interviews/ChangeModeModal.vue'

import { map, assign } from 'lodash'

export default {
    data() {
        return {
            approveComment: '',
            rejectComment: '',
            commentMaxLength: 1500,
            newResponsibleId: null,
            rejectToNewResponsible: false,
            doApproveReceivedByInterviewer: false,
            isReassignReceivedByInterviewer: false,
        }
    },
    methods: {
        assignSelected() {
            this.newResponsibleId = null
            this.$refs.assignModal.modal({ keyboard: false })
        },
        cahngeToCawiSelected() {
            this.$refs.modalChangeToCAWI.modal({ keyboard: false })
        },
        changeToCapiSelected() {
            this.$refs.modalChangeToCAPI.modal({ keyboard: false })
        },
        deleteSelected() {
            this.$refs.deleteModal.modal({ keyboard: false })
        },
        arrayMap: function (array, mapping) {
            array = array || []
            var result = []
            for (var i = 0, j = array.length; i < j; i++)
                result.push(mapping(array[i], i))
            return result
        },

        arrayFilter: function (array, predicate) {
            array = array || []
            var result = []
            for (var i = 0, j = array.length; i < j; i++)
                if (predicate(array[i], i)) result.push(array[i])
            return result
        },
        assign() {
            const self = this

            var filteredItems = []
            filteredItems.push({
                id: this.$config.model.id,
                receivedByInterviewerAtUtc:
                    this.$config.model.receivedByInterviewerAtUtc,
            })

            if (!this.isReassignReceivedByInterviewer) {
                filteredItems = this.arrayFilter(
                    filteredItems,
                    function (item) {
                        return item.receivedByInterviewerAtUtc === null
                    },
                )
            }

            if (filteredItems.length == 0) {
                this.$refs.assignModal.hide()
                return
            }

            var commands = this.arrayMap(
                map(filteredItems, (interview) => {
                    return interview.id
                }),
                function (rowId) {
                    var item = {
                        InterviewId: rowId,
                        InterviewerId:
                            self.newResponsibleId.iconClass === 'interviewer'
                                ? self.newResponsibleId.key
                                : null,
                        SupervisorId:
                            self.newResponsibleId.iconClass === 'supervisor'
                                ? self.newResponsibleId.key
                                : null,
                    }
                    return JSON.stringify(item)
                },
            )

            var command = {
                type:
                    self.$config.model.isSupervisor &&
                        self.newResponsibleId.iconClass === 'interviewer'
                        ? 'AssignInterviewerCommand'
                        : 'AssignResponsibleCommand',
                commands: commands,
            }

            this.executeCommand(
                command,
                function () { },
                function () {
                    self.$refs.assignModal.hide()
                    self.newResponsibleId = null
                    window.location.reload(true)
                },
            )
        },

        getCommand(commandName, Ids, comment) {
            var commands = this.arrayMap(Ids, function (rowId) {
                var item = { InterviewId: rowId, Comment: comment }
                return JSON.stringify(item)
            })

            var command = {
                type: commandName,
                commands: commands,
            }

            return command
        },

        deleteInterviews() {
            const self = this
            var filteredItems = []
            filteredItems.push({ id: this.$config.model.id })

            if (filteredItems.length == 0) {
                this.$refs.deleteModal.hide()
                return
            }

            var command = this.getCommand(
                'DeleteInterviewCommand',
                map(filteredItems, (interview) => {
                    return interview.id
                }),
            )

            this.$store.dispatch('stop')
            this.executeCommand(
                command,
                function () { },
                function () {
                    self.$refs.deleteModal.hide()
                    window.location = self.$config.model.interviewsUrl
                },
            )
        },

        approve() {
            this.$refs.confirmApprove.promt((ok) => {
                if (ok) {
                    this.$store
                        .dispatch('approve', this.approveComment)
                        .then(() => {
                            window.location = this.$config.model.interviewsUrl
                        })
                }
            })
        },
        reject() {
            this.$refs.rejectConfirm.promt(async (ok) => {
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
        showModeDetails() {
            this.$refs.modeDetails.modal()
        },
        hideModeDetails() {
            this.$refs.modeDetails.hide()
        },

        changeInterviewModeToCawi(confirmReceivedByInterviewer) {
            this.changeInterviewMode(
                [
                    {
                        id: this.$config.model.id,
                        receivedByInterviewerAtUtc:
                            this.$config.model.receivedByInterviewerAtUtc,
                    },
                ],
                'CAWI',
                confirmReceivedByInterviewer,
            )
        },
        changeInterviewModeToCapi(confirmReceivedByInterviewer) {
            this.changeInterviewMode(
                [
                    {
                        id: this.$config.model.id,
                        receivedByInterviewerAtUtc:
                            this.$config.model.receivedByInterviewerAtUtc,
                    },
                ],
                'CAPI',
                confirmReceivedByInterviewer,
            )
        },

        changeInterviewMode(filteredItems, mode, confirmReceivedByInterviewer) {
            const self = this

            if (!confirmReceivedByInterviewer) {
                filteredItems = this.arrayFilter(
                    filteredItems,
                    function (item) {
                        return item.receivedByInterviewerAtUtc === null
                    },
                )
            }

            if (filteredItems.length == 0) {
                return
            }

            const commands = map(filteredItems, (i) => {
                return JSON.stringify({
                    InterviewId: i.id,
                    Mode: mode,
                })
            })

            const command = {
                type: 'ChangeInterviewModeCommand',
                commands,
            }

            this.executeCommand(
                command,
                function () { },
                function () {
                    window.location.reload(true)
                },
            )
        },

        executeCommand(command, onSuccess, onDone) {
            var url = this.config.commandsUrl
            var requestHeaders = {}

            $.ajax({
                cache: false,
                type: 'post',
                headers: requestHeaders,
                url: url,
                data: command,
                dataType: 'json',
            })
                .done(function (data) {
                    if (onSuccess !== undefined) onSuccess(data)
                })
                .fail(function (jqXhr, textStatus, errorThrown) {
                    if (jqXhr.status === 401) {
                        location.reload()
                    }
                    //display error
                })
                .always(function () {
                    if (onDone !== undefined) onDone()
                })
        },
    },

    computed: {
        config() {
            return this.$config.model
        },

        canBeReassigned() {
            return (
                !this.$config.model.isObserving &&
                this.$config.model.canBeReassigned
            )
        },
        canBeDeleted() {
            return (
                !this.$config.model.isObserving &&
                !this.$config.model.isSupervisor &&
                this.$config.model.canBeDeleted
            )
        },
        canChangeToCapi() {
            return (
                !this.$config.model.isObserving &&
                this.$config.model.canChangeToCapi
            )
        },
        canChangeToCawi() {
            return (
                !this.$config.model.isObserving &&
                this.$config.model.canChangeToCawi
            )
        },

        responsibleRole() {
            return this.$config.model.responsibleRole.toLowerCase()
        },

        isReceivedByInterviewerAtUtc() {
            return this.$config.model.receivedByInterviewerAtUtc != null
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
                this.$config.model.approveReject.supervisorApproveAllowed ||
                this.$config.model.approveReject.hqOrAdminApproveAllowed
            )
        },
        receivedByInterviewer() {
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
        calendarEvent() {
            return this.$config.model.calendarEvent
        },

        calendarEventTime() {
            return this.calendarEvent != null
                ? convertToLocal(
                    this.calendarEvent.startUtc,
                    this.calendarEvent.startTimezone,
                )
                : ''
        },
        interviewinCawiMode() {
            return this.$config.model.interviewMode === 2
        },
        webLink() {
            return this.$config.model.webInterviewUrl ?? ''
        },
        showMoreButton() {
            return true
        },
    },

    components: {
        SwitchLanguage,
        StatusesHistory,
        OverviewModal,
        ChangeToCapi,
    },
}
</script>
