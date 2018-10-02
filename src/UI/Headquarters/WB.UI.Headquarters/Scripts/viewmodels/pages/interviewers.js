Supervisor.VM.Interviewers = function (listViewUrl, archiveUsersUrl, ajax, interviewersPageUrl, supervisorsUrl, $moveUserToAnotherTeamUrl) {
    Supervisor.VM.Interviewers.superclass.constructor.apply(this, [listViewUrl, archiveUsersUrl, ajax]);

    var self = this;

    self.Url = new Url(interviewersPageUrl);

    self.SupervisorUrl = supervisorsUrl;
    self.IsSupervisorsLoading = ko.observable(false);
    self.Supervisors = function (query, sync, pageSize) {
        self.IsSupervisorsLoading(true);
        self.SendRequest(self.SupervisorUrl, { query: query, pageSize: pageSize, showLocked: true }, function (response) {
            sync(response.Users, response.TotalCountByQuery);
        }, true, true, function () {
            self.IsSupervisorsLoading(false);
        });
    }
    self.SelectedSupervisor = ko.observable();

    self.Archived = ko.observable(false);
    self.Facet = ko.observable();

    self.GetFilterMethod = function () {

        return {
            SupervisorName : _.isUndefined(self.SelectedSupervisor())
                                ? null
                                : self.SelectedSupervisor().UserName,
            Archived : self.Archived(),
            Facet: self.Facet()
        }
    };

    var formatNames = function (interviewers, limit) {
        limit = limit || 3;

        var names = ko.utils.arrayMap(interviewers || [], function (i) { return i.userName; });

        if (names == null || names == undefined || names.length === 0)
            return '';

        if (names.length === 1)
            return names[0];

        if (names.length <= limit) {
            var sliceLength = Math.min(limit, names.length);
            return self.namesFormatLessThanLimit.format(names.slice(0, sliceLength - 1).join(', '), names[sliceLength - 1]);
        }

        return self.namesFormatMoreThanLimit.format(names.slice(0, limit).join(', '), names.length - limit);
    };

    self.load = function () {
        if (self.QueryString['supervisor'] != null) {
            self.SelectedSupervisor({ UserName: self.QueryString['supervisor'] });
        }

        self.Archived(self.QueryString['archived']);
        self.Facet(self.QueryString['Facet'] || null);

        self.Url.query['supervisor'] = self.QueryString['supervisor'] || "";
        self.Url.query['archived'] = self.QueryString['archived'] || "";
        self.Url.query['Facet'] = self.QueryString['Facet'] || "";

        setTimeout(function() {
            $('.facet').selectpicker('val', self.Facet());
        }, 300);

        self.SelectedSupervisor.subscribe(self.reloadDataTable);
        self.Archived.subscribe(self.reloadDataTable);
        self.Facet.subscribe(self.reloadDataTable);
        
        self.initDataTable(this.onDataTableDataReceived, this.onTableInitComplete);
    };

    var showMoveInterviewersProgress = function (interviewers, supervisor, whatToDoWithAssignments) {
        var messageTemplateId = "#move-interviewer-progress-template";

        var model = {
            Interviewers: ko.observableArray(interviewers),
            Supervisor: ko.observable(supervisor)
        };

        ko.utils.arrayForEach(model.Interviewers(),
            function (interviewer) {
                interviewer.inProgress = ko.observable(false);
                interviewer.processed = ko.observable(false);
                interviewer.interviewsProcessed = ko.observable("-");
                interviewer.interviewsProcessedWithErrors = ko.observable("-");
                interviewer.assignmentsProcessed = ko.observable("-");
                interviewer.assignmentsProcessedWithErrors = ko.observable("-");
                interviewer.errors = ko.observableArray([]);
            });

        model.MoveInterviewers = function (modal) {
            var createRequest = function (index) {
                if (index >= interviewers.length) {
                    modal.update({
                        title: self.movingCompleted,
                        buttons: {
                            closer: true
                        }
                    });
                    self.reloadDataTable();
                    return;
                }

                var interviewer = model.Interviewers()[index];
                interviewer.inProgress(true);

                var request = {
                    InterviewerId: interviewer.userId,
                    OldSupervisorId: interviewer.supervisorId,
                    NewSupervisorId: supervisor.UserId,
                    Mode: whatToDoWithAssignments
                };

                ajax.sendRequest($moveUserToAnotherTeamUrl,
                    "post",
                    request,
                    false,
                    // onSuccess
                    function (data) {
                        interviewer.inProgress(false);
                        interviewer.processed(true);
                        interviewer.interviewsProcessed(data.InterviewsProcessed);
                        interviewer.interviewsProcessedWithErrors(data.InterviewsProcessedWithErrors);
                        interviewer.assignmentsProcessed(data.AssignmentsProcessed);
                        interviewer.assignmentsProcessedWithErrors(data.AssignmentsProcessedWithErrors);
                        interviewer.errors(data.Errors);
                    },
                    //onDone
                    function () {
                        setTimeout(function () {
                            createRequest(index + 1);
                        }, 500);
                    }
                );
            }

            createRequest(0);
        }

        var messageTemplate = $("<div/>").html($(messageTemplateId).html())[0];
        var messageHtml = $(messageTemplate).html();

        var modal = notifier.modal(self.movingIsInProgress, messageHtml);

        modal.get().attr("id", "move-interviewer-progress");

        ko.applyBindings(model, modal.get()[0]);

        model.MoveInterviewers(modal);
    };

    self.MoveToAnotherTeam = function () {
        var messageTemplateId = "#move-interviewer-to-another-team-template";

        var selectedUsers = ko.utils.arrayFilter(self.Datatable.data(), function(i) {
            return self.SelectedItems().indexOf(i.userId) > -1;
        });

        var selectedInterviewers = ko.utils.arrayMap(selectedUsers, function(interviewer) {
            return ko.toJS(interviewer);
        });

        var countInterviewersToMove = ko.observable(0);

        var model = {
            CountInterviewersToAssign: countInterviewersToMove,
            Users: self.CreateUsersViewModel(supervisorsUrl),
            UpdateCounter: function () {
                (model.Users.AssignTo() == undefined)
                    ? countInterviewersToMove(0)
                    : countInterviewersToMove(selectedInterviewers.length);
            },
            ClearSelectedSupervisor: function () {
                model.Users.AssignTo(undefined);
                countInterviewersToMove(0);
            },
            WhatToDoWithAssignments: ko.observable("ReassigntToOriginalSupervisor"),
            SelectedSupervisor: ko.observable(),
            InterviewersToMove: ko.observableArray([]),
            InterviewersToStay: ko.observableArray([])
        };

        model.InterviewersToMoveNamesOnly = ko.observable();
        model.InterviewersToStayNamesOnly = ko.observable();

        model.Users.AssignTo.subscribe(function(newValue) {
            var isSuvervisorSelected = newValue != undefined && newValue != null;
            model.InterviewersToMove.removeAll();
            model.InterviewersToStay.removeAll();

            if (isSuvervisorSelected) {
                model.SelectedSupervisor(newValue.UserName);
                ko.utils.arrayForEach(selectedInterviewers,
                    function(interviewer) {
                        if (interviewer.supervisorName === newValue.UserName) {
                            model.InterviewersToStay.push(interviewer);
                        } else {
                            model.InterviewersToMove.push(interviewer);
                        }
                    });

                model.InterviewersToMoveNamesOnly(formatNames(model.InterviewersToMove()));
                model.InterviewersToStayNamesOnly(formatNames(model.InterviewersToStay()));
            } else {
                model.SelectedSupervisor(undefined);
            }
        });

        var messageHtml = self.getBindedHtmlTemplate(messageTemplateId, model);

        var title = self.moveInterviewerPopupTitle.format(formatNames(selectedInterviewers));

        var confirm = notifier.confirm(title, messageHtml, function (result) {
            // add error message
            if (_.isUndefined(model.Users.AssignTo()))
                return;

            if (_.isEmpty(model.WhatToDoWithAssignments()))
                return;

            if (result) {
                if (model.InterviewersToMove().length > 0) {
                    showMoveInterviewersProgress(model.InterviewersToMove(),
                        model.Users.AssignTo(),
                        model.WhatToDoWithAssignments());
                }
            }
        });

        confirm.attr("id", "move-interviewer-confirmation");

        ko.applyBindings(model, confirm[0]);
    };
};

Supervisor.Framework.Classes.inherit(Supervisor.VM.Interviewers, Supervisor.VM.EditableUsers);
