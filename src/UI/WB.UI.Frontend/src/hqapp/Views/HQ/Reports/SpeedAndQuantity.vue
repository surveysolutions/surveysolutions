<template>
    <HqLayout :title="title"
        :subtitle="reportDescription"
        :hasFilter="true">
        <div slot="subtitle">
            <a
                v-if="this.model.canNavigateToQuantityBySupervisors"
                :href="getSupervisorsUrl"
                class="btn btn-default">
                <span class="glyphicon glyphicon-arrow-left"></span>
                {{ $t('PeriodicStatusReport.BackToSupervisors') }}
            </a>
        </div>

        <Filters slot="filters">
            <FilterBlock :title="$t('PeriodicStatusReport.InterviewActions')">
                <Typeahead
                    ref="reportTypeControl"
                    control-id="reportTypeId"
                    no-clear
                    data-vv-name="reportTypeId"
                    data-vv-as="reportType"
                    :placeholder="$t('PeriodicStatusReport.InterviewActions')"
                    :value="reportTypeId"
                    :values="this.$config.model.reportTypes"
                    v-on:selected="reportTypeSelected"/>
            </FilterBlock>

            <FilterBlock :title="$t('Common.Questionnaire')">
                <Typeahead
                    ref="questionnaireIdControl"
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
                    ref="questionnaireVersionControl"
                    control-id="questionnaireVersion"
                    data-vv-name="questionnaireVersion"
                    data-vv-as="questionnaireVersion"
                    :placeholder="$t('Common.AllVersions')"
                    :disabled="questionnaireId == null "
                    :value="questionnaireVersion"
                    :values="questionnaireId == null ? [] : questionnaireId.versions"
                    v-on:selected="questionnaireVersionSelected"/>
            </FilterBlock>
            <FilterBlock :title="$t('PeriodicStatusReport.OverTheLast')">
                <Typeahead
                    ref="overTheLast"
                    control-id="overTheLast"
                    no-clear
                    data-vv-name="overTheLast"
                    data-vv-as="overTheLast"
                    :placeholder="$t('PeriodicStatusReport.OverTheLast')"
                    :value="overTheLast"
                    :values="this.$config.model.overTheLasts"
                    v-on:selected="overTheLastSelected"/>
            </FilterBlock>

            <FilterBlock :title="$t('PeriodicStatusReport.PeriodUnit')">
                <Typeahead
                    ref="period"
                    control-id="period"
                    no-clear
                    :placeholder="$t('PeriodicStatusReport.Period')"
                    data-vv-name="period"
                    data-vv-as="period"
                    v-on:selected="periodSelected"
                    :value="period"
                    :values="this.$config.model.periods"></Typeahead>
            </FilterBlock>

            <FilterBlock :title="$t('PeriodicStatusReport.LastDateToShowLabel')">
                <DatePicker :config="datePickerConfig"
                    :value="selectedDate"></DatePicker>
            </FilterBlock>
        </Filters>

        <div class="clearfix">
            <div class="col-sm-8">
                <h4 v-html="selectedQuestionnaireTitle" />
            </div>
        </div>

        <DataTables
            ref="table"
            v-if="mounted"
            :tableOptions="tableOptions"
            :addParamsToRequest="addParamsToRequest"
            @ajaxComplete="onTableReload"
            noPaging
            noSearch
            exportable
            hasTotalRow
            noSelect></DataTables>
    </HqLayout>
</template>

<script>
import moment from 'moment'
import momentDuration from 'moment-duration-format'
import routeSync from '~/shared/routeSync'
import { find } from 'lodash'

export default {
    mixins: [routeSync],

    data() {
        return {
            mounted: false,
            questionnaireId: null,
            questionnaireVersion: null,
            reportTypeId: null,
            overTheLast: null,
            period: null,
            from: null,
            dateFormat: 'YYYY-MM-DD',
            dateTimeRanges: [],
            columns: [],
            loading: {
                report: false,
            },
        }
    },
    mounted() {
        if (this.query.questionnaireId) {
            this.questionnaireId = find(this.$config.model.questionnaires, {key: this.query.questionnaireId})
        }

        if (this.questionnaireId && this.query.questionnaireVersion) {
            this.questionnaireVersion = find(this.questionnaireId.versions, {key: this.query.questionnaireVersion})
        }

        if (this.query.reportType) {
            this.reportTypeId = find(this.model.reportTypes, {key: this.query.reportType})
            if(this.reportTypeId == null) {
                this.reportTypeId = this.model.reportTypes[0]
            }
        } else if (this.reportTypeId == null && this.model.reportTypes.length > 0) {
            var index = this.model.reportTypes
            this.reportTypeId = this.model.reportTypes[0]
        }


        if (this.query.columnCount) {
            this.overTheLast = find(this.model.overTheLasts, {key: this.query.columnCount})
        } else if (this.overTheLast == null && this.model.overTheLasts.length > 6) {
            this.overTheLast = this.model.overTheLasts[6]
        }

        if (this.query.period) {
            this.period = find(this.model.periods, {key: this.query.period})
        } else if (this.period == null && this.model.periods.length > 0) {
            this.period = this.model.periods[0]
        }

        if (this.query.from) {
            this.from = this.query.from
        } else if (this.from == null) {
            this.from = moment().format(this.dateFormat)
        }

        this.loadReportData()
        this.mounted = true
    },

    methods: {
        loadReportData(reloadPage = false) {
            if(reloadPage) return // reload will happen when route change
            if (this.$refs.table) {
                this.$refs.table.reload()
            }
        },

        routeUpdated(route) {
            window.location.reload()
        },

        prepareColumns() {
            var self = this
            var columns = []

            columns.push({
                data: 'responsibleName',
                name: 'Responsible',
                title: self.model.responsibleColumnName,
                orderable: false,
                render: function(data, type, row) {
                    if (data == undefined || row.DT_RowClass == 'total-row') {
                        if (self.model.supervisorId) {
                            return self.$t('Strings.AllInterviewers')
                        } else {
                            return self.$t('Strings.AllTeams')
                        }
                    }

                    if (self.model.canNavigateToQuantityByTeamMember) {
                        const interviewersUrl = self.getInterviewersUrl(row.responsibleId)
                        return `<a href='${interviewersUrl}'">${data}</a>`
                    } else {
                        return `<span>${data}</span>`
                    }
                },
            })

            const count = self.columnsCount
            for (let i = 0; i < count; i++) {
                const index = i
                columns.push({
                    class: 'type-numeric short-row',
                    title: '',
                    data: '',
                    name: `date_${index}`,
                    orderable: false,

                    render: function(data, type, row) {
                        if (row.quantityByPeriod) {
                            var quantity = row.quantityByPeriod[index]
                            return self.renderQuantityValue(quantity)
                        }
                        if (row.speedByPeriod) {
                            var speed = row.speedByPeriod[index]
                            return self.renderSpeedValue(speed)
                        }
                        return '-'
                    },
                })
            }

            columns.push({
                data: 'average',
                name: 'Average',
                title: this.$t('PeriodicStatusReport.Average'),
                orderable: true,
                render: function(data, type, row) {
                    if (row.quantityByPeriod) {
                        return self.renderQuantityValue(data)
                    }
                    if (row.speedByPeriod) {
                        return self.renderSpeedValue(data)
                    }
                    return '-'
                },
            })
            columns.push({
                data: 'total',
                name: 'Total',
                title: this.$t('PeriodicStatusReport.Total'),
                orderable: false,
                render: function(data, type, row) {
                    if (row.quantityByPeriod) {
                        return self.renderQuantityValue(data)
                    }
                    if (row.speedByPeriod) {
                        return self.renderSpeedValue(data)
                    }
                    return '-'
                },
            })

            this.columns = columns
        },

        updateDateColumnsInfo() {

            const table = this.$refs.table.table

            for (let i = 0; i < this.columns.length; i++) {
                const column = table.column(`date_${i}:name`)

                const dateRange = (i >= this.dateTimeRanges.length) ? null : this.dateTimeRanges[i]
                if(dateRange) {
                    column.visible(true)
                    const header = column.header()
                    const date = moment(dateRange.to).format(this.dateFormat)
                    header.title = date
                    header.textContent = date
                } else {
                    column.visible(false)
                }
            }
        },
        renderQuantityValue(value) {
            if (value == null) return '-'

            var formatedValue = this.formatNumber(value)
            return `<span>${formatedValue}</span>`
        },
        renderSpeedValue(value) {
            if (value == null) return '-'

            const duration = moment.duration(value, 'minutes')
            const formated = moment.utc(duration.as('milliseconds')).format('D[d] H[h] mm[m]')
            return `<span>${formated}</span>`
        },
        reportTypeSelected(option) {
            this.reportTypeId = option

            this.onChange(query => {
                query.reportType = this.reportTypeId.key
            })
            this.loadReportData(true)
        },
        questionnaireSelected(option) {
            this.questionnaireId = option
            this.onChange(query => {
                query.questionnaireId = (this.questionnaireId || {}).key
            })
            this.loadReportData(true)
        },
        questionnaireVersionSelected(option) {
            this.questionnaireVersion = option
            this.onChange(query => {
                query.questionnaireVersion = (this.questionnaireVersion || {}).key
            })
            this.loadReportData(true)
        },
        periodSelected(option) {
            this.period = option
            var newVal = this.period.key
            if (newVal === 'd') {
                this.overTheLast = this.model.overTheLasts[6]
            } else if (newVal === 'w') {
                this.overTheLast = this.model.overTheLasts[3]
            } else if (newVal === 'm') {
                this.overTheLast = this.model.overTheLasts[2]
            }
            this.onChange(query => {
                query.period = (this.period || {}).key
            })
            this.onChange(query => {
                query.columnCount = (this.overTheLast || {}).key
            })
            this.prepareColumns()
            this.loadReportData(true)
        },

        overTheLastSelected(option) {
            this.overTheLast = option
            this.onChange(query => {
                query.columnCount = (this.overTheLast || {}).key
            })
            this.prepareColumns()
            this.loadReportData(true)
        },

        addParamsToRequest(requestData) {
            requestData.questionnaireId = (this.questionnaireId || {}).key
            requestData.questionnaireVersion = (this.questionnaireVersion || {}).key
            requestData.reportType = this.reportTypeId.key
            requestData.columnCount = this.overTheLast.key
            requestData.period = this.period.key
            requestData.from = this.from
            requestData.supervisorId = this.model.supervisorId
            requestData.timezoneOffsetMinutes = new Date().getTimezoneOffset()
            requestData.pageIndex = 1
            requestData.pageSize = 50000
        },
        formatNumber(value) {
            if (value == null || value == undefined) return value
            var language =
                (navigator.languages && navigator.languages[0]) || navigator.language || navigator.userLanguage
            return value.toLocaleString(language)
        },
        getInterviewersUrl(supervisorId) {
            return (
                this.model.interviewersUrl +
                '?questionnaireId=' +
                ((this.questionnaireId || {}).key || '') +
                '&questionnaireVersion=' +
                ((this.questionnaireVersion || {}).key || '') +
                '&from=' +
                this.from +
                '&period=' +
                this.period.key +
                '&columnCount=' +
                this.overTheLast.key +
                '&reportType=' +
                this.reportTypeId.key +
                '&supervisorId=' +
                supervisorId
            )
        },
        onTableReload(data) {
            this.dateTimeRanges = data.dateTimeRanges || []
            this.updateDateColumnsInfo()
        },
    },
    watch: {
        selectedDate(to) {
            this.onChange(query => {
                query.from = to
            })

        },
    },
    computed: {
        model() {
            return this.$config.model
        },
        title() {
            var title = this.model.reportName == 'Speed' ? this.$t('MainMenu.Speed') : this.$t('MainMenu.Quantity')
            title = `<span>${title}: </span><span>${(this.reportTypeId || {}).value}</span>`

            if (this.model.supervisorId) {
                title +=
                    '<br />' +
                    this.$t('PeriodicStatusReport.InTheSupervisorTeamFormat').replace('{0}', this.model.supervisorName)
            }

            return title
        },
        reportDescription() {
            return this.model.reportNameDescription
        },
        getSupervisorsUrl() {
            return (
                this.model.supervisorsUrl +
                '?questionnaireId=' +
                ((this.questionnaireId || {}).key || '') +
                '&questionnaireVersion=' +
                ((this.questionnaireVersion || {}).key || '') +
                '&from=' +
                (this.from || '') +
                '&period=' +
                ((this.period || {}).key || '') +
                '&reportType=' +
                ((this.reportTypeId || {}).key || '') +
                '&columnCount=' +
                ((this.overTheLast || {}).key || '')
            )
        },
        queryString() {
            return {
                questionnaireId: (this.questionnaireId || {}).key,
                questionnaireVersion: (this.questionnaireVersion || {}).key,
                reportType: this.reportTypeId.key,
                columnCount: this.overTheLast.key,
                period: this.period.key,
                from: this.from,
            }
        },
        columnsCount() {
            return (this.overTheLast || {}).key || 7
        },
        supervisorId() {
            return this.$route.params.supervisorId
        },
        selectedDate() {
            return this.from || moment().format(this.dateFormat)
        },
        datePickerConfig() {
            var self = this
            return {
                mode: 'single',
                maxDate: 'today',
                wrap: true,
                minDate: self.model.minAllowedDate,
                enableTime: false,
                dateFormat: 'Y-m-d',
                onChange: (selectedDates, dateStr, instance) => {
                    const date = selectedDates.length > 0 ? moment(selectedDates[0]).format(this.dateFormat) : null

                    if (date != null && date != self.from) {
                        self.from = date
                        self.loadReportData(true)
                    }
                },
            }
        },

        selectedQuestionnaireTitle() {
            const name = this.questionnaireId == null ? this.$t('Common.AllQuestionnaires') : this.questionnaireId.value
            const version = this.questionnaireVersion == null? this.$t('Common.AllVersions') : this.questionnaireVersion.value
            return `${name}, ${version}`
        },

        tableOptions() {

            if (this.columns.length == 0) {
                this.prepareColumns()
            }

            return {
                deferLoading: 0,
                columns: this.columns,
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: 'GET',
                    contentType: 'application/json',
                },
                responsive: false,
                ordering: false,
                sDom: 'rf<"table-with-scroll"t>ip',
            }
        },
    },
}
</script>
