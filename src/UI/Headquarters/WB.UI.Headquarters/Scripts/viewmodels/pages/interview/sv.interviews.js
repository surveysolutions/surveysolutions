Supervisor.VM.SVInterviews = function (listViewUrl, interviewDetailsUrl, responsiblesUrl, usersToAssignUrl, commandExecutionUrl) {
    Supervisor.VM.SVInterviews.superclass.constructor.apply(this, arguments);
    var self = this;

    self.IsAssignToLoading = ko.observable(false);
    self.UsersToAssignUrl = usersToAssignUrl;
    self.Users = function (query, sync, pageSize) {
        self.IsAssignToLoading(true);
        self.SendRequest(self.UsersToAssignUrl, { query: query, pageSize: pageSize }, function (response) {
            sync(response.Users, response.TotalCountByQuery);
        }, true, true, function() {
            self.IsAssignToLoading(false);
        });
    }
    self.AssignTo = ko.observable();

    self.CanAssignTo = ko.computed(function() {
        return !(self.IsNothingSelected && _.isUndefined(self.AssignTo()));
    });

    self.Assign = function () {

        var commandName = "AssignInterviewerCommand";
        var parametersFunc = function(item) { return { InterviewerId: self.AssignTo().UserId, InterviewId: item.InterviewId } };
        var filterFunc = function(item) {
            return item.CanBeReassigned()
                && !(item.Status() == 'InterviewerAssigned' && item.ResponsibleId() == self.AssignTo().UserId);
        };
        var messageTemplateId = "#confirm-assign-template";
        var continueMessageTemplateId = "#confirm-continue-message-template";
        var onSuccessCommandExecuting = function() {
            self.AssignTo(undefined);
        };
        var onCancelConfirmation = function () {
            self.AssignTo(undefined);
        };

        var filteredItems = self.GetSelectedItemsAfterFilter(function(item) {
                                return item.CanBeReassigned()
                                    && !(item.Status() == 'InterviewerAssigned' && item.ResponsibleId() == self.AssignTo().UserId);
                            });
        var receivedByInterviewerItems = _.filter(filteredItems, function(item) { return item.ReceivedByInterviewer() === true });

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
            "ApproveInterviewCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function (item) { return item.CanApprove(); },
            "#confirm-approve-template",
            "#confirm-continue-message-template"
        );
    };

    self.RejectInterview = function () {
        self.sendCommandAfterFilterAndConfirm(
            "RejectInterviewCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function (item) { return item.CanReject(); },
            "#confirm-reject-template",
            "#confirm-continue-message-template"
        );
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.SVInterviews, Supervisor.VM.InterviewsBase);