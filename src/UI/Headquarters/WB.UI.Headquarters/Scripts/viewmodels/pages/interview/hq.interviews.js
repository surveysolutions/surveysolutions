Supervisor.VM.HQInterviews = function (listViewUrl, interviewDetailsUrl, responsiblesUrl, commandExecutionUrl, usersToAssignUrl) {
    Supervisor.VM.HQInterviews.superclass.constructor.apply(this, [listViewUrl, interviewDetailsUrl, responsiblesUrl, null, commandExecutionUrl]);

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
    self.IsAssignToLoading = ko.observable(false);
    self.AssignTo = ko.observable();
    self.UsersToAssignUrl = usersToAssignUrl;
    self.Users = function (query, sync, pageSize) {
        self.IsAssignToLoading(true);
        self.SendRequest(self.UsersToAssignUrl, { query: query, pageSize: pageSize }, function (response) {
            sync(response.Users, response.TotalCountByQuery);
        }, true, true, function () {
            self.IsAssignToLoading(false);
        });
    }
    self.Assign = function () {
        var commandName = "AssignSupervisorCommand";
        var messageTemplateId = "#confirm-assign-to-other-team-template";
        var continueMessageTemplateId = "#confirm-continue-message-template";
      
        var eligibleSelectedItems = self.GetSelectedItemsAfterFilter(function (item) {
            return item.CanAssingToOtherTeam() && (item.ResponsibleId() !== self.AssignTo().UserId);
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
                            SupervisorId: self.AssignTo().UserId,
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
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.HQInterviews, Supervisor.VM.InterviewsBase);