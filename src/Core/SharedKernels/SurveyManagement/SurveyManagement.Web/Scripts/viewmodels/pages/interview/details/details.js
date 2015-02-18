Supervisor.VM.Details = function (settings, filter, filteredComboboxes) {
    Supervisor.VM.Details.superclass.constructor.apply(this, [settings.Urls.CommandExecution]);

    var self = this,
        config = new Config(),
        datacontext = new DataContext(config, settings.Interview.InterviewId);

    self.filteredComboboxes = filteredComboboxes.map(function(combobox) {
        combobox.options = combobox.options.map(function(option) {
            option.label = decodeURIComponent(option.label);
            return option;
        });
        return combobox;
    });
    self.changeStateComment = ko.observable('');

    self.addComment = function (element, questionId, underscoreJoinedQuestionRosterVector) {

        var commentInputElement = $(element);
        var comment = commentInputElement.val();

        if (comment == "") return;

        var command = datacontext.getCommand(config.commands.setCommentCommand, {
            comment: comment,
            question: {
                id: questionId,
                rosterVector: parseRosterVector(underscoreJoinedQuestionRosterVector)
            }
        });
        self.SendCommand(command, function () {
            commentInputElement.val('');

            var commentInfo = { userName: settings.UserName, comment: comment, date: new Date() };
            var commentTemplate = $("<div/>").html($('#comment-template').html())[0];
            ko.applyBindings(commentInfo, commentTemplate);

            var commentListElement = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector, "commentList"));
            if (commentListElement.children().length == 0) {
                var commentsCounterElement = $("#commentsCounter");
                commentsCounterElement.text(parseInt(commentsCounterElement.text()) + 1);
            }
            commentListElement.removeClass("hidden");
            commentListElement.append($(commentTemplate).html());
        });
    };

    self.flagAnswer = function (element, questionId, underscoreJoinedQuestionRosterVector) {

        var question = {
            id: questionId,
            rosterVector: parseRosterVector(underscoreJoinedQuestionRosterVector),
            isAnswerFlagged: $(element).hasClass("btn-info")
        }

        var commandName = question.isAnswerFlagged ? config.commands.removeFlagFromAnswer : config.commands.setFlagToAnswer;

        var command = datacontext.getCommand(commandName, question);
        self.SendCommand(command, function () {
            var flagsCounterElement = $("#flagsCounter");

            if (question.isAnswerFlagged) {
                if (filter.filteredBy == 'Flagged') {
                    var answerRowElement = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector, "answerRow"));
                    answerRowElement.remove();
                } else {
                    $(element).removeClass("btn-info");
                    $(element).addClass("btn-default");
                }
                flagsCounterElement.text(parseInt(flagsCounterElement.text()) - 1);
            } else {
                $(element).removeClass("btn-default");
                $(element).addClass("btn-info");
                flagsCounterElement.text(parseInt(flagsCounterElement.text()) + 1);
            }
        });

    }

    self.saveFilteredComboboxAnswer = function(questionId, underscoreJoinedQuestionRosterVector) {
        var answerElement = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector));
        var answerLabel = answerElement.val();

        var filteredCombobox = _.find(self.filteredComboboxes, function (item) {
            return item.id == getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector);
        });

        var answer = _.find(filteredCombobox.options, function(option) {
            return option.value == filteredCombobox.selectedValue().id();
        });

        var chooseOneOfSuggestedValuesError = "Choose one of suggested values";

        if (_.isEmpty(answer)) {
            self.ShowError(chooseOneOfSuggestedValuesError);
            return;
        }

        var observableSelectedOptionId = ko.observable(answerLabel).extend({
            required: true,
            equal: {
                params: answer.label,
                message: chooseOneOfSuggestedValuesError
            }
        });

        if (!observableSelectedOptionId.isValid()) {
            self.ShowError(observableSelectedOptionId.error);
            return;
        }

        var selectedOptionId = filteredCombobox.selectedValue().id();

        var question = prepareQuestionForCommand(questionId, underscoreJoinedQuestionRosterVector);
        question.selectedOption = ko.observable(selectedOptionId);

        sendAnswerCommand(config.commands.answerSingleOptionQuestionCommand, question);
    };

    self.saveTextAnswer = function (questionId, underscoreJoinedQuestionRosterVector) {
        var answerElement = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector));
        var answer = answerElement.val();
        var observableTextAnswer = ko.observable(answer).extend({ required: true });

        if (!observableTextAnswer.isValid()) {
            self.ShowError(observableTextAnswer.error);
            return;
        }

        var question = prepareQuestionForCommand(questionId, underscoreJoinedQuestionRosterVector);
        question.answer = ko.observable(observableTextAnswer());

        sendAnswerCommand(config.commands.answerTextQuestionCommand, question);
    };

    self.saveNumericIntegerAnswer = function (questionId, underscoreJoinedQuestionRosterVector) {
        var answerElement = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector));
        var answer = answerElement.val();
        var observableTextAnswer = ko.observable(answer).extend({ required: true, numericValidator: -1 });

        if (!observableTextAnswer.isValid()) {
            self.ShowError(observableTextAnswer.error);
            return;
        }

        var question = prepareQuestionForCommand(questionId, underscoreJoinedQuestionRosterVector);
        question.answer = ko.observable(observableTextAnswer());

        sendAnswerCommand(config.commands.answerNumericIntegerQuestionCommand, question);
    };

    self.saveNumericRealAnswer = function (questionId, underscoreJoinedQuestionRosterVector, countOfDecimalPlaces) {
        var answerElement = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector));
        var answer = answerElement.val();
        var observableTextAnswer = ko.observable(answer).extend({ required: true });
        if (countOfDecimalPlaces) {
            observableTextAnswer.extend({ numericValidator: countOfDecimalPlaces });
        } else {
            observableTextAnswer.extend({ numericValidator: true });
        }

        if (!observableTextAnswer.isValid()) {
            self.ShowError(observableTextAnswer.error);
            return;
        }

        var question = prepareQuestionForCommand(questionId, underscoreJoinedQuestionRosterVector);
        question.answer = ko.observable(answerElement.val());

        sendAnswerCommand(config.commands.answerNumericRealQuestionCommand, question);
    };

    self.saveCategoricalOneAnswer = function (questionId, underscoreJoinedQuestionRosterVector) {
        var answerElementId = getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector);
        var answerOptionValue = $("input:radio[name=" + answerElementId + "]:checked").val();

        var question = prepareQuestionForCommand(questionId, underscoreJoinedQuestionRosterVector);
        question.selectedOption = ko.observable(answerOptionValue);

        sendAnswerCommand(config.commands.answerSingleOptionQuestionCommand, question);
    };

    self.saveCategoricalMultiAnswer = function (questionId, underscoreJoinedQuestionRosterVector, areAnswersOrdered, maxAllowedAnswers, selectedOptionsAsString) {
        var answerElementId = getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector);
        var answerOptionValues = $("input:checkbox[name=" + answerElementId + "]:checked").map(function() { return parseFloat($(this).val()); }).get();
        
        var observableSelectedOptionIds = ko.observableArray(answerOptionValues).extend({
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
                        if (_.isUndefined(maxAllowedAnswers) || _.isNull(maxAllowedAnswers) || !_.isNumber(maxAllowedAnswers)) {
                            return true;
                        }

                        return val.length <= maxAllowedAnswers;
                    },
                    message: 'Number of selected answers more than number of maximum permitted answers'
                }
            ]
        });

        if (!observableSelectedOptionIds.isValid()) {
            self.ShowError(observableSelectedOptionIds.error);
            return;
        }

        var question = prepareQuestionForCommand(questionId, underscoreJoinedQuestionRosterVector);
        question.areAnswersOrdered = false;

        if (areAnswersOrdered) {
            var selectedOptions = selectedOptionsAsString == "" ? [] : selectedOptionsAsString.split(',').map(function(answerAsString) { return parseFloat(answerAsString); });
            if (selectedOptions.length > answerOptionValues.length) {
                _.remove(selectedOptions, function (answer) {
                    return !_.contains(answerOptionValues, answer);
                });
            } else {
                var selectedAnswer = _.find(answerOptionValues, function(answer) {
                    return !_.contains(selectedOptions, answer);
                });
                selectedOptions.push(selectedAnswer);
            }
            question.selectedOptions = ko.observable(selectedOptions);
        } else {
            question.selectedOptions = ko.observable(answerOptionValues);
        }

        sendAnswerCommand(config.commands.answerMultipleOptionsQuestionCommand, question);
    };

    self.load = function () {
        $("input[mask]").each(function(index, item) {
            ko.bindingHandlers.maskFormatter.init(item, function () {
                return $(item).attr("mask");
            });
        });
        $("input.numeric").each(function (index, item) {
            $(item).keydown(function() {
                ko.bindingHandlers.numericformatter.update(item, ko.observable($(item).val()));
            });
        });
        _.forEach(self.filteredComboboxes, function (filteredCombobox) {
            var filteredComboboxElement = $("#" + filteredCombobox.id);

            filteredCombobox.selectedValue = ko.observable({ id: ko.observable(), value: ko.observable() });
            var selectedOptionId = filteredComboboxElement.val();
            if (selectedOptionId) {
                filteredCombobox.selectedValue().id(selectedOptionId);
                filteredCombobox.selectedValue().value(_.find(filteredCombobox.options, function (option) { return option.value == selectedOptionId; }).label);
            }
            ko.bindingHandlers.typeahead.init(filteredComboboxElement, function () {
                return filteredCombobox.options;
            }, filteredCombobox.selectedValue);
        });
        updateCommentDates();
        setInterval(updateCommentDates, 60000);
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

    self.ToggleFilter = function () {
        if (self.IsFilterOpen()) {
            $('body').addClass('menu-hidden');

            $('#content').removeClass('col-md-9');
            $('#content').removeClass('col-md-offset-3');
            $('#content').addClass('col-md-12');
        } else {
            $('body').removeClass('menu-hidden');

            $('#content').addClass('col-md-9');
            $('#content').addClass('col-md-offset-3');
            $('#content').removeClass('col-md-12');
        }
        self.IsFilterOpen(!self.IsFilterOpen());
    };

    function updateCommentDates() {
        $(".comment-date").each(function (index, item) {
            $(item).text(moment($(item).attr("date")).fromNow());
        });
    };

    function sendAnswerCommand(commandName, question) {
        var command = datacontext.getCommand(commandName, question);
        self.SendCommand(command, function () {
            location.reload();
        });
    };

    function getInterviewItemIdWithPostfix(questionId, rosterVector, postfix) {
        return questionId + "_" + rosterVector + "_" + (_.isUndefined(postfix) ? "" : postfix);
    }

    function parseRosterVector(rosterVector) {
        if (rosterVector == "")
            return [];

        return rosterVector.split('_').map(function (vector) { return vector.replace('-', '.'); });
    }

    function prepareQuestionForCommand(questionId, underscoreJoinedQuestionRosterVector) {
        return {
            id: questionId,
            rosterVector: parseRosterVector(underscoreJoinedQuestionRosterVector)
        };
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.Details, Supervisor.VM.BasePage);