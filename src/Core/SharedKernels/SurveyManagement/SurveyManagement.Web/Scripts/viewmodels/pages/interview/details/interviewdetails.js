Supervisor.VM.InterviewDetails = function (settings) {
    Supervisor.VM.InterviewDetails.superclass.constructor.apply(this, [settings.Urls.CommandExecution]);

    var self = this,
        config = new Config(),
        datacontext = new DataContext(config, settings.Interview.InterviewId);

    self.filter = ko.observable('all');
    self.questionnaire = ko.observable();
    self.groups = ko.observableArray();
    self.entities = ko.observableArray();
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

    self.questions = ko.computed(function() {
        return _.filter(self.entities(), function(entity) { return !self.isStaticText(entity); });
    });

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
        $.each(self.entities(), function(index, entity) {
            entity.matchFilter(f);
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

            var entities = self.prepareGroupsAndQuestionsAndReturnAllEntities(interview.Groups);

            self.questionnaire(interview.InterviewInfo);
            self.groups(interview.Groups);
            self.entities(entities);

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
    
    self.prepareGroupsAndQuestionsAndReturnAllEntities = function (groups) {
        var allEntities = [];
        $.each(groups, function(i, group) {

            $.each(group.entities, function(j, entity) {
                allEntities.push(entity);

                entity.isVisible = ko.observable(true);
                entity.isSelected = ko.observable(false);

                if (self.isStaticText(entity)) {
                    entity.matchFilter = function(filter) {
                        switch (filter) {
                        case "all":
                            entity.isVisible(true);
                            break;
                        default:
                            entity.isVisible(false);
                            break;
                        }
                    };
                } else {
                    entity.comments = ko.observableArray(entity.comments);
                    entity.isFlagged = ko.observable(entity.isFlagged);
                    entity.markerStyle = ko.computed(function() {
                        if (entity.isInvalid) {
                            return "invalid";
                        }
                        if (entity.scope == "Supervisor") {
                            return "supervisor";
                        }
                        return "";
                    });
                    entity.matchFilter = function(filter) {
                        switch (filter) {
                        case "all":
                            entity.isVisible(true);
                            break;
                        case "flaged":
                            entity.isVisible(entity.isFlagged());
                            break;
                        case "commented":
                            entity.isVisible(entity.comments().length > 0);
                            break;
                        case "answered":
                            entity.isVisible(entity.isAnswered);
                            break;
                        case "invalid":
                            entity.isVisible(entity.isInvalid);
                            break;
                        case "supervisor":
                            entity.isVisible(entity.scope == "Supervisor");
                            break;
                        case "enabled":
                            entity.isVisible(entity.isEnabled);
                            break;
                        }
                    };
                    if (entity.scope == "Supervisor") {
                        switch (entity.questionType) {
                        case config.questionTypes.Text:
                            entity.answer = ko.observable(entity.answer).extend({ required: true });
                            break;
                        case config.questionTypes.Numeric:
                            entity.answer = ko.observable(entity.answer).extend({ required: true });
                            if (entity.isInteger) {
                                entity.answer.extend({ numericValidator: -1 });
                            } else if (!_.isNull(entity.countOfDecimalPlaces)) {
                                entity.answer.extend({ numericValidator: entity.countOfDecimalPlaces });
                            } else {
                                entity.answer.extend({ numericValidator: true });
                            }
                            break;
                        case config.questionTypes.SingleOption:
                            entity.selectedOption = ko.observable(entity.selectedOption).extend({
                                validation: [
                                    {
                                        validator: function(val) {
                                            if (_.isNull(val) || _.isUndefined(val) || _.isEmpty(val))
                                                return false;
                                            return true;
                                        },
                                        message: 'At least one option should be checked'
                                    }
                                ]
                            });
                            entity.answer = ko.computed(function() {
                                var o = _.find(entity.options, function(option) {
                                    return entity.selectedOption() == option.value;
                                });
                                return _.isEmpty(o) ? "" : o.label;
                            });
                            break;
                        case config.questionTypes.MultyOption:
                            var selectedOptionsSource = entity.selectedOptions;
                            entity.selectedOptions = ko.observableArray().extend({
                                validation: [
                                    {
                                        validator: function(val) {
                                            if (_.isNull(val) || _.isUndefined(val) || _.isEmpty(val))
                                                return false;
                                            return val.length > 0;
                                        },
                                        message: 'At least one option should be checked'
                                    },
                                    {
                                        validator: function(val) {
                                            if (_.isUndefined(entity.maxAllowedAnswers) || _.isNull(entity.maxAllowedAnswers)) {
                                                return true;
                                            }

                                            return val.length <= entity.maxAllowedAnswers;
                                        },
                                        message: 'Number of selected answers more than number of maximum permitted answers'
                                    }
                                ]
                            });

                            if (entity.areAnswersOrdered) {
                                entity.orderedOptionsSelection = ko.observableArray([]);
                                entity.selectedOptionsCount = 0;
                                entity.orderSelectedOptions = function() {
                                    if (entity.selectedOptionsCount != entity.selectedOptions().length) {
                                        if (entity.selectedOptionsCount > entity.selectedOptions().length) {
                                            _.each(entity.orderedOptionsSelection(), function(answer) {
                                                if (!_.contains(entity.selectedOptions(), answer)) {
                                                    entity.orderedOptionsSelection.remove(answer);
                                                }
                                            });
                                        }
                                        _.each(entity.options, function(option) {
                                            var orderIndex = entity.orderedOptionsSelection().indexOf(option.value);
                                            if (_.contains(entity.selectedOptions(), option.value)) {
                                                if (_.isNull(option.orderNo())) {
                                                    option.orderNo(entity.selectedOptions().length);
                                                    entity.orderedOptionsSelection.push(option.value);
                                                } else {
                                                    if (orderIndex > -1) {
                                                        option.orderNo(orderIndex + 1);
                                                    }
                                                }
                                            } else {
                                                if (entity.selectedOptionsCount > entity.selectedOptions().length) {
                                                    if (orderIndex == -1) {
                                                        option.orderNo(null);
                                                    }
                                                }
                                            }
                                        });
                                        entity.selectedOptionsCount = entity.selectedOptions().length;
                                    }
                                };
                                entity.answer = ko.computed(function() {
                                    var selected = _.filter(entity.options, function(option) {
                                        return _.contains(entity.selectedOptions(), option.value);
                                    });

                                    var a = _.reduce(selected, function(result, option) {
                                        return result + option.label + ", ";
                                    }, "").trim();
                                    if (_.isEmpty(a) == false) {
                                        a = a.substring(0, a.length - 1);
                                    }
                                    return a;
                                });

                                entity.selectedOptions.subscribe(function() {
                                    entity.orderSelectedOptions();
                                });

                                $.each(entity.options, function(index, option) {
                                    option.orderNo = ko.observable(option.orderNo);
                                });
                                $.each(selectedOptionsSource, function(index, option) {
                                    entity.selectedOptions.push(option);
                                });
                            }
                            break;
                        }
                        entity.errors = ko.validation.group(entity);
                    }
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
                return _.reduce(group.entities, function (count, entity) {
                    return count + (entity.isVisible() && !self.isStaticText(entity) ? 1 : 0);
                }, 0);
            });
        });
        return allEntities;
    };

    self.isStaticText = function(entity) {
        return entity.hasOwnProperty('staticText');
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