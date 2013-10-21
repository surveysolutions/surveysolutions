Config = function() {
    var commands = {
        answerDateTimeQuestionCommand: "AnswerDateTimeQuestionCommand",
        answerMultipleOptionsQuestionCommand: "AnswerMultipleOptionsQuestionCommand",
        answerNumericRealQuestionCommand: "AnswerNumericRealQuestionCommand",
        answerNumericIntegerQuestionCommand: "AnswerNumericIntegerQuestionCommand",
        answerSingleOptionQuestionCommand: "AnswerSingleOptionQuestionCommand",
        answerTextQuestionCommand: "AnswerTextQuestionCommand",
        answerGeoLocationQuestionCommand: "AnswerGeoLocationQuestionCommand",

        setCommentCommand: "CommentAnswerCommand",
        setAnswerCommand: "SetAnswerCommand",
        setFlagToAnswer: "SetFlagToAnswerCommand",
        removeFlagFromAnswer: "RemoveFlagFromAnswerCommand"
    },
        statusMap = {
            Created: "Created",
            SupervisorAssigned: "Supervisor assigned",
            Deleted: "Deleted",
            Restored: "Restored",
            InterviewerAssigned: "Interviewer assigned",
            ReadyForInterview: "Ready for interview",
            SentToCapi: "Sent to capi",
            Completed: "Completed",
            Restarted: "Restarted",
            ApprovedBySupervisor: "Approved by supervisor",
            RejectedBySupervisor: "Rejected by supervisor",
        };
    return {
        commands: commands,
        statusMap: statusMap
    };
};