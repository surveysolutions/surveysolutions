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
                this.$refs.table.reload();
            }
        },
        prepareColumns() {
            var self = this;
            var columns = []
            columns.push({
                data: "responsibleName",
                name: "Responsible",
                title: self.model.responsibleColumnName,
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

            const count = self.columnsCount
            for(let i = 0; i < count; i++) {
                //const date = this.dateTimeRanges[i];
                const index = i
                columns.push({
                    class: "type-numeric",
                    title: '',//moment(date.to).format(this.dateFormat),
                    data: '',//date.from,
                    name: '',//date.from,
                    orderable: false,
                    render: function(data, type, row) {
                        if (row.quantityByPeriod) {
                            var value = row.quantityByPeriod[index]
                            return self.renderQuantityValue(value)
                        }
                        if (row.quantityByPeriod) {
                            var value = row.speedByPeriod[index]
                            return self.renderSpeedValue(value)
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
                        return self.renderQuantityValue(data)
                    }
                    if (row.quantityByPeriod) {
                        return self.renderSpeedValue(data)
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
                        return self.renderQuantityValue(data)
                    }
                    if (row.quantityByPeriod) {
                        return self.renderSpeedValue(data)
                    }
                    return '-'
                }
            })

            this.columns = columns
        },
        updateDateColumnsInfo() {
            for(let i = 0; i < this.dateTimeRanges.length; i++) {
                const date = this.dateTimeRanges[i];
                const column = this.columns[i + 1]
                column.title = moment(date.to).format(this.dateFormat)
                column.data = date.from
                column.name = date.from

                column.sTitle = column.title
                column.sName = column.name
                column.mData = column.data
            }
        },
        renderQuantityValue(value) {
            if (value == null)
                return '-'

            var formatedValue = this.formatNumber(value)
            return `<span>${formatedValue}</span>`
        },
        renderSpeedValue(value) {
            if (value == null)
                return '-'

            var formatedValue = moment.duration(value, "minutes").format("D[d] H[h] mm[m]")
            return `<span>${formatedValue}</span>`
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
            this.prepareColumns()
            this.loadReportData()
        },
        overTheLastSelected(option) {
            this.overTheLast = option
            this.prepareColumns()
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
            this.updateDateColumnsInfo();
        }
    },
    computed: {
        model() {
            return this.$config.model;
        },
        title() {
            var title = this.model.reportName == 'Speed'
                    ? this.$t('MainMenu.Speed')
                    : this.$t('MainMenu.Quantity')
            //title = `<span>${title}:</span><span>${(this.reportTypeId || {}).name}</span>`

            if (this.model.supervisorId) {
                title += this.$t('PeriodicStatusReport.InTheSupervisorTeamFormat', this.model.supervisorName)
            }

            return title
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
