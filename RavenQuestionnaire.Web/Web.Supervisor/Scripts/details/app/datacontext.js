/// <reference path="../../assign/app/datacontext.js" />
define('app/datacontext',
    ['knockout', 'jquery', 'lodash', 'app/dataservice', 'app/mapper', 'app/config'],

    function (ko, $, _, dataservice, mapper, config) {
        var EntitySet = function (mapper) {
            var items = {},
                mapDtoToContext = function (dto, extras) {
                    var id = mapper.getDtoId(dto);
                    items[id] = mapper.fromDto(dto, extras);
                    return items[id];
                },
                getLocalById = function (id) {
                    return !!id && !!items[id] ? items[id] : null;
                },
                getAllLocal = function () {
                    return _.values(items);
                },
                getData = function (dtos, extras) {
                    return $.Deferred(function (def) {
                        if (!items || _.isEmpty(items)) {
                            _.each(dtos, function (dto) {
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
			user = {},
			questionnaire = {},
            parseData = function (input) {
                return $.Deferred(function (def) {
                    var q = input.questionnaire;
                    _.extend(questionnaire, mapper.interview.fromDto(q));
                    _.extend(user, mapper.user.fromDto(q.User));
                    var rawQuestions = [];
                    _.each(input.questionnaire.Groups, function (group) {
                        var propagationVector = group.PropagationVector;
                        _.each(group.Questions, function (question) {
                            question.PropagationVector = propagationVector;
                            rawQuestions.push(question);
                        });
                    });
                    questions.getData(rawQuestions);

                    groups.getData(input.questionnaire.Groups, questions);
                    def.resolve();
                }).promise();


            },
            prepareQuestion = function () {
                return _.map(questions.getAllLocal(), function (question) {

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
                userId: user.id(),
                answerTime: new Date()
            };
        };
        
        commands[config.commands.answerDateTimeQuestionCommand] = function (args) {
            var question = questions.getLocalById(args.questionId);
            var command = prepareQuestionCommand(question);
            command.answer = question.answer();
            return command;

        };
        
        commands[config.commands.answerGeoLocationQuestionCommand] = function (args) {
            var question = questions.getLocalById(args.questionId);
            var command = prepareQuestionCommand(question);
            command.answer = {};
            command.answer.timestamp = question.answer.timestamp();
            command.answer.latitude = question.answer.latitude();
            command.answer.longitude = question.answer.longitude();
            command.answer.accuracy = question.answer.accuracy();
            return command;
        };
        
        commands[config.commands.answerMultipleOptionsQuestionCommand] = function (args) {
            var question = questions.getLocalById(args.questionId);
            var command = prepareQuestionCommand(question);
            command.selectedValues = question.selectedOptions();
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

        commands[config.commands.answerSingleOptionQuestionCommand] = function (args) {
            var question = questions.getLocalById(args.questionId);
            var command = prepareQuestionCommand(question);
            command.selectedValue = question.selectedOption();
            return command;
        };
        
        commands[config.commands.answerTextQuestionCommand] = function (args) {
            var question = questions.getLocalById(args.questionId);
            var command = prepareQuestionCommand(question);
            command.answer = question.answer();
            return command;
        };

        commands[config.commands.setFlagToAnswer] = function (args) {
            var question = questions.getLocalById(args.questionId);
            return {
                questionId: question.id(),
                propagationVector: question.propagationVector(),
                interviewId: questionnaire.id(),
                userId: user.id(),
            };
        };

        commands[config.commands.removeFlagFromAnswer] = function (args) {
            var question = questions.getLocalById(args.questionId);
            return {
                questionId: question.id(),
                propagationVector: question.propagationVector(),
                interviewId: questionnaire.id(),
                userId: user.id(),
            };
        };

        commands[config.commands.setCommentCommand] = function (args) {
            var question = questions.getLocalById(args.questionId);
            return {
                interviewId : questionnaire.id(), 
                userId: user.id(), 
                questionId : question.id(), 
                propagationVector: question.propagationVector(), 
                commentTime: new Date(), 
                comment: args.comment
            };
        };

        var sendCommand = function (commandName, args, callbacks) {
            return $.Deferred(function (def) {
                var command = {
                    type: commandName,
                    command: ko.toJSON(commands[commandName](args))
                };

                dataservice.sendCommand({
                    success: function (response, status) {
                        if (callbacks && callbacks.success) {
                            callbacks.success();
                        }
                        def.resolve(response);
                    },
                    error: function (response, xhr) {
                        if (callbacks && callbacks.error) {
                            callbacks.error(response);
                        }
                        def.reject(response);
                        return;
                    }
                }, command);
            }).promise();
        };

        return {
            questionnaire: questionnaire,
            user: user,
            questions: questions,
            groups: groups,
            sendCommand: sendCommand,
            parseData: parseData
        };
    });