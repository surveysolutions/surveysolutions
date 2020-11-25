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
    </HqLayout>
</template>

<script>
import {DateFormats} from '~/shared/helpers'
import moment from 'moment'
import {map, join, toNumber, filter} from 'lodash'
import gql from 'graphql-tag'
import _sanitizeHtml from 'sanitize-html'
const sanitizeHtml = text => _sanitizeHtml(text,  { allowedTags: [], allowedAttributes: [] })


const query = gql`query interviews($workspace: String!, $order: InterviewSort, $skip: Int, $take: Int, $where: InterviewFilter) {
  interviews(workspace: $workspace, order_by: $order, skip: $skip, take: $take, where: $where) {
    totalCount
    filteredCount
    nodes {
      id
      key
      assignmentId
      updateDate
      status
      receivedByInterviewerAtUtc
      actionFlags
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
                and.push({questionnaireId: this.where.questionnaireId})

                if(this.where.questionnaireVersion) {
                    and.push({questionnaireVersion: this.where.questionnaireVersion})
                }
            }
            if(this.where.assignmentId){
                and.push({assignmentId: this.where.assignmentId})
            }

            and.push({ status_in: this.$config.model.statuses})

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
                        AND: [...self.whereQuery],
                    }

                    const search = data.search.value

                    if(search && search != '') {
                        where.AND.push(
                            {
                                OR: [
                                    { key_starts_with: search.toLowerCase() },
                                    { identifyingData_some: {
                                        valueLowerCase_starts_with: search.toLowerCase(),
                                    },
                                    }],
                            })
                    }

                    if(where.AND.length > 0) {
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
                            return `${sanitizeHtml(node.entity.label || node.entity.questionText)}: <strong>${node.value}</strong>`
                        })

                        const dom = join(entitiesWithTitles, ', ')
                        return dom
                    },
                    responsivePriority: 4,
                },
                {
                    data: 'updateDate',
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
            ]

            return columns
        },

        clearAssignmentFilter() {
            this.assignmentId = null
        },
    },
}
</script>
