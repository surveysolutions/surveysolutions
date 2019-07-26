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
            v-if="selectedRows.length"
            @click="unapproveInterview">{{ $t("Common.Unapprove")}}</button>
          <button
            class="btn btn-link"
            v-if="selectedRows.length"
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
          <p>{{ $t("Interviews.AssignToOtherTeamConfirmMessage", {count: selectedRows.length}, "SV approved", "HQ approved" )}}</p>
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
        <p>{{ $t("Interviews.DeleteConfirmMessageHQ", {count: selectedRows.length}, "SupervisorAssigned", "InterviewerAssigned" )}}</p>
      </div>
      <div slot="actions">
        <button
          type="button"
          class="btn btn-primary"
          @click="deleteInterviews"
        >{{ $t("Common.Delete") }}</button>
        <button type="button" class="btn btn-link" data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
      </div>
    </ModalFrame>

    <ModalFrame ref="approveModal">
      <form onsubmit="return false;">
        <div class="action-container">
          <p>{{ $t("Interviews.ApproveConfirmMessageHQ", {count: selectedRows.length}, "Completed", "ApprovedBySupervisor" )}}</p>
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
          <p>{{ $t("Interviews.RejectConfirmMessageHQ", {count: selectedRows.length}, "Completed", "ApprovedBySupervisor" )}}</p>
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
          <p>{{ $t("Interviews.UnapproveConfirmMessageHQ", {count: selectedRows.length}, "ApprovedByHeadquarters")}}</p>
        </div>
      </form>
      <div slot="actions">
        <button
          type="button"
          class="btn btn-primary"
          @click="unapproveInterviews"
        >{{ $t("Common.Unapprove") }}</button>
        <button type="button" class="btn btn-link" data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
      </div>
    </ModalFrame>

    <ModalFrame ref="statusHistory">
      <div class="action-container">
        <h3>{{ $t("Pages.HistoryOfStatuses_Interview")}}</h3>
      </div>

      <div slot="actions">
        <button
          type="button"
          class="btn btn-primary"
          @click="viewInterview"
        >{{ $t("Pages.HistoryOfStatuses_ViewInterview") }}</button>
        <button type="button" class="btn btn-link" data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
      </div>
    </ModalFrame>

    <!--script type="text/html" id="interview-status-history-template">
    <div class="modal fade" id="statusHistoryModal" tabindex="-1" role="dialog" aria-labelledby="statusHistoryModal">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true"></span></button>
                    <h3>@Pages.HistoryOfStatuses_Interview</h3>
                    <p><a data-bind="attr: { href: interviewUrl, title: key },text: key" class="title-row"></a> by <span href="#" data-bind="text: responsible, css: { interviewer: isResponsibleInterviewer, supervisor: !isResponsibleInterviewer}"></span></p>
                </div>
                <div class="modal-body">
                    <h3>@Pages.HistoryOfStatuses_Title</h3>
                    <div class="table-with-scroll">
                        <table class="table table-striped history">
                            <thead>
                            <tr>
                                <td>@Pages.HistoryOfStatuses_State</td>
                                <td>@Pages.HistoryOfStatuses_On</td>
                                <td>@Pages.HistoryOfStatuses_By</td>
                                <td>@Pages.HistoryOfStatuses_AssignedTo</td>
                                <td>@Pages.HistoryOfStatuses_Comment</td>
                            </tr>
                            </thead>
                            <tbody  data-bind="foreach:statusHistory">
                            <tr>
                                <td data-bind="text: StatusHumanized"></td>
                                <td class="date" data-bind="text: $root.formatDate(Date)"></td>
                                <td><span data-bind="text: Responsible, attr: { 'class' : ResponsibleRole }"></span></td>
                                <td><span data-bind="text: Assignee, attr: { 'class' : AssigneeRole }"></span></td>
                                <td data-bind="text: Comment"></td>
                            </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                <div class="modal-footer">
                    <a class="btn btn-link" data-bind="attr: { href: interviewUrl, title: key }">@Pages.HistoryOfStatuses_ViewInterview</a>
                    <button type="button" class="btn btn-link" data-dismiss="modal">@Pages.CloseLabel</button>
                </div>
            </div>
        </div>
    </div>
    </script-->

    <Confirm ref="confirmRestart" id="restartModal" slot="modals">
      {{ $t("Pages.InterviewerHq_RestartConfirm") }}
      <FilterBlock>
        <div class="form-group">
          <div class="field">
            <input class="form-control with-clear-btn" type="text" v-model="restart_comment" />
          </div>
        </div>
      </FilterBlock>
    </Confirm>

    <Confirm
      ref="confirmDiscard"
      id="discardConfirm"
      slot="modals"
    >{{ $t("Pages.InterviewerHq_DiscardConfirm") }}</Confirm>
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
            totalRows: 0,
            assignmentId: null,
            responsibleId: null,
            responsibleParams: { showArchived: true, showLocked: true },
            newResponsibleId: null,
            statusChangeComment: null,
            status: null,
            selectedStatus: null,
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
                    orderable: true
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
            var id = this.selectedRows[0];
            window.location = self.config.interviewReviewUrl + "/" + id.replace(/-/g, "");
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
          var selectedItems = this.$refs.table.table.rows( { selected: true } ).data();

          var filteredItems = this.arrayFilter(selectedItems,
                function (item) 
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
                type: "AssignResponsibleCommand",
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

        approveInterviews() {
          const self = this;
          var selectedItems = this.$refs.table.table.rows( { selected: true } ).data();

          var filteredItems = this.arrayFilter(selectedItems,
                function (item) 
                {
                    var value = item.canApprove;
                    return !isNaN(value) && value;
            });

            if(filteredItems.length == 0)
            {
              this.$refs.approveModal.hide();
              return;
            }

            var command = this.getCommand("HqApproveInterviewCommand", _.map(filteredItems, question => {return question.interviewId;}), this.statusChangeComment);

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
          var selectedItems = this.$refs.table.table.rows( { selected: true } ).data();

            var filteredItems = this.arrayFilter(selectedItems,
                function (item) 
                {
                    var value = item.canReject;
                    return !isNaN(value) && value;
            });

            if(filteredItems.length == 0)
            {
              this.$refs.rejectModal.hide();
              return;
            }

            var command = this.getCommand("HqRejectInterviewCommand", _.map(filteredItems, question => {return question.interviewId;}), this.statusChangeComment);

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
            var selectedItems = this.$refs.table.table.rows( { selected: true } ).data();

            var filteredItems = this.arrayFilter(selectedItems,
                function (item) 
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
          var selectedItems = this.$refs.table.table.rows( { selected: true } ).data();

          var filteredItems = this.arrayFilter(selectedItems,
                function (item) 
                {
                    var value = item.canDelete;
                    return !isNaN(value) && value;
            });

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
            this.$refs.deleteModal.modal({
                keyboard: false
            });
        },
        newResponsibleSelected(newValue) {
            this.newResponsibleId = newValue;
        },

        showStatusHistory(interviewId) {
            this.$refs.statusHistory.modal({
                keyboard: false
            });

            //'@Url.RouteUrl("DefaultApiWithAction", new {httproute = "", controller = "InterviewApi", action = "ChangeStateHistory"})'

            /*
            self.SendRequest(url,
            { interviewId: interview.InterviewId() },
            function(statusHistory) {

                $(modalId).parent().remove();

                var historyModalModel = {
                    key: interview.Key(),
                    interviewUrl: $detailsUrl + '/' + interview.InterviewId(),
                    responsible: interview.ResponsibleName(),
                    isResponsibleInterviewer: interview.ResponsibleRole() === 4,
                    statusHistory: statusHistory,
                    formatDate: function(date) {
                        return moment.utc(date).local().format('MMM DD, YYYY HH:mm');
                    }
                };

                $('body').append($("<div/>").html($(statusHistoryTemplateId).html())[0]);

                ko.applyBindings(historyModalModel, $(modalId)[0]);

                $(modalId).modal('show', { backdrop: true });
            },
            false,
            false);*/
        },

        contextMenuItems({ rowData, rowIndex }) {
            const menu = [];
            const self = this;

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
                    callback: () => self.config.profileUrl + "/" + rowData.responsibleId
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
            
            if (this.assignmentId) { data.assignmentId = this.assignmentId; };
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
                queryString.QuestionnaireId = this.questionnaireId.value;
            }
            if (this.questionnaireVersion != null) {
                queryString.questionnaireVersion = this.questionnaireVersion.key;
            }

            if (this.responsibleId) queryString.responsible = this.responsibleId.value;
            if (this.assignmentId) queryString.assignmentId = this.assignmentId;

            if (this.status) queryString.status = this.status.key;

            this.$router.push({ query: queryString });
        }
    },

    mounted() {

// load url params

      //this.userRole = this.$route.query.userRole;

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
