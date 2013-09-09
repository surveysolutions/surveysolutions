define('app/config', ['knockout', 'knockout.validation'],
    function(ko) {
        var commands = {
            setFlagCommand: "SetFlagCommand",
            setCommentCommand: "SetCommentCommand",
            setAnswerCommand: "SetAnswerCommand",
            setFlagToAnswer: "SetFlagToAnswer",
            removeFlagFromAnswer: "RemoveFlagFromAnswer",
            commentAnswer: "CommentAnswer"
        },
            questionTypeMap = {
                0: "SingleOption",
                3: "MultyOption",
                4: "Numeric",
                5: "DateTime",
                6: "GpsCoordinates",
                7: "Text",
                8: "AutoPropagate",
            };
        return {
            commands: commands,
            questionTypeMap: questionTypeMap
        };
    });