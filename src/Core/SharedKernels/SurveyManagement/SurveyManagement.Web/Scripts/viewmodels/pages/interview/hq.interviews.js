Supervisor.VM.HQInterviews = function(listViewUrl, interviewDetailsUrl, users, commandExecutionUrl) {
    Supervisor.VM.HQInterviews.superclass.constructor.apply(this, arguments);

    var self = this;

    self.DeleteInterview = function () {
        self.sendCommandAfterFilterAndConfirm(
            "DeleteInterviewCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function(item) { return item.CanDelete(); },
            "#confirm-delete-template",
            "#confirm-continue-message-template"
        );
    };

    self.ApproveInterview = function () {
        self.sendCommandAfterFilterAndConfirm(
            "HqApproveInterviewCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function (item) { return item.CanApproveOrReject(); },
            "#confirm-approve-template",
            "#confirm-continue-message-template"
        );
    };

    self.RejectInterview = function () {
        self.sendCommandAfterFilterAndConfirm(
            "HqRejectInterviewCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function (item) { return item.CanApproveOrReject(); },
            "#confirm-reject-template",
            "#confirm-continue-message-template"
        );
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.HQInterviews, Supervisor.VM.InterviewsBase);