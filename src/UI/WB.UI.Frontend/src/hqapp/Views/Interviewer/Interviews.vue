<template>
    <HqLayout :title="title"
        :hasFilter="true">
        <Filters slot="filters">
            <FilterBlock :title="$t('Common.Questionnaire')">
                <Typeahead
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
                    control-id="questionnaireVersion"
                    data-vv-name="questionnaireVersion"
                    data-vv-as="questionnaireVersion"
                    :placeholder="$t('Common.AllVersions')"
                    :disabled="questionnaireId == null "
                    :value="questionnaireVersion"
                    :values="questionnaireId == null ? [] : questionnaireId.versions"
                    v-on:selected="questionnaireVersionSelected"/>
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
        </Filters>

        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            :contextMenuItems="contextMenuItems" />

        <Confirm ref="confirmRestart"
            id="restartModal"
            slot="modals">
            {{ $t("Pages.InterviewerHq_RestartConfirm") }}
            <FilterBlock>
                <div class="form-group">
                    <div class="field">
                        <input
                            class="form-control with-clear-btn"
                            type="text"
                            v-model="restart_comment"/>
                    </div>
                </div>
            </FilterBlock>
        </Confirm>

        <Confirm
            ref="confirmDiscard"
            id="discardConfirm"
            slot="modals">{{ $t("Pages.InterviewerHq_DiscardConfirm") }}</Confirm>

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
import moment from 'moment-timezone'
import {addOrUpdateCalendarEvent, deleteCalendarEvent } from './calendarEventsHelper'
import {map, join, toNumber, filter, escape} from 'lodash'
import gql from 'graphql-tag'
import _sanitizeHtml from 'sanitize-html'
const sanitizeHtml = text => _sanitizeHtml(text,  { allowedTags: [], allowedAttributes: [] })


const query = gql`query interviews($workspace: String!, $order: [InterviewSort!], $skip: Int, $take: Int, $where: InterviewsFilter) {
  interviews(workspace: $workspace, order: $order, skip: $skip, take: $take, where: $where) {
    totalCount
    filteredCount
    nodes {
      id
      key
      assignmentId
      updateDateUtc
      status
      receivedByInterviewerAtUtc
      actionFlags
      calendarEvent {
          publicKey
          comment
          startUtc
          startTimezone
      }
      identifyingData {
        entity {
          questionText
          label
        }
        value
      }
    }
  }
}`

export default {
    data() {
        return {
            restart_comment: null,
            questionnaireId: null,
            questionnaireVersion: null,
            assignmentId: null,
            editCalendarComment: null,
            newCalendarStart : null,
            newCalendarStarTimezone : null,
            calendarEventId : null,
            calendarInterviewId : null,
            calendarInterviewKey : null,
            calendarAssinmentId : null,
            draw: 0,
        }
    },

    watch: {
        questionnaireId: function() {
            this.reload()
        },
        questionnaireVersion: function() {
            this.reload()
        },
        assignmentId: function() {
            this.reload()
        },
    },

    computed: {
        title() {
            return this.$config.title
        },
        where() {
            const data = {}

            if (this.questionnaireId) data.questionnaireId = this.questionnaireId.key
            if (this.questionnaireVersion) data.questionnaireVersion = toNumber(this.questionnaireVersion.key)
            if (this.assignmentId) data.assignmentId = toNumber(this.assignmentId)

            return data
        },
        whereQuery() {
            const and = []

            if(this.where.questionnaireId) {
                and.push({questionnaireId: {eq : this.where.questionnaireId.replaceAll('-','')}})

                if(this.where.questionnaireVersion) {
                    and.push({questionnaireVersion: {eq : this.where.questionnaireVersion}})
                }
            }
            if(this.where.assignmentId){
                and.push({assignmentId: {eq : this.where.assignmentId}})
            }

            and.push({ status : {in: this.$config.model.statuses}})

            return and
        },
        tableOptions() {
            const self = this
            return {
                rowId: 'id',
                order: [[3, 'desc']],
                deferLoading: 0,
                columns: this.getTableColumns(),
                pageLength: 20,

                ajax(data, callback, _) {
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
                        where.and.push(
                            {
                                or: [
                                    { key: {startsWith: search.toLowerCase()}},
                                    { identifyingData: {some: {valueLowerCase: {startsWith: search.toLowerCase()}}}},
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
                    })
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
    },

    methods: {
        questionnaireSelected(newValue) {
            this.questionnaireId = newValue
            this.questionnaireVersion = null
        },

        questionnaireVersionSelected(newValue) {
            this.questionnaireVersion = newValue
        },

        reload() {
            this.$refs.table.reload()
        },

        editCalendarEvent(interviewId, interviewKey, assignmentId, calendarEvent) {
            this.calendarInterviewId = interviewId
            this.calendarInterviewKey = interviewKey
            this.calendarAssinmentId = assignmentId
            this.calendarEventId = calendarEvent?.publicKey
            this.editCalendarComment = calendarEvent?.comment
            this.newCalendarStart = calendarEvent?.startUtc
            this.newCalendarStarTimezone = calendarEvent?.startTimezone
            this.$refs.editCalendarModal.modal({keyboard: false})
        },

        updateCalendarEvent() {
            const self = this

            this.$refs.editCalendarModal.hide()

            const startDate = moment(self.newCalendarStart).format('YYYY-MM-DD[T]HH:mm:ss.SSSZ')

            const variables = {
                interviewId : self.calendarInterviewId,
                interviewKey: self.calendarInterviewKey,
                assignmentId : self.calendarAssinmentId,
                publicKey : self.calendarEventId == null ? null : self.calendarEventId.replaceAll('-',''),
                newStart : startDate,
                comment : self.editCalendarComment,
                startTimezone: moment.tz.guess(),
                workspace: self.$store.getters.workspace,
            }

            addOrUpdateCalendarEvent(self.$apollo, variables, self.reload)

        },
        deleteCalendarEvent() {
            const self = this
            this.$refs.editCalendarModal.hide()

            deleteCalendarEvent(self.$apollo, {
                'publicKey' : self.calendarEventId == null ? null : self.calendarEventId.replaceAll('-',''),
                workspace: self.$store.getters.workspace,
            }, self.reload)

        },
        contextMenuItems({rowData, rowIndex}) {
            const menu = []
            const self = this

            if (rowData.actionFlags.indexOf('CANBEOPENED') >= 0) {
                menu.push({
                    name: self.$t('Pages.InterviewerHq_OpenInterview'),
                    callback: () => self.$store.dispatch('openInterview', rowData.id),
                })
            }

            if (rowData.actionFlags.indexOf('CANBEDELETED') >= 0) {
                menu.push({
                    name: self.$t('Pages.InterviewerHq_DiscardInterview'),
                    callback() {
                        self.discardInterview(rowData.id, rowIndex)
                    },
                })
            }

            if (rowData.actionFlags.indexOf('CANBERESTARTED') >= 0) {
                menu.push({
                    name: self.$t('Pages.InterviewerHq_RestartInterview'),
                    callback: () => {
                        self.$refs.table.disableRow(rowIndex)
                        self.restartInterview(rowData.id)
                    },
                })
            }

            const canCalendarBeEdited =  rowData.actionFlags.indexOf('CANBEOPENED') >= 0
            menu.push({
                name: self.$t('Common.EditCalendarEvent'),
                className: canCalendarBeEdited ? 'primary-text' : '',
                callback: () => self.editCalendarEvent(rowData.id, rowData.key, rowData.assignmentId, rowData.calendarEvent),
                disabled: !canCalendarBeEdited,
            })

            return menu
        },

        discardInterview(interviewId, rowIndex) {
            const self = this
            this.$refs.confirmDiscard.promt(ok => {
                if (ok) {
                    self.$refs.table.disableRow(rowIndex)
                    self.$store.dispatch('discardInterview', {
                        interviewId,
                        callback: self.reload,
                    })
                }
            })
        },

        restartInterview(interviewId) {
            const self = this

            self.$refs.confirmRestart.promt(ok => {
                if (ok) {
                    $.post(
                        this.$config.model.interviewerHqEndpoint + '/RestartInterview/' + interviewId,
                        {comment: self.restart_comment},
                        () => {
                            self.restart_comment = ''
                            self.$store.dispatch('openInterview', interviewId)
                        }
                    )
                } else {
                    self.$refs.table.reload()
                }
            })
        },

        addFilteringParams(data) {
            data.statuses = this.$config.model.statuses

            data.questionnaireId = (this.questionnaireId || {}).key
            data.questionnaireVersion = (this.questionnaireVersion || {}).key

            if (this.assignmentId) {
                data.assignmentId = this.assignmentId
            }
        },

        getTableColumns() {
            const self = this
            const columns = [
                {
                    data: 'key',
                    name: 'Key',
                    title: this.$t('Common.InterviewKey'),
                    orderable: true,
                    searchable: true,
                    width: '180px',
                },
                {
                    data: 'assignmentId',
                    name: 'AssignmentIdKey',
                    title: this.$t('Common.Assignment'),
                    orderable: false,
                    searchable: false,
                    width: '50px',
                },
                {
                    data: 'identifyingData',
                    title: this.$t('Assignments.IdentifyingQuestions'),
                    class: 'prefield-column first-identifying last-identifying sorting_disabled visible',
                    orderable: false,
                    searchable: false,
                    render(data) {
                        const delimiter = self.mode == 'dense'

                        var entitiesWithTitles = map(filter(data, d => d.value != null && d.value != ''), node => {
                            return `${sanitizeHtml(node.entity.label || node.entity.questionText)}: <strong>${sanitizeHtml(node.value)}</strong>`
                        })

                        const dom = join(entitiesWithTitles, ', ')
                        return dom
                    },
                    responsivePriority: 4,
                },
                {
                    data: 'updateDateUtc',
                    title: this.$t('Assignments.UpdatedAt'),
                    searchable: false,
                    render(data) {
                        return moment
                            .utc(data)
                            .local()
                            .format(DateFormats.dateTimeInList)
                    },
                    width: '180px',
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

        clearAssignmentFilter() {
            this.assignmentId = null
        },
    },
}
</script>
