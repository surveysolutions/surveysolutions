define('app/config', ['knockout', 'knockout.validation'],
    function(ko) {
        var commands = {
            setCommentCommand: "CommentAnswerCommand",
            setAnswerCommand: "SetAnswerCommand",
            setFlagToAnswer: "SetFlagToAnswerCommand",
            removeFlagFromAnswer: "RemoveFlagFromAnswerCommand"
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