<template>
    <HqLayout :title="title">
        <DataTables ref="table"
            :tableOptions="tableOptions"
            :contextMenuItems="contextMenuItems"></DataTables>
    </HqLayout>
</template>

<script>
import {DateFormats} from '~/shared/helpers'
import moment from 'moment'
import {map, join} from 'lodash'

export default {
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
    },

    methods: {
        contextMenuItems({rowData}) {
            return [
                {
                    name: this.$t('Assignments.CreateInterview'),
                    callback: () => this.$store.dispatch('createInterview', rowData.id),
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
            ]

            return columns
        },
    },
}
</script>
