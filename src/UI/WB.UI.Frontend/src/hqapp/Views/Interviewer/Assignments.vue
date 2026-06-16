<template>
    <HqLayout :title="title">
        <DataTables ref="table" :tableOptions="tableOptions" :contextMenuItems="contextMenuItems">
        </DataTables>

        <ModalFrame ref="editCalendarModal" :title="$t('Common.EditCalendarEvent')">
            <form onsubmit="return false;">

                <div class="form-group">
                    <DatePicker :config="datePickerConfig" :value="selectedDate">
                    </DatePicker>
                    <div v-if="dateInPast">
                        <span class="text-danger">{{ $t("Assignments.DateFromPast") }}</span>
                    </div>
                </div>

                <div class="form-group">
                    <label class="control-label" for="commentsId">
                        {{ $t("Assignments.Comments") }}
                    </label>
                    <textarea control-id="commentsId" v-model="editCalendarComment"
                        :placeholder="$t('Assignments.EnterComments')" name="comments" rows="6" maxlength="500"
                        class="form-control" />
                </div>
            </form>
            <template v-slot:actions>
                <div>
                    <button type="button" class="btn btn-primary" role="confirm" @disable="saveDisabled"
                        @click="updateCalendarEvent">
                        {{ $t("Common.Save") }}</button>
                    <button type="button" class="btn btn-link" data-bs-dismiss="modal" role="cancel">{{
                        $t("Common.Cancel")
                        }}</button>
                    <button type="button" class="btn btn-danger pull-right" role="delete" v-if="calendarEventId != null"
                        @click="deleteCalendarEvent">
                        {{ $t("Common.Delete") }}</button>
                </div>
            </template>
        </ModalFrame>

        <ModalFrame ref="completeModal" :title="$t('Assignments.CompleteAssignmentTitle')">
            <p>{{ $t('Assignments.CompleteAssignmentMessage') }}</p>
            <form onsubmit="return false;">
                <div class="form-group">
                    <label class="control-label" for="completeCommentId">
                        {{ $t("Assignments.Comments") }}
                    </label>
                    <textarea control-id="completeCommentId" v-model="statusChangeComment"
                        :placeholder="$t('Assignments.EnterComments')" name="comments" rows="4" maxlength="500"
                        autocomplete="off" class="form-control" />
                </div>
            </form>
            <template v-slot:actions>
                <div>
                    <button type="button" class="btn btn-primary" @click="confirmComplete">{{
                        $t("Assignments.Complete") }}</button>
                    <button type="button" class="btn btn-link" data-bs-dismiss="modal">{{ $t("Common.Cancel")
                        }}</button>
                </div>
            </template>
        </ModalFrame>

        <ModalFrame ref="reopenModal" :title="$t('Assignments.ReopenAssignmentTitle')">
            <p>{{ $t('Assignments.ReopenAssignmentMessage') }}</p>
            <form onsubmit="return false;">
                <div class="form-group">
                    <label class="control-label" for="reopenCommentId">
                        {{ $t("Assignments.Comments") }}
                    </label>
                    <textarea control-id="reopenCommentId" v-model="statusChangeComment"
                        :placeholder="$t('Assignments.EnterComments')" name="comments" rows="4" maxlength="500"
                        autocomplete="off" class="form-control" />
                </div>
            </form>
            <template v-slot:actions>
                <div>
                    <button type="button" class="btn btn-primary" @click="confirmReopen">{{
                        $t("Assignments.Reopen") }}</button>
                    <button type="button" class="btn btn-link" data-bs-dismiss="modal">{{ $t("Common.Cancel")
                        }}</button>
                </div>
            </template>
        </ModalFrame>
    </HqLayout>
</template>

<script>
import * as toastr from 'toastr'
import { DateFormats, convertToLocal } from '~/shared/helpers'
import { updateCalendarEvent, addAssignmentCalendarEvent, deleteCalendarEvent } from './calendarEventsHelper'
import moment from 'moment'
import { map, join, escape } from 'lodash-es'

import DOMPurify from 'dompurify'
const sanitizeHtml = text => DOMPurify.sanitize(text, { ALLOWED_TAGS: [], ALLOWED_ATTR: [] })

export default {
    data() {
        return {
            editCalendarComment: null,
            newCalendarStart: null,
            newCalendarStarTimezone: null,
            calendarEventId: null,
            calendarAssinmentId: null,
            statusChangeId: null,
            statusChangeComment: null,
        }
    },

    computed: {
        title() {
            return this.$config.title
        },
        dataTable() {
            return this.$refs.table.table
        },
        tableOptions() {
            return {
                rowId: row => {
                    return `row_${row.id}`
                },
                deferLoading: 0,
                order: [[5, 'desc']],
                columns: this.getTableColumns(),
                ajax: {
                    url: this.$config.model.assignmentsEndpoint,
                    type: 'GET',
                    contentType: 'application/json',
                },
                sDom: 'rf<"table-with-scroll"t>ip',
            }
        },
        selectedDate() {
            return this.newCalendarStart
        },
        datePickerConfig() {
            var self = this
            return {
                mode: 'single',
                enableTime: true,
                wrap: true,
                static: true,
                onChange: (selectedDates, dateStr, instance) => {
                    const start = selectedDates.length > 0 ? moment(selectedDates[0]).format(DateFormats.dateTime) : null

                    if (start != null && start != self.newCalendarStart) {
                        self.newCalendarStart = start
                    }
                },
            }
        },
        dateInPast() {
            return moment(this.selectedDate) < moment()
        },
        saveDisabled() {
            return !this.newCalendarStart
        },
    },

    methods: {
        reload() {
            this.$refs.table.reload()
        },
        contextMenuItems({ rowData }) {
            const items = [
                {
                    name: this.$t('Assignments.CreateInterview'),
                    className: 'assignment-create',
                    disabled: rowData.status === 'Completed',
                    callback: () => this.$store.dispatch('createInterview', rowData.id),
                },
                {
                    name: this.$t('Common.EditCalendarEvent'),
                    className: 'primary-text',
                    disabled: rowData.status === 'Completed',
                    callback: () => this.editCalendarEvent(rowData.id, rowData.calendarEvent),
                },
            ]

            if (rowData.status === 'Open' && this.$config.model.allowInterviewerChangeAssignmentStatus) {
                items.push({
                    name: this.$t('Assignments.Complete'),
                    className: 'primary-text',
                    callback: () => this.openCompleteModal(rowData.id),
                })
            }

            if (rowData.status === 'Completed' && this.$config.model.allowInterviewerChangeAssignmentStatus) {
                items.push({
                    name: this.$t('Assignments.Reopen'),
                    className: 'primary-text',
                    callback: () => this.openReopenModal(rowData.id),
                })
            }

            return items
        },

        openCompleteModal(rowId) {
            this.statusChangeId = rowId
            this.statusChangeComment = null
            this.$refs.completeModal.modal()
        },

        openReopenModal(rowId) {
            this.statusChangeId = rowId
            this.statusChangeComment = null
            this.$refs.reopenModal.modal()
        },

        async confirmComplete() {
            await this.changeAssignmentStatus('Completed', this.$refs.completeModal)
        },

        async confirmReopen() {
            await this.changeAssignmentStatus('Open', this.$refs.reopenModal)
        },

        async changeAssignmentStatus(status, modalRef) {
            if (!this.statusChangeId) return
            try {
                await this.$hq.Assignments.changeStatus(this.statusChangeId, status, this.statusChangeComment || null)
                modalRef.hide()
                this.statusChangeId = null
                this.statusChangeComment = null
                this.reload()
            } catch (error) {
                const msg = error?.response?.data?.message || error?.message || this.$t('Common.Error')
                toastr.error(msg)
            }
        },

        getTableColumns() {
            const self = this

            const columns = [
                {
                    data: 'id',
                    name: 'Id',
                    title: this.$t('Common.Assignment'),
                    responsivePriority: 2,
                    width: '5%',
                },
                {
                    data: 'quantity',
                    name: 'Quantity',
                    class: 'type-numeric',
                    title: this.$t('Assignments.InterviewsNeeded'),
                    orderable: false,
                    searchable: false,
                    width: '11%',
                    render(data, type, row) {
                        if (row.quantity < 0) {
                            return '<span>' + self.$t('Assignments.Unlimited') + '</span>'
                        }

                        return row.interviewsCount > row.quantity ? 0 : row.quantity - row.interviewsCount
                    },
                    defaultContent: '<span>' + this.$t('Assignments.Unlimited') + '</span>',
                },
                {
                    data: 'questionnaireTitle',
                    name: 'QuestionnaireTitle',
                    title: this.$t('Assignments.Questionnaire'),
                    orderable: true,
                    searchable: true,
                    render(data, type, row) {
                        return '(ver. ' + row.questionnaireId.version + ') ' + escape(row.questionnaireTitle)
                    },
                },
                {
                    data: 'identifyingQuestions',
                    title: this.$t('Assignments.IdentifyingQuestions'),
                    className: 'prefield-column first-identifying last-identifying sorting_disabled visible',
                    orderable: false,
                    searchable: false,
                    render(data) {
                        var questionsWithTitles = map(data, function (question) {
                            return question.title + ': ' + sanitizeHtml(question.answer)
                        })
                        return join(questionsWithTitles, ', ')
                    },
                    responsivePriority: 4,
                },
                {
                    data: 'updatedAtUtc',
                    name: 'UpdatedAtUtc',
                    title: this.$t('Assignments.UpdatedAt'),
                    searchable: false,
                    render(data) {
                        var date = moment.utc(data)
                        return date.local().format(DateFormats.dateTimeInList)
                    },
                },
                {
                    data: 'createdAtUtc',
                    name: 'CreatedAtUtc',
                    title: this.$t('Assignments.CreatedAt'),
                    searchable: false,
                    render: function (data) {
                        var date = moment.utc(data)
                        return date.local().format(DateFormats.dateTimeInList)
                    },
                },
                {
                    data: 'receivedByTabletAtUtc',
                    name: 'ReceivedByTabletAtUtc',
                    title: this.$t('Assignments.ReceivedByTablet'),
                    searchable: false,
                    render(data) {
                        if (data)
                            return moment
                                .utc(data)
                                .local()
                                .format(DateFormats.dateTimeInList)
                        return self.$t('Common.No')
                    },
                },
                {
                    data: 'comments',
                    name: 'Comments',
                    title: this.$t('Assignments.DetailsComments'),
                    searchable: false,
                    orderable: true,
                    render(data, type, row) {
                        const parts = []
                        if (data) parts.push(escape(data))
                        if (row.statusComment) parts.push('<em>' + escape(row.statusComment) + '</em>')
                        return parts.join('<br/>')
                    },
                },
                {
                    data: 'calendarEvent',
                    title: this.$t('Common.CalendarEvent'),
                    orderable: false,
                    searchable: false,
                    render: function (data) {
                        if (data != null && data.startUtc != null) {
                            var hasComment = !(data.comment == null || data.comment == '')
                            return '<span data-bs-toggle="tooltip" title="'
                                + (hasComment ? escape(data.comment) : self.$t('Assignments.NoComment'))
                                + '">'
                                + convertToLocal(data.startUtc, data.startTimezone)
                                + (hasComment ? ('<br/>' + escape(data.comment)).replaceAll('\n', '<br/>') : '')
                                + '</span>'
                        }
                        return ''
                    },
                    width: '180px',
                },
                {
                    data: 'status',
                    name: 'Status',
                    title: this.$t('Assignments.Status'),
                    searchable: false,
                    orderable: true,
                    render(data) {
                        const statusMap = {
                            'Open': self.$t('Assignments.StatusOpen'),
                            'Completed': self.$t('Assignments.StatusCompleted'),
                        }
                        return statusMap[data] || data
                    },
                },
            ]

            return columns
        },
        deleteCalendarEvent() {
            const self = this
            this.$refs.editCalendarModal.hide()

            deleteCalendarEvent(self.$apollo, {
                'publicKey': self.calendarEventId == null ? null : self.calendarEventId.replaceAll('-', ''),
                workspace: self.$store.getters.workspace,
            }, self.reload)
        },

        editCalendarEvent(assignmentId, calendarEvent) {
            this.calendarAssinmentId = assignmentId
            this.calendarEventId = calendarEvent?.publicKey
            this.editCalendarComment = calendarEvent?.comment
            this.newCalendarStart = calendarEvent?.startUtc ?? moment().add(1, 'days').hours(10).startOf('hour').format(DateFormats.dateTime)
            this.newCalendarStarTimezone = calendarEvent?.startTimezone
            this.$refs.editCalendarModal.modal({ keyboard: false })
        },
        updateCalendarEvent() {
            const self = this

            this.$refs.editCalendarModal.hide()
            const startDate = moment(self.newCalendarStart).format('YYYY-MM-DD[T]HH:mm:ss.SSSZ')

            const variables = {
                newStart: startDate,
                comment: self.editCalendarComment,
                startTimezone: moment.tz.guess(),
                workspace: self.$store.getters.workspace,
            }


            if (self.calendarEventId != null) {
                variables.publicKey = self.calendarEventId.replaceAll('-', ''),
                    updateCalendarEvent(self.$apollo, variables, self.reload)
            }
            else {
                variables.assignmentId = self.calendarAssinmentId,
                    addAssignmentCalendarEvent(self.$apollo, variables, self.reload)
            }
        },
    },
}
</script>
