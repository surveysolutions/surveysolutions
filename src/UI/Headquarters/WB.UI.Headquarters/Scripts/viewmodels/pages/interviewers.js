Supervisor.VM.Interviewers = function (listViewUrl, archiveUsersUrl, ajax, interviewersPageUrl, supervisorsUrl) {
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
    }

    self.MoveToAnotherTeam = function () {
        var commandName = "MoveInterviewerToAnotherTeamCommand";
        var messageTemplateId = "#move-interviewer-to-another-team-template";

        var selectedInterviewers = ko.utils.arrayFilter(self.Datatable.data(), function(i) {
            return self.SelectedItems().indexOf(i.userId) > -1;
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
            if (_.isUndefined(model.Users.AssignTo()))
                return;

            if (result) {
                if (model.InterviewersToMove().length > 0) {
                    var getParamsToAssignToAnotherTeam = function (interview) {
                        return {
                            OriginalSupervisorId: interview.supervisorId,
                            NewSupervisorId: model.Users.AssignTo().SupervisorId,
                            InterviewId: interview.InterviewId
                        }
                    };

                    var onSuccessCommandExecuting = function () {
                        model.Users.AssignTo(undefined);
                    };

                    self.sendCommand(commandName, getParamsToAssignToAnotherTeam, model.InterviewersToMove(), onSuccessCommandExecuting);
                }
            }
            else {
                model.Users.AssignTo(undefined);
            }
        });

        confirm.attr("id", "move-interviewer-confirmation");

        confirm.find(".alert.alert-warning").removeClass(".alert").removeClass(".alert-warning");

        ko.applyBindings(model, confirm[0]);
    };
};

Supervisor.Framework.Classes.inherit(Supervisor.VM.Interviewers, Supervisor.VM.EditableUsers);
