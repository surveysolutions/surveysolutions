<template>
  <HqLayout :title="title" :hasFilter="true">
    <Filters slot="filters">
      <FilterBlock :title="$t('PeriodicStatusReport.InterviewActions')">
        <Typeahead
          ref="reportTypeControl"
          control-id="reportTypeId"
          no-clear
          fuzzy
          data-vv-name="reportTypeId"
          data-vv-as="reportType"
          :placeholder="$t('PeriodicStatusReport.InterviewActions')"
          :value="reportTypeId"
          :values="this.$config.model.reportTypes"
          v-on:selected="reportTypeSelected"
        />
      </FilterBlock>


      <FilterBlock :title="$t('Common.Questionnaire')">
        <Typeahead
          ref="questionnaireIdControl"
          control-id="questionnaireId"
          fuzzy
          data-vv-name="questionnaireId"
          data-vv-as="questionnaire"
          :placeholder="$t('Common.AllQuestionnaires')"
          :value="questionnaireId"
          :values="this.$config.model.questionnaires"
          v-on:selected="questionnaireSelected"
        />
      </FilterBlock>

      <FilterBlock :title="$t('Common.QuestionnaireVersion')">
        <Typeahead
          ref="questionnaireVersionControl"
          control-id="questionnaireVersion"
          fuzzy
          data-vv-name="questionnaireVersion"
          data-vv-as="questionnaireVersion"
          :placeholder="$t('Common.AllVersions')"
          :disabled="questionnaireId == null "
          :value="questionnaireVersion"
          :values="questionnaireId == null ? [] : questionnaireId.versions"
          v-on:selected="questionnaireVersionSelected"
        />
      </FilterBlock>
      <FilterBlock :title="$t('PeriodicStatusReport.OverTheLast')">
        <Typeahead
          ref="overTheLast"
          control-id="overTheLast"
          fuzzy
          no-clear          
          data-vv-name="overTheLast"
          data-vv-as="overTheLast"
          :placeholder="$t('PeriodicStatusReport.OverTheLast')"
          :value="overTheLast"
          :values="this.$config.model.overTheLasts"
          v-on:selected="overTheLastSelected"
        />
      </FilterBlock>

      <FilterBlock :title="$t('PeriodicStatusReport.PeriodUnit')">
        <Typeahead
          ref="period"
          control-id="period"
          fuzzy
          no-clear          
          :placeholder="$t('PeriodicStatusReport.Period')"
          data-vv-name="period"
          data-vv-as="period"
          v-on:selected="periodSelected"
          :value="period"
          :values="this.$config.model.periods"
        ></Typeahead>
      </FilterBlock>

      <FilterBlock :title="$t('PeriodicStatusReport.LastDateToShowLabel')">
        <DatePicker 
          :config="datePickerConfig"
          :value="from"
        ></DatePicker>
      </FilterBlock>
    </Filters>

    <DataTables
      ref="table"
      :tableOptions="tableOptions"
      :addParamsToRequest="addParamsToRequest"
      @ajaxComplete="onTableReload"
      noPaging
      noSearch
      exportable
      hasTotalRow 
      noSelect
    >
      
    </DataTables>

  </HqLayout>
</template>

<script>

import moment from "moment";

export default {
    data() {
        return {
            questionnaireId: null,
            questionnaireVersion: null,
            reportTypeId: null,
            overTheLast: null,
            period: null,
            from: null,
            dateFormat: "YYYY-MM-DD",
            dateTimeRanges: [],
            columns: [],
            loading : {
                report: false
            }
        }
    },
    mounted() {
        if (this.reportTypeId == null && this.model.reportTypes.length > 0) {
            this.reportTypeId = this.model.reportTypes[0]
        }
        if (this.overTheLast == null && this.model.overTheLasts.length > 6) {
            this.overTheLast = this.model.overTheLasts[6]
        }
        if (this.period == null && this.model.periods.length > 0) {
            this.period = this.model.periods[0]
        }
        if (this.from == null) {
            this.from = moment().format(this.dateFormat)
        }
        this.loadReportData()
    },
    methods: {
        loadReportData() {
            if (this.$refs.table){
                this.prepareColumns();
                this.$refs.table.reload();
            }
        },
        prepareColumns() {
            var self = this;
            var columns = []
            columns.push({
                data: "responsibleName",
                name: "Responsible",
                title: this.$t('Model.ResponsibleColumnName'),
                orderable: true,
                render: function(data, type, row) {
                    if (data == undefined) {
                        if (self.model.supervisorId) {
                            return self.$t('Strings.AllInterviewers')
                        }
                        else {
                            return self.$t('Strings.AllTeams')
                        }
                    }
 
                    if (self.model.canNavigateToQuantityByTeamMember)
                    {
                        const interviewersUrl = self.getInterviewersUrl(row.responsibleId)
                        return `<a href='${interviewersUrl}'">${data}</a>`
                    }
                    else
                    {
                        return `<span>${data}</span>`
                    }
                }
            })

            for(let i = 0; i < this.dateTimeRanges.length; i++) {
                const date = this.dateTimeRanges[i];
                const index = i
                columns.push({
                    class: "type-numeric",
                    title: moment(date.to).format(this.dateFormat),
                    data: date.from,
                    name: date.from,
                    orderable: false,
                    render: function(data, type, row) {
                        if (row.quantityByPeriod) {
                            var value = row.quantityByPeriod[index]
                            var formatedValue = self.formatNumber(data)
                            return `<span>${value}</span>`
                        }
                        if (row.quantityByPeriod) {
                            var value = row.speedByPeriod[index]
                            if (value == null)
                                return '-'

                            var formatedValue = moment.duration(data, "minutes").format("D[d] H[h] mm[m]")
                            return `<span>${value}</span>`
                        }
                        return '-'
                    }
                })
            }
            columns.push({
                data: "average",
                name: "Average",
                title: this.$t('PeriodicStatusReport.Average'),
                orderable: true,
                render: function(data, type, row) {
                    if (row.quantityByPeriod) {
                        var formatedValue = self.formatNumber(data)
                        return `<span>${formatedValue}</span>`
                    }
                    if (row.quantityByPeriod) {
                        if (data == null)
                            return '-'

                        var formatedValue = moment.duration(data, "minutes").format("D[d] H[h] mm[m]")
                        return `<span>${formatedValue}</span>`
                    }
                    return '-'
                }
            })
            columns.push({
                data: "total",
                name: "Total",
                title: this.$t('PeriodicStatusReport.Total'),
                orderable: true,
                render: function(data, type, row) {
                    if (row.quantityByPeriod) {
                        var value = self.formatNumber(data)
                        return `<span>${value}</span>`
                    }
                    if (row.quantityByPeriod) {
                        var value = moment.duration(data, "minutes").format("D[d] H[h] mm[m]")
                        return `<span>${value}</span>`
                    }
                    return '-'
                }
            })

            this.columns = columns
        },
        reportTypeSelected(option) {
            this.reportTypeId = option
            this.loadReportData()
        },
        questionnaireSelected(option){
            this.questionnaireId = option
            this.loadReportData()
        },
        questionnaireVersionSelected(option) {
            this.questionnaireVersion = option
            this.loadReportData()
        },
        statusSelected(option) {
            this.status = option
            this.loadReportData()
        },
        periodSelected(option) {
            this.period = option
            var newVal = this.period.key
            if (newVal === "d") {
                this.overTheLast = this.model.overTheLasts[6]
            } else if (newVal === "w") {
                this.overTheLast = this.model.overTheLasts[3]
            } else if (newVal === "m") {
                this.overTheLast = this.model.overTheLasts[2]
            }
            this.loadReportData()
        },
        overTheLastSelected(option) {
            this.overTheLast = option
            this.loadReportData()
        },
        addParamsToRequest(requestData) {
            requestData.questionnaireId = (this.questionnaireId || {}).key
            requestData.questionnaireVersion = (this.questionnaireVersion || {}).key
            requestData.reportType = this.reportTypeId.key
            requestData.columnCount = this.overTheLast.key
            requestData.period = this.period.key
            requestData.from = this.from
            requestData.timezoneOffsetMinutes = new Date().getTimezoneOffset()
        },
        renderCell(data, row, facet) {
            const formatedNumber = this.formatNumber(data);
            if(data === 0 || row.DT_RowClass == "total-row") {
                return `<span>${formatedNumber}</span>`;
            }

            if (!this.supervisorId) {
                return `<a href='${this.$config.model.interviewersBaseUrl}?Facet=${facet}&supervisor=${row.teamName}'>${formatedNumber}</a>`;
            }
         
            return this.getLinkToInterviewerProfile(data, row);
        },
        formatNumber(value) {
            if (value == null || value == undefined)
                return value;
            var language = navigator.languages && navigator.languages[0] ||
               navigator.language ||  
               navigator.userLanguage; 
            return value.toLocaleString(language);
        },
        hasIssue(data) {
            return data.lowStorageCount || data.wrongDateOnTabletCount
        },
        getInterviewersUrl(supervisorId){
            return this.model.interviewersUrl
                + '?questionnaireId=' + (this.questionnaireId || {}).key
                + '&questionnaireVersion=' + (this.questionnaireVersion || {}).key
                + '&from=' + this.from
                + '&period=' + this.period.key
                + '&columnCount=' + this.overTheLast.key
                + '&reportType=' + this.reportTypeId.key
                + '&supervisorId=' + supervisorId;
        },
        getSupervisorsUrl() {
            return this.model.supervisorsUrl
                + '?questionnaireId=' + (this.questionnaireId || {}).key
                + '&questionnaireVersion=' + (this.questionnaireVersion || {}).key
                + '&from=' + this.from
                + '&period=' + this.period.key
                + '&reportType=' + this.reportTypeId.key
                + '&columnCount=' + this.overTheLast.key;
        },
        onTableReload(data) {
            this.dateTimeRanges = data.dateTimeRanges || []
        }
    },
    computed: {
        model() {
            return this.$config.model;
        },
        title() {
            return "report title"
/*          <span>@(String.Compare(Model.ReportName,"speed", CultureInfo.InvariantCulture,CompareOptions.IgnoreCase) == 0? MainMenu.Speed : MainMenu.Quantity ): </span><span data-bind="text: ReportTypeName"></span>
            @(Model.SupervisorId.HasValue ? string.Format(PeriodicStatusReport.InTheSupervisorTeamFormat, @Model.SupervisorName) : "")
*/
        },
        reportDescription() {
            return this.model.reportNameDescription
        },
        columnsCount() {
            return (this.overTheLast || {}).key || 7
        },
        supervisorId() {
            return this.$route.params.supervisorId
        },
        datePickerConfig() {
            var self = this
            return {
                mode: "date",
                maxDate: "today",
                wrap: true,
                minDate: self.model.minAllowedDate, 
                enableTime: false, 
                dateFormat: 'Y-m-d'
            };
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
                    type: "GET",
                    contentType: 'application/json'
                },
                responsive: false,
                order: [[0, 'asc']],
                sDom: 'rf<"table-with-scroll"t>ip'
            }
        }
    }
}
</script>
