DataContext = function(mapper, config) {
    var EntitySet = function(mapper) {
        var items = {},
            mapDtoToContext = function(dto, extras) {
                var id = mapper.getDtoId(dto);
                items[id] = mapper.fromDto(dto, extras);
                return items[id];
            },
            getLocalById = function(id) {
                return !!id && !!items[id] ? items[id] : null;
            },
            getAllLocal = function() {
                return _.values(items);
            },
            getData = function(dtos, extras) {
                return $.Deferred(function(def) {
                    if (!items || _.isEmpty(items)) {
                        $.each(dtos, function(index, dto) {
                            mapDtoToContext(dto, extras);
                        });
                        def.resolve(getAllLocal());
                    } else {
                        def.resolve(getAllLocal());
                    }
                }).promise();
            };

        return {
            mapDtoToContext: mapDtoToContext,
            getAllLocal: getAllLocal,
            getLocalById: getLocalById,
            getData: getData
        };
    },
        questions = new EntitySet(mapper.question),
        groups = new EntitySet(mapper.group),
        questionnaire = {},
        parseData = function(q) {
            return $.Deferred(function(def) {
                $.extend(questionnaire, mapper.interview.fromDto(q));
                var rawQuestions = [];
                $.each(q.Groups, function(index, group) {
                    var propagationVector = group.PropagationVector;
                    $.each(group.Questions, function(index, question) {
                        question.PropagationVector = propagationVector;
                        rawQuestions.push(question);
                    });
                });
                questions.getData(rawQuestions);

                groups.getData(q.Groups, questions);
                def.resolve();
            }).promise();
        },
        prepareQuestion = function() {
            return _.map(questions.getAllLocal(), function(question) {

                var answer = {
                    Id: question.id(),
                    Type: question.type(),
                    Answer: question.hasOptions() ? "" : question.selectedOption(),
                    Answers: []
                };

                if (question.hasOptions()) {
                    if (question.type() == "SingleOption")
                        answer.Answers.push(question.selectedOption());
                    else
                        answer.Answers = question.selectedOptions();
                }

                return answer;
            });
        };

    var commands = {};

    var prepareQuestionCommand = function(question) {
        return {
            questionId: question.id(),
            propagationVector: question.propagationVector(),
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
        command.selectedValues = question.orderedOptionsSelection();
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
            propagationVector: question.propagationVector(),
            interviewId: questionnaire.id()
        };
    };

    commands[config.commands.removeFlagFromAnswer] = function(args) {
        var question = questions.getLocalById(args.questionId);
        return {
            questionId: question.id(),
            propagationVector: question.propagationVector(),
            interviewId: questionnaire.id()
        };
    };

    commands[config.commands.setCommentCommand] = function(args) {
        var question = questions.getLocalById(args.questionId);
        return {
            interviewId: questionnaire.id(),
            questionId: question.id(),
            propagationVector: question.propagationVector(),
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
        questionnaire: questionnaire,
        questions: questions,
        groups: groups,
        getCommand: getCommand,
        parseData: parseData
    };
};