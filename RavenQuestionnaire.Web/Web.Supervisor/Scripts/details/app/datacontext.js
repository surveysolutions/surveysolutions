define('app/datacontext',
    ['knockout', 'jquery', 'lodash', 'app/dataservice', 'app/mapper', 'app/config'],

    function (ko, $, _, dataservice, mapper, config) {
       
        var prepareQuestion = function () {
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

        /*commands[config.commands.setFlagToAnswer] = function(args) {
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
        };*/

        commands[config.commands.setCommentCommand] = function (args) {
            return args;
        };
        
        commands[config.commands.setFlagCommand] = function (args) {
            return args;
        };
        //commands["CreateInterviewWithFeaturedQuestionsCommand"] = function(args) {
        //    return {
        //        interviewId : questionnaire.id,
        //        questionnaireId: questionnaire.templateId,
        //        featuredAnswers: prepareQuestion(),
        //        responsible: {
        //            Id: args.id,
        //            Name: args.name
        //        },
        //        creator: currentUser
        //    };
        //};
        
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
            sendCommand: sendCommand
        };
    });