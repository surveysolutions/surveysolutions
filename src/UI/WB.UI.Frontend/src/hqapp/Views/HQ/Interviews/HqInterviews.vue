<template>
    <HqLayout :title="title"
        :hasFilter="true">
        <div slot="headers">
            <a href="MapDashboard"
                style="float:right; margin-right:320px; margin-top:14px;">
                <img style="padding-top:2px;"
                    height="26px;"
                    src="/img/google-maps-markers/map.png"
                    :title="$t('Common.MapDashboard')" />
            </a>
            <h1>{{title}}</h1>
        </div>

        <Filters slot="filters">
            <FilterBlock :title="$t('Common.Questionnaire')">
                <Typeahead
                    ref="questionnaireIdControl"
                    control-id="questionnaireId"
                    data-vv-name="questionnaireId"
                    data-vv-as="questionnaire"
                    :placeholder="$t('Common.AllQuestionnaires')"
                    :value="questionnaireId"
                    :values="this.$config.model.questionnaires"
                    v-on:selected="questionnaireSelected"/>
            </FilterBlock>

            <FilterBlock :title="$t('Common.QuestionnaireVersion')">
                <Typeahead
                    ref="questionnaireVersionControl"
                    control-id="questionnaireVersion"
                    data-vv-name="questionnaireVersion"
                    data-vv-as="questionnaireVersion"
                    :placeholder="$t('Common.AllVersions')"
                    :disabled="questionnaireId == null"
                    :value="questionnaireVersion"
                    :values="questionnaireId == null ? [] : questionnaireId.versions"
                    v-on:selected="questionnaireVersionSelected" />
            </FilterBlock>

            <FilterBlock :title="$t('Common.Status')">
                <Typeahead
                    control-id="status"
                    :selectedKey="selectedStatus"
                    data-vv-name="status"
                    data-vv-as="status"
                    :placeholder="$t('Common.AllStatuses')"
                    :value="status"
                    :values="statuses"
                    v-on:selected="statusSelected"/>
            </FilterBlock>

            <FilterBlock :title="$t('Common.Responsible')">
                <Typeahead
                    control-id="responsibleId"
                    :placeholder="$t('Common.AllResponsible')"
                    :value="responsibleId"
                    :ajax-params="responsibleParams"
                    v-on:selected="userSelected"
                    :fetch-url="config.api.responsible"></Typeahead>
            </FilterBlock>

            <FilterBlock :title="$t('Pages.Filters_Assignment')">
                <div class="input-group">
                    <input
                        class="form-control with-clear-btn"
                        :placeholder="$t('Common.AllAssignments')"
                        type="text"
                        v-model="assignmentId"/>
                    <div class="input-group-btn"
                        @click="clearAssignmentFilter">
                        <div class="btn btn-default">
                            <span class="glyphicon glyphicon-remove"
                                aria-hidden="true"></span>
                        </div>
                    </div>
                </div>
            </FilterBlock>

            <FilterBlock :title="$t('Pages.Filters_InterviewMode')">
                <Typeahead
                    no-search
                    control-id="responsibleId"
                    :placeholder="$t('Pages.Filters_InterviewModePlaceHolder')"
                    :value="interviewMode"
                    :values="interviewModes"
                    v-on:selected="inteviewModeSelected"></Typeahead>
            </FilterBlock>

            <InterviewFilter slot="additional"
                :questionnaireId="where.questionnaireId"
                :questionnaireVersion="where.questionnaireVersion"
                :value="conditions"
                :exposedValuesFilter="exposedValuesFilter"
                @change="questionFilterChanged"
                @changeFilter="changeExposedValuesFilter" />
        </Filters>

        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            :contextMenuItems="contextMenuItems"
            @selectedRowsChanged="rows => selectedRows = rows"
            @page="resetSelection"
            @ajaxComplete="isLoading = false"
            :selectable="showSelectors"
            :selectableId="'id'">
            <div
                class="panel panel-table"
                v-if="selectedRows.length"
                id="pnlInterviewContextActions">
                <div class="panel-body">
                    <input
                        class="double-checkbox-white"
                        id="q1az"
                        type="checkbox"
                        checked
                        disabled="disabled"/>
                    <label for="q1az">
                        <span class="tick"></span>
                        {{ selectedRows.length + " " + $t("Pages.Interviews_Selected") }}
                    </label>
                    <button
                        class="btn btn-lg btn-success"
                        v-if="selectedRows.length"
                        :disabled="getFilteredToAssign().length == 0"
                        @click="assignInterview">{{ $t("Common.Assign") }}</button>
                    <button
                        class="btn btn-lg btn-success"
                        v-if="selectedRows.length"
                        :disabled="getFilteredToApprove().length == 0"
                        @click="approveInterview">{{ $t("Common.Approve")}}</button>
                    <button
                        class="btn btn-lg reject"
                        v-if="selectedRows.length"
                        :disabled="getFilteredToReject().length == 0"
                        @click="rejectInterview">{{ $t("Common.Reject")}}</button>
                    <button
                        class="btn btn-lg btn-primary"
                        v-if="selectedRows.length && !config.isSupervisor"
                        :disabled="getFilteredToUnApprove().length == 0"
                        @click="unapproveInterview">{{ $t("Common.Unapprove")}}</button>
                    <button
                        class="btn btn-lg btn-primary"
                        v-if="selectedRows.length && !config.isSupervisor"
                        :disabled="getFilteredToCapi().length == 0"
                        @click="changeToCAPI">{{ $t("Common.ChangeToCAPI")}}</button>
                    <button
                        class="btn btn-lg btn-primary"
                        v-if="selectedRows.length && !config.isSupervisor"
                        :disabled="getFilteredToCawi().length == 0"
                        @click="changeToCAWI">{{ $t("Common.ChangeToCAWI")}}</button>
                    <button
                        class="btn btn-link"
                        v-if="selectedRows.length && !config.isSupervisor"
                        :disabled="getFilteredToDelete().length == 0"
                        @click="deleteInterview">{{ $t("Common.Delete")}}</button>
                </div>
            </div>
        </DataTables>

        <ModalFrame ref="assignModal"
            :title="$t('Common.Assign')">
            <form onsubmit="return false;">
                <div class="form-group"
                    v-if="getFilteredToAssign().length > 0">
                    <label
                        class="control-label"
                        for="newResponsibleId">{{$t("Assignments.SelectResponsible")}}</label>
                    <Typeahead
                        control-id="newResponsibleId"
                        :placeholder="$t('Common.Responsible')"
                        :value="newResponsibleId"
                        :ajax-params="{ }"
                        @selected="newResponsibleSelected"
                        :fetch-url="config.api.responsible"></Typeahead>
                </div>
                <div id="pnlAssignToOtherTeamConfirmMessage">
                    <p
                        v-html="this.config.isSupervisor ? $t('Interviews.AssignConfirmMessage', {
                            count: this.getFilteredToAssign().length,
                            status1: 'Supervisor assigned',
                            status2: 'Interviewer assigned',
                            status3: 'Rejected by Supervisor'} )
                            : $t('Interviews.AssignToOtherTeamConfirmMessage', {
                                count: this.getFilteredToAssign().length,
                                status1: 'Approved by Supervisor',
                                status2: 'Approved by Headquarters'} )"></p>
                </div>

                <div v-if="CountReceivedByInterviewerItems() > 0">
                    <br />
                    <input
                        type="checkbox"
                        id="reassignReceivedByInterviewer"
                        v-model="isReassignReceivedByInterviewer"
                        class="checkbox-filter"/>
                    <label for="reassignReceivedByInterviewer"
                        style="font-weight: normal">
                        <span class="tick"></span>
                        {{$t("Interviews.AssignReceivedConfirm", CountReceivedByInterviewerItems())}}
                    </label>
                    <br />
                    <span v-if="isReassignReceivedByInterviewer"
                        class="text-danger">
                        {{$t("Interviews.AssignReceivedWarning")}}
                    </span>
                </div>
            </form>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-primary"
                    role="confirm"
                    @click="assign"
                    :disabled="!newResponsibleId || getFilteredToAssign().length == 0">{{ $t("Common.Assign") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                    role="cancel">{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>
        <ModalFrame ref="deleteModal"
            :title="$t('Common.Delete')">
            <div class="action-container">
                <p
                    v-html="$t('Interviews.DeleteConfirmMessageHQ', {count: this.getFilteredToDelete().length, status1: 'Supervisor assigned', status2: 'Interviewer assigned'})"></p>
            </div>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-primary"
                    role="confirm"
                    @click="deleteInterviews"
                    :disabled="getFilteredToDelete().length==0">{{ $t("Common.Delete") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                    role="cancel">{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>
        <ModalFrame ref="approveModal"
            :title="$t('Common.Approve')">
            <form onsubmit="return false;">
                <div class="action-container"
                    v-if="this.config.isSupervisor">
                    <h3>
                        {{$t('Interviews.ApproveConfirmMessage', {count: this.getFilteredToApprove().length })}}
                    </h3>
                    <p>
                        <strong>{{$t('Interviews.Note')}}</strong>
                        {{approveBySupervisorAllowedStatusesMessage}}
                    </p>
                </div>
                <div class="action-container"
                    v-else>
                    <p v-html="$t('Interviews.ApproveConfirmMessageHQ', {count: this.getFilteredToApprove().length, status1: 'Completed', status2: 'Approved by Supervisor', status3: 'Rejected by Supervisor'} )"></p>
                </div>

                <div class="form-group"
                    v-if="CountReceivedByInterviewerItems() > 0">
                    <br />
                    <input
                        type="checkbox"
                        id="approveReceivedByInterviewer"
                        v-model="isApproveReceivedByInterviewer"
                        class="checkbox-filter"/>
                    <label for="approveReceivedByInterviewer"
                        style="font-weight: normal">
                        <span class="tick"></span>
                        {{$t("Interviews.AssignReceivedConfirm", CountReceivedByInterviewerItems())}}
                    </label>
                    <br />
                    <span v-if="isApproveReceivedByInterviewer"
                        class="text-danger">
                        {{$t("Interviews.ApproveReceivedWarning")}}
                    </span>
                </div>

                <div>
                    <label
                        for="txtStatusApproveComment">{{$t("Pages.ApproveRejectPartialView_CommentLabel")}}:</label>
                    <textarea
                        class="form-control"
                        rows="10"
                        maxlength="200"
                        name="txtStatusChangeComment"
                        id="txtStatusApproveComment"
                        v-model="statusChangeComment"></textarea>
                </div>
            </form>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-primary"
                    role="confirm"
                    @click="approveInterviews"
                    :disabled="getFilteredToApprove().length==0">{{ $t("Common.Approve") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                    role="cancel">{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>
        <ModalFrame ref="rejectModal"
            :title="$t('Common.Reject')"
            id="rejectModel">
            <form onsubmit="return false;">
                <div class="action-container">
                    <p
                        v-if="!config.isSupervisor"
                        v-html="$t('Interviews.RejectConfirmMessageHQ', {count: this.getFilteredToReject().length, status1: 'Completed', status2: 'Approved by Supervisor'} )"></p>
                    <p
                        v-if="config.isSupervisor"
                        v-html="$t('Interviews.RejectConfirmMessage', {count: this.getFilteredToReject().length, status1: 'Completed', status2: 'Rejected by Headquarters'} )"></p>
                </div>

                <div>
                    <div class="options-group">
                        <Radio
                            :label="$t('Interviews.RejectToOriginal')"
                            :radioGroup="false"
                            name="rejectToNewResponsible"
                            :value="rejectToNewResponsible"
                            @input="rejectToNewResponsible = false; newResponsibleId = null" />
                        <Radio
                            :label="$t('Interviews.RejectToNewResponsible')"
                            :radioGroup="true"
                            name="rejectToNewResponsible"
                            :value="rejectToNewResponsible"
                            @input="rejectToNewResponsible = true" />
                        <p>
                            <Typeahead
                                v-if="rejectToNewResponsible == true"
                                control-id="rejectResponsibleId"
                                :placeholder="$t('Common.Responsible')"
                                :value="newResponsibleId"
                                :ajax-params="{ }"
                                @selected="newResponsibleSelected"
                                :fetch-url="config.api.responsible"></Typeahead>
                        </p>
                    </div>
                </div>

                <div>
                    <label
                        for="txtStatusChangeComment">{{$t("Pages.ApproveRejectPartialView_CommentLabel")}} :</label>
                    <textarea
                        class="form-control"
                        rows="10"
                        maxlength="200"
                        id="txtStatusChangeComment"
                        v-model="statusChangeComment"></textarea>
                </div>
            </form>
            <div slot="actions">
                <button
                    id="rejectOk"
                    type="button"
                    class="btn btn-primary"
                    role="confirm"
                    @click="rejectInterviews"
                    :disabled="getFilteredToReject().length==0 || (rejectToNewResponsible == true && newResponsibleId == null)">{{ $t("Common.Reject") }}</button>
                <button
                    id="rejectCancel"
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                    role="cancel">{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>
        <ModalFrame ref="unapproveModal"
            :title="$t('Common.Unapprove')">
            <form onsubmit="return false;">
                <div class="action-container">
                    <p
                        v-html="$t('Interviews.UnapproveConfirmMessageHQ', {count : this.getFilteredToUnApprove().length, status1: 'Approved by Headquarters'})"></p>
                </div>
            </form>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-primary"
                    role="confirm"
                    @click="unapproveInterviews"
                    :disabled="getFilteredToUnApprove().length==0">{{ $t("Common.Unapprove") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                    role="cancel">{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>
        <ModalFrame ref="statusHistory"
            :title="$t('Pages.HistoryOfStatuses_Title')">
            <div class="action-container">
                <p>
                    <a
                        class="interview-id title-row"
                        @click="viewInterview"
                        href="javascript:void(0)">{{interviewKey}}</a> by
                    <span :class="responsibleClass"
                        v-html="responsibleLink"></span>
                </p>
            </div>
            <div class="table-with-scroll">
                <table
                    class="table table-striped table-condensed table-hover table-break-words history"
                    id="statustable">
                    <thead>
                        <tr>
                            <td>{{ $t("Pages.HistoryOfStatuses_State")}}</td>
                            <td>{{ $t("Pages.HistoryOfStatuses_On")}}</td>
                            <td>{{ $t("Pages.HistoryOfStatuses_By")}}</td>
                            <td>{{ $t("Pages.HistoryOfStatuses_AssignedTo")}}</td>
                            <td>{{ $t("Pages.HistoryOfStatuses_Comment")}}</td>
                        </tr>
                    </thead>
                </table>
            </div>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-link"
                    role="confirm"
                    @click="viewInterview">{{ $t("Pages.HistoryOfStatuses_ViewInterview") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                    role="cancel">{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>
        <ChangeToCapi ref="modalChangeToCAWI"
            :title="$t('Common.ChangeToCAWI')"
            :confirmMessage="$t('Common.ChangeToCAWIConfirmHQ', {
                count: getFilteredToCawi().length})"
            :filteredCount="getFilteredToCawi().length"
            @confirm="changeInterviewMode(getFilteredToCawi(), 'CAWI')" />

        <ChangeToCapi ref="modalChangeToCAPI"
            :title="$t('Common.ChangeToCAPI')"
            :confirmMessage="$t('Common.ChangeToCAPIConfirmHQ', {
                count: getFilteredToCapi().length})"
            :filteredCount="getFilteredToCapi().length"
            @confirm="changeInterviewMode(getFilteredToCapi(), 'CAPI')" />
    </HqLayout>
</template>

<script>
import {DateFormats} from '~/shared/helpers'
import moment from 'moment'
import {lowerCase, find, filter, flatten, map,
    join, assign, isNaN, isNumber, toNumber, isEqual} from 'lodash'
import InterviewFilter from './InterviewQuestionsFilters'
import gql from 'graphql-tag'
import * as toastr from 'toastr'
import ChangeToCapi from './ChangeModeModal.vue'

import _sanitizeHtml from 'sanitize-html'
const sanitizeHtml = text => _sanitizeHtml(text,  { allowedTags: [], allowedAttributes: [] })

const query = gql`query hqInterviews($workspace: String!, $order: [InterviewSort!], $skip: Int, $take: Int, $where: InterviewsFilter) {
  interviews(workspace: $workspace, order: $order, skip: $skip, take: $take, where: $where) {
    totalCount
    filteredCount
    nodes {
      id
      key
      clientKey
      status
      questionnaireId
      responsibleId
      responsibleName
      interviewMode
      responsibleRole
      errorsCount
      assignmentId
      updateDateUtc
      receivedByInterviewerAtUtc
      actionFlags
      questionnaireVersion
      notAnsweredCount
      identifyingData {
        entity {
          variable
          questionText
          label
        }
        value
      }
    }
  }
}`

/** convert
 * [{variable, field, value}, {variable, field, value}]
 * ["variable,field,value", "variable,field,value"]
 */
function conditionToQueryString(conditions) {
    const result = []
    conditions.forEach(c => {
        result.push(`${c.variable},${c.field},${JSON.stringify(c.value)}`)
    })
    return result.length > 0 ? result : null
}

function queryStringToCondition(queryStringArray) {
    const result = []
    queryStringArray.forEach(q => {
        const parts = q.split(',')
        const value = parts.slice(2).join(',')

        result.push({
            variable: parts[0],
            field: parts[1],
            value: JSON.parse(value),
        })
    })
    return result
}

export default {
    components: {
        InterviewFilter,
        ChangeToCapi,
    },

    data() {
        return {
            restart_comment: null,
            questionnaireId: null,
            questionnaireVersion: null,
            isLoading: false,
            selectedRows: [],
            interviewMode: null,
            selectedRowWithMenu: null,
            totalRows: 0, filteredCount: 0,
            draw: 0,
            assignmentId: null,
            responsibleId: null,
            responsibleParams: {showArchived: true, showLocked: true},
            newResponsibleId: null,
            rejectToNewResponsible: false,
            statusChangeComment: null,
            status: null,
            selectedStatus: null,
            unactiveDateStart: null,
            unactiveDateEnd: null,
            statuses: this.$config.model.statuses,
            isApproveReceivedByInterviewer:false,
            isReassignReceivedByInterviewer: false,
            isVisiblePrefilledColumns: true,

            conditions: [],

            interviewModes: [{ key: 'CAWI', value: 'CAWI'}, { key: 'CAPI', value: 'CAPI'}],
            exposedValuesFilter: null,

        }
    },

    computed: {
        approveBySupervisorAllowedStatusesMessage(){
            const completedName = this.$t('Strings.InterviewStatus_Completed')
            const rejectedByHqName = this.$t('Strings.InterviewStatus_RejectedByHeadquarters')
            const rejectedBySvName = this.$t('Strings.InterviewStatus_RejectedBySupervisor')

            return this.$t('Interviews.ApproveConfirmMessage_Statuses', {status1: completedName, status2: rejectedByHqName, status3: rejectedBySvName})
        },
        rowData() {
            return (this.interviewData.edges || []).map(e => e.node)
        },

        interviewData() {
            if(this.interviews == null) {
                return {}
            }
            return this.interviews
        },
        interviewKey() {
            return this.selectedRowWithMenu != undefined ? this.selectedRowWithMenu.key : ''
        },
        responsibleLink() {
            if (this.selectedRowWithMenu == undefined) return ''

            return lowerCase(this.selectedRowWithMenu.responsibleRole) == 'interviewer'
                ? '<a href="' +
                      this.config.profileUrl +
                      '/' +
                      this.selectedRowWithMenu.responsibleId +
                      '">' +
                      this.selectedRowWithMenu.responsibleName +
                      '</a>'
                : this.selectedRowWithMenu.responsibleName
        },
        responsibleClass() {
            const result = this.selectedRowWithMenu != null ? lowerCase(this.selectedRowWithMenu.responsibleRole) : ''
            return result
        },

        tableColumns() {
            const self = this
            return [
                {
                    data: 'key',
                    name: 'Key',
                    title: this.$t('Common.InterviewKey'),
                    orderable: true,
                    searchable: true,
                    responsivePriority: 2,
                    className: 'interview-id title-row',
                    render(data, type, row) {
                        const append = data === row.clientKey ? '' : ` <span class="text-muted">(${row.clientKey})</span>`
                        const result =
                            `<a href="${self.config.interviewReviewUrl}/${row.id}">${data}${append}</a>`
                        return result
                    },
                    createdCell(td, cellData, rowData, row, col) {
                        $(td).attr('role', 'key')
                    },
                    width: '50px',
                },
                {
                    data: 'identifyingData',
                    title: this.$t('Assignments.IdentifyingQuestions'),
                    className: 'prefield-column first-identifying last-identifying sorting_disabled visible',
                    orderable: false,
                    searchable: false,
                    render(data) {
                        const delimiter = self.mode == 'dense'

                        var questionsWithTitles = map(filter(data, d => d.value != null && d.value != ''), node => {
                            return `${sanitizeHtml(node.entity.label || node.entity.questionText)}: <strong>${sanitizeHtml(node.value)}</strong>`
                        })

                        const dom = join(questionsWithTitles, ', ')
                        return dom
                    },
                    createdCell(td, cellData, rowData, row, col) {
                        $(td).attr('role', 'prefield')
                    },
                },
                {
                    data: 'responsibleName',
                    name: 'ResponsibleName',
                    title: this.$t('Common.Responsible'),
                    orderable: true,
                    createdCell(td, cellData, rowData, row, col) {
                        $(td).attr('role', 'responsible')
                    },
                    width: '100px',
                },
                {
                    data: 'updateDateUtc',
                    name: 'UpdateDateUtc',
                    title: this.$t('Assignments.UpdatedAt'),
                    className: 'date last-update',
                    searchable: false,
                    render(data) {
                        return moment
                            .utc(data)
                            .local()
                            .format(DateFormats.dateTimeInList)
                    },
                    createdCell(td, cellData, rowData, row, col) {
                        $(td).attr('role', 'updated')
                    },
                    width: '100px',
                },
                {
                    data: 'errorsCount',
                    name: 'ErrorsCount',
                    class: 'type-numeric',
                    title: this.$t('Interviews.Errors'),
                    orderable: true,
                    render(data) {
                        return data > 0 ? '<span style=\'color:red;\'>' + data + '</span>' : '0'
                    },
                    createdCell(td, cellData, rowData, row, col) {
                        $(td).attr('role', 'errors')
                    },
                    width: '45px',
                },{
                    data: 'notAnsweredCount',
                    name: 'NotAnsweredCount',
                    class: 'type-numeric',
                    title: this.$t('Interviews.NotAnsweredCount'),
                    orderable: true,
                    render(data) {
                        return data === null ? `<span class="text-muted">${self.$t('Common.Unknown')}</span>` : data
                    },
                    createdCell(td, cellData, rowData, row, col) {
                        $(td).attr('role', 'nonAnswered')
                    },
                    width: '50px',
                },
                {
                    data: 'interviewMode',
                    name: 'InterviewMode',
                    title: this.$t('Common.InterviewMode'),
                    orderable: false,
                    createdCell(td, cellData, rowData, row, col) {
                        $(td).attr('role', 'mode')
                    },
                    width: '50px',
                },
                {
                    data: 'status',
                    name: 'Status',
                    title: this.$t('Common.Status'),
                    orderable: true,
                    render(data) {
                        return find(self.statuses, s => s.key == data).value
                    },
                    createdCell(td, cellData, rowData, row, col) {
                        $(td).attr('role', 'status')
                    },
                    width: '120px',
                },
                {
                    data: 'receivedByInterviewerAtUtc',
                    name: 'ReceivedByInterviewerAtUtc',
                    title: this.$t('Common.ReceivedByInterviewer'),
                    render(data) {
                        if (data)
                            return moment
                                .utc(data)
                                .local()
                                .format(DateFormats.dateTimeInList)
                        return self.$t('Common.No')
                    },
                    createdCell(td, cellData, rowData, row, col) {
                        $(td).attr('role', 'received')
                    },
                    width: '50px',
                },
                {
                    data: 'assignmentId',
                    name: 'AssignmentId',
                    title: this.$t('Common.Assignment'),
                    orderable: true,
                    searchable: false,
                    createdCell(td, cellData, rowData, row, col) {
                        $(td).attr('role', 'assignment')
                    },
                    width: '50px',
                },
            ]
        },

        tableOptions() {
            const columns = this.tableColumns.filter(x => x.if == null || x.if())

            var defaultSortIndex = 3 //findIndex(columns, { name: "UpdateDate" });

            if (this.showSelectors) defaultSortIndex += 1

            const self = this

            var tableOptions = {
                rowId: function(row) {
                    return `row_${row.id}`
                },
                order: [[defaultSortIndex, 'desc']],
                deferLoading: 0,
                columns,
                pageLength: 20,
                ajax (data, callback, settings) {
                    const order = {}
                    const order_col = data.order[0]
                    const column = data.columns[order_col.column]

                    order[column.data] = order_col.dir.toUpperCase()

                    const variables = {
                        order: order,
                        skip: data.start,
                        take: data.length,
                        workspace: self.$store.getters.workspace,
                    }

                    const where = {
                        and: [...self.whereQuery],
                    }

                    const search = data.search.value

                    if(search && search != '') {
                        where.and.push({ or: [
                            { key: { startsWith: search.toLowerCase() }},
                            { clientKey: { startsWith: search.toLowerCase() }},
                            { responsibleNameLowerCase: { startsWith: search.toLowerCase() }},
                            { supervisorNameLowerCase: { startsWith: search.toLowerCase() }},
                            { identifyingData: { some: { valueLowerCase: { startsWith: search.toLowerCase()}}}},
                        ],
                        })
                    }

                    if(where.and.length > 0) {
                        variables.where = where
                    }

                    self.$apollo.query({
                        query,
                        variables: variables,
                        fetchPolicy: 'network-only',
                    }).then(response => {
                        const data = response.data.interviews
                        self.totalRows = data.totalCount
                        self.filteredCount = data.filteredCount
                        callback({
                            recordsTotal: data.totalCount,
                            recordsFiltered: data.filteredCount,
                            draw: ++this.draw,
                            data: data.nodes,
                        })
                    }).catch(err => {
                        callback({
                            recordsTotal: 0,
                            recordsFiltered: 0,
                            data: [],
                            error: err.toString(),
                        })
                        console.error(err)
                        toastr.error(err.message.toString())
                    })
                },
                select: {
                    style: 'multi',
                    selector: 'td>.checkbox-filter',
                    info: false,
                },
                dom: 'fritp',
                sDom: 'rf<"table-with-scroll"t>ip',
                searchHighlight: true,
            }

            return tableOptions
        },

        showSelectors() {
            return !this.config.isObserver && !this.config.isObserving
        },

        title() {
            return this.$t('Common.Interviews') + ' (' + this.formatNumber(this.filteredCount) + ')'
        },

        config() {
            return this.$config.model
        },

        where() {
            const data = {}

            if (this.status) data.status = this.status.key
            if (this.questionnaireId) data.questionnaireId = this.questionnaireId.key
            if (this.questionnaireVersion) data.questionnaireVersion = toNumber(this.questionnaireVersion.key)
            if (this.responsibleId) data.responsibleName = this.responsibleId.value
            if (this.assignmentId) data.assignmentId = toNumber(this.assignmentId)
            if (this.interviewMode) data.interviewMode = this.interviewMode.key

            return data
        },

        whereQuery() {
            const and = []
            const self = this

            if(this.where.questionnaireId) {
                and.push({questionnaireId: {eq: this.where.questionnaireId.replaceAll('-','')}})

                if(this.where.questionnaireVersion) {
                    and.push({questionnaireVersion: {eq: this.where.questionnaireVersion}})
                }
            }

            if(this.where.status) {
                and.push({ status: {in: JSON.parse(this.status.alias)}})
            }

            if(this.where.interviewMode) {
                and.push({interviewMode: {eq: this.where.interviewMode}})
            }

            if(this.conditions != null && this.conditions.length > 0) {

                var identifyingData = []
                this.conditions.forEach(cond => {
                    if(cond.value == null) return

                    const value_filter = { entity: {variable: {eq: cond.variable}}}
                    const value = isNumber(cond.value) ? cond.value : cond.value.toLowerCase()

                    var field_values = cond.field.split('|')
                    var value_part = {}
                    value_part[field_values[1]] = value
                    value_filter[field_values[0]] = value_part

                    and.push({identifyingData : {some: value_filter}})
                })

            }

            if(this.exposedValuesFilter != null) {
                and.push(this.exposedValuesFilter)
            }

            if(this.responsibleId) {
                and.push({
                    or: [
                        { responsibleName: {eq: this.responsibleId.value }},
                        { supervisorName: {eq: this.responsibleId.value }},
                    ]})
            }

            if(this.unactiveDateStart) {
                and.push({ updateDateUtc: {gte: this.unactiveDateStart}})
            }

            if(this.unactiveDateEnd) {
                and.push({ updateDateUtc: {lte: this.unactiveDateEnd}})
            }

            if(this.assignmentId) {
                and.push({ assignmentId: {eq: parseInt(this.assignmentId) }})
            }

            return and
        },

        queryString() {
            const query = Object.assign({}, this.where)

            const conditions = this.conditions

            if(conditions.length > 0) {
                query.conditions = conditionToQueryString(conditions)
            }

            return query
        },
    },

    methods: {
        questionFilterChanged(conditions) {
            this.conditions = conditions
            this.reloadTableAndSaveRoute()
        },
        changeExposedValuesFilter(exposedValuesFilter) {
            this.exposedValuesFilter = exposedValuesFilter
            this.reloadTableAndSaveRoute()
        },

        togglePrefield() {
            this.isVisiblePrefilledColumns = !this.isVisiblePrefilledColumns
            return false
        },

        getFilteredToDelete() {
            return this.getFilteredItems(function(item) {
                var value = item.actionFlags.indexOf('CANBEDELETED') >= 0
                return !isNaN(value) && value
            })
        },

        getFilteredToCapi() {
            return this.getFilteredItems(function(item) {
                var value = item.actionFlags.indexOf('CANCHANGETOCAPI') >= 0
                return !isNaN(value) && value
            })

        },

        getFilteredToCawi() {
            return this.getFilteredItems(function(item) {
                var value = item.actionFlags.indexOf('CANCHANGETOCAWI') >= 0
                return !isNaN(value) && value
            })
        },

        getFilteredToAssign() {
            return this.getFilteredItems(function(item) {
                var value =  item.actionFlags.indexOf('CANBEREASSIGNED') >= 0
                return !isNaN(value) && value
            })
        },
        getFilteredToApprove() {
            return this.getFilteredItems(function(item) {
                var value = item.actionFlags.indexOf('CANBEAPPROVED') >= 0
                return !isNaN(value) && value
            })
        },
        getFilteredToReject() {
            return this.getFilteredItems(function(item) {
                var value =  item.actionFlags.indexOf('CANBEREJECTED') >= 0
                return !isNaN(value) && value
            })
        },
        getFilteredToUnApprove() {
            return this.getFilteredItems(function(item) {
                var value =  item.actionFlags.indexOf('CANBEUNAPPROVEDBYHQ') >= 0
                return !isNaN(value) && value
            })
        },
        isNeedShowAssignInterviewers() {
            return (
                this.arrayFilter(this.getFilteredToReject(), function(item) {
                    return item.isNeedInterviewerAssign
                }).length > 0
            )
        },
        CountReceivedByInterviewerItems() {
            return this.getFilteredItems(function(item) {
                return item.receivedByInterviewerAtUtc != null
            }).length
        },
        questionnaireSelected(newValue) {
            this.questionnaireId = newValue

            if(newValue != null && newValue.versions != null && newValue.versions.length == 1) {
                this.questionnaireVersion = newValue.versions[0]
            }
            else {
                this.questionnaireVersion = null
            }
            this.conditions = []
            this.queryExposedVariables = { logicalOperator : 'all', children : [] }
        },

        questionnaireVersionSelected(newValue) {
            this.questionnaireVersion = newValue
            this.conditions = []
            this.queryExposedVariables = { logicalOperator : 'all', children : [] }
        },

        userSelected(newValue) {
            this.responsibleId = newValue
        },

        statusSelected(newValue) {
            this.status = newValue
        },

        inteviewModeSelected(newValue) {
            this.interviewMode = newValue
        },

        viewInterview() {
            var id = this.selectedRowWithMenu.id
            window.location = this.config.interviewReviewUrl + '/' + id.replace(/-/g, '')
        },

        arrayFilter: function(array, predicate) {
            array = array || []
            var result = []
            for (var i = 0, j = array.length; i < j; i++) if (predicate(array[i], i)) result.push(array[i])
            return result
        },

        assign() {
            const self = this

            var filteredItems = this.getFilteredToAssign()

            if (!this.isReassignReceivedByInterviewer) {
                filteredItems = this.arrayFilter(filteredItems, function(item) {
                    return item.receivedByInterviewerAtUtc === null
                })
            }

            if (filteredItems.length == 0) {
                this.$refs.assignModal.hide()
                return
            }

            var commands = this.arrayMap(
                map(filteredItems, interview => {
                    return interview.id
                }),
                function(rowId) {
                    var item = {
                        InterviewId: rowId,
                        InterviewerId:
                            self.newResponsibleId.iconClass === 'interviewer' ? self.newResponsibleId.key : null,
                        SupervisorId:
                            self.newResponsibleId.iconClass === 'supervisor' ? self.newResponsibleId.key : null,
                    }
                    return JSON.stringify(item)
                }
            )

            var command = {
                type: self.config.isSupervisor ? 'AssignInterviewerCommand' : 'AssignResponsibleCommand',
                commands: commands,
            }

            this.executeCommand(
                command,
                function() {},
                function() {
                    self.$refs.assignModal.hide()
                    self.newResponsibleId = null
                    self.reloadTable()
                }
            )
        },

        assignInterview() {
            this.newResponsibleId = null
            this.$refs.assignModal.modal({keyboard: false})
        },

        getFilteredItems(filterPredicat) {
            if (this.$refs.table == undefined) return []

            var selectedItems = this.$refs.table.table.rows({selected: true}).data()

            if (selectedItems.length !== 0 && selectedItems[0] != null)
                return this.arrayFilter(selectedItems, filterPredicat)

            return this.selectedRowWithMenu == null ? [] : this.arrayFilter([this.selectedRowWithMenu], filterPredicat)
        },

        approveInterviews() {
            const self = this
            var filteredItems = this.getFilteredToApprove()

            if (!this.isApproveReceivedByInterviewer) {
                filteredItems = this.arrayFilter(filteredItems, function(item) {
                    return item.receivedByInterviewerAtUtc === null
                })
            }

            if (filteredItems.length == 0) {
                this.$refs.approveModal.hide()
                return
            }

            var command = this.getCommand(
                self.config.isSupervisor ? 'ApproveInterviewCommand' : 'HqApproveInterviewCommand',
                map(filteredItems, interview => {
                    return interview.id
                }),
                this.statusChangeComment
            )

            this.executeCommand(
                command,
                function() {},
                function() {
                    self.$refs.approveModal.hide()
                    self.reloadTable()
                }
            )
        },
        approveInterview() {
            this.statusChangeComment = null
            this.$refs.approveModal.modal()
        },

        rejectInterviews() {
            const self = this

            var filteredItems = this.getFilteredToReject()

            if (filteredItems.length == 0) {
                this.$refs.rejectModal.hide()
                return
            }

            if (!self.config.isSupervisor) {
                var command = null

                if (self.newResponsibleId == null)
                {
                    command = this.getCommand(
                        'HqRejectInterviewCommand',
                        map(filteredItems, interview => {
                            return interview.id
                        }),
                        this.statusChangeComment
                    )
                }
                else if (self.newResponsibleId.iconClass === 'interviewer')
                {
                    var rejToIntCommands = this.arrayMap(
                        map(filteredItems, interview => {
                            return interview.id
                        }),
                        function(rowId) {
                            var item = {
                                InterviewId: rowId,
                                InterviewerId: self.newResponsibleId.key,
                                Comment: self.statusChangeComment,
                            }
                            return JSON.stringify(item)
                        }
                    )

                    command = {
                        type: 'HqRejectInterviewToInterviewerCommand',
                        commands: rejToIntCommands,
                    }
                }
                else if (self.newResponsibleId.iconClass === 'supervisor')
                {
                    var rejToSvCommands = this.arrayMap(
                        map(filteredItems, interview => {
                            return interview.id
                        }),
                        function(rowId) {
                            var item = {
                                InterviewId: rowId,
                                SupervisorId: self.newResponsibleId.key,
                                Comment: self.statusChangeComment,
                            }
                            return JSON.stringify(item)
                        }
                    )

                    command = {
                        type: 'HqRejectInterviewToSupervisorCommand',
                        commands: rejToSvCommands,
                    }
                }

                this.executeCommand(
                    command,
                    function() {},
                    function() {
                        self.$refs.rejectModal.hide()
                        self.reloadTable()
                    }
                )
            } else {
                if (self.newResponsibleId == null) {
                    var cmd = this.getCommand(
                        'RejectInterviewCommand',
                        map(filteredItems, interview => {
                            return interview.id
                        }),
                        this.statusChangeComment
                    )

                    this.executeCommand(
                        cmd,
                        function() {},
                        function() {
                            self.$refs.rejectModal.hide()
                            self.reloadTable()
                        }
                    )
                }
                else if (self.newResponsibleId != null) {
                    var commands = this.arrayMap(
                        map(filteredItems, interview => {
                            return interview.id
                        }),
                        function(rowId) {
                            var item = {
                                InterviewId: rowId,
                                InterviewerId: self.newResponsibleId.key,
                                Comment: self.statusChangeComment,
                            }
                            return JSON.stringify(item)
                        }
                    )

                    var rejectCommand = {
                        type: 'RejectInterviewToInterviewerCommand',
                        commands: commands,
                    }

                    this.executeCommand(
                        rejectCommand,
                        function() {},
                        function() {
                            self.$refs.rejectModal.hide()
                            self.reloadTable()
                        }
                    )

                    return
                }

                self.$refs.rejectModal.hide()
            }
        },

        rejectInterview() {
            this.statusChangeComment = null
            this.newResponsibleId = null
            this.rejectToNewResponsible = false

            this.$refs.rejectModal.modal({
                keyboard: false,
            })
        },

        arrayMap: function(array, mapping) {
            array = array || []
            var result = []
            for (var i = 0, j = array.length; i < j; i++) result.push(mapping(array[i], i))
            return result
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
                .done(function(data) {
                    if (onSuccess !== undefined) onSuccess(data)
                })
                .fail(function(jqXhr, textStatus, errorThrown) {
                    if (jqXhr.status === 401) {
                        location.reload()
                    }
                    //display error
                })
                .always(function() {
                    if (onDone !== undefined) onDone()
                })
        },

        getCommand(commandName, Ids, comment) {
            var commands = this.arrayMap(Ids, function(rowId) {
                var item = {InterviewId: rowId, Comment: comment}
                return JSON.stringify(item)
            })

            var command = {
                type: commandName,
                commands: commands,
            }

            return command
        },

        unapproveInterviews() {
            const self = this
            var filteredItems = this.getFilteredToUnApprove()

            if (filteredItems.length == 0) {
                this.$refs.unapproveModal.hide()
                return
            }

            var command = this.getCommand(
                'UnapproveByHeadquarterCommand',
                map(filteredItems, interview => {
                    return interview.id
                })
            )

            this.executeCommand(
                command,
                function() {},
                function() {
                    self.$refs.unapproveModal.hide()
                    self.reloadTable()
                }
            )
        },

        unapproveInterview() {
            this.$refs.unapproveModal.modal({keyboard: false})
        },

        deleteInterviews() {
            const self = this
            var filteredItems = this.getFilteredToDelete()
            if (filteredItems.length == 0) {
                this.$refs.deleteModal.hide()
                return
            }

            var command = this.getCommand(
                'DeleteInterviewCommand',
                map(filteredItems, interview => {
                    return interview.id
                })
            )

            this.executeCommand(
                command,
                function() {},
                function() {
                    self.$refs.deleteModal.hide()
                    self.reloadTable()
                }
            )
        },

        deleteInterview() {
            this.$refs.deleteModal.modal({keyboard: false})
        },

        changeToCAWI() {
            this.$refs.modalChangeToCAWI.modal({keyboard: false})
        },

        changeToCAPI() {
            this.$refs.modalChangeToCAPI.modal({keyboard: false})
        },

        changeInterviewMode(filteredItems, mode) {
            const self = this

            const commands = map(filteredItems, i => {
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
                function() {},
                function() {
                    self.reloadTable()
                }
            )
        },

        newResponsibleSelected(newValue) {
            this.newResponsibleId = newValue
        },

        async showStatusHistory() {
            var self = this
            const statusHistoryList = await this.$http.post(this.config.api.interviewStatuses, {
                interviewId: this.selectedRowWithMenu.id,
            })

            if (statusHistoryList.data.length != 0) {
                $('#statustable').dataTable({
                    paging: false,
                    ordering: false,
                    info: false,
                    searching: false,
                    retrieve: true,
                    columns: [
                        {data: 'statusHumanized'},
                        {
                            data: 'date',
                            render: function(data, type, row) {
                                return moment
                                    .utc(data)
                                    .local()
                                    .format('MMM DD, YYYY HH:mm')
                            },
                        },
                        {
                            data: 'responsible',
                            render: function(data, type, row) {
                                var resultString = '<span class="' + lowerCase(row.responsibleRole) + '">'
                                resultString += data
                                resultString += '</span>'
                                return resultString
                            },
                        },
                        {
                            data: 'assignee',
                            render: function(data, type, row) {
                                var resultString = '<span class="' + lowerCase(row.assigneeRole) + '">'
                                resultString += data
                                resultString += '</span>'
                                return resultString
                            },
                        },
                        {data: 'comment'},
                    ],
                })

                var table = $('#statustable').dataTable()

                table.fnClearTable()
                table.fnAddData(statusHistoryList.data)
                table.fnDraw()

                self.$refs.statusHistory.modal({keyboard: false})
            }
        },

        contextMenuItems({rowData, rowIndex}) {
            const menu = []
            const self = this

            self.selectedRowWithMenu = rowData

            menu.push({
                name: self.$t('Pages.InterviewerHq_OpenInterview'),
                callback: () => {
                    window.location = `${self.config.interviewReviewUrl}/${rowData.id}`
                },
            })

            menu.push({
                name: self.$t('Common.ShowStatusHistory'),
                callback: () => self.showStatusHistory(),
            })

            if (rowData.responsibleRole === 'INTERVIEWER') {
                menu.push({
                    name: self.$t('Common.OpenResponsiblesProfile'),
                    callback: () => (window.location = self.config.profileUrl + '/' + rowData.responsibleId),
                })
            }

            menu.push({
                name: self.$t('Common.OpenAssignment'),
                callback: () => (window.location = `${self.config.assignmentsUrl}/${rowData.assignmentId}`),
            })

            if (!self.config.isObserving) {
                menu.push({
                    className: 'context-menu-separator context-menu-not-selectable',
                })

                const canBeAssigned =  rowData.actionFlags.indexOf('CANBEREASSIGNED') >= 0
                menu.push({
                    name: self.$t('Common.Assign'),
                    className: canBeAssigned ? 'primary-text' : '',
                    callback: () => self.assignInterview(),
                    disabled: !canBeAssigned,
                })

                const canBeApproved = rowData.actionFlags.indexOf('CANBEAPPROVED') >= 0
                menu.push({
                    name: self.$t('Common.Approve'),
                    className: canBeApproved ? 'success-text' : '',
                    callback: () => self.approveInterview(),
                    disabled: !canBeApproved,
                })

                const canBeRejected = rowData.actionFlags.indexOf('CANBEREJECTED') >= 0
                menu.push({
                    name: self.$t('Common.Reject'),
                    className: canBeRejected ? 'error-text' : '',
                    callback: () => self.rejectInterview(),
                    disabled: !canBeRejected,
                })

                if(rowData.actionFlags.indexOf('CANCHANGETOCAPI') >= 0) {
                    menu.push({
                        name: self.$t('Common.ChangeToCAPI'),
                        callback: () => self.changeToCAPI(),
                    })
                }

                if(rowData.actionFlags.indexOf('CANCHANGETOCAWI') >= 0) {
                    menu.push({
                        name: self.$t('Common.ChangeToCAWI'),
                        callback: () => self.changeToCAWI(),
                    })
                }

                if (!self.config.isSupervisor) {
                    menu.push({
                        name: self.$t('Common.Unapprove'),
                        callback: () => self.unapproveInterview(),
                        disabled: rowData.actionFlags.indexOf('CANBEUNAPPROVEDBYHQ') < 0,
                    })

                    menu.push({
                        className: 'context-menu-separator context-menu-not-selectable',
                    })

                    const canBeDeleted = rowData.actionFlags.indexOf('CANBEDELETED') >= 0
                    menu.push({
                        name: self.$t('Common.Delete'),
                        className: canBeDeleted ? 'error-text' : '',
                        callback: () => self.deleteInterview(),
                        disabled: !canBeDeleted,
                    })
                }

            }

            return menu
        },

        resetSelection() {
            this.selectedRows.splice(0, this.selectedRows.length)
        },

        clearAssignmentFilter() {
            this.assignmentId = null
        },

        formatNumber(value) {
            if (value == null || value == undefined) return value
            var language =
                (navigator.languages && navigator.languages[0]) || navigator.language || navigator.userLanguage
            return value.toLocaleString(language)
        },
        startWatchers(props, watcher) {
            var iterator = prop => this.$watch(prop, watcher)

            props.forEach(iterator, this)
        },

        reloadTable() {
            this.isLoading = true
            this.selectedRows.splice(0, this.selectedRows.length)

            if (this.$refs.table) {
                this.$refs.table.reload()
            }
        },

        reloadTableAndSaveRoute() {
            this.reloadTable()
            this.addParamsToQueryString()
        },

        addParamsToQueryString() {
            const query = Object.assign({} , this.queryString)

            if (!isEqual(this.$route.query, query)) {
                this.$router.push({ query })
                    .catch(() => {})
            }
        },

        async loadResponsibleIdByName(onDone) {
            if (this.$route.query.responsibleName != undefined) {
                const requestParams = assign(
                    {
                        query: this.$route.query.responsibleName,
                        pageSize: 1,
                        cache: false,
                        showArchived: true,
                        showLocked: true,
                    },
                    this.ajaxParams
                )

                const response = await this.$http.get(this.config.api.responsible, {params: requestParams})

                onDone(
                    response.data.options.length > 0 && response.data.options[0].value == this.$route.query.responsibleName
                        ? response.data.options[0].key
                        : undefined
                )
            } else onDone()
        },

        loadQuestionnaireId(onDone) {
            const questionnaireId = this.$route.query.questionnaireId
            const version = this.$route.query.questionnaireVersion

            onDone(questionnaireId, version)
        },

        initPageFilters() {
            const self = this
            const query = this.$route.query

            this.unactiveDateStart = query.unactiveDateStart
            this.unactiveDateEnd = query.unactiveDateEnd
            this.assignmentId = query.assignmentId

            if (query.status != null) {
                self.status = self.statuses.find(o => o.key === query.status)
            }

            if(query.mode != null) {
                self.interviewMode = self.interviewModes.find(o => o.key == query.mode)
            }

            self.loadQuestionnaireId((questionnaireId, version) => {
                if (questionnaireId != null) {
                    self.questionnaireId = self.$config.model.questionnaires.find(q => q.key == questionnaireId)
                    if (version != null && self.questionnaireId != null) {
                        self.questionnaireVersion = self.questionnaireId.versions.find(v => v.key == version)

                        if(query.conditions != null) {
                            self.conditions = queryStringToCondition(flatten([query.conditions]))
                        }
                    } else {
                        if(version == null && self.questionnaireId.versions.length == 1) {
                            self.questionnaireVersionSelected(self.questionnaireId.versions[0])
                        }
                    }
                }
            })

            self.loadResponsibleIdByName(responsibleId => {
                if (responsibleId != null)
                    self.responsibleId = {key: responsibleId, value: query.responsibleName}
                else
                    self.responsibleId = null

                self.startWatchers(
                    ['responsibleId',
                        'questionnaireId',
                        'status',
                        'assignmentId',
                        'questionnaireVersion',
                        'interviewMode'],
                    self.reloadTableAndSaveRoute.bind(self)
                )
            })
        },
    },

    mounted() {
        this.initPageFilters()
    },

    watch: {
        '$route'(to) {
            if(!isEqual(to.query, this.queryString)) {
                this.initPageFilters()
                this.reloadTable()
            }
        },
    },
}
</script>
