Supervisor.VM.HQInterviews = function(listViewUrl, interviewDetailsUrl, users, commandExecutionUrl) {
    Supervisor.VM.HQInterviews.superclass.constructor.apply(this, arguments);

    var self = this;

    self.deleteInterview = function() {
        var commands = ko.utils.arrayMap(self.SelectedItems(), function (rawItem) {
            var item = ko.mapping.toJS(rawItem);
            return ko.toJSON({
                InterviewId: item.InterviewId
            });
        });

        var command = {
            type: "DeleteInterviewCommand",
            commands: commands
        };

        self.SendCommands(command, function (failedCommandIds) {
            var deletedInterviews = ko.utils.arrayFilter(self.SelectedItems(), function(item) {
                return $.inArray(item.InterviewId(), failedCommandIds) == -1;
            });

            for (var i = deletedInterviews.length - 1; i >= 0; i--) {
                self.Items.remove(deletedInterviews[i]);
            }

            self.TotalCount(self.TotalCount() - deletedInterviews.length);
        });
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.HQInterviews, Supervisor.VM.InterviewsBase);