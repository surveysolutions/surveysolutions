Supervisor.VM.InterviewDetails = function (settings) {
    Supervisor.VM.InterviewDetails.superclass.constructor.apply(this, [settings.Urls.CommandExecution]);

    var self = this,
        config = new Config(),
        datacontext = new DataContext(config, settings.Interview.InterviewId);

    self.filter = ko.observable('all');
    self.questionnaire = ko.observable();
    self.groups = ko.observableArray();
    self.questions = ko.observableArray();
    self.currentQuestion = ko.observable();
    self.currentComment = ko.observable('');
    self.changeStateComment = ko.observable('');
    self.changeStateHistory = ko.observable();
    

    self.closeDetails = function() {
        $('body').removeClass('details-visible');
    };
    self.showDetails = function(question, event) {
        event.stopPropagation();
        if (_.isNull(self.currentQuestion()) == false &&
            _.isUndefined(self.currentQuestion()) == false) {
            self.currentQuestion().isSelected(false);
        }
        self.currentQuestion(question);
        self.currentQuestion().isSelected(true);
        $('body').addClass('details-visible');
    };

    self.flagedCount = ko.computed(function() {
        return _.reduce(self.questions(), function(count, question) {
            return count + (question.isFlagged() ? 1 : 0);
        }, 0);
    });
    self.answeredCount = ko.computed(function() {
        return _.reduce(self.questions(), function(count, question) {
            return count + (question.isAnswered ? 1 : 0);
        }, 0);
    });
    self.commentedCount = ko.computed(function() {
        return _.reduce(self.questions(), function(count, question) {
            return count + (question.comments().length > 0 ? 1 : 0);
        }, 0);
    });
    self.invalidCount = ko.computed(function() {
        return _.reduce(self.questions(), function(count, question) {
            return count + (question.isInvalid ? 1 : 0);
        }, 0);
    });
    self.editableCount = ko.computed(function() {
        return _.reduce(self.questions(), function(count, question) {
            return count + (question.scope == "Supervisor" ? 1 : 0);
        }, 0);
    });
    self.enabledCount = ko.computed(function() {
        return _.reduce(self.questions(), function(count, question) {
            return count + (question.isEnabled ? 1 : 0);
        }, 0);
    });
    self.applyQuestionFilter = function(f) {
        self.filter(f);
        $.each(self.groups(), function(index, group) {
            group.isVisible(true);
            group.isSelected(false);
        });
        $.each(self.questions(), function(index, question) {
            question.matchFilter(f);
        });
        $.each(self.groups(), function(index, group) {
            if (group.visibleQuestionsCount() > 0) {
                group.isSelected(true);
            }
        });
    };
    self.addComment = function() {
        var command = datacontext.getCommand(config.commands.setCommentCommand, {
            comment: self.currentComment(),
            question: self.currentQuestion()
        });
        self.SendCommand(command, function () {
            self.currentQuestion().comments().push({
                text: self.currentComment(),
                date: new Date(),
                userName: settings.UserName
            });
            self.currentComment('');
            self.currentQuestion().comments.valueHasMutated();
        });
    };
    self.flagAnswer = function(question) {
        var commandName = question.isFlagged()
            ? config.commands.removeFlagFromAnswer
            : config.commands.setFlagToAnswer;

        var command = datacontext.getCommand(commandName, question);
        self.SendCommand(command, function () {
            question.isFlagged(!question.isFlagged());
        });
    };
    self.saveAnswer = function(question) {
        var commandName = "";
        switch (question.questionType) {
        case config.questionTypes.Text:
            commandName = config.commands.answerTextQuestionCommand;
            break;
        case config.questionTypes.Numeric:
            commandName = question.isInteger ? config.commands.answerNumericIntegerQuestionCommand : config.commands.answerNumericRealQuestionCommand;
            break;
        case config.questionTypes.DateTime:
            commandName = config.commands.answerDateTimeQuestionCommand;
            break;
        case config.questionTypes.GpsCoordinates:
            commandName = config.commands.answerGeoLocationQuestionCommand;
            break;
        case config.questionTypes.SingleOption:
            commandName = config.commands.answerSingleOptionQuestionCommand;
            break;
        case config.questionTypes.MultyOption:
            commandName = config.commands.answerMultipleOptionsQuestionCommand;
            break;
        }

        var command = datacontext.getCommand(commandName, question);
        self.SendCommand(command);
    };
    self.load = function () {
        self.SendRequest(settings.Urls.InterviewDetails, settings.Interview, function (interview) {

            var questions = self.prepareGroupsAndQuestionsAndReturnAllQuestions(interview.Groups);

            self.questionnaire(interview.InterviewInfo);
            self.groups(interview.Groups);
            self.questions(questions);

            Router({
                '/group/:groupId': function (groupId) {
                    self.applyQuestionFilter('all');
                    var visibleGroupsIds = [groupId];
                    $.each(self.groups(), function (index, group) {
                        if (_.contains(visibleGroupsIds, group.uiId)) {
                            group.isVisible(true);
                        } else if (_.contains(visibleGroupsIds, group.parentId)) {
                            visibleGroupsIds.push(group.uiId);
                            group.isVisible(true);
                        } else {
                            group.isVisible(false);
                        }
                    });
                },
                '/:filter': function (f) {
                    self.applyQuestionFilter(f);
                }
            }).init();
        });
    };
    
    self.prepareGroupsAndQuestionsAndReturnAllQuestions = function (groups) {
        var allQuestions = [];
        $.each(groups, function(i, group) {

            $.each(group.questions, function (j, question) {
                allQuestions.push(question);
                question.isVisible = ko.observable(true);
                question.isSelected = ko.observable(false);

                question.comments = ko.observableArray(question.comments);
                question.isFlagged = ko.observable(question.isFlagged);
                question.markerStyle = ko.computed(function() {
                    if (question.isInvalid) {
                        return "invalid";
                    }
                    if (question.scope == "Supervisor") {
                        return "supervisor";
                    }
                    return "";
                });
                question.matchFilter = function(filter) {
                    switch (filter) {
                    case "all":
                        question.isVisible(true);
                        break;
                    case "flaged":
                        question.isVisible(question.isFlagged());
                        break;
                    case "commented":
                        question.isVisible(question.comments().length > 0);
                        break;
                    case "answered":
                        question.isVisible(question.isAnswered);
                        break;
                    case "invalid":
                        question.isVisible(question.isInvalid);
                        break;
                    case "supervisor":
                        question.isVisible(question.scope == "Supervisor");
                        break;
                    case "enabled":
                        question.isVisible(question.isEnabled);
                        break;
                    }
                };

                if (question.scope == "Supervisor") {
                    switch (question.questionType) {
                        case config.questionTypes.Text:
                            question.answer = ko.observable(question.answer).extend({ required: true });
                            break;
                        case config.questionTypes.Numeric:
                            question.answer = ko.observable(question.answer).extend({ required: true });
                            if (question.isInteger) {
                                question.answer.extend({ numericValidator: -1 });
                            } else if (!_.isNull(question.countOfDecimalPlaces)) {
                                question.answer.extend({ numericValidator: question.countOfDecimalPlaces });
                            } else {
                                question.answer.extend({ numericValidator: true });
                            }
                            break;
                        case config.questionTypes.SingleOption:
                            question.selectedOption = ko.observable(question.selectedOption).extend({
                                validation: [{
                                    validator: function (val) {
                                        if (_.isNull(val) || _.isUndefined(val) || _.isEmpty(val))
                                            return false;
                                        return true;
                                    },
                                    message: 'At least one option should be checked'
                                }]
                            });
                            question.answer = ko.computed(function () {
                                var o = _.find(question.options, function (option) {
                                    return question.selectedOption() == option.value;
                                });
                                return _.isEmpty(o) ? "" : o.label;
                            });
                            break;
                        case config.questionTypes.MultyOption:
                            var selectedOptionsSource = question.selectedOptions;
                            question.selectedOptions = ko.observableArray().extend({
                                validation: [
                                    {
                                        validator: function (val) {
                                            if (_.isNull(val) || _.isUndefined(val) || _.isEmpty(val))
                                                return false;
                                            return val.length > 0;
                                        },
                                        message: 'At least one option should be checked'
                                    },
                                    {
                                        validator: function (val) {
                                            if (_.isUndefined(question.maxAllowedAnswers) || _.isNull(question.maxAllowedAnswers)) {
                                                return true;
                                            }

                                            return val.length <= question.maxAllowedAnswers;
                                        },
                                        message: 'Number of selected answers more than number of maximum permitted answers'
                                    }]
                            });

                            if (question.areAnswersOrdered) {
                                question.orderedOptionsSelection = ko.observableArray([]);
                                question.selectedOptionsCount = 0;
                                question.orderSelectedOptions = function () {
                                    if (question.selectedOptionsCount != question.selectedOptions().length) {
                                        if (question.selectedOptionsCount > question.selectedOptions().length) {
                                            _.each(question.orderedOptionsSelection(), function (answer) {
                                                if (!_.contains(question.selectedOptions(), answer)) {
                                                    question.orderedOptionsSelection.remove(answer);
                                                }
                                            });
                                        }
                                        _.each(question.options, function (option) {
                                            var orderIndex = question.orderedOptionsSelection().indexOf(option.value);
                                            if (_.contains(question.selectedOptions(), option.value)) {
                                                if (_.isNull(option.orderNo())) {
                                                    option.orderNo(question.selectedOptions().length);
                                                    question.orderedOptionsSelection.push(option.value);
                                                } else {
                                                    if (orderIndex > -1) {
                                                        option.orderNo(orderIndex + 1);
                                                    }
                                                }
                                            } else {
                                                if (question.selectedOptionsCount > question.selectedOptions().length) {
                                                    if (orderIndex == -1) {
                                                        option.orderNo(null);
                                                    }
                                                }
                                            }
                                        });
                                        question.selectedOptionsCount = question.selectedOptions().length;
                                    }
                                };
                                question.answer = ko.computed(function () {
                                    var selected = _.filter(question.options, function (option) {
                                        return _.contains(question.selectedOptions(), option.value);
                                    });
                                    
                                    var a = _.reduce(selected, function (result, option) {
                                        return result + option.label + ", ";
                                    }, "").trim();
                                    if (_.isEmpty(a) == false) {
                                        a = a.substring(0, a.length - 1);
                                    }
                                    return a;
                                });

                                question.selectedOptions.subscribe(function () {
                                    question.orderSelectedOptions();
                                });

                                $.each(question.options, function (index, option) {
                                    option.orderNo = ko.observable(option.orderNo);
                                });
                                $.each(selectedOptionsSource, function (index, option) {
                                    question.selectedOptions.push(option);
                                });
                            }
                            break;
                    }
                    question.errors = ko.validation.group(question);
                }
            });

            group.isVisible = ko.observable(true);
            group.isSelected = ko.observable(false);
            group.css = ko.computed(function () {
                return "level" + group.depth + (group.isSelected() ? " selected" : "");
            });
            group.href = ko.computed(function () {
                return "#/group/" + group.uiId;
            });
            group.visibleQuestionsCount = ko.computed(function () {
                return _.reduce(group.questions, function (count, question) {
                    return count + (question.isVisible() ? 1 : 0);
                }, 0);
            });
        });
        return allQuestions;
    };

    self.showApproveModal = function () {
        $('#approveModal').modal('show');
    };
    
    self.showRejectModal = function () {
        $('#rejectModal').modal('show');
    };
    
    self.approveInterview = function () {
        self.changeState(config.commands.approveInterviewCommand);
    };
    self.rejectInterview = function () {
        self.changeState(config.commands.rejectInterviewCommand);
    };
    
    self.hQApproveInterview = function () {
        self.changeState(config.commands.hQApproveInterviewCommand);
    };
    self.hQRejectInterview = function () {
        self.changeState(config.commands.hQRejectInterviewCommand);
    };

    self.changeState = function (commandName) {
        var command = datacontext.getCommand(commandName, { comment: self.changeStateComment() });
        self.SendCommand(command, function () {
            if (!_.isNull(settings.UrlReferrer)) {
                window.location = settings.UrlReferrer;
            } else {
                window.location = settings.Urls.Interviews;
            }
            
        });
    };

    $('#statusHistoryModal').on('show.bs.modal', function (e) {
        self.changeStateHistory(undefined);
        self.SendRequest(settings.Urls.ChangeStateHistory, { InterviewId: self.questionnaire().id }, function (data) {
            self.changeStateHistory(data);
            $('#statesHistoryPopover').show();
        });
    });

    var isHistoryShowed = false;
    self.showStatesHistory = function () {
        if (!isHistoryShowed) {
            self.changeStateHistory(undefined);
            self.SendRequest(settings.Urls.ChangeStateHistory, { InterviewId: self.questionnaire().id }, function (data) {
                self.changeStateHistory(data);
                $('#statesHistoryPopover').show();
            });
        } else {
            $('#statesHistoryPopover').hide();
        }
        isHistoryShowed = !isHistoryShowed;
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.InterviewDetails, Supervisor.VM.BasePage);