<template>
    <HqLayout
        :title="$t('Pages.SurveysAndStatuses_Overview')"
        :subtitle="$t('Pages.SurveysAndStatuses_HeadquartersDescription')"
        :hasFilter="true">
        <div slot="subtitle">
            <p v-if="questionnaireId != null">
                <a id="lnkBackToQuestionnaires"
                    :href="$config.model.selfUrl">{{$t('Reports.ToAllQuestionnaires')}}</a>
            </p>
        </div>
        <Filters slot="filters">
            <FilterBlock :title="$t('Pages.SurveysAndStatuses_SupervisorTitle')">
                <Typeahead
                    control-id="responsibleSelector"
                    ref="responsibleIdControl"
                    data-vv-name="responsibleId"
                    data-vv-as="responsible"
                    :placeholder="$t('Strings.AllTeams')"
                    :value="responsible"
                    v-on:selected="selectResponsible"
                    :fetch-url="$config.model.responsiblesUrl"/>
            </FilterBlock>
        </Filters>

        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            :addParamsToRequest="addFilteringParams"
            :no-search="true"
            exportable
            :hasTotalRow="questionnaireId != null"></DataTables>
    </HqLayout>
</template>
<script>

import {formatNumber} from './helpers'
import routeSync from '~/shared/routeSync'
import { escape } from 'lodash'

export default {
    mixins: [routeSync],
    data() {
        return {
            responsible: null,
        }
    },
    mounted() {
        if(this.$route.query.responsible) {
            this.responsible = {key: '1', value: this.$route.query.responsible}
        }
    },
    methods: {
        addFilteringParams(data) {
            if (this.responsible) {
                data.responsibleName = this.responsible.value
            }

            if(this.questionnaireId) {
                data.questionnaireId = this.questionnaireId
            }
        },
        selectResponsible(value) {
            this.responsible = value
            this.onChange(s => {
                s.responsible = (value || {}).value == null ? null : value.value
            })
        },
        reloadTable() {
            if(this.$refs.table != null) {
                this.$refs.table.reload()
            }
        },
        getLinkToInterviews(data, row, status) {
            const value = escape(data)
            if (value == 0)
                return '<span>0</span>'
            const responsibleName = (this.responsible || {}).value
            const url = `${this.$config.model.interviewsUrl}?questionnaireId=${row.questionnaireId}&questionnaireVersion=${row.questionnaireVersion || ''}&responsibleName=${encodeURI(responsibleName || '')}&status=${status.toUpperCase()}`
            return `<a href=${url}>${formatNumber(value)}</a>`
        },
    },
    watch: {
        responsible() {
            this.reloadTable()
        },
    },
    computed: {
        tableOptions() {
            var self = this
            let columns = [
                {
                    data: 'questionnaireTitle',
                    title: this.$t('Reports.SurveyName'),
                    className: 'type-title changed-recently',
                    name: 'QuestionnaireTitle',
                    render(data, type, row) {
                        if (!data) {
                            return self.$t('Strings.AllQuestionnaires')
                        }

                        if(self.questionnaireId != null) {

                            return escape(data)
                        }

                        var url = window.location.href
                        if (url.indexOf('?') > 0)
                            url += '&'
                        else
                            url += '?'
                        url += `questionnaireId=${row.questionnaireId}`

                        return `<a href=${url}>${escape(data)}</a>`
                    },
                },
                {
                    data: 'supervisorAssignedCount',
                    className: 'type-numeric',
                    name: 'SupervisorAssignedCount',
                    title: this.$t('Reports.SupervisorAssigned'),
                    render(data, type, row) {
                        return self.getLinkToInterviews(
                            data,
                            row,
                            'SupervisorAssigned')
                    },
                },
                {
                    data: 'interviewerAssignedCount',
                    className: 'type-numeric',
                    name: 'InterviewerAssignedCount',
                    title: this.$t('Reports.InterviewerAssigned'),
                    render(data, type, row) {
                        return self.getLinkToInterviews(
                            data,
                            row,
                            'InterviewerAssigned')
                    },
                },
                {
                    data: 'completedCount',
                    name: 'CompletedCount',
                    className: 'type-numeric',
                    title: this.$t('Reports.Completed'),
                    render(data, type, row) {
                        return self.getLinkToInterviews(
                            data,
                            row,
                            'Completed')
                    },
                },
                {
                    data: 'rejectedBySupervisorCount',
                    name: 'RejectedBySupervisorCount',
                    className: 'type-numeric',
                    title: this.$t('Reports.RejectedBySupervisor'),
                    render(data, type, row) {
                        return self.getLinkToInterviews(
                            data,
                            row,
                            'RejectedBySupervisor')
                    },
                },
                {
                    data: 'approvedBySupervisorCount',
                    name: 'ApprovedBySupervisorCount',
                    className: 'type-numeric',
                    title: this.$t('Reports.ApprovedBySupervisor'),
                    render(data, type, row) {
                        return self.getLinkToInterviews(
                            data,
                            row,
                            'ApprovedBySupervisor')
                    },
                },
                {
                    data: 'rejectedByHeadquartersCount',
                    name: 'RejectedByHeadquartersCount',
                    className: 'type-numeric',
                    title: this.$t('Reports.RejectedByHQ'),
                    render(data, type, row) {
                        return self.getLinkToInterviews(
                            data,
                            row,
                            'RejectedByHeadquarters')
                    },
                },
                {
                    data: 'approvedByHeadquartersCount',
                    name: 'ApprovedByHeadquartersCount',
                    className: 'type-numeric',
                    title: this.$t('Reports.ApprovedByHQ'),
                    render(data, type, row) {
                        return self.getLinkToInterviews(
                            data,
                            row,
                            'ApprovedByHeadquarters')
                    },
                },
                {
                    data: 'totalCount',
                    name: 'TotalCount',
                    className: 'type-numeric',
                    title: this.$t('Common.Total'),
                    render(data, type, row) {
                        return self.getLinkToInterviews(
                            data,
                            row,
                            '')
                    },
                },
            ]

            if(self.questionnaireId) {
                columns.splice(1, 0,
                    {
                        data: 'questionnaireVersion',
                        name: 'QuestionnaireVersion',
                        className: 'type-numeric version centered-italic',
                        title: this.$t('Reports.TemplateVersion'),
                    })
            }

            return {
                deferLoading: 0,
                columns: columns,
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: 'GET',
                    contentType: 'application/json',
                },
                sDom: 'rf<"table-with-scroll"t>ip',
                order: [[0, 'desc']],
                bInfo: false,
                footer: true,
                responsive: false,
                createdRow: function(row) {
                    $(row)
                        .find('td:eq(0)')
                        .attr('nowrap', 'nowrap')
                },
            }
        },
        questionnaireId() {
            return this.$route.query.questionnaireId
        },
        queryString() {
            return {
                questionnaireId: this.questionnaireId,
                responsible: this.responsibleName,
            }
        },
    },
}
</script>
