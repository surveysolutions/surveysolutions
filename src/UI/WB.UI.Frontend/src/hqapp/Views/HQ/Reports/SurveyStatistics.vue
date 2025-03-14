<template>
    <HqLayout has-filter :title="$t('MainMenu.SurveyStatistics')" :subtitle="$t('Pages.SurveyStatisticsDescription')">
        <div class="clearfix">
            <div class="col-sm-8">
                <h4>{{ this.filter.questionnaire == null ? $t('Common.AllQuestionnaires') :
                    this.filter.questionnaire.Title}},
                    {{ this.filter.version == null ? $t('Common.AllVersions').toLowerCase() : ('ver. ' +
                        this.filter.version)}}</h4>
            </div>
        </div>
        <div>
            <div class="row">
                <div class="col-md-6">
                    <QuestionDetail :question="filter.question" :title="$t('Reports.Question')"></QuestionDetail>
                </div>
                <div class="col-md-6">
                    <QuestionDetail :question="filter.condition" :title="$t('Reports.ConditionQuestion')">
                    </QuestionDetail>
                </div>
            </div>
        </div>

        <template v-slot:filters>
            <Filters>
                <SurveyStatisticsFilter @input="filterChanged" @mounted="filtersLoaded" :isSupervisor="isSupervisor" />
            </Filters>
        </template>
        <div class="alert-warning" v-if="warnings.length > 0">{{
            $t('Reports.QuestionnaireCompatibilityIssues_UnknownAnswer')}}
        </div>

        <DataTables ref="table" v-if="isFiltersLoaded" noSearch exportable multiorder hasTotalRow noSelect
            :tableOptions="tableOptions" :pageLength="isPivot ? this.filter.condition.Answers.length : 15"
            @ajaxComplete="onTableReload" :addParamsToRequest="addFilteringParams"></DataTables>
    </HqLayout>
</template>

<script>
import QuestionDetail from './QuestionDetail'
import SurveyStatisticsFilter from './SurveyStatisticsFilter'
import { map, round, concat } from 'lodash'

export default {
    components: {
        SurveyStatisticsFilter,
        QuestionDetail,
    },

    data() {
        return {
            isFiltersLoaded: false,
            filter: {
                questionnaire: null,
                question: null,
                answers: null,
                condition: null,
                expandTeams: false,
                min: this.min,
                max: this.max,
                pivot: false,
            },
            warnings: [],
            status: {
                isRunning: false,
                lastRefresh: null,
            },
            // eslint-disable-next-line vue/no-reserved-keys
            _timer: null,
        }
    },

    methods: {
        filterChanged(filter) {
            Object.keys(filter).forEach(key => {
                this.filter[key] = filter[key]
            })

            this.reloadTable()
        },

        reloadTable() {
            if (this.isFiltersLoaded && this.$refs.table != null) {
                this.$refs.table.reload()
            }
        },

        filtersLoaded() {
            this.isFiltersLoaded = true
        },

        addFilteringParams(data) {
            data.questionnaireId = this.filter.questionnaireId
            data.question = this.filter.questionId
            data.expandTeams = this.filter.expandTeams
            data.min = this.filter.min
            data.max = this.filter.max
            data.statuses = this.filter.status
            data.version = this.filter.version

            if (this.filter.condition != null) {
                data.conditionalQuestion = this.filter.condition.Id
                data.pivot = this.filter.pivot

                if (!data.pivot || this.filter.conditionAnswers.length > 0) {
                    data.condition = map(this.filter.conditionAnswers, 'Answer')
                }
            }
        },

        onTableReload(data) {
            this.warnings = data.warnings || []
        },
    },

    computed: {
        isSupervisor() {
            return this.$config.model.isSupervisor
        },

        isPivot() {
            return (this.filter.pivot == true || this.filter.pivot == 'true') && this.filter.condition != null
        },

        conditionAnswers() {
            if (this.condition == null) return 15

            return this.condition.Answers.length
        },

        numericColumns() {
            if (this.filter.question != null) {
                if (this.filter.question.Type == 'Numeric') {
                    return [
                        { name: this.$t('Reports.Count'), data: 'count' },
                        { name: this.$t('Reports.Average'), data: 'average' },
                        { name: this.$t('Reports.Median'), data: 'median' },
                        { name: this.$t('Reports.Sum'), data: 'sum' },
                        { name: this.$t('Reports.Min'), data: 'min' },
                        { name: this.$t('Reports.Percentile05'), data: 'percentile_05' },
                        { name: this.$t('Reports.Percentile50'), data: 'percentile_50' },
                        { name: this.$t('Reports.Percentile95'), data: 'percentile_95' },
                        { name: this.$t('Reports.Max'), data: 'max' },
                    ].map(a => ({
                        class: 'type-numeric',
                        title: a.name,
                        data: a.data,
                        name: a.data,
                        orderable: true,
                        render(data) {
                            return round(data, 3)
                        },
                    }))
                }
            }

            return []
        },

        totalColumn() {
            if (this.filter.question == null) return []

            if (this.filter.question.HasTotal || this.isPivot) {
                return [
                    {
                        class: 'type-numeric',
                        title: this.$t('Pages.Total'),
                        data: 'total',
                        name: 'total',
                        orderable: true,
                    },
                ]
            }

            return []
        },

        categoriesColumns() {
            if (this.filter.question == null) return []

            const answers = this.filter.question.Answers
            return answers == null
                ? []
                : answers.map(a => ({
                    class: 'type-numeric',
                    title: a.Text,
                    data: a.Column,
                    name: a.Column,
                    orderable: true,
                }))
        },

        identifyingColumns() {
            if (this.isPivot) {
                return [
                    {
                        data: 'variable',
                        name: 'variable',
                        title: this.filter.condition.Label || this.filter.condition.VariableName,
                        orderable: true,
                    },
                ]
            }

            const columns = []

            if (!this.isSupervisor) {
                columns.push({
                    data: 'TeamLead',
                    name: 'TeamLead',
                    title: this.$t('Report.COLUMN_TEAMS'),
                    orderable: true,
                    visible: !this.isSupervisor,
                })
            }

            if (this.isSupervisor || this.filter.expandTeams) {
                columns.push({
                    data: 'Responsible',
                    name: 'Responsible',
                    title: this.$t('Report.COLUMN_TEAM_MEMBER'),
                    orderable: true,
                })
            }

            return columns
        },

        tableColumns() {
            return [
                ...this.identifyingColumns,
                ...this.numericColumns,
                ...this.categoriesColumns,
                ...this.totalColumn
            ]
        },

        tableOptions() {
            return {
                columns: this.tableColumns,
                order: [],
                ajax: {
                    url: this.$hq.Report.SurveyStatistics.Uri,
                    type: 'GET',
                    contentType: 'application/json',
                },
                responsive: false,
                sDom: 'rf<"table-with-scroll"t>ip',
            }
        },
    },
}
</script>
