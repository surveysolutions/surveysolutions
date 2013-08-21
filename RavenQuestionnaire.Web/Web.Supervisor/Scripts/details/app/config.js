define('app/config', ['knockout', 'knockout.validation'],
function (ko) {
    var commands = {
        setFlagCommand: "SetFlagCommand",
        setCommentCommand: "SetCommentCommand",
        setAnswerCommand: "SetAnswerCommand"
        /*
        setFlagToAnswer: "SetFlagToAnswer",
        removeFlagFromAnswer: "RemoveFlagFromAnswer",
        commentAnswer: "CommentAnswer"
        */
    };
    return {
        commands: commands
    };
});