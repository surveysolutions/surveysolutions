Supervisor.VM.HQInterviews = function (listViewUrl, interviewDetailsUrl, responsiblesUrl, commandExecutionUrl, usersToAssignUrl, notifier) {
    Supervisor.VM.HQInterviews.superclass.constructor.apply(this, [listViewUrl, interviewDetailsUrl, responsiblesUrl, null, commandExecutionUrl, notifier]);

    var self = this;

    self.DeleteInterview = function (selectedRowAsArray) {
        self.sendCommandAfterFilterAndConfirm(
            selectedRowAsArray,
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
                            SupervisorId: self.AssignTo().SupervisorId,
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

    self.ApproveInterview = function (selectedRowAsArray) {
        self.sendCommandAfterFilterAndConfirm(
            selectedRowAsArray,
            "HqApproveInterviewCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function (item) { return item.CanApprove(); },
            "#confirm-approve-template",
            "#confirm-continue-message-template"
        );
    };

    self.RejectInterview = function (selectedRowAsArray) {
        self.sendCommandAfterFilterAndConfirm(
            selectedRowAsArray,
            "HqRejectInterviewCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function (item) { return item.CanReject(); },
            "#confirm-reject-template",
            "#confirm-continue-message-template"
        );
    };

    self.UnapproveInterview = function (selectedRowAsArray) {
        self.sendCommandAfterFilterAndConfirm(
            selectedRowAsArray,
            "UnapproveByHeadquarterCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function (item) { return item.CanUnapprove(); },
            "#confirm-unapprove-template",
            "#confirm-continue-message-template"
        );
    };

    self.AssignInterview = function (selectedRowAsArray) {
        var commandName = "AssignResponsibleCommand";
        var messageTemplateId = "#assign-interview-template";
        var continueMessageTemplateId = "#confirm-continue-message-template";

        var eligibleSelectedItems = self.GetSelectedItemsAfterFilter(selectedRowAsArray, function (item) {
            return item.CanBeReassigned();
        });

        var countInterviewsToAssign = ko.observable(0);
        var receivedByInterviewerItemsCount = _.filter(eligibleSelectedItems, function (item) { return item.ReceivedByInterviewer() === true }).length;

        var model = {
            CountInterviewsToAssign: countInterviewsToAssign,
            Users: self.CreateUsersViewModel(usersToAssignUrl),
            CountReceivedByInterviewerItems: receivedByInterviewerItemsCount,
            IsReassignReceivedByInterviewer: ko.observable(false),
            StoreInteviewer: function () {
                model.RecalculateCountInterviewsToAssign();
            },
            RecalculateCountInterviewsToAssign: function() {
                var itemsThatShouldBeReassigned = model.GetListInterviewsToAssign();
                countInterviewsToAssign(itemsThatShouldBeReassigned.length);
            },
            ClearAssignTo: function () {
                model.Users.AssignTo(undefined);
                countInterviewsToAssign(0);
            },
            GetListInterviewsToAssign: function () {

                var itemsThatShouldBeReassigned = [];

                if (model.Users.AssignTo() == undefined) {
                    return itemsThatShouldBeReassigned;
                }

                if (model.IsReassignReceivedByInterviewer() == true) {
                    itemsThatShouldBeReassigned = eligibleSelectedItems;
                } else {
                    itemsThatShouldBeReassigned = _.filter(eligibleSelectedItems, function (item) { return item.ReceivedByInterviewer() === false });
                }

                itemsThatShouldBeReassigned = _.filter(itemsThatShouldBeReassigned,
                    function (item) {
                        return !(item.Status() == 'InterviewerAssigned' && item.ResponsibleId() == model.Users.AssignTo().UserId);
                    });

                return itemsThatShouldBeReassigned;
            }
        };
        model.IsReassignReceivedByInterviewer.subscribe(function () { model.RecalculateCountInterviewsToAssign(); });

        var messageHtml = self.getBindedHtmlTemplate(messageTemplateId, model);

        if (eligibleSelectedItems.length === 0) {
            notifier.alert('', messageHtml);
            return;
        }

        messageHtml += $(continueMessageTemplateId).html();

        notifier.confirm('', messageHtml, function (result) {

            if (_.isUndefined(model.Users.AssignTo()))
                return;

            if (result) {
                var itemsThatShouldBeReassigned = model.GetListInterviewsToAssign();

                if (itemsThatShouldBeReassigned.length > 0) {
                    var getParamsToAssignToOtherTeam = function (interview) {
                        return {
                            SupervisorId: model.Users.AssignTo().SupervisorId,
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
