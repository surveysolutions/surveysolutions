define('app/datacontext',
    ['knockout', 'jquery', 'lodash', 'app/dataservice', 'app/mapper', 'app/config'],

    function (ko, $, _, dataservice, mapper, config) {
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
                                _.each(dtos, function(dto){ 
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
            parseData = function(input) {
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

        commands[config.commands.setFlagToAnswer] = function (args) {
            return {
                questionId: args.questionId,
                interviewId: args.interviewId,
                userId: args.userId
            };
        };

        commands[config.commands.removeFlagFromAnswer] = function (args) {
            return {
                questionId: args.questionId,
                interviewId: args.interviewId,
                userId: args.userId
            };
        };

        commands[config.commands.setCommentCommand] = function (args) {
            return args;
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
            groups : groups,
            sendCommand: sendCommand,
            parseData: parseData
        };
    });