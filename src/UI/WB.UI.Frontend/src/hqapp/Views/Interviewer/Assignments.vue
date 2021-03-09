<template>
    <HqLayout :title="title">
        <DataTables ref="table"
            :tableOptions="tableOptions"
            :contextMenuItems="contextMenuItems"></DataTables>

        <ModalFrame ref="editCalendarModal"
            :title="$t('Common.EditCalendarEvent')">
            <form onsubmit="return false;">

                <div class="form-group">
                    <DatePicker :config="datePickerConfig"
                        :value="selectedDate">
                    </DatePicker>
                    <div  v-if="dateInPast">
                        <span class="text-danger">{{ $t("Assignments.DateFromPast") }}</span>
                    </div>
                </div>

                <div class="form-group">
                    <label class="control-label"
                        for="commentsId">
                        {{ $t("Assignments.Comments") }}
                    </label>
                    <textarea
                        control-id="commentsId"
                        v-model="editCalendarComment"
                        :placeholder="$t('Assignments.EnterComments')"
                        name="comments"
                        rows="6"
                        maxlength="500"
                        class="form-control"/>
                </div>
            </form>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-primary"
                    role="confirm"
                    @disable="saveDisabled"
                    @click="updateCalendarEvent">
                    {{ $t("Common.Save") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                    role="cancel">{{ $t("Common.Cancel") }}</button>
                <button
                    type="button"
                    class="btn btn-danger pull-right"
                    role="delete"
                    v-if="calendarEventId != null"
                    @click="deleteCalendarEvent">
                    {{ $t("Common.Delete") }}</button>
            </div>
        </ModalFrame>
    </HqLayout>
</template>

<script>
import {DateFormats, convertToLocal} from '~/shared/helpers'
import {updateCalendarEvent, addAssignmentCalendarEvent, deleteCalendarEvent } from './calendarEventsHelper'
import moment from 'moment-timezone'
import {map, join, escape } from 'lodash'

import _sanitizeHtml from 'sanitize-html'
const sanitizeHtml = text => _sanitizeHtml(text,  { allowedTags: [], allowedAttributes: [] })

export default {
    data() {
        return {
            editCalendarComment: null,
            newCalendarStart : null,
            newCalendarStarTimezone : null,
            calendarEventId : null,
            calendarAssinmentId : null,
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
                    return `row${row.id}`
                },
                deferLoading: 0,
                order: [[4, 'desc']],
                columns: this.getTableColumns(),
                ajax: {
                    url: this.$config.model.assignmentsEndpoint,
                    type: 'GET',
                    contentType: 'application/json',
                },
                select: {
                    style: 'multi',
                    selector: 'td>.checkbox-filter',
                },
                sDom: 'rf<"table-with-scroll"t>ip',
            }
        },
        selectedDate(){
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

                    if(start != null && start != self.newCalendarStart){
                        self.newCalendarStart = start
                    }
                },
            }
        },
        dateInPast(){
            return moment(this.selectedDate) < moment()
        },
        saveDisabled(){
            return !this.newCalendarStart
        },
    },

    methods: {
        reload() {
            this.$refs.table.reload()
        },
        contextMenuItems({rowData}) {
            return [
                {
                    name: this.$t('Assignments.CreateInterview'),
                    callback: () => this.$store.dispatch('createInterview', rowData.id),
                },
                {
                    name: this.$t('Common.EditCalendarEvent'),
                    className: 'primary-text',
                    callback: () => this.editCalendarEvent(rowData.id, rowData.calendarEvent),
                },
            ]
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
                    class: 'prefield-column first-identifying last-identifying sorting_disabled visible',
                    orderable: false,
                    searchable: false,
                    render(data) {
                        var questionsWithTitles = map(data, function(question) {
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
                    render: function(data) {
                        var date = moment.utc(data)
                        return date.local().format(DateFormats.dateTimeInList)
                    },
                },
                {
                    data: 'comments',
                    name: 'Comments',
                    title: this.$t('Assignments.DetailsComments'),
                    searchable: false,
                    orderable: true,
                },
                {
                    data: 'calendarEvent',
                    title: this.$t('Common.CalendarEvent'),
                    orderable: false,
                    searchable: false,
                    render: function(data) {
                        if(data != null && data.startUtc != null) {
                            var hasComment = !(data.comment == null || data.comment == '')
                            return '<span data-toggle="tooltip" title="'
                                + ( hasComment ? escape(data.comment) : self.$t('Assignments.NoComment'))
                                + '">'
                                + convertToLocal(data.startUtc, data.startTimezone)
                                + ( hasComment ? ('<br/>' + escape(data.comment)).replaceAll('\n', '<br/>') : '')
                                + '</span>'
                        }
                        return ''
                    },
                    width: '180px',
                },
            ]

            return columns
        },
        deleteCalendarEvent() {
            const self = this
            this.$refs.editCalendarModal.hide()

            deleteCalendarEvent(self.$apollo, {
                'publicKey' : self.calendarEventId == null ? null : self.calendarEventId.replaceAll('-',''),
                workspace: self.$store.getters.workspace,
            }, self.reload)
        },

        editCalendarEvent(assignmentId, calendarEvent) {
            this.calendarAssinmentId = assignmentId
            this.calendarEventId = calendarEvent?.publicKey
            this.editCalendarComment = calendarEvent?.comment
            this.newCalendarStart = calendarEvent?.startUtc ?? moment().add(1, 'days').hours(10).startOf('hour').format(DateFormats.dateTime)
            this.newCalendarStarTimezone = calendarEvent?.startTimezone
            this.$refs.editCalendarModal.modal({keyboard: false})
        },
        updateCalendarEvent() {
            const self = this

            this.$refs.editCalendarModal.hide()
            const startDate = moment(self.newCalendarStart).format('YYYY-MM-DD[T]HH:mm:ss.SSSZ')

            const variables = {
                newStart : startDate,
                comment : self.editCalendarComment,
                startTimezone: moment.tz.guess(),
                workspace: self.$store.getters.workspace,
            }


            if(self.calendarEventId != null){
                variables.publicKey = self.calendarEventId.replaceAll('-',''),
                updateCalendarEvent(self.$apollo, variables, self.reload)
            }
            else{
                variables.assignmentId = self.calendarAssinmentId,
                addAssignmentCalendarEvent(self.$apollo, variables, self.reload)
            }
        },
    },
}
</script>
