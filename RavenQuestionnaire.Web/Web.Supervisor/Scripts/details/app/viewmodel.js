define('app/viewmodel', ['knockout', 'app/datacontext', 'director', 'input', 'app/config', 'app/model'],
    function (ko, datacontext, Router, input, config, model) {
        var self = this,
            questionnaire = ko.observable(),
            groups = ko.observableArray(),
            questions = ko.observableArray(),
            currentQuestion = ko.observable(),
            currentComment = ko.observable(''),
            closeDetails = function () {
                $('#content').removeClass('details-visible');
            },
            showDetails = function (question, event) {
                event.stopPropagation();
                if (_.isNull(currentQuestion()) == false && _.isUndefined(currentQuestion()) == false) {
                    currentQuestion().isSelected(false);
                }
                currentQuestion(question);
                currentQuestion().isSelected(true);
                $('#content').addClass('details-visible');
            },
            isSaving = ko.observable(false),
            flagedCount = ko.computed(function () {
                return _.reduce(questions(), function (count, question) {
                    return count + (question.isFlagged() ? 1 : 0);
                }, 0);
            }),
            answeredCount = ko.computed(function () {
                return _.reduce(questions(), function (count, question) {
                    return count + (question.isAnswered() ? 1 : 0);
                }, 0);
            }),
            commentedCount = ko.computed(function () {
                return _.reduce(questions(), function (count, question) {
                    return count + (question.comments().length > 0 ? 1 : 0);
                }, 0);
            }),
            invalidCount = ko.computed(function () {
                return _.reduce(questions(), function (count, question) {
                    return count + (question.isValid() ? 0 : 1);
                }, 0);
            }),
            editableCount = ko.computed(function () {
                return _.reduce(questions(), function (count, question) {
                    return count + (question.scope() == "Supervisor" ? 1 : 0);
                }, 0);
            }),
            enabledCount = ko.computed(function () {
                return _.reduce(questions(), function (count, question) {
                    return count + (question.isEnabled() ? 1 : 0);
                }, 0);
            }),
            filter = ko.observable('all'),
            applyQuestionFilter = function (f) {
                filter(f);
                _.each(groups(), function (group) {
                    group.isVisible(true);
                    group.isSelected(false);
                });
                _.each(questions(), function (question) {
                    question.matchFilter(f);
                });
                _.each(groups(), function (group) {
                    if (group.visibleQuestionsCount() > 0) {
                        group.isSelected(true);
                    }
                });
            },
            addComment = function () {
                isSaving(true);
                datacontext.sendCommand(config.commands.setCommentCommand, {
                    comment: currentComment(),
                    questionId: currentQuestion().uiId()
                }, {
                    success: function (response) {
                        var comment = new model.Comment();
                        comment.text(currentComment());
                        comment.date(new Date());
                        comment.userName(datacontext.user.name());
                        comment.userId(datacontext.user.id());
                        currentQuestion().comments.push(comment);
                        currentComment('');
                        isSaving(false);
                    },
                    error: function (response) {
                        self.ShowError(response.error);
                        isSaving(false);
                    }
                });
            },
            flagAnswer = function (question) {
                isSaving(true);
                var commandName = question.isFlagged()
                    ? config.commands.removeFlagFromAnswer
                    : config.commands.setFlagToAnswer;

                datacontext.sendCommand(commandName, { questionId: question.uiId() },
                    {
                        success: function (response) {
                            question.isFlagged(!question.isFlagged());
                            isSaving(false);
                        },
                        error: function (response) {
                            self.ShowError(response.error);
                            isSaving(false);
                        }
                    });
            },
            saveAnswer = function (question) {
                isSaving(true);

                var commandName = "";
                switch (question.questionType()) {
                    case "Text": commandName = config.commands.answerTextQuestionCommand; break;
                    case "AutoPropagate": commandName = config.commands.answerNumericQuestionCommand; break;
                    case "Numeric": commandName = config.commands.answerNumericQuestionCommand; break;
                    case "DateTime": commandName = config.commands.answerDateTimeQuestionCommand; break;
                    case "GpsCoordinates": commandName = config.commands.answerGeoLocationQuestionCommand; break;
                    case "SingleOption":
                        commandName = config.commands.answerSingleOptionQuestionCommand; break;
                    case "MultyOption":
                        commandName = config.commands.answerMultipleOptionsQuestionCommand; break;
                }
                datacontext.sendCommand(commandName, { questionId: question.uiId() },
                    {
                        success: function (response) {
                            isSaving(false);
                        },
                        error: function (response) {
                            self.ShowError(response.error);
                            isSaving(false);
                        }
                    });
            },
            filterClick = function (ui, e) {
                //console.log(ui);
                //console.log(e);
            },
        init = function () {
            questionnaire(datacontext.questionnaire);
            groups(datacontext.groups.getAllLocal());
            questions(datacontext.questions.getAllLocal());
            Router({
                '/group/:groupId': function (groupId) {
                    applyQuestionFilter('all');
                    var visibleGroupsIds = [groupId];
                    _.each(groups(), function (group) {
                        if (_.contains(visibleGroupsIds, group.uiId())) {
                            group.isVisible(true);
                        } else if (_.contains(visibleGroupsIds, group.parentId())) {
                            visibleGroupsIds.push(group.uiId());
                            group.isVisible(true);
                        } else {
                            group.isVisible(false);
                        }
                    });
                },
                '/:filter': function (f) {
                    applyQuestionFilter(f);
                }
            }).init();
        };

        self = {
            filter: filter,
            commentedCount: commentedCount,
            flagedCount: flagedCount,
            answeredCount: answeredCount,
            invalidCount: invalidCount,
            editableCount: editableCount,
            enabledCount: enabledCount,
            questionnaire: questionnaire,
            groups: groups,
            isSaving: isSaving,
            init: init,
            closeDetails: closeDetails,
            showDetails: showDetails,
            currentQuestion: currentQuestion,
            currentComment: currentComment,
            addComment: addComment,
            flagAnswer: flagAnswer,
            saveAnswer: saveAnswer,
            filterClick: filterClick
        };

        ko.utils.extend(self, new Supervisor.VM.BasePage());

        return self;
    });