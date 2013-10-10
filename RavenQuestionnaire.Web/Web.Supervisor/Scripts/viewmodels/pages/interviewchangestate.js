Supervisor.VM.InterviewChangeState = function(commandExecutionUrl, interviewId, interviewUrl) {
    Supervisor.VM.InterviewChangeState.superclass.constructor.apply(this, arguments);

    var self = this;
    self.InterviewId = interviewId;
    self.InterviewUrl = interviewUrl;

    self.Comment = ko.observable('');
    self.Approve = function() {
        self.changeState("ApproveInterviewCommand", "approved");
    };
    self.Reject = function() {
        self.changeState("RejectInterviewCommand", 'rejected');
    };
    self.changeState = function(commandName, status) {
        var command = {
            type: commandName,
            command: ko.toJSON({
                interviewId: interviewId,
                comment: self.Comment().trim()
            })
        };

        self.SendCommands(command, function(data) {
            window.location = self.InterviewUrl.concat("?statchange=", status);
        });
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.InterviewChangeState, Supervisor.VM.BasePage);