Supervisor.VM.SVInterviews = function (listViewUrl, interviewDetailsUrl, responsiblesUrl, usersToAssignUrl, commandExecutionUrl) {
    Supervisor.VM.SVInterviews.superclass.constructor.apply(this, arguments);
    var self = this;

    self.CreateUsersViewModel = function () {
        var users = {};
        users.IsAssignToLoading = ko.observable(false);
        users.UsersToAssignUrl = usersToAssignUrl;
        users.LoadUsers = function (query, sync, pageSize) {
            users.IsAssignToLoading(true);
            self.SendRequest(users.UsersToAssignUrl, { query: query, pageSize: pageSize }, function (response) {
                sync(response.Users, response.TotalCountByQuery);
            }, true, true, function () {
                users.IsAssignToLoading(false);
            });
        }
        users.AssignTo = ko.observable();
        return users;
    }

    self.Users = self.CreateUsersViewModel();

//    self.IsAssignToLoading = ko.observable(false);
//    self.UsersToAssignUrl = usersToAssignUrl;
//    self.Users = function (query, sync, pageSize) {
//        self.IsAssignToLoading(true);
//        self.SendRequest(self.UsersToAssignUrl, { query: query, pageSize: pageSize }, function (response) {
//            sync(response.Users, response.TotalCountByQuery);
//        }, true, true, function() {
//            self.IsAssignToLoading(false);
//        });
//    }
//    self.AssignTo = ko.observable();

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
//        self.sendCommandAfterFilterAndConfirm(
//            "RejectInterviewToInterviewerCommand",
//            function (item) { return { InterviewId: item.InterviewId } },
//            function (item) { return item.CanReject(); },
//            "#confirm-reject-template",
//            "#confirm-continue-message-template",
//            undefined,
//            undefined,
//            function(model) {
//                model.AssignTo = self.AssignTo;
//               // model.Assign = self.Assign;
//                model.AssignToInt = function () { alert('assign'); };
//                //model.Users = self.Users;
//                model.LoadUsers = function (query, sync, pageSize) { alert('users'); };;
//                model.IsNothingSelected = self.IsNothingSelected;
//                model.IsResponsiblesLoading = self.IsResponsiblesLoading;
//                model.InterviewsModel = self;
//            }
//        );

        var rejectToInterviewerCommandName = "RejectInterviewToInterviewerCommand";
        var rejectCommandName = "RejectInterviewCommand";
        var messageTemplateId = "#confirm-reject-template";
        var continueMessageTemplateId = "#confirm-continue-message-template";

        var filteredItems = self.GetSelectedItemsAfterFilter(function (item) { return item.CanReject(); });
        var model = {
            items: filteredItems,
            AssignTo: self.Users.AssignTo,
            // model.Assign = self.Assign;
            AssignToInt: function () { alert('assign'); },
            Users: self.Users.LoadUsers,
            LoadUsers: self.Users.LoadUsers,
            //LoadUsers: function (query, sync, pageSize) { alert('users'); },
            IsNothingSelected: self.IsNothingSelected,
            IsResponsiblesLoading: self.IsResponsiblesLoading,
            InterviewsModel: self
        }

        var messageHtml = self.getBindedHtmlTemplate(messageTemplateId, model);

        if (filteredItems.length === 0) {
            bootbox.alert(messageHtml);
            return;
        }

        messageHtml += $(continueMessageTemplateId).html();

        bootbox.confirm(messageHtml, function (result) {
            if (result) {
                $.each(filteredItems, function (index, interview) {
                    var commandName = interview.IsNeedInterviewerAssign() ? rejectToInterviewerCommandName : rejectCommandName;
                    var commandParamsSetter = function(item) {
                        return { InterviewId: item.InterviewId, InterviewerId: result.InterviewerId }
                    };
                    self.sendCommand(commandName, commandParamsSetter, interview);
                });
                
            }
        });

        ko.applyBindings(model, $(".reject-intervieweer")[0]);

        alert('test');
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.SVInterviews, Supervisor.VM.InterviewsBase);