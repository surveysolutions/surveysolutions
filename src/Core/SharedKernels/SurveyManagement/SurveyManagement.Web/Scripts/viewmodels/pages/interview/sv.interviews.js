Supervisor.VM.SVInterviews = function (listViewUrl, interviewDetailsUrl, users, commandExecutionUrl) {
    Supervisor.VM.SVInterviews.superclass.constructor.apply(this, arguments);
    var self = this;

    self.Assign = function (user) {
        self.sendCommandAfterFilterAndConfirm(
            "AssignInterviewerCommand",
            function (item) { return { InterviewerId: user.UserId, InterviewId: item.InterviewId }},
            function (item) { return item.CanBeReassigned(); },
            "#confirm-assign-template",
            "#confirm-continue-message-template"
        );
    };

    self.ApproveInterview = function () {
        self.sendCommandAfterFilterAndConfirm(
            "ApproveInterviewCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function (item) { return item.CanApproveOrReject(); },
            "#confirm-approve-template",
            "#confirm-continue-message-template"
        );
    };

    self.RejectInterview = function () {
        self.sendCommandAfterFilterAndConfirm(
            "RejectInterviewCommand",
            function (item) { return { InterviewId: item.InterviewId } },
            function (item) { return item.CanApproveOrReject(); },
            "#confirm-reject-template",
            "#confirm-continue-message-template"
        );
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.SVInterviews, Supervisor.VM.InterviewsBase);