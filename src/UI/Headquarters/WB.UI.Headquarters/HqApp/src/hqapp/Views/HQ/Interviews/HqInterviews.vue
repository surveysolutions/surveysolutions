<template>
  <HqLayout :title="title" :hasFilter="true">
    <Filters slot="filters">
      <FilterBlock :title="$t('Common.Questionnaire')">
        <Typeahead
          control-id="questionnaireId"
          fuzzy
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
          fuzzy
          data-vv-name="questionnaireVersion"
          data-vv-as="questionnaireVersion"
          :placeholder="$t('Common.AllVersions')"
          :disabled="questionnaireId == null "
          :value="questionnaireVersion"
          :values="questionnaireId == null ? [] : questionnaireId.versions"
          v-on:selected="questionnaireVersionSelected"/>
      </FilterBlock>
      <FilterBlock :title="$t('Common.Status')">
        <Typeahead
          control-id="status"
          fuzzy
          :selectedKey="selectedStatus"
          data-vv-name="status"
          data-vv-as="status"
          :placeholder="$t('Common.AllStatuses')"
          :value="status"
          :values="statuses"
          v-on:selected="statusSelected"/>
      </FilterBlock>

      <FilterBlock :title="$t('Common.Responsible')">
        <Typeahead
          control-id="responsibleId"
          :placeholder="$t('Common.AllResponsible')"
          :value="responsibleId"
          :ajax-params="responsibleParams"
          v-on:selected="userSelected"
          :fetch-url="config.api.responsible"></Typeahead>
      </FilterBlock>

      <FilterBlock :title="$t('Pages.Filters_Assignment')">
        <div class="input-group">
          <input
            class="form-control with-clear-btn"
            :placeholder="$t('Common.AllAssignments')"
            type="text"
            v-model="assignmentId"/>
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
      :selectableId="'interviewId'">
      <div class="panel panel-table" v-if="selectedRows.length" id="pnlInterviewContextActions">
        <div class="panel-body">
          <input
            class="double-checkbox-white"
            id="q1az"
            type="checkbox"
            checked
            disabled="disabled"/>
          <label for="q1az">
            <span class="tick"></span>
            {{ selectedRows.length + " " + $t("Pages.Interviews_Selected") }}
          </label>
          <button
            class="btn btn-lg btn-success"
            v-if="selectedRows.length"
            @click="assignInterview">{{ $t("Common.Assign") }}</button>
          <button
            class="btn btn-lg btn-success"
            v-if="selectedRows.length"
            @click="approveInterview">{{ $t("Common.Approve")}}</button>
          <button
            class="btn btn-lg reject"
            v-if="selectedRows.length"
            @click="rejectInterview">{{ $t("Common.Reject")}}</button>
          <button
            class="btn btn-lg btn-primary"
            v-if="selectedRows.length && !config.isSupervisor"
            @click="unapproveInterview">{{ $t("Common.Unapprove")}}</button>
          <button
            class="btn btn-link"
            v-if="selectedRows.length && !config.isSupervisor"
            @click="deleteInterview">{{ $t("Common.Delete")}}</button>
        </div>
      </div>
    </DataTables>

    <ModalFrame ref="assignModal">
      <p>{{$t("Interviews.ChooseResponsible")}}</p>
      <form onsubmit="return false;">
        <div class="form-group">
          <label
            class="control-label"
            for="newResponsibleId">{{$t("Assignments.SelectResponsible")}}</label>
          <Typeahead
            control-id="newResponsibleId"
            :placeholder="$t('Common.Responsible')"
            :value="newResponsibleId"
            :ajax-params="{ }"
            @selected="newResponsibleSelected"
            :fetch-url="config.api.responsible"></Typeahead>
        </div>
        <div id="pnlAssignToOtherTeamConfirmMessage">
          <p>{{ $t("Interviews.AssignToOtherTeamConfirmMessage", this.selectedRows.length, "SV approved", "HQ approved" )}}</p>
        </div>
        <!-- support checkbox for received items-->

        <!--div data-bind="if: CountReceivedByInterviewerItems > 0">
                    <br />
                    <input type="checkbox" id="reassignReceivedByInterviewer" data-bind="checked: IsReassignReceivedByInterviewer" class="checkbox-filter" />
                    <label for="reassignReceivedByInterviewer" style="font-weight: normal">
                        <span class="tick"></span>
                        @Html.Raw(string.Format(Interviews.AssignReceivedConfirm, "<span data-bind='text: CountReceivedByInterviewerItems'></span>"))
                    </label>
        </div-->
      </form>
      <div slot="actions">
        <button type="button" class="btn btn-primary"
          @click="assign" :disabled="!newResponsibleId">{{ $t("Common.Assign") }}</button>
        <button type="button" class="btn btn-link" data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
      </div>
    </ModalFrame>

    <ModalFrame ref="deleteModal">
      <div class="action-container">
        <p v-html="$t('Interviews.DeleteConfirmMessageHQ', {'0': this.getFilteredToDelete.length,'1': 'SupervisorAssigned', '2': 'InterviewerAssigned'})"></p>
      </div>
      <div slot="actions">
        <button type="button"
          class="btn btn-primary"
          @click="deleteInterviews"
          :disabled="getFilteredToDelete.length==0"
        >{{ $t("Common.Delete") }}</button>
        <button type="button" class="btn btn-link" data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
      </div>
    </ModalFrame>

    <ModalFrame ref="approveModal">
      <form onsubmit="return false;">
        <div class="action-container">
          <p>{{ $t("Interviews.ApproveConfirmMessageHQ", this.selectedRows.length, "Completed", "ApprovedBySupervisor" )}}</p>
        </div>
        <div>
          <label for="txtStatusChangeComment"
          >{{$t("Pages.ApproveRejectPartialView_CommentLabel")}} :</label>
          <textarea class="form-control"
            rows="10"
            maxlength="200"
            id="txtStatusChangeComment"
            v-model="statusChangeComment"></textarea>
        </div>
      </form>
      <div slot="actions">
        <button
          type="button"
          class="btn btn-primary"
          @click="approveInterviews"
        >{{ $t("Common.Approve") }}</button>
        <button type="button" class="btn btn-link" data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
      </div>
    </ModalFrame>

    <ModalFrame ref="rejectModal">
      <form onsubmit="return false;">
        <div class="action-container">
          <p>{{ $t("Interviews.RejectConfirmMessageHQ", {0: this.selectedRows.length,1: "Completed", 2: "ApprovedBySupervisor"} )}}</p>
        </div>
        <div>
          <label
            for="txtStatusChangeComment"
          >{{$t("Pages.ApproveRejectPartialView_CommentLabel")}} :</label>
          <textarea
            class="form-control"
            rows="10"
            maxlength="200"
            id="txtStatusChangeComment"
            v-model="statusChangeComment"
          ></textarea>
        </div>
      </form>
      <div slot="actions">
        <button type="button"
          class="btn btn-primary"
          @click="rejectInterviews">{{ $t("Common.Reject") }}</button>
        <button type="button" class="btn btn-link" data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
      </div>
    </ModalFrame>

    <ModalFrame ref="unapproveModal">
      <form onsubmit="return false;">
        <div class="action-container">
          <p>{{ $t("Interviews.UnapproveConfirmMessageHQ", {"0" : this.selectedRows.length,"1": "ApprovedByHeadquarters"})}}</p>
        </div>
      </form>
      <div slot="actions">
        <button type="button"
          class="btn btn-primary"
          @click="unapproveInterviews"
        >{{ $t("Common.Unapprove") }}</button>
        <button type="button" class="btn btn-link" data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
      </div>
    </ModalFrame>

    <ModalFrame ref="statusHistory" 
                :title="$t('Pages.HistoryOfStatuses_Title')">
      <div class="action-container">        
        <p><a class="interview-id title-row" @click="viewInterview" href="#">{{interviewKey}}</a> by <span :class="getResponsibleClass" href="#">{{responsibleName}}</span></p>
      </div>    
      <div class="table-with-scroll">     
          <table class="table table-striped table-condensed history" id="statustable">
              <thead>
              <tr>
                  <td>{{ $t("Pages.HistoryOfStatuses_State")}}</td>
                  <td>{{ $t("Pages.HistoryOfStatuses_On")}}</td>
                  <td>{{ $t("Pages.HistoryOfStatuses_By")}}</td>
                  <td>{{ $t("Pages.HistoryOfStatuses_AssignedTo")}}</td>
                  <td>{{ $t("Pages.HistoryOfStatuses_Comment")}}</td>
              </tr>
              </thead>              
          </table>
      </div>
      <div slot="actions">
        <button type="button"
          class="btn btn-link"
          @click="viewInterview"          
        >{{ $t("Pages.HistoryOfStatuses_ViewInterview") }}</button>
        <button type="button" class="btn btn-link" data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
      </div>
    </ModalFrame>        
  </HqLayout>
</template>

<script>
import { DateFormats } from "~/shared/helpers";

export default {
    data() {
        return {
            restart_comment: null,
            questionnaireId: null,
            questionnaireVersion: null,
            isLoading: false,
            selectedRows: [],
            selectedRowWithMenu:null,
            totalRows: 0,
            assignmentId: null,
            responsibleId: null,
            responsibleParams: { showArchived: true, showLocked: true },
            newResponsibleId: null,
            statusChangeComment: null,
            status: null,
            selectedStatus: null,
            unactiveDateStart:null,
            unactiveDateEnd:null,
            statuses: this.$config.model.statuses
        };
    },

    watch: {
        questionnaireId: function() {
            this.reloadTable();
        },
        questionnaireVersion: function() {
            this.reloadTable();
        },
        assignmentId: function() {
            this.reloadTable();
        },
        responsibleId: function() {
            this.reloadTable();
        },
        status: function() {
            this.reloadTable();
        }
    },

    computed: {
        title() {
            return this.$config.title;
        },

        getFilteredToDelete(){
          return this.getFileredItems(function (item) 
                {
                    var value = item.canDelete;
                    return !isNaN(value) && value;
            });
        },

        interviewKey() {
            return this.selectedRowWithMenu != undefined ? this.selectedRowWithMenu.key: "";
        },
        responsibleName(){
            return this.selectedRowWithMenu != undefined ? this.selectedRowWithMenu.responsibleName: "";
        },
        getResponsibleClass(){
          return this.selectedRowWithMenu != undefined 
            ? ( this.selectedRowWithMenu.isResponsibleInterviewer ? "interviewer": "supervisor") 
            : "";
        },
        tableColumns() {
            const self = this;
            return [
                {
                    data: "key",
                    name: "Key",
                    title: this.$t("Common.InterviewKey"),
                    orderable: true,
                    searchable: true,
                    responsivePriority: 2,
                    class: "interview-id title-row",
                    render(data, type, row) {
                        var result = "<a href='" + self.config.interviewReviewUrl + '/' + row.interviewId + "'>" + data + "</a>";
                        return result;
                    }
                },
                {
                    data: "featuredQuestions",
                    title: this.$t("Assignments.IdentifyingQuestions"),
                    class: "prefield-column first-identifying last-identifying sorting_disabled visible",
                    orderable: false,
                    searchable: false,
                    render(data) {
                        var questionsWithTitles = _.map(data, question => {
                            return question.question + ": " + question.answer;
                        });
                        return _.join(questionsWithTitles, ", ");
                    }
                },
                {
                    data: "responsibleName",
                    name: "ResponsibleName",
                    title: this.$t("Common.Responsible"),
                    orderable: true
                },
                {
                    data: "lastEntryDateUtc",
                    name: "UpdateDate",
                    title: this.$t("Assignments.UpdatedAt"),
                    class: "date last-update",
                    searchable: false,
                    render(data) {
                        return moment
                            .utc(data)
                            .local()
                            .format(DateFormats.dateTimeInList);
                    }
                },
                {
                    data: "errorsCount",
                    name: "ErrorsCount",
                    title: this.$t("Interviews.Errors"),
                    orderable: true,                    
                    render(data) {
                        return data > 0 ? "<span style='color:red;'>" + data + "</span>" : "0";
                    }
                },
                {
                    data: "status",
                    name: "Status",
                    title: this.$t("Common.Status"),
                    orderable: true
                },
                {
                    data: "receivedByInterviewer",
                    name: "ReceivedByInterviewer",
                    title: this.$t("Common.ReceivedByInterviewer"),
                    render(data) {
                        return data ? self.$t("Common.Yes") : self.$t("Common.No");
                    }
                },
                {
                    data: "assignmentId",
                    name: "AssignmentId",
                    title: this.$t("Common.Assignment"),
                    orderable: true,
                    searchable: false
                }
            ];
        },

        tableOptions() {
            const columns = this.tableColumns.filter(x => x.if == null || x.if());

            var defaultSortIndex = 3; //_.findIndex(columns, { name: "UpdateDate" });
            if (this.showSelectors) defaultSortIndex += 1;

            var tableOptions = {
                rowId: function(row) {
                    return `row_${row.interviewId}`;
                },
                order: [[defaultSortIndex, "desc"]],
                deferLoading: 0,
                columns,
                ajax: {
                    url: this.$config.model.allInterviews,
                    type: "GET"
                },
                select: {
                    style: "multi",
                    selector: "td>.checkbox-filter",
                    info: false
                },
                sDom: 'rf<"table-with-scroll"t>ip',
                headerCallback: thead => {
                    for (let i = 0; i < columns.length; i++) {
                        $(thead)
                            .find("th")
                            .eq(i)
                            .attr("title", columns[i].tooltip);
                    }
                },
                searchHighlight: true
            };

            return tableOptions;
        },        
        showSelectors() {
            return !this.config.isObserver && !this.config.isObserving;
        },
        title() {
            return this.$t("Common.Interviews") + " (" + this.formatNumber(this.totalRows) + ")";
        },
        config() {
            return this.$config.model;
        }
    },

    methods: {
        questionnaireSelected(newValue) {
            this.questionnaireId = newValue;
            this.questionnaireVersion = null;
        },

        questionnaireVersionSelected(newValue) {
            this.questionnaireVersion = newValue;
        },

        userSelected(newValue) {
            this.responsibleId = newValue;
        },
        statusSelected(newValue) {
            this.status = newValue;
        },

        viewInterview(){
            var id = this.selectedRowWithMenu.interviewId;
            window.location = this.config.interviewReviewUrl + "/" + id.replace(/-/g, "");
        },

        arrayFilter: function (array, predicate) {
            array = array || [];
            var result = [];
            for (var i = 0, j = array.length; i < j; i++)
                if (predicate(array[i], i))
                    result.push(array[i]);
            return result;
        },

        assign() {        
          const self = this;
          var filteredItems = this.getFileredItems(function (item) 
                {
                    var value = item.canBeReassigned && item.responsibleId !== self.newResponsibleId.key;
                    return !isNaN(value) && value;
          });

          if(filteredItems.length == 0)
          {
            this.$refs.assignModal.hide();
            return;
          };

          var commands = this.arrayMap(_.map(filteredItems, question => {return question.interviewId;}), function(rowId) {
                var item = {
                    InterviewId: rowId,                    
                    InterviewerId: self.newResponsibleId.iconClass === "interviewer" ? self.newResponsibleId.key: null,
                    SupervisorId: self.newResponsibleId.iconClass === "supervisor" ? self.newResponsibleId.key : null
                };
                return JSON.stringify(item);
          });

            var command = {
                type: self.config.isSupervisor ? "AssignInterviewerCommand": "AssignResponsibleCommand",
                commands: commands
            };

            this.executeCommand(
                command,
                function() {},
                function() {
                    self.$refs.assignModal.hide();
                    self.newResponsibleId = null;
                    self.reloadTable();
                }
            );
        },

        assignInterview() {
            this.newResponsibleId = null;
            this.$refs.assignModal.modal({ keyboard: false });
        },

        getFileredItems(filterPredicat){
          
          if(this.$refs.table == undefined)
            return [];

          var selectedItems = this.$refs.table.table.rows( { selected: true } ).data();

          if(selectedItems.length !== 0 && selectedItems[0] != null)
            return this.arrayFilter(selectedItems, filterPredicat);           

          return this.arrayFilter( [this.selectedRowWithMenu], filterPredicat);
        },

        approveInterviews() {     
          const self = this;     
          var filteredItems = this.getFileredItems(function (item) 
                {  var value = item.canApprove;
                   return !isNaN(value) && value;
            });

            if(filteredItems.length == 0)
            {
              this.$refs.approveModal.hide();
              return;
            }

            var command = this.getCommand(
              self.config.isSupervisor ? "ApproveInterviewCommand" :"HqApproveInterviewCommand", 
            _.map(filteredItems, question => {return question.interviewId;}), this.statusChangeComment);

            this.executeCommand(
                command,
                function() {},
                function() {
                    self.$refs.approveModal.hide();
                    self.reloadTable();
                }
            );
        },
        approveInterview() {
            this.statusChangeComment = null;
            this.$refs.approveModal.modal({
                keyboard: false
            });
        },

        rejectInterviews() {
          const self = this;
            var filteredItems = this.getFileredItems(function (item) 
                {
                    var value = item.canReject;
                    return !isNaN(value) && value;
            });

            if(filteredItems.length == 0)
            {
              this.$refs.rejectModal.hide();
              return;
            }

            var command = this.getCommand(
              self.config.isSupervisor ? "RejectInterviewToInterviewerCommand" : "HqRejectInterviewCommand", 
              _.map(filteredItems, question => {return question.interviewId;}), this.statusChangeComment);

            this.executeCommand(
                command,
                function() {},
                function() {
                    self.$refs.rejectModal.hide();
                    self.reloadTable();
                }
            );
        },
        rejectInterview() {
            this.statusChangeComment = null;
            this.$refs.rejectModal.modal({
                keyboard: false
            });
        },

        arrayMap: function(array, mapping) {
            array = array || [];
            var result = [];
            for (var i = 0, j = array.length; i < j; i++) result.push(mapping(array[i], i));
            return result;
        },

        executeCommand(command, onSuccess, onDone) {
            var url = this.config.commandsUrl;
            var requestHeaders = {};
            requestHeaders[global.input.settings.acsrf.tokenName] = global.input.settings.acsrf.token;

            $.ajax({
                cache: false,
                type: "post",
                headers: requestHeaders,
                url: url,
                data: command,
                dataType: "json"
            })
                .done(function(data) {
                    if (onSuccess !== undefined) onSuccess(data);
                })
                .fail(function(jqXhr, textStatus, errorThrown) {
                    if (jqXhr.status === 401) {
                        location.reload();
                    }
                    //display error
                })
                .always(function() {
                    if (onDone !== undefined) onDone();
                });
        },

        getCommand(commandName, Ids, comment) {
            var commands = this.arrayMap(Ids, function(rowId) {
                var item = { InterviewId: rowId, Comment: comment };
                return JSON.stringify(item);
            });

            var command = {
                type: commandName,
                commands: commands
            };

            return command;
        },

        unapproveInterviews() {
          const self = this;
          var filteredItems = this.getFileredItems(function (item)  
                {
                    var value = item.canUnapprove;
                    return !isNaN(value) && value;
            });

            if(filteredItems.length == 0)
            {
              this.$refs.unapproveModal.hide();
              return;
            }
            
            var command = this.getCommand("UnapproveByHeadquarterCommand", _.map(filteredItems, question => {return question.interviewId;}));

            this.executeCommand(
                command,
                function() {},
                function() {
                    self.$refs.unapproveModal.hide();
                    self.reloadTable();
                }
            );
        },

        unapproveInterview() {
            this.$refs.unapproveModal.modal({ keyboard: false });
        },

        deleteInterviews() {
            const self = this;          
            var filteredItems = this.getFilteredToDelete();
            if(filteredItems.length == 0)
            {
                this.$refs.deleteModal.hide();
                return;
            }

            var command = this.getCommand("DeleteInterviewCommand", _.map(filteredItems, question => {return question.interviewId;}));

            this.executeCommand(
                command,
                function() {},
                function() {
                    self.$refs.deleteModal.hide();
                    self.reloadTable();
                }
            );
        },

        deleteInterview() {
            this.$refs.deleteModal.modal({keyboard: false});
        },
        newResponsibleSelected(newValue) {
            this.newResponsibleId = newValue;
        },

        showStatusHistory() {
          var self = this;
          
          $.ajax({
            type: "POST",
            url: this.config.api.interviewStatuses,            
            data: { interviewId: this.selectedRowWithMenu.interviewId },
            success: function (statusHistoryList) {                 
                 if (statusHistoryList.length != 0) {
                      $('#statustable').dataTable({
                        "paging": false,
                      "ordering": false,
                          "info": false,
                     "searching": false,
                      "retrieve": true,                          
                       "columns":[
                            {data: "StatusHumanized"},
                            {data: "Date",
                             render: function ( data, type, row ) {
                              return moment.utc(data).local().format('MMM DD, YYYY HH:mm');}
                            },
                            {data: "Responsible"},
                            {data: "Assignee"},
                            {data: "Comment"}],                    
                          });                      

                      var table = $('#statustable').dataTable();

                      table.fnClearTable();
                      table.fnAddData(statusHistoryList);
                      table.fnDraw();

                      self.$refs.statusHistory.modal({keyboard: false});
                 }}
            });      
        },

        contextMenuItems({ rowData, rowIndex }) {
            const menu = [];
            const self = this;

            self.selectedRowWithMenu = rowData;

            menu.push({
                name: self.$t("Pages.InterviewerHq_OpenInterview"),
                callback: () => {
                    window.location = self.config.interviewReviewUrl + "/" + rowData.interviewId.replace(/-/g, "");
                }
            });

            menu.push({
                name: self.$t("Common.ShowStatusHistory"),
                callback: () => self.showStatusHistory()
            });

            if (rowData.responsibleRole === "Interviewer") {
                menu.push({
                    name: self.$t("Common.OpenResponsiblesProfile"),
                    callback: () => window.location = self.config.profileUrl + "/" + rowData.responsibleId
                });
            }

            if (!self.config.isObserving) {
                menu.push({
                    className: "context-menu-separator context-menu-not-selectable"
                });

                menu.push({
                    name: self.$t("Common.Assign"),
                    className: "primary-text",
                    callback: () => self.assignInterview()
                });

                menu.push({
                    name: self.$t("Common.Approve"),
                    className: "success-text",
                    callback: () => self.approveInterview()
                });

                menu.push({
                    name: self.$t("Common.Reject"),
                    className: "error-text",
                    callback: () => self.rejectInterview()
                });

                if(!self.config.isSupervisor)
                {
                    menu.push({
                        name: self.$t("Common.Unapprove"),
                        callback: () => self.unapproveInterview()
                    });

                    menu.push({
                        className: "context-menu-separator context-menu-not-selectable"
                    });

                    menu.push({
                        name: self.$t("Common.Delete"),
                        className: "error-text",
                        callback: () => self.deleteInterview()
                    });
                }
            }

            return menu;
        },
        resetSelection() {
            this.selectedRows.splice(0, this.selectedRows.length);
        },

        addParamsToRequest(data) {
            data.status = (this.status || {}).key;
            data.questionnaireId = (this.questionnaireId || {}).key;
            data.questionnaireVersion = (this.questionnaireVersion || {}).key;
            data.responsibleId = (this.responsibleId || {}).key;
            data.responsibleName = (this.responsibleId || {}).value;

            if (this.assignmentId) { data.assignmentId = this.assignmentId; };
            if (this.unactiveDateStart) { data.unactiveDateStart = this.unactiveDateStart; };
            if (this.unactiveDateEnd) { data.unactiveDateEnd = this.unactiveDateEnd; };           

        },
        addParamsToRequestStatuses(data) {                        
            data.interviewId = this.selectedRowWithMenu.interviewId; 
        },
        clearAssignmentFilter() {
            this.assignmentId = null;
        },
        formatNumber(value) {
            if (value == null || value == undefined) return value;
            var language =
                (navigator.languages && navigator.languages[0]) || navigator.language || navigator.userLanguage;
            return value.toLocaleString(language);
        },

        reloadTable() {
            this.isLoading = true;
            this.selectedRows.splice(0, this.selectedRows.length);
            this.$refs.table.reload(self.reloadTable);

            this.addParamsToQueryString();
        },

        addParamsToQueryString() {
            var queryString = {};

            if (this.questionnaireId != null) {
                queryString.templateId = this.questionnaireId.key;
            }
            if (this.questionnaireVersion != null) {
                queryString.templateVersion = this.questionnaireVersion.key;
            }

            if (this.responsibleId) queryString.responsible = this.responsibleId.value;
            if (this.assignmentId) queryString.assignmentId = this.assignmentId;

            if (this.status) queryString.status = this.status.key;

            this.$router.push({ query: queryString });
        }
    },

    mounted() {

      this.unactiveDateStart = this.$route.query.unactiveDateStart;
      this.unactiveDateEnd = this.$route.query.unactiveDateEnd;

// load url params

      //this.unactiveDateStart = this.$route.query.unactiveDateStart;

/* 
        self.Url.query['templateId'] = self.QueryString['templateId'] || "";
        self.Url.query['templateVersion'] = self.QueryString['templateVersion'] || "";
        self.Url.query['status'] = self.QueryString['status'] || "";
        self.Url.query['responsible'] = self.QueryString['responsible'] || "";
        self.Url.query['searchBy'] = self.QueryString['searchBy'] || "";
        self.Url.query['assignmentId'] = self.QueryString['assignmentId'] || "";
        self.Url.query['unactiveDateStart'] = decodeURIComponent(self.QueryString['unactiveDateStart'] || "");
        self.Url.query['unactiveDateEnd'] = decodeURIComponent(self.QueryString['unactiveDateEnd'] || "");
        self.Url.query['teamId'] = self.QueryString['teamId'] || "";
*/

        this.reloadTable();
    }
};
</script>
