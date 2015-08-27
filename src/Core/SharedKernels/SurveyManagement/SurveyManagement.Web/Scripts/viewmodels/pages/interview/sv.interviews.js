Supervisor.VM.SVInterviews = function (listViewUrl, interviewDetailsUrl, responsibles, users, commandExecutionUrl) {
    Supervisor.VM.SVInterviews.superclass.constructor.apply(this, arguments);
    var self = this;

    self.Users = users;
    self.AssignTo = ko.observable();

    self.CanAssignTo = ko.computed(function() {
        return !(self.IsNothingSelected && _.isUndefined(self.AssignTo()));
    });

    self.Assign = function () {
        self.sendCommandAfterFilterAndConfirm(
            "AssignInterviewerCommand",
            function (item) { return { InterviewerId: self.AssignTo().UserId, InterviewId: item.InterviewId }},
            function(item) {
                return item.CanBeReassigned()
                    && !(item.Status() == 'InterviewerAssigned' && item.ResponsibleId() == self.AssignTo().UserId);
            },
            "#confirm-assign-template",
            "#confirm-continue-message-template",
            function() {
                self.AssignTo(undefined);
            },
            function() {
                self.AssignTo(undefined);
            }
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