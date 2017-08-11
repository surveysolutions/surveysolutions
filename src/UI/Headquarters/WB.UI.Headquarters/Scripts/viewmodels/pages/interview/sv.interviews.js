Supervisor.VM.SVInterviews = function (listViewUrl, interviewDetailsUrl, responsiblesUrl, usersToAssignUrl, commandExecutionUrl, notifier) {
    Supervisor.VM.SVInterviews.superclass.constructor.apply(this, arguments);
    var self = this;

    self.Users = self.CreateUsersViewModel(usersToAssignUrl);

    self.CanAssignTo = ko.computed(function() {
        return !(self.IsNothingSelected && _.isUndefined(self.Users.AssignTo()));
    });

    self.AssignInterview = function () {

        var commandName = "AssignInterviewerCommand";

        var messageTemplateId = "#confirm-assign-template";
        var continueMessageTemplateId = "#confirm-continue-message-template";
        var onSuccessCommandExecuting = function() {
            self.Users.AssignTo(undefined);
        };
        var onCancelConfirmation = function () {
            self.Users.AssignTo(undefined);
        };

        var filteredItems = self.GetSelectedItemsAfterFilter(function(item) { return item.CanBeReassigned(); });
        var receivedByInterviewerItemsCount = _.filter(filteredItems, function (item) { return item.ReceivedByInterviewer() === true }).length;
        var countInterviewsToAssign = ko.observable(0);

        var model = {
            IsExistsItemsToAssign: filteredItems.length > 0,
            CountInterviewsToAssign: countInterviewsToAssign,
            CountReceivedByInterviewerItems: receivedByInterviewerItemsCount,
            IsReassignReceivedByInterviewer: ko.observable(false),
            Users: self.CreateUsersViewModel(usersToAssignUrl),
            StoreInteviewer: function () { model.RecalculateCountInterviewsToAssign(); },
            RecalculateCountInterviewsToAssign: function() {
                var itemsThatShouldBeReassigned = model.GetListInterviewsToAssign();
                countInterviewsToAssign(itemsThatShouldBeReassigned.length);
            },
            GetListInterviewsToAssign: function () {

                var itemsThatShouldBeReassigned = [];

                if (model.Users.AssignTo() == undefined) {
                    return itemsThatShouldBeReassigned;
                }

                if (model.IsReassignReceivedByInterviewer() == true) {
                    itemsThatShouldBeReassigned = filteredItems;
                } else {
                    itemsThatShouldBeReassigned = _.filter(filteredItems, function (item) { return item.ReceivedByInterviewer() === false });
                }

                itemsThatShouldBeReassigned = _.filter(itemsThatShouldBeReassigned,
                    function (item) {
                        return !(item.Status() == 'InterviewerAssigned' && item.ResponsibleId() == model.Users.AssignTo().UserId);
                    });

                return itemsThatShouldBeReassigned;
            },
            ClearAssignTo: function () {
                model.Users.AssignTo(undefined);
                countInterviewsToAssign(0);
            }
        };
        model.IsReassignReceivedByInterviewer.subscribe(function () { model.RecalculateCountInterviewsToAssign(); });


        var messageHtml = self.getBindedHtmlTemplate(messageTemplateId, model);

        if (filteredItems.length === 0) {
            notifier.alert('', messageHtml);
            return;
        }

        messageHtml += $(continueMessageTemplateId).html();

        notifier.confirm('', messageHtml, function (result) {
            if (result) {
                var itemsThatShouldBeReassigned = model.GetListInterviewsToAssign();

                var parametersFunc = function (item) { return { InterviewerId: model.Users.AssignTo().UserId, InterviewId: item.InterviewId } };

                if (itemsThatShouldBeReassigned.length > 0) {
                    self.sendCommand(commandName, parametersFunc, itemsThatShouldBeReassigned, onSuccessCommandExecuting);
                }
            } else {
                if (!_.isUndefined(onCancelConfirmation)) {
                    onCancelConfirmation();
                }
            }
        });

        ko.applyBindings(model, $(".assign-interviewer")[0]);
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

        var messageHtml = self.getBindedHtmlTemplate(messageTemplateId, model);

        if (filteredItems.length === 0) {
            notifier.alert('', messageHtml);
            return;
        }

        messageHtml += $(continueMessageTemplateId).html();

        notifier.confirm('', messageHtml, function (result) {
            if (result) {
                var interviewer = model.Users.AssignTo();
                $.each(filteredItems, function (index, interview) {
                    if (interview.IsNeedInterviewerAssign()) {
                        if (interviewer != undefined)
                        {
                            self.sendCommand(rejectToInterviewerCommandName,
                                function (interview) { return { InterviewId: interview.InterviewId, InterviewerId: interviewer.UserId } },
                                [interview]);
                        }
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

    self.BuildMenuItem = function () {
       
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.SVInterviews, Supervisor.VM.InterviewsBase);