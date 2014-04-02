Config = function() {
    var questionTypes = {
            SingleOption: 0,
            MultyOption: 3,
            Numeric: 4,
            DateTime: 5,
            GpsCoordinates: 6,
            Text: 7,
            AutoPropagate: 8,
            TextList: 9,
            QRBarcode: 10
        },
        questionTemplateByType = {
            0: "SingleOption",
            3: "MultyOption",
            4: "Numeric",
            5: "DateTime",
            6: "GpsCoordinates",
            7: "Text",
            8: "AutoPropagate",
            9: "TextList",
            10: "QRBarcode"
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
        questionTemplateByType: questionTemplateByType,
        commands: commands,
        statusMap: statusMap
    };
};