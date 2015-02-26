Supervisor.VM.HQInterviews = function(listViewUrl, interviewDetailsUrl, users, commandExecutionUrl) {
    Supervisor.VM.HQInterviews.superclass.constructor.apply(this, arguments);

    var self = this;

    self.deleteInterview = function () {
        self._sendCommandAfterFilterAndConfirm(
            "DeleteInterviewCommand",
            function(item) { return item.CanDelete(); },
            "#confirm-delete-template"
        );
    };

    self.approveInterview = function () {
        self._sendCommandAfterFilterAndConfirm(
            "HqApproveInterviewCommand",
            function (item) { return item.CanApproveOrReject(); },
            "#confirm-approve-template"
        );
    };

    self.rejectInterview = function () {
        self._sendCommandAfterFilterAndConfirm(
            "HqRejectInterviewCommand",
            function (item) { return item.CanApproveOrReject(); },
            "#confirm-reject-template"
        );
    };

    self._sendCommandAfterFilterAndConfirm = function (commandName, filterFunc, confirmMessageTemplateId) {
        var allItems = self.SelectedItems();
        var filteredItems = ko.utils.arrayFilter(allItems, filterFunc);

        if (filteredItems.length === allItems.length) {
            self._sendCommand(commandName, filteredItems);
        }

        var messageTemplate = $("<div/>").html($(confirmMessageTemplateId).html())[0];
        ko.applyBindings(filteredItems, messageTemplate);
        var confirmMessage = $(messageTemplate).html();

        if (filteredItems.length === 0) {
            bootbox.alert(confirmMessage);
            return;
        }

        confirmMessage += $("#confirm-continue-message-template").html();

        bootbox.confirm(confirmMessage, function (result) {
            if (result)
                self._sendCommand(commandName, filteredItems);
        });
    };

    self._sendCommand = function (commandName, items) {
        var commands = ko.utils.arrayMap(items, function (rawItem) {
            var item = ko.mapping.toJS(rawItem);
            return ko.toJSON({
                InterviewId: item.InterviewId
            });
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
Supervisor.Framework.Classes.inherit(Supervisor.VM.HQInterviews, Supervisor.VM.InterviewsBase);