<template>
  <HqLayout :title="title" :hasFilter="true">
    <Filters slot="filters">
      <FilterBlock :title="$t('PeriodicStatusReport.InterviewActions')">
        <Typeahead
          ref="reportTypeControl"
          control-id="reportTypeId"
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
          control-id="status"
          fuzzy
          :selectedKey="selectedStatus"
          data-vv-name="status"
          data-vv-as="status"
          :placeholder="$t('Common.AllStatuses')"
          :value="status"
          :values="statuses"
          v-on:selected="statusSelected"
        />
      </FilterBlock>

      <FilterBlock :title="$t('PeriodicStatusReport.PeriodUnit')">
        <Typeahead
          control-id="responsibleId"
          :placeholder="$t('Common.AllResponsible')"
          :value="responsibleId"
          :ajax-params="responsibleParams"
          v-on:selected="userSelected"
          :fetch-url="config.api.responsible"
        ></Typeahead>
      </FilterBlock>

      <FilterBlock :title="$t('Pages.Filters_Assignment')">
        <div class="input-group">
          <input
            class="form-control with-clear-btn"
            :placeholder="$t('Common.AllAssignments')"
            type="text"
            v-model="assignmentId"
          />
          <div class="input-group-btn" @click="clearAssignmentFilter">
            <div class="btn btn-default">
              <span class="glyphicon glyphicon-remove" aria-hidden="true"></span>
            </div>
          </div>
        </div>
      </FilterBlock>
    </Filters>

    <DataTables
      ref="table"
      :tableOptions="tableOptions"
      :addParamsToRequest="addParamsToRequest"
      :contextMenuItems="contextMenuItems"
      @selectedRowsChanged="rows => selectedRows = rows"
      @page="resetSelection"
      @totalRows="(rows) => totalRows = rows"
      @ajaxComlpete="isLoading = false"
      :selectable="showSelectors"
      :selectableId="'interviewId'"
    >
      <div class="panel panel-table" v-if="selectedRows.length" id="pnlInterviewContextActions">
        <div class="panel-body">
          <input
            class="double-checkbox-white"
            id="q1az"
            type="checkbox"
            checked
            disabled="disabled"
          />
          <label for="q1az">
            <span class="tick"></span>
            {{ selectedRows.length + " " + $t("Pages.Interviews_Selected") }}
          </label>
          <button
            class="btn btn-lg btn-success"
            v-if="selectedRows.length"
            @click="assignInterview"
          >{{ $t("Common.Assign") }}</button>
          <button
            class="btn btn-lg btn-success"
            v-if="selectedRows.length"
            @click="approveInterview"
          >{{ $t("Common.Approve")}}</button>
          <button
            class="btn btn-lg reject"
            v-if="selectedRows.length"
            @click="rejectInterview"
          >{{ $t("Common.Reject")}}</button>
          <button
            class="btn btn-lg btn-primary"
            v-if="selectedRows.length && !config.isSupervisor"
            @click="unapproveInterview"
          >{{ $t("Common.Unapprove")}}</button>
          <button
            class="btn btn-link"
            v-if="selectedRows.length && !config.isSupervisor"
            @click="deleteInterview"
          >{{ $t("Common.Delete")}}</button>
        </div>
      </div>
    </DataTables>


    <main class="hold-transition">
        <div class="container-fluid">
            <div class="row">
                <aside class="filters">
                    <div class="foldback-button" id="hide-filters">
                        <span class="arrow"></span>
                        <span class="arrow"></span>
                        <span class="glyphicon glyphicon-tasks" aria-hidden="true"></span>
                    </div>
                    <div class="filters-container">
                        <h4>@Pages.FilterTitle</h4>
                        @if (Model.ReportTypes.Length > 1)
                        {
                            <div class="block-filter">
                                <h5>@PeriodicStatusReport.InterviewActions</h5>
                                <select id="reportTypeSelector" data-bind="value: SelectedType, selectPicker: {}">
                                    @foreach (var item in Model.ReportTypes)
                                    {
                                        <option value='@((int) item)'>@PeriodicStatusReport.ResourceManager.GetString(@item.ToString())</option>
                                    }
                                </select>
                            </div>
                        }
                        <div class="block-filter">
                            <h5>@PeriodicStatusReport.Questionnaire</h5>
                            <select id="questionnaireSelector" 
                                    data-bind="value: SelectedQuestionnaire, 
                                            selectPicker: {}, 
                                            options: questionnaires,
                                            optionsText: 'title',
                                            optionsCaption: AllQuestionnariesCaption">
                            </select>
                        </div>
                        <div class="block-filter">
                            <h5>@Common.QuestionnaireVersion</h5>
                            <select id="questionnaireVersionSelector" 
                                    data-bind="enable: SelectedQuestionnaire, 
                                            value: SelectedQuestionnaireVersion, 
                                            selectPicker: {}, 
                                            options: QuestionnaireVersions,
                                            optionsCaption: AllVersionsCaption,">
                            </select>
                        </div>

                        <div class="block-filter">
                            <h5>@PeriodicStatusReport.OverTheLast</h5>
                            <select data-bind="value: ColumnCount, selectPicker: {}" data-size="5">
                                @for (int i = 1; i <= 12; i++)
                                {
                                    <option value="@i">@i</option>
                                }
                            </select>
                        </div>
                        <div class="block-filter">
                            <h5>@PeriodicStatusReport.PeriodUnit</h5>
                            <select id="periodSelector" data-bind="value: Period, selectPicker: {}">
                                <option value='d'>@PeriodicStatusReport.Day</option>
                                <option value='w'>@PeriodicStatusReport.Week</option>
                                <option value='m'>@PeriodicStatusReport.Month</option>
                            </select>

                        </div>
                        <div class="block-filter">
                            <h5>@PeriodicStatusReport.LastDateToShowLabel</h5>
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

                    </div>
                </aside>
                <div class="main-information">
                    <div class="page-header">
                        <h1>
                            <span>@(String.Compare(Model.ReportName,"speed", CultureInfo.InvariantCulture,CompareOptions.IgnoreCase) == 0? MainMenu.Speed : MainMenu.Quantity ): </span><span data-bind="text: ReportTypeName"></span>
                            @(Model.SupervisorId.HasValue ? string.Format(PeriodicStatusReport.InTheSupervisorTeamFormat, @Model.SupervisorName) : "")
                        </h1>
                        <i>@Model.ReportNameDescription</i>
                    </div>
                    <div class="clearfix">
                        <div class="col-sm-8">

                            <h4 data-bind="text: (SelectedQuestionnaire() == undefined ? AllQuestionnariesCaption : SelectedQuestionnaire().title) + ', ' + (SelectedQuestionnaireVersion() == undefined ? AllVersionsCaption.toLowerCase() : 'ver.' + SelectedQuestionnaireVersion())" ></h4>

                            @if (Model.CanNavigateToQuantityBySupervisors)
                            {
                                <a data-bind="attr: { href: GetSupervisorsUrl() }" class="btn btn-default"><span class="glyphicon glyphicon-arrow-left"></span>@PeriodicStatusReport.BackToSupervisors</a>
                            }

                            <h2 data-bind="visible: Pager().TotalItemCount() == 0">
                                @Pages.NoResults
                            </h2>
                        </div>
                    </div>
                    <div class="dataTables_wrapper no-footer">
                        <div class="table-with-scroll" data-bind="visible: Pager().TotalItemCount() > 0">
                            <table class="table table-striped table-bordered table-condensed table-hover">
                                <thead>
                                <tr>
                                    <th>@Model.ResponsibleColumnName</th>
                                    <!-- ko foreach:DateTimeRanges -->
                                    <th>
                                        <span data-bind="text: $root.GetPeriodName($data)"></span>
                                    </th>
                                    <!-- /ko -->
                                    <th>@PeriodicStatusReport.Average</th>
                                    <th>@PeriodicStatusReport.Total</th>
                                </tr>
                                </thead>
                                <tbody>
                                @if (Model.TotalRowPresent)
                                {
                                    <tr class="total-row">
                                        <td><span><b>@((isSupervisor || Model.SupervisorId.HasValue) ? Strings.AllInterviewers : Strings.AllTeams)</b></span></td>
                                        <!-- ko foreach: ((TotalRow() || {}).@(Model.ReportName)ByPeriod || []) -->
                                        <td class="type-numeric"><span data-bind='text: $root.Format@(Model.ReportName)Period($data)'></span></td>
                                        <!-- /ko -->
                                        <td class="type-numeric">
                                            <span data-bind="text:Format@(Model.ReportName)Period(getTotalAverage())"></span>
                                        </td>
                                        <td class="type-numeric">
                                            <span data-bind="text:Format@(Model.ReportName)Period(getTotalCount())"></span>
                                        </td>
                                    </tr>
                                }
                                <!-- ko foreach:Items -->
                                <tr>
                                    <td>@if (Model.CanNavigateToQuantityByTeamMember)
                                        {
                                            <a data-bind="attr: { href: GetInterviewersUrl(ResponsibleId()) }, text: ResponsibleName"> </a>
                                        }
                                        else
                                        {
                                            <span data-bind="text: ResponsibleName"></span>
                                        }
                                    </td>
                                    <!-- ko foreach:@(Model.ReportName + "ByPeriod") -->
                                    <td class="type-numeric"><span data-bind='text: $root.Format@(Model.ReportName)Period($data)'></span></td>
                                    <!-- /ko -->
                                    <td class="type-numeric"><span data-bind="text: $root.Format@(Model.ReportName)Period(Average())"></span></td>
                                    <td class="type-numeric"><span data-bind="text: $root.Format@(Model.ReportName)Period(Total())"></span></td>
                                </tr>
                                <!-- /ko -->
                                </tbody>
                            
                            </table>
                        </div>
                        <!-- ko if:Pager().LastPage() > 1 -->
                        <div data-bind="template: { name: 'datatable-pager', data: Pager }"></div>
                        <!-- /ko -->
                    </div>
                    <!-- ko if:Pager().TotalItemCount() > 0 -->
                    @Html.Partial("_export-buttons")
                    <!-- /ko -->
                </div>
            </div>
        </div>
        @Html.Partial("_ListDataTablePagingScript")
    </main>


