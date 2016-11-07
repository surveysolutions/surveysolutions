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
       
        var parametersFunc = function (item) { return { InterviewerId: self.AssignTo().UserId, InterviewId: item.InterviewId } };
        var filterFunc = function (item) {
            return item.CanBeReassigned()
                && !(item.Status() == 'InterviewerAssigned' && item.ResponsibleId() == self.AssignTo().UserId);
        };
        var messageTemplateId = "#confirm-assign-template";
        var continueMessageTemplateId = "#confirm-continue-message-template";
        var onSuccessCommandExecuting = function () {
            self.AssignTo(undefined);
        };
        var onCancelConfirmation = function () {
            self.AssignTo(undefined);
        };

        var filteredItems = self.GetSelectedItemsAfterFilter(function (item) {
            return item.CanBeReassigned()
                && !(item.Status() == 'InterviewerAssigned' && item.ResponsibleId() == self.AssignTo().UserId);
        });
        var receivedByInterviewerItems = _.filter(filteredItems, function (item) { return item.ReceivedByInterviewer() === true });

        var popupViewModel = {
            allItems: filteredItems,
            receivedItems: receivedByInterviewerItems
        };
        var messageHtml = self.getBindedHtmlTemplate(messageTemplateId, popupViewModel);

        if (filteredItems.length === 0) {
            bootbox.alert(messageHtml);
            return;
        }

        messageHtml += $(continueMessageTemplateId).html();

        bootbox.confirm(messageHtml, function (result) {
            if (result) {
                var itemsThatShouldBeReassigned = [];
                if ($("#reassignReceivedByInterviewer").is(':checked')) {
                    itemsThatShouldBeReassigned = filteredItems;
                } else {
                    itemsThatShouldBeReassigned = _.filter(filteredItems, function (item) { return item.ReceivedByInterviewer() === false });
                }

                if (itemsThatShouldBeReassigned.length > 0) {
                    self.sendCommand(commandName, parametersFunc, itemsThatShouldBeReassigned, onSuccessCommandExecuting);
                }
            } else {
                if (!_.isUndefined(onCancelConfirmation)) {
                    onCancelConfirmation();
                }
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