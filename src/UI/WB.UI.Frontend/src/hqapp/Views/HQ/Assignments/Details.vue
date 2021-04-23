<template>
    <main>
        <div class="container">
            <div class="row">
                <div class="page-header">
                    <ol class="breadcrumb">
                        <li>
                            <a href="../Assignments">{{$t('MainMenu.Assignments')}}</a>
                        </li>
                    </ol>
                </div>
            </div>
        </div>
        <div class="container-fluid">
            <div class="row">
                <div class="col-sm-6">
                    <h3>{{$t('Assignments.AssignmentHistory')}}</h3>
                    <DataTables
                        ref="table"
                        :tableOptions="tableOptions"
                        :noSearch="true"
                        :noPaging="false"
                        :wrapperClass=" { 'table-wrapper': true }"></DataTables>
                </div>

                <div class="col-sm-6">
                    <h3>
                        {{$t('Assignments.AssignmentInfo')}}
                        <a
                            v-if="model.webMode"
                            :href="webInterviewUrl"
                            target="_blank">
                            <span
                                :title="$t('Assignments.StartWebInterview')"
                                class="glyphicon glyphicon-link"/>
                        </a>
                        <span v-if="this.model.isArchived"
                            class="label label-default">{{$t('Common.Archived')}}</span>
                    </h3>
                    <table class="table table-striped table-bordered">
                        <tbody>
                            <tr>
                                <td class="text-nowrap">
                                    {{$t('Assignments.AssignmentId')}}
                                </td>
                                <td>{{model.id}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{$t('Assignments.Questionnaire')}}
                                </td>
                                <td>{{model.questionnaire.title}} ({{$t('Assignments.QuestionnaireVersion', {version: model.questionnaire.version} )}})</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{$t("Common.Responsible")}}
                                </td>
                                <td v-if="isInterviewerResponsible">
                                    <span class="interviewer">
                                        <a
                                            v-bind:href="interviewerProfileUrl">{{model.responsible.name}}</a>
                                    </span>
                                </td>
                                <td v-else>
                                    <span class="supervisor">{{model.responsible.name}}</span>
                                </td>
                            </tr>
                            <tr v-if="model.isHeadquarters">
                                <td class="text-nowrap">
                                    {{$t("Assignments.Size")}}
                                </td>
                                <td>{{quantity}}</td>
                            </tr>
                            <tr v-if="model.isHeadquarters">
                                <td class="text-nowrap">
                                    {{$t("Assignments.Count")}}
                                </td>
                                <td>
                                    <a v-bind:href="interviewsUrl">
                                        {{model.interviewsProvided}}
                                        <span class="glyphicon glyphicon-link"/>
                                    </a>
                                </td>
                            </tr>
                            <tr v-if="!model.isHeadquarters">
                                <td class="text-nowrap">
                                    {{$t("Assignments.InterviewsNeeded")}}
                                </td>
                                <td>{{interviewsCount}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{$t("Assignments.IdentifyingQuestions")}}
                                </td>
                                <td>
                                    <div
                                        v-bind:key="question.id"
                                        v-for="question in model.identifyingData"
                                        class="overview-item">
                                        <div class="item-content">
                                            <h4>
                                                <span>{{ question.title }}</span>
                                            </h4>
                                            <div class="answer">
                                                <div>{{question.answer}}</div>
                                            </div>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{$t("Assignments.UpdatedAt")}}
                                </td>
                                <td>{{updatedDate}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{$t("Assignments.CreatedAt")}}
                                </td>
                                <td>{{createdDate}}</td>
                            </tr>
                            <tr>
                                <td
                                    class="text-nowrap">{{$t("Assignments.IsAudioRecordingEnabled")}}</td>
                                <td>{{isAudioRecordingEnabled}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{$t("Assignments.Email")}}
                                </td>
                                <td>{{model.email}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{this.$t("Assignments.Password")}}
                                </td>
                                <td>{{model.password}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{$t("Assignments.ReceivedByTablet")}}
                                </td>
                                <td>{{isReceivedByTablet}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{this.$t("Assignments.WebMode")}}
                                </td>
                                <td>{{isWebMode}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{this.$t("Assignments.DetailsComments")}}
                                </td>
                                <td>{{model.comments}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">
                                    {{$t("Common.CalendarEvent")}}
                                </td>
                                <td>
                                    <div>
                                        {{calendarEventTime}}
                                    </div>
                                    <div v-html="calendarEventComment">

                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </main>
</template>


<script>
import Vue from 'vue'
import {DateFormats, convertToLocal} from '~/shared/helpers'
import moment from 'moment-timezone'
import {escape} from 'lodash'

export default {
    computed: {
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
            return this.model.isAudioRecordingEnabled ? this.$t('Common.Yes') : this.$t('Common.No')
        },
        isReceivedByTablet() {
            return this.model.receivedByTabletAtUtc != null
                ? moment.utc(this.model.receivedByTabletAtUtc).local().format(DateFormats.dateTimeInList)
                : this.$t('Common.No')
        },
        isWebMode() {
            return this.model.webMode === false ? this.$t('Common.Capi') : this.$t('Common.Cawi')
        },
        interviewsCount() {
            if (this.model.quantity == null || this.model.quantity < 0) return this.$t('Assignments.Unlimited')

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
            return this.model.quantity == null ? this.$t('Assignments.Unlimited') : this.model.quantity
        },
        calendarEventTime() {
            return this.model.calendarEvent != null
                ? convertToLocal(this.model.calendarEvent.startUtc, this.model.calendarEvent.startTimezone)
                : ''
        },
        calendarEventComment() {
            if (this.model.calendarEvent == null)
                return ''

            return this.model.calendarEvent.comment == null || this.model.calendarEvent.comment == ''
                ? this.$t('Assignments.NoComment')
                : escape(this.model.calendarEvent.comment).replaceAll('\n', '<br/>')
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
                                let createdText = self.$t('Assignments.Action_Created_Responsible', {
                                    responsible: data.Responsible,
                                })
                                if (data.comment) {
                                    createdText +=
                                        '<br/>' + self.$t('Assignments.Action_Created_Comment', {comment: escape(data.Comment)})
                                }
                                return createdText
                            }
                            case 'AudioRecordingChanged':
                                if (data.AudioRecording) {
                                    return self.$t('Assignments.Action_AudioRecordingChanged_True')
                                } else {
                                    return self.$t('Assignments.Action_AudioRecordingChanged_False')
                                }
                            case 'Reassigned': {
                                let result = self.$t('Assignments.Action_Reassigned_To', {
                                    newResponsible: data.NewResponsible,
                                })
                                if (data.Comment) {
                                    result += '<br/>'
                                    result += self.$t('Assignments.Action_Reassigned_To_Comment', {
                                        comment: escape(data.Comment),
                                    })
                                }
                                return result
                            }
                            case 'QuantityChanged':
                                if (data.Quantity == null) {
                                    return self.$t('Assignments.Action_QuantityChanged_To_Unlimited')
                                }
                                return self.$t('Assignments.Action_QuantityChanged_To', {quantity: data.Quantity})
                            case 'WebModeChanged':
                                if (data.WebMode) {
                                    return self.$t('Assignments.Action_WebModeChanged_True')
                                } else {
                                    return self.$t('Assignments.Action_WebModeChanged_False')
                                }
                        }
                        return ''
                    },
                },
            ]

            var tableOptions = {
                rowId: function(row) {
                    return `row_${row.Id}`
                },
                deferLoading: 0,
                columns,
                ordering: false,
                ajax: {
                    url: `${this.$hq.basePath}api/v1/assignments/${this.model.id}/history`,
                    type: 'GET',
                    contentType: 'application/json',
                    dataSrc: function ( responseJson ) {
                        responseJson.recordsTotal = responseJson.RecordsFiltered
                        responseJson.recordsFiltered = responseJson.RecordsFiltered
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