</template>

<script>
export default {
    mounted() {
        if (this.$refs.table){
            this.$refs.table.reload();
        }
    },
    methods: {
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
        getLinkToInterviewerProfile(data, row){
            const formatedNumber = this.formatNumber(data)
            const linkClass = this.hasIssue(row) ? "text-danger" : ""

            return `<a href='${this.$config.model.interviewerProfileUrl}/${row.teamId}'><hi class='${linkClass}'>${formatedNumber}</hi></a>`;
        }
    },
    computed: {
        config() {
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
                        data: "teamName",
                        name: "TeamName",
                        title: self.supervisorId ? this.$t('DevicesInterviewers.Interviewers') : this.$t("DevicesInterviewers.Teams"),
                        orderable: true,
                        render: function(data, type, row) {
                            if(self.supervisorId) {
                                return self.getLinkToInterviewerProfile(data, row)
                            }
                            
                            const linkClass = self.hasIssue(row) ? "text-danger" : ""
                            return `<a href='${window.location}/${row.teamId}'><hi class='${linkClass}'>${data}</hi></a>`
                        }
                    },
                    {
                        data: "neverSynchedCount",
                        name: "NeverSynchedCount",
                        "class": "type-numeric",
                        title: this.$t("DevicesInterviewers.NeverSynchronized"),
                        orderable: true,
                        render: function (data, type, row) {
                            return self.renderCell(data, row, 'NeverSynchonized');
                        }
                    },
                    {
                        data: "noQuestionnairesCount",
                        name: "NoQuestionnairesCount",
                        "class": "type-numeric",
                        orderable: true,
                        title: this.$t("DevicesInterviewers.NoAssignments"),
                        render: function(data, type, row) {
                            return self.renderCell(data, row, 'NoAssignmentsReceived');
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
                    url: this.supervisorId ? this.$config.model.dataUrl + '/' + this.supervisorId : this.$config.model.dataUrl,
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
