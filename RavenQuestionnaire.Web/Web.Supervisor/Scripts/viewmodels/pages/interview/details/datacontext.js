DataContext = function(config) {
    
    var commands = {};

    var prepareQuestionCommand = function(question) {
        return {
            questionId: question.id(),
            rosterVector: question.rosterVector(),
            interviewId: questionnaire.id(),
            answerTime: new Date()
        };
    };

    commands[config.commands.answerDateTimeQuestionCommand] = function(args) {
        var question = questions.getLocalById(args.questionId);
        var command = prepareQuestionCommand(question);
        command.answer = question.answer();
        return command;

    };

    commands[config.commands.answerGeoLocationQuestionCommand] = function(args) {
        var question = questions.getLocalById(args.questionId);
        var command = prepareQuestionCommand(question);
        command.answer = {};
        command.answer.timestamp = question.answer.timestamp();
        command.answer.latitude = question.answer.latitude();
        command.answer.longitude = question.answer.longitude();
        command.answer.accuracy = question.answer.accuracy();
        return command;
    };

    commands[config.commands.answerMultipleOptionsQuestionCommand] = function(args) {
        var question = questions.getLocalById(args.questionId);
        var command = prepareQuestionCommand(question);
        command.selectedValues = question.areAnswersOrdered() ? question.orderedOptionsSelection() :
            question.selectedOptions();
        return command;
    };

        commands[config.commands.answerNumericRealQuestionCommand] = function (args) {
            var question = questions.getLocalById(args.questionId);
            var command = prepareQuestionCommand(question);
            command.answer = question.answer();
            return command;
        };
        
        commands[config.commands.answerNumericIntegerQuestionCommand] = function (args) {
        var question = questions.getLocalById(args.questionId);
        var command = prepareQuestionCommand(question);
        command.answer = question.answer();
        return command;
    };

    commands[config.commands.answerSingleOptionQuestionCommand] = function(args) {
        var question = questions.getLocalById(args.questionId);
        var command = prepareQuestionCommand(question);
        command.selectedValue = question.selectedOption();
        return command;
    };

    commands[config.commands.answerTextQuestionCommand] = function(args) {
        var question = questions.getLocalById(args.questionId);
        var command = prepareQuestionCommand(question);
        command.answer = question.answer();
        return command;
    };

    commands[config.commands.setFlagToAnswer] = function(args) {
        var question = questions.getLocalById(args.questionId);
        return {
            questionId: question.id(),
            rosterVector: question.rosterVector(),
            interviewId: questionnaire.id()
        };
    };

    commands[config.commands.removeFlagFromAnswer] = function(args) {
        var question = questions.getLocalById(args.questionId);
        return {
            questionId: question.id(),
            rosterVector: question.rosterVector(),
            interviewId: questionnaire.id()
        };
    };

    commands[config.commands.setCommentCommand] = function(args) {
        var question = questions.getLocalById(args.questionId);
        return {
            interviewId: questionnaire.id(),
            questionId: question.id(),
            rosterVector: question.rosterVector(),
            commentTime: new Date(),
            comment: args.comment
        };
    };
    
    commands[config.commands.approveInterviewCommand] = function (args) {
        return {
            interviewId: questionnaire.id(),
            commentTime: new Date(),
            comment: args.comment
        };
    };
    
    commands[config.commands.rejectInterviewCommand] = function (args) {
        return {
            interviewId: questionnaire.id(),
            commentTime: new Date(),
            comment: args.comment
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