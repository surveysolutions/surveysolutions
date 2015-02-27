Supervisor.VM.SVInterviews = function (listViewUrl, interviewDetailsUrl, users, commandExecutionUrl) {
    Supervisor.VM.SVInterviews.superclass.constructor.apply(this, arguments);
    var self = this;

    self.Assign = function (user) {
        self._sendCommandAfterFilterAndConfirm(
            "AssignInterviewerCommand",
            function (item) { return { InterviewerId: user.UserId, InterviewId: item.InterviewId }},
            function (item) { return item.CanBeReassigned(); },
            "#confirm-assign-template"
        );
    };

    self.approveInterview = function () {
        self._sendCommandAfterFilterAndConfirm(
            "ApproveInterviewCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function (item) { return item.CanApproveOrReject(); },
            "#confirm-approve-template"
        );
    };

    self.rejectInterview = function () {
        self._sendCommandAfterFilterAndConfirm(
            "RejectInterviewCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function (item) { return item.CanApproveOrReject(); },
            "#confirm-reject-template"
        );
    };

    self._sendCommandAfterFilterAndConfirm = function (commandName, parametersFunc, filterFunc, confirmMessageTemplateId) {
        var allItems = self.SelectedItems();
        var filteredItems = ko.utils.arrayFilter(allItems, filterFunc);

        if (filteredItems.length === allItems.length) {
            self._sendCommand(commandName, parametersFunc, filteredItems);
        }

        var messageTemplate = $("<div/>").html($(confirmMessageTemplateId).html())[0];
        ko.applyBindings(filteredItems, messageTemplate);
        var confirmMessage = $(messageTemplate).html();

        if (filteredItems.length === 0) {
            bootbox.alert(confirmMessage);
            return;
        }

        confirmMessage += '<br/>' + $("#confirm-continue-message-template").html();

        bootbox.confirm(confirmMessage, function (result) {
            if (result)
                self._sendCommand(commandName, parametersFunc, filteredItems);
        });
    };

    self._sendCommand = function (commandName, parametersFunc, items) {
        var commands = ko.utils.arrayMap(items, function (rawItem) {
            var item = ko.mapping.toJS(rawItem);
            return ko.toJSON(parametersFunc(item));
        });

        var command = {
            type: commandName,
            commands: commands
        };

        self.SendCommands(command, function () {
            self.load();
        });
    };

    self.selectAll = function (checkbox) {
        var isCheckboxSelected = $(checkbox).is(":checked");
        ko.utils.arrayForEach(self.Items(), function (item) {
            item.IsSelected(isCheckboxSelected);
        });
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.SVInterviews, Supervisor.VM.InterviewsBase);