<template>
    <main class="web-interview">
        <div class="container-fluid">
            <div class="row">
                <div class="panel panel-details contains-action-buttons">
                    <div class="panel-body clearfix">
                        <div class="about-questionnaire clearfix">
                            <div class="about-questionnaire-details clearfix">
                                <ul class="main-info-column list-unstyled pull-left">
                                    <li id="detailsInfo_interviewKeyListItem">
                                        {{ $t('Assignments.AssignmentId') }}:
                                        {{ model.id }}
                                    </li>
                                    <li id="detailsInfo_qusetionnaireTitleListItem" class="questionnaire-title">
                                        [ver.{{ model.questionnaire.version }}]
                                        {{ model.questionnaire.title }}
                                    </li>
                                </ul>
                                <ul class="list-unstyled pull-left table-info">
                                    <li id="detailsInfo_lastUpdatedListItem">
                                        <span class="data-label">{{
                                            this.$t(
                                                'Assignments.CreatedAt',
                                            )
                                        }}:</span>
                                        <span class="data">{{
                                            createdDate
                                        }}</span>
                                    </li>
                                    <li id="detailsInfo_responsibleListItem">
                                        <span class="data-label">{{
                                            this.$t('Details.Responsible')
                                        }}:
                                        </span>
                                        <span v-if="isInterviewerResponsible" class="data">
                                            <a v-bind:href="interviewerProfileUrl" class="interviewer">{{
                                                model.responsible.name }}</a>
                                        </span>
                                        <span v-else class="data supervisor">{{
                                            model.responsible.name
                                        }}</span>
                                    </li>
                                </ul>
                                <ul class="list-unstyled pull-left table-info">
                                    <li id="detailsInfo_lastUpdatedListItem">
                                        <span class="data-label">{{
                                            this.$t('Details.LastUpdated')
                                        }}:</span>
                                        <span class="data">{{ updatedDate }}</span>
                                    </li>
                                    <li>
                                        <span class="data-label">{{ $t("Common.CalendarEvent") }}:</span>
                                        <span class="data" data-toggle="tooltip" v-if="calendarEventComment != null"
                                            :title="((calendarEventComment == null || calendarEventComment == '') ? this.$t('Assignments.NoComment') : calendarEventComment)">
                                            {{ calendarEventTime }}
                                        </span>
                                    </li>
                                </ul>
                            </div>
                        </div>
                        <div class="questionnaire-details-actions clearfix">
                            <div class="buttons-container">
                                <div class="dropdown aside-menu" v-if="showMoreButton">
                                    <button type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"
                                        class="btn btn-link">
                                        <span></span>
                                    </button>
                                    <ul class="dropdown-menu">
                                        <li v-if="!isArchived">
                                            <a href="#" @click="assignSelected">
                                                {{
                                                    $t("Common.Assign")
                                                }}
                                            </a>
                                        </li>
                                        <li v-if="isHeadquarters && !isArchived">
                                            <a href="#" @click="closeSelected">
                                                {{
                                                    $t("Assignments.Close")
                                                }}
                                            </a>
                                        </li>
                                        <li v-if="isHeadquarters && !isArchived">
                                            <a href="#" @click="archiveSelected">
                                                {{
                                                    $t("Assignments.Archive")
                                                }}
                                            </a>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!--  -->

                <div class="col-sm-6" style="padding-top: 30px;">
                    <h3>{{ $t('Assignments.AssignmentHistory') }}</h3>
                    <DataTables ref="assignmentHistoryTable" :tableOptions="tableOptions" noSearch :noPaging="false"
                        :wrapperClass="{ 'table-wrapper': true }"></DataTables>
                </div>

                <div class="col-sm-6" style="padding-top: 30px;">
                    <h3>
                        {{ $t('Assignments.AssignmentInfo') }}
                        <a v-if="model.webMode && model.invitationToken" :href="webInterviewUrl" target="_blank">
                            <span :title="$t('Assignments.StartWebInterview')" class="glyphicon glyphicon-link" />
                        </a>
                        <span v-if="this.model.isArchived" class="label label-default">{{ $t('Common.Archived') }}</span>
                    </h3>
                    <table class="table table-striped table-bordered">
                        <tbody>
                            <tr v-if="model.isHeadquarters">
                                <td class="text-nowrap">
                                    {{ $t('Assignments.Size') }}
                                </td>
                                <td>{{ quantity }}</td>
                            </tr>
                            <tr v-if="model.isHeadquarters">
                                <td class="text-nowrap">
                                    {{ $t('Assignments.Count') }}
                                </td>
                                <td>
                                    <a v-bind:href="interviewsUrl">
                                        {{ model.interviewsProvided }}
                                        <span class="glyphicon glyphicon-link" />
                                    </a>
                                </td>
                            </tr>
                            <tr v-if="!model.isHeadquarters">
                                <td class="text-nowrap">
                                    {{ $t('Assignments.InterviewsNeeded') }}
                                </td>
                                <td>{{ interviewsCount }}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{ $t('Assignments.IdentifyingQuestions') }}
                                </td>
                                <td>
                                    <div v-bind:key="question.id" v-for="question in model.identifyingData"
                                        class="overview-item">
                                        <div class="item-content">
                                            <h4>
                                                <span>{{
                                                    question.title
                                                }}</span>
                                            </h4>
                                            <div class="answer">
                                                <div v-html="question.answer"></div>
                                            </div>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{
                                        $t(
                                            'Assignments.IsAudioRecordingEnabled',
                                        )
                                    }}
                                </td>
                                <td>{{ isAudioRecordingEnabled }}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{ $t('Assignments.Email') }}
                                </td>
                                <td>{{ model.email }}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{ this.$t('Assignments.Password') }}
                                </td>
                                <td>{{ model.password }}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{ $t('Assignments.ReceivedByTablet') }}
                                </td>
                                <td>{{ isReceivedByTablet }}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{ this.$t('Assignments.WebMode') }}
                                </td>
                                <td>{{ isWebMode }}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{ this.$t('Assignments.DetailsComments') }}
                                </td>
                                <td>{{ model.comments }}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>


                <ModalFrame ref="assignModal" :title="$t('Common.Assign')">
                    <form onsubmit="return false;">
                        <div class="form-group" :class="{ 'has-warning': showWebModeReassignWarning }">
                            <label class="control-label" for="newResponsibleId">{{ $t("Assignments.SelectResponsible")
                            }}</label>
                            <Typeahead control-id="newResponsibleId" :placeholder="$t('Common.Responsible')"
                                :value="newResponsibleId" :ajax-params="{}" @selected="newResponsibleSelected"
                                :fetch-url="config.api.responsible"></Typeahead>
                            <span class="help-block" v-if="showWebModeReassignWarning">
                                {{ $t('Assignments.WebModeReassignToNonInterviewer', { count: 1 }) }}
                            </span>
                        </div>
                        <div class="form-group">
                            <label class="control-label" for="commentsId">
                                {{ $t("Assignments.Comments") }}
                            </label>
                            <textarea control-id="commentsId" v-model="reassignComment"
                                :placeholder="$t('Assignments.EnterComments')" name="comments" rows="6" maxlength="500"
                                class="form-control" />
                        </div>
                    </form>
                    <div slot="actions">
                        <button type="button" class="btn btn-primary" @click="assign" :disabled="!newResponsibleId">{{
                            $t("Common.Assign") }}</button>
                        <button type="button" class="btn btn-link" data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
                    </div>
                </ModalFrame>


            </div>
        </div>
    </main>
</template>

<script>
import Vue from 'vue'
import { DateFormats, convertToLocal } from '~/shared/helpers'
import { RoleNames } from '~/shared/constants'

import moment from 'moment-timezone'
import { escape } from 'lodash'

import '@/assets/css/markup-web-interview.scss'
import '@/assets/css/markup-interview-review.scss'

export default {
    data() {
        return {
            newResponsibleId: null,
            reassignComment: null,
        }
    },
    methods: {
        assignSelected() {
            this.$refs.assignModal.modal({
                keyboard: false,
            })
        },

        async assign() {
            await this.$http.post(this.config.api.assignments + '/Assign', {
                responsibleId: this.newResponsibleId.key,
                comments: this.reassignComment,
                ids: this.selectedRows,
            })

            this.$refs.assignModal.hide()
            this.newResponsibleId = null
            this.reassignComment = null
            //this.reloadTable()
        },

        newResponsibleSelected(newValue) {
            this.newResponsibleId = newValue
        },
    },

    computed: {
        showWebModeReassignWarning() {
            if (!this.newResponsibleId) return false

            return isWebMode && this.newResponsibleId.iconClass !== RoleNames.INTERVIEWER.toLowerCase()
        },

        isWebMode() { return false },
        closeSelected() { },
        archiveSelected() { },

        isHeadquarters() {
            return true
        },
        isArchived() {
            return false
        },
        showMoreButton() {
            return true
        },

        config() {
            return this.$config.model
        },
        model() {
            return this.$config.model
        },
        updatedDate() {
            var date = moment.utc(this.model.updatedAtUtc)
            return date.local().format(DateFormats.dateTime)
        },
        createdDate() {
            var date = moment.utc(this.model.createdAtUtc)
            return date.local().format(DateFormats.dateTime)
        },
        isAudioRecordingEnabled() {
            return this.model.isAudioRecordingEnabled
                ? this.$t('Common.Yes')
                : this.$t('Common.No')
        },
        isReceivedByTablet() {
            return this.model.receivedByTabletAtUtc != null
                ? moment
                    .utc(this.model.receivedByTabletAtUtc)
                    .local()
                    .format(DateFormats.dateTimeInList)
                : this.$t('Common.No')
        },
        isWebMode() {
            return this.model.webMode === false
                ? this.$t('Common.Capi')
                : this.$t('Common.Cawi')
        },
        interviewsCount() {
            if (this.model.quantity == null || this.model.quantity < 0)
                return this.$t('Assignments.Unlimited')

            return this.model.interviewsProvided > this.model.quantity
                ? 0
                : this.model.quantity - this.model.interviewsProvided
        },
        isInterviewerResponsible() {
            return this.model.responsible.role === 'interviewer'
        },
        interviewerProfileUrl() {
            return '../Interviewer/Profile/' + this.model.responsible.id
        },
        interviewsUrl() {
            return '../Interviews?assignmentId=' + this.model.id
        },
        webInterviewUrl() {
            return `../WebInterview/${this.model.invitationToken}/Start`
        },
        quantity() {
            return this.model.quantity == null
                ? this.$t('Assignments.Unlimited')
                : this.model.quantity
        },
        calendarEventTime() {
            return this.model.calendarEvent != null
                ? convertToLocal(
                    this.model.calendarEvent.startUtc,
                    this.model.calendarEvent.startTimezone,
                )
                : ''
        },
        calendarEventComment() {
            if (this.model.calendarEvent == null) return ''

            return this.model.calendarEvent.comment == null ||
                this.model.calendarEvent.comment == ''
                ? this.$t('Assignments.NoComment')
                : escape(this.model.calendarEvent.comment).replaceAll(
                    '\n',
                    '<br/>',
                )
        },

        tableOptions() {
            var self = this

            const columns = [
                {
                    data: 'Action',
                    title: self.$t('Assignments.Action'),
                    render(data) {
                        return self.$t('Assignments.Action_' + data)
                    },
                },
                {
                    data: 'ActorName',
                    title: self.$t('Assignments.Actor'),
                },
                {
                    data: 'UtcDate',
                    width: '180px',
                    title: self.$t('Assignments.Date'),
                    render(data) {
                        return moment
                            .utc(data)
                            .local()
                            .format(DateFormats.dateTimeInList)
                    },
                },
                {
                    data: 'AdditionalData',
                    title: self.$t('Assignments.Details_Column'),
                    width: '50%',
                    render(data, type, row) {
                        switch (row.Action) {
                            case 'Created': {
                                let createdText = self.$t(
                                    'Assignments.Action_Created_Responsible',
                                    {
                                        responsible: data.Responsible,
                                    },
                                )
                                if (data.Comment) {
                                    createdText +=
                                        '<br/>' +
                                        self.$t(
                                            'Assignments.Action_Created_Comment',
                                            { comment: escape(data.Comment) },
                                        )
                                }

                                if (data.UpgradedFromId) {
                                    createdText +=
                                        '<br/>' +
                                        self.$t(
                                            'Assignments.Action_UpgradedFrom',
                                            {
                                                id: `<a href='./${data.UpgradedFromId}'>${data.UpgradedFromId}</a>`,
                                            },
                                        )
                                }
                                return createdText
                            }
                            case 'AudioRecordingChanged':
                                if (data.AudioRecording) {
                                    return self.$t(
                                        'Assignments.Action_AudioRecordingChanged_True',
                                    )
                                } else {
                                    return self.$t(
                                        'Assignments.Action_AudioRecordingChanged_False',
                                    )
                                }
                            case 'Reassigned': {
                                let result = self.$t(
                                    'Assignments.Action_Reassigned_To',
                                    {
                                        newResponsible: data.NewResponsible,
                                    },
                                )
                                if (data.Comment) {
                                    result += '<br/>'
                                    result += self.$t(
                                        'Assignments.Action_Reassigned_To_Comment',
                                        {
                                            comment: escape(data.Comment),
                                        },
                                    )
                                }
                                return result
                            }
                            case 'QuantityChanged':
                                if (data.Quantity == null) {
                                    return self.$t(
                                        'Assignments.Action_QuantityChanged_To_Unlimited',
                                    )
                                }
                                return self.$t(
                                    'Assignments.Action_QuantityChanged_To',
                                    { quantity: data.Quantity },
                                )
                            case 'WebModeChanged':
                                if (data.WebMode) {
                                    return self.$t(
                                        'Assignments.Action_WebModeChanged_True',
                                    )
                                } else {
                                    return self.$t(
                                        'Assignments.Action_WebModeChanged_False',
                                    )
                                }
                            case 'ReceivedByTablet':
                                return data.DeviceId
                        }
                        return ''
                    },
                },
            ]

            var tableOptions = {
                rowId: function (row) {
                    return `row_${row.Id}`
                },
                deferLoading: 0,
                columns,
                ordering: false,
                ajax: {
                    url: `${this.$hq.basePath}api/v1/assignments/${this.model.id}/history`,
                    type: 'GET',
                    contentType: 'application/json',
                    dataSrc: function (responseJson) {
                        responseJson.recordsTotal = responseJson.RecordsFiltered
                        responseJson.recordsFiltered =
                            responseJson.RecordsFiltered
                        return responseJson.History
                    },
                },
            }

            return tableOptions
        },
    },
    mounted() {
        Vue.nextTick(() => {
            window.ajustNoticeHeight()
            window.ajustDetailsPanelHeight()
        })
    },
}
</script>
