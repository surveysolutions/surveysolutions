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
                    @click="updateCalendarEvent">
                    {{ $t("Common.Save") }}</button>
                <button
                    type="button"
                    class="btn btn-danger"
                    role="delete"
                    v-if="calendarEventId != null"
                    @click="deleteCalendarEvent">
                    {{ $t("Common.Delete") }}</button>

                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                    role="cancel">{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>
    </HqLayout>
</template>

<script>
import {DateFormats} from '~/shared/helpers'
import moment from 'moment'
import {map, join} from 'lodash'

export default {
    data() {
        return {
            editCalendarComment: null,
            newCalendarDate : null,
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
            return this.newCalendarDate
        },
        datePickerConfig() {
            var self = this
            return {
                mode: 'single',
                minDate: 'today',
                enableTime: true,
                wrap: true,
                static: true,
                onChange: (selectedDates, dateStr, instance) => {
                    const start = selectedDates.length > 0 ? moment(selectedDates[0]).format(DateFormats.dateTime) : null

                    if(start != null && start != self.newCalendarDate){
                        self.newCalendarDate = start
                    }
                },
            }
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
                        return '(ver. ' + row.questionnaireId.version + ') ' + row.questionnaireTitle
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
                            return question.title + ': ' + question.answer
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
                    render(data) {
                        if(data != null && data.start != null)
                            return '<span data-toggle="tooltip" title="'
                                + (data.comment == null ? '(no comment)' : data.comment)
                                + '">'
                                + moment(data.start)
                                    .format(DateFormats.dateTimeInList)
                                + '</span>'
                    },
                    width: '180px',
                },
            ]

            return columns
        },
        deleteCalendarEvent() {
            const self = this
            this.$refs.editCalendarModal.hide()
            self.$store.dispatch('deleteCalendarEvent',
                {
                    id: self.calendarEventId,
                    callback: self.reload,
                })
        },

        editCalendarEvent(assignmentId, calendarEvent) {
            this.calendarAssinmentId = assignmentId
            this.calendarEventId = (calendarEvent != null && calendarEvent != undefined) ? calendarEvent.publicKey : null
            this.editCalendarComment = (calendarEvent != null && calendarEvent != undefined) ? calendarEvent.comment : null
            this.newCalendarDate = (calendarEvent != null && calendarEvent != undefined) ? calendarEvent.start : null
            this.$refs.editCalendarModal.modal({keyboard: false})
        },
        updateCalendarEvent() {
            const self = this

            this.$refs.editCalendarModal.hide()

            self.$store.dispatch('saveCalendarEvent', {
                assignmentId : self.calendarAssinmentId,
                id : self.calendarEventId,
                newDate : self.newCalendarDate,
                comment : self.editCalendarComment,

                callback: self.reload,
            })
        },
    },
}
</script>
