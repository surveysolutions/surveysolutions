DataContext = function(config, interviewId) {

    var commands = {};

    var prepareQuestionCommand = function(question) {
        return {
            questionId: question.id,
            rosterVector: question.rosterVector,
            interviewId: interviewId,
            answerTime: new Date()
        };
    };

    commands[config.commands.answerMultipleOptionsQuestionCommand] = function(question) {
        var command = prepareQuestionCommand(question);
        command.selectedValues = question.areAnswersOrdered ? question.orderedOptionsSelection() :
            question.selectedOptions();
        return command;
    };

    commands[config.commands.answerYesNoQuestion] = function (question) {
        var command = prepareQuestionCommand(question);
        command.answeredOptions = question.selectedOptions();
        return command;
    };

    commands[config.commands.answerNumericRealQuestionCommand] = function(question) {
        var command = prepareQuestionCommand(question);
        command.answer = question.answer();
        return command;
    };

    commands[config.commands.answerNumericIntegerQuestionCommand] = function(question) {
        var command = prepareQuestionCommand(question);
        command.answer = question.answer().split(',').join('');
        return command;
    };

    commands[config.commands.answerSingleOptionQuestionCommand] = function(question) {
        var command = prepareQuestionCommand(question);
        command.selectedValue = question.selectedOption();
        return command;
    };

    commands[config.commands.answerTextQuestionCommand] = function(question) {
        var command = prepareQuestionCommand(question);
        command.answer = question.answer();
        return command;
    };

    commands[config.commands.setFlagToAnswer] = function(question) {
        return {
            questionId: question.id,
            rosterVector: question.rosterVector,
            interviewId: interviewId
        };
    };

    commands[config.commands.removeFlagFromAnswer] = function(question) {
        return {
            questionId: question.id,
            rosterVector: question.rosterVector,
            interviewId: interviewId
        };
    };

    commands[config.commands.setCommentCommand] = function (args) {
        return {
            interviewId: interviewId,
            questionId: args.question.id,
            rosterVector: args.question.rosterVector,
            commentTime: new Date(),
            comment: args.comment
        };
    };

    commands[config.commands.approveInterviewCommand] = function(args) {
        return {
            interviewId: interviewId,
            commentTime: new Date(),
            comment: args.comment
        };
    };

    commands[config.commands.rejectInterviewCommand] = function(args) {
        return {
            interviewId: interviewId,
            commentTime: new Date(),
            comment: args.comment
        };
    };

    commands[config.commands.rejectInterviewToInterviewerCommand] = function (args) {
        return {
            interviewId: interviewId,
            interviewerId: args.interviewerId,
            commentTime: new Date(),
            comment: args.comment
        };
    };

    commands[config.commands.hQApproveInterviewCommand] = function(args) {
        return {
            interviewId: interviewId,
            commentTime: new Date(),
            comment: args.comment
        };
    };

    commands[config.commands.hQRejectInterviewCommand] = function(args) {
        return {
            interviewId: interviewId,
            commentTime: new Date(),
            comment: args.comment
        };
    };

    commands[config.commands.unapproveByHeadquarterCommand] = function (args) {
        return {
            interviewId: interviewId,
            commentTime: new Date(),
            comment: args.comment
        };
    };

    commands[config.commands.switchTranslation] = function (args) {
        return {
            interviewId: interviewId,
            language: args.language
        };
    };

    var getCommand = function(commandName, args) {
        return {
            type: commandName,
            command: ko.toJSON(commands[commandName](args))
        };
    };

    return {
        getCommand: getCommand
    };
};