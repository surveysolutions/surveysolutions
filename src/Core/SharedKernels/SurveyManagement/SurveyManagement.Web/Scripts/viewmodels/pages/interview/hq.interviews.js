Supervisor.VM.HQInterviews = function(listViewUrl, interviewDetailsUrl, users, commandExecutionUrl) {
    Supervisor.VM.HQInterviews.superclass.constructor.apply(this, arguments);

    var self = this;

    self.deleteInterview = function () {
        self._sendCommand("DeleteInterviewCommand");
    };

    self.approveInterview = function () {
        self._sendCommand("HqApproveInterviewCommand");
    };

    self.rejectInterview = function () {
        self._sendCommand("HqRejectInterviewCommand");
    };

    self._sendCommand = function (commandName) {
        var commands = ko.utils.arrayMap(self.SelectedItems(), function (rawItem) {
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
            if (item.CanDelete() || item.CanApproveOrReject()) {
                item.IsSelected(isCheckboxSelected);
            }
        });
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.HQInterviews, Supervisor.VM.InterviewsBase);