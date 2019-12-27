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

      <!--FilterBlock :title="$t('PeriodicStatusReport.LastDateToShowLabel')">
        <div class="input-group">
            <div class="form-date input-group" id="dates-range">
                <input type="text" data-bind="flatpickr: FromDate, flatpickrOptions: { minDate: '@Model.MinAllowedDate.ToString("s")', maxDate: 'today', wrap: true, enableTime: false, dateFormat: 'Y-m-d'}" placeholder="Select start date" class="form-control flatpickr-input" readonly="readonly" data-input>
                <button type="submit" class="btn btn-link btn-clear">
                    <span></span>
                </button>
                <span class="input-group-addon" data-toggle>
                    <span class="calendar"></span>
                </span>
            </div>
        </div>
      </FilterBlock-->
    </Filters>

    <DataTables
      ref="table"
      :tableOptions="tableOptions"
      :addParamsToRequest="addParamsToRequest"
      noPaging
      noSearch
      exportable
    >
      
    </DataTables>

  </HqLayout>
</template>

<script>
export default {
    data() {
        return {
            questionnaireId: null,
            questionnaireVersion: null,
            reportTypeId: null,
            overTheLast: null,
            period: null,
            loading : {
                supervisors: false
            }
        }
    },
    mounted() {
        if (this.reportTypeId == null && this.model.reportTypes.length > 0) {
            this.reportTypeSelected(this.model.reportTypes[0]);
        } 
        if (this.overTheLast == null && this.model.overTheLasts.length > 6) {
            this.overTheLastSelected(this.model.overTheLasts[6]);
        }
        if (this.period == null && this.model.periods.length > 0) {
            this.periodSelected(this.model.periods[0]);
        }
        if (this.$refs.table){
            this.$refs.table.reload();
        }
    },
    methods: {
        title() {
            return "report title"
        },
        reportTypeSelected(option) {
            this.reportTypeId = option.key
        },
        questionnaireSelected(option){
            this.questionnaireId = option.key
        },
        questionnaireVersionSelected(option) {
            this.questionnaireVersion = option.key
        },
        selectedOverTheLast(option) {
            this.overTheLast = option.key
        },
        statusSelected(option) {
            this.status = option.key
        },
        periodSelected(option) {
            this.period = option.key
        },
        overTheLastSelected(option) {
            this.overTheLast = option.key
        },
        addParamsToRequest(requestData) {
            requestData.questionnaireId = (this.questionnaireId || {}).key
            requestData.questionnaireVersion = (this.questionnaireVersion || {}).key
            requestData.reportTypeId = (this.reportTypeId || {}).key
            requestData.overTheLast = this.overTheLast
            requestData.period = this.period
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
            var url = new Url(window.location.href);
            return this.model.interviewersUrl
                + '?questionnaireId=' + url.query['questionnaireId']
                + '&questionnaireVersion=' + url.query['questionnaireVersion']
                + '&from=' + url.query['from']
                + '&period=' + url.query['period']
                + '&columnCount=' + url.query['columnCount']
                + '&reportType=' + url.query['reportType']
                + '&supervisorId=' + supervisorId;
        },
        getSupervisorsUrl() {
            var url = new Url(window.location.href);
            return this.model.supervisorsUrl
                + '?questionnaireId=' + url.query['questionnaireId']
                + '&questionnaireVersion=' + url.query['questionnaireVersion']
                + '&from=' + url.query['from']
                + '&period=' + url.query['period']
                + '&reportType=' + url.query['reportType']
                + '&columnCount=' + url.query['columnCount'];
        }
    },
    computed: {
        model() {
            return this.$config.model;
        },
        supervisorId() {
            return this.$route.params.supervisorId
        },
        tableOptions() {
            var self = this;
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: "responsible",
                        name: "Responsible",
                        title: this.$t('Model.ResponsibleColumnName'),
                        orderable: true,
                        render: function(data, type, row) {
                            if (self.model.canNavigateToQuantityByTeamMember)
                            {
                                const interviewersUrl = this.getInterviewersUrl(ResponsibleId())
                                return `<a href='${interviewersUrl}'">${data}</a>`
                            }
                            else
                            {
                                return `<span>${data}</span>`
                            }
                        }
                    },
                    {
                        data: "neverUploadedCount",
                        name: "NeverUploadedCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.NeverUploaded"),
                        render: function(data, type, row) {
                             return self.renderCell(data, row, 'NeverUploaded');
                        }
                    },
                    {
                        data: "reassignedCount",
                        name: "ReassignedCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.TabletReassigned"),
                        render: function(data, type, row) {
                            return self.renderCell(data, row, 'TabletReassigned');
                        }
                    },
                    {
                        data: "outdatedCount",
                        name: "OutdatedCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.OldInterviewerVersion"),
                        render: function (data, type, row) {
                            return self.renderCell(data, row, 'OutdatedApp');
                        }
                    },
                    {
                        data: "teamSize",
                        name: "TeamSize",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.TeamSize")
                    }
                ],
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
