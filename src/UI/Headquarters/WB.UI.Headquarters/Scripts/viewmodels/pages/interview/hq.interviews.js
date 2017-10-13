Supervisor.VM.HQInterviews = function (listViewUrl, interviewDetailsUrl, responsiblesUrl, commandExecutionUrl, usersToAssignUrl, notifier) {
    Supervisor.VM.HQInterviews.superclass.constructor.apply(this, [listViewUrl, interviewDetailsUrl, responsiblesUrl, null, commandExecutionUrl, notifier]);

    var self = this;

    self.DeleteInterview = function () {
        self.sendCommandAfterFilterAndConfirm(
            "DeleteInterviewCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function(item) { return item.CanDelete(); },
            "#confirm-delete-template",
            "#confirm-continue-message-template"
        );
    };
    self.Assign = function () {
        var commandName = "AssignResponsibleCommand";
        var messageTemplateId = "#confirm-assign-to-other-team-template";
        var continueMessageTemplateId = "#confirm-continue-message-template";
      
        var eligibleSelectedItems = self.GetSelectedItemsAfterFilter(function (item) {
            return item.CanBeReassigned() && item.ResponsibleId() !== self.AssignTo().ResponsibleId;
        });
        
        var popupViewModel = {
            eligibleSelectedItems: eligibleSelectedItems
        };

        var messageHtml = self.getBindedHtmlTemplate(messageTemplateId, popupViewModel);

        if (eligibleSelectedItems.length === 0) {
            bootbox.alert(messageHtml);
            return;
        }

        messageHtml += $(continueMessageTemplateId).html();

        bootbox.confirm(messageHtml, function (result) {
            if (result) {
                var itemsThatShouldBeReassigned = eligibleSelectedItems;

                if (itemsThatShouldBeReassigned.length > 0) {
                    var getParamsToAssignToOtherTeam = function(interview) {
                        return {
                            SupervisorId: self.AssignTo().SupervisorId === self.AssignTo().ResponsibleId ? self.AssignTo().SupervisorId : null,
                            InterviewerId: self.AssignTo().InterviewerId,
                            InterviewId: interview.InterviewId
                        }
                    };

                    var onSuccessCommandExecuting = function () {
                        self.AssignTo(undefined);
                    };

                    self.sendCommand(commandName, getParamsToAssignToOtherTeam, itemsThatShouldBeReassigned, onSuccessCommandExecuting);
                }
            }
            else {
                self.AssignTo(undefined);
            }
        });
    };

    self.ApproveInterview = function () {
        self.sendCommandAfterFilterAndConfirm(
            "HqApproveInterviewCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function (item) { return item.CanApprove(); },
            "#confirm-approve-template",
            "#confirm-continue-message-template"
        );
    };

    self.RejectInterview = function () {
        self.sendCommandAfterFilterAndConfirm(
            "HqRejectInterviewCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function (item) { return item.CanReject(); },
            "#confirm-reject-template",
            "#confirm-continue-message-template"
        );
    };

    self.UnapproveInterview = function () {
        self.sendCommandAfterFilterAndConfirm(
            "UnapproveByHeadquarterCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function (item) { return item.CanUnapprove(); },
            "#confirm-unapprove-template",
            "#confirm-continue-message-template"
        );
    };

    self.AssignInterview = function () {
        var commandName = "AssignResponsibleCommand";
        var messageTemplateId = "#assign-interview-template";
        var continueMessageTemplateId = "#confirm-continue-message-template";

        var eligibleSelectedItems = self.GetSelectedItemsAfterFilter(function (item) {
            return item.CanBeReassigned();
        });

        var countInterviewsToAssign = ko.observable(0);

        var model = {
            CountInterviewsToAssign: countInterviewsToAssign,
            Users: self.CreateUsersViewModel(usersToAssignUrl),
            StoreInteviewer: function () {
                model.Users.AssignTo() == undefined
                    ? countInterviewsToAssign(0)
                    : countInterviewsToAssign(eligibleSelectedItems.length);
            },
            ClearAssignTo: function () {
                model.Users.AssignTo(undefined);
                countInterviewsToAssign(0);
            }
        };

        var messageHtml = self.getBindedHtmlTemplate(messageTemplateId, model);

        if (eligibleSelectedItems.length === 0) {
            notifier.alert('', messageHtml);
            return;
        }

        messageHtml += $(continueMessageTemplateId).html();

        notifier.confirm('Confirmation Needed', messageHtml, function (result) {

            if (_.isUndefined(model.Users.AssignTo()))
                return;

            if (result) {
                var itemsThatShouldBeReassigned = eligibleSelectedItems;

                if (itemsThatShouldBeReassigned.length > 0) {
                    var getParamsToAssignToOtherTeam = function (interview) {
                        return {
                            SupervisorId: model.Users.AssignTo().SupervisorId === model.Users.AssignTo().ResponsibleId ? model.Users.AssignTo().SupervisorId: null,
                            InterviewerId: model.Users.AssignTo().InterviewerId,
                            InterviewId: interview.InterviewId
                        }
                    };

                    var onSuccessCommandExecuting = function () {
                        model.Users.AssignTo(undefined);
                    };

                    self.sendCommand(commandName, getParamsToAssignToOtherTeam, itemsThatShouldBeReassigned, onSuccessCommandExecuting);
                }
            }
            else {
                model.Users.AssignTo(undefined);
            }
        });

        ko.applyBindings(model, $(".assign-interviewer")[0]);
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.HQInterviews, Supervisor.VM.InterviewsBase);