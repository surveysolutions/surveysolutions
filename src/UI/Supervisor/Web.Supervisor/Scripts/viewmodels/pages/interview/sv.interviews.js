Supervisor.VM.SVInterviews = function(listViewUrl, interviewDetailsUrl, users, commandExecutionUrl) {
    Supervisor.VM.SVInterviews.superclass.constructor.apply(this, arguments);

    var self = this;

    self.Assign = function(user) {
        var commands = ko.utils.arrayMap(self.SelectedItems(), function(rawItem) {
            var item = ko.mapping.toJS(rawItem);
            return ko.toJSON({
                InterviewerId: user.UserId,
                InterviewId: item.InterviewId
            });
        });

        var command = {
            type: "AssignInterviewerCommand",
            commands: commands
        };

        self.SendCommands(command, function(failedCommandIds) {
            var assignedInterviews = ko.utils.arrayFilter(self.SelectedItems(), function(item) {
                return $.inArray(item.InterviewId(), failedCommandIds) == -1;
            });

            ko.utils.arrayFilter(assignedInterviews, function(item) {
                item.IsSelected(false);
                item.ResponsibleId(user.UserId);
                item.ResponsibleName(user.UserName);
                if (item.Status() != "RejectedBySupervisor") {
                    item.Status("InterviewerAssigned");
                }
            });
        });
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.SVInterviews, Supervisor.VM.InterviewsBase);