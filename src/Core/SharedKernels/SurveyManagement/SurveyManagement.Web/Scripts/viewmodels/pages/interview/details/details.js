Supervisor.VM.Details = function (settings, filter) {
    Supervisor.VM.Details.superclass.constructor.apply(this, [settings.Urls.CommandExecution]);

    var self = this,
        config = new Config(),
        datacontext = new DataContext(config, settings.Interview.InterviewId);

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

            var commentListElement = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector, "commentList"));
            if (commentListElement.children().length == 0) {
                var commentsCounterElement = $("#commentsCounter");
                commentsCounterElement.text(parseInt(commentsCounterElement.text()) + 1);
            }
            commentListElement.append('<dt>' + settings.UserName + ' <span class="text-normal">' + comment + '</span></dt><dd><small class="comment-date" date="' + moment(new Date()).fromNow() + '"></small></dd>');
            commentListElement.removeClass("hidden");
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

    self.saveTextAnswer = function (questionId, underscoreJoinedQuestionRosterVector) {
        var answerElement = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector));

        var question = prepareQuestionForCommand(questionId, underscoreJoinedQuestionRosterVector);
        question.answer = ko.observable(answerElement.val());

        sendAnswerCommand(config.commands.answerTextQuestionCommand, question);
    };

    self.saveNumericIntegerAnswer = function (questionId, underscoreJoinedQuestionRosterVector) {
        var answerElement = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector));

        var question = prepareQuestionForCommand(questionId, underscoreJoinedQuestionRosterVector);
        question.answer = ko.observable(answerElement.val());

        sendAnswerCommand(config.commands.answerNumericIntegerQuestionCommand, question);
    };

    self.saveNumericRealAnswer = function (questionId, underscoreJoinedQuestionRosterVector, countOfDecimalPlaces) {
        var answerElement = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector));

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

    self.saveCategoricalMultiAnswer = function (questionId, underscoreJoinedQuestionRosterVector, areAnswersOrdered, maxAnswersCount, selectedOptionsAsString) {
        var answerElementId = getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector);
        var answerOptionValues = $("input:checkbox[name=" + answerElementId + "]:checked").map(function() { return parseFloat($(this).val()); }).get();
        
        var question = prepareQuestionForCommand(questionId, underscoreJoinedQuestionRosterVector);
        question.areAnswersOrdered = false;

        if (areAnswersOrdered) {
            var selectedOptions = selectedOptionsAsString.split(',').map(function (answerAsString) { return parseFloat(answerAsString); });
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
        $("input[mask]").each(function (index, item) {
            ko.bindingHandlers.maskFormatter.init(this, function () {
                return $(item).attr("mask");
            });
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