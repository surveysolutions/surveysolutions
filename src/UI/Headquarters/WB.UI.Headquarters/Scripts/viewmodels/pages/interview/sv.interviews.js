Supervisor.VM.SVInterviews = function (listViewUrl, interviewDetailsUrl, responsiblesUrl, usersToAssignUrl, commandExecutionUrl) {
    Supervisor.VM.SVInterviews.superclass.constructor.apply(this, arguments);
    var self = this;

    self.Users = self.CreateUsersViewModel(usersToAssignUrl);

    self.CanAssignTo = ko.computed(function() {
        return !(self.IsNothingSelected && _.isUndefined(self.Users.AssignTo()));
    });

    self.Assign = function () {

        var commandName = "AssignInterviewerCommand";
        var parametersFunc = function (item) { return { InterviewerId: self.Users.AssignTo().UserId, InterviewId: item.InterviewId } };

        var messageTemplateId = "#confirm-assign-template";
        var continueMessageTemplateId = "#confirm-continue-message-template";
        var onSuccessCommandExecuting = function() {
            self.Users.AssignTo(undefined);
        };
        var onCancelConfirmation = function () {
            self.Users.AssignTo(undefined);
        };

        var filteredItems = self.GetSelectedItemsAfterFilter(function(item) {
                                return item.CanBeReassigned()
                                    && !(item.Status() == 'InterviewerAssigned' && item.ResponsibleId() == self.Users.AssignTo().UserId);
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
        var rejectToInterviewerCommandName = "RejectInterviewToInterviewerCommand";
        var rejectCommandName = "RejectInterviewCommand";
        var messageTemplateId = "#confirm-reject-template";
        var continueMessageTemplateId = "#confirm-continue-message-template";

        var filteredItems = self.GetSelectedItemsAfterFilter(function (item) { return item.CanReject(); });
        var isNeedShowAssignInterviewers = filteredItems.some(function (item) { return item.IsNeedInterviewerAssign(); });
        var countReadyToReject = 0;
        for (var i = 0; i < filteredItems.length; i++) {
            countReadyToReject += (!filteredItems[i].IsNeedInterviewerAssign());
        }
        var countAllInterviewsToReject = filteredItems.length;
        var countInterviewsToReject = ko.observable(countReadyToReject);

        var model = {
            CountInterviewsToReject: countInterviewsToReject,
            Users: self.CreateUsersViewModel(usersToAssignUrl),
            StoreInteviewer: function () {
                model.Users.AssignTo() == undefined
                    ? countInterviewsToReject(countReadyToReject)
                    : countInterviewsToReject(countAllInterviewsToReject);
            },
            IsNeedShowAssignInterviewers: isNeedShowAssignInterviewers,
            ClearAssignTo: function () {
                model.Users.AssignTo(undefined);
                countInterviewsToReject(countReadyToReject);
            }
        }

        var messageHtml = $(messageTemplateId).html(); 

        if (filteredItems.length === 0) {
            bootbox.alert(messageHtml);
            return;
        }

        messageHtml += $(continueMessageTemplateId).html();

        bootbox.confirm(messageHtml, function (result) {
            if (result) {
                var interviewer = model.Users.AssignTo();
                $.each(filteredItems, function (index, interview) {
                    if (interview.IsNeedInterviewerAssign()) {
                        self.sendCommand(rejectToInterviewerCommandName,
                            function (interview) { return { InterviewId: interview.InterviewId, InterviewerId: interviewer.UserId } },
                            [ interview ]);
                    } else {
                        self.sendCommand(rejectCommandName,
                            function (interview) { return { InterviewId: interview.InterviewId } },
                            [ interview ]);
                    }
                });
                
            }
        });

        ko.applyBindings(model, $(".reject-interviewer")[0]);
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.SVInterviews, Supervisor.VM.InterviewsBase);