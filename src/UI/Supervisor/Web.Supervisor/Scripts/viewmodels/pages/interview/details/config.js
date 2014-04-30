Config = function() {
    var questionTypes = {
            SingleOption: "SingleOption",
            MultyOption: "MultyOption",
            Numeric: "Numeric",
            DateTime: "DateTime",
            GpsCoordinates: "GpsCoordinates",
            Text: "Text",
            TextList: "TextList",
            QRBarcode: "QRBarcode"
        },
        commands = {
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
            removeFlagFromAnswer: "RemoveFlagFromAnswerCommand",
            approveInterviewCommand: "ApproveInterviewCommand",
            rejectInterviewCommand: "RejectInterviewCommand",

            hQApproveInterviewCommand: "HqApproveInterviewCommand",
            hQRejectInterviewCommand: "HqRejectInterviewCommand"
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

            ApprovedByHeadquarter: "Approved by HQ",
            RejectedByHeadquarter: "Rejected by HQ"
        };
    return {
        questionTypes: questionTypes,
        commands: commands,
        statusMap: statusMap
    };
};