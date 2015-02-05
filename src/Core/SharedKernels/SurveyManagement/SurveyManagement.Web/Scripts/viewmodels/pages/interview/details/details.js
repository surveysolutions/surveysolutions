Supervisor.VM.Details = function (settings) {
    Supervisor.VM.Details.superclass.constructor.apply(this, [settings.Urls.CommandExecution]);

    var self = this,
        config = new Config(),
        datacontext = new DataContext(config, settings.Interview.InterviewId);

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
            commentListElement.append('<dt>' + settings.UserName + '<span class="text-normal">' + comment + '</span></dt><dd><small class="comment-date">' + moment(new Date()).fromNow() + '</small></dd>');
            commentListElement.removeClass("hidden");
        });
    };

    self.setFlag = function (element, questionId, underscoreJoinedQuestionRosterVector) {
        var commandName = config.commands.setFlagToAnswer;

        var question = {
            id: questionId,
            rosterVector: parseRosterVector(underscoreJoinedQuestionRosterVector)
        }

        var command = datacontext.getCommand(commandName, question);
        self.SendCommand(command, function () {
            var flagIndicator = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector, "flagIndicator"));
            var setFlagMenuItem = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector, "setFlagMenuItem"));
            var removeFlagMenuItem = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector, "removeFlagMenuItem"));

            $(flagIndicator).removeClass("hidden");
            $(removeFlagMenuItem).removeClass("hidden");
            $(setFlagMenuItem).addClass("hidden");

            var flagsCounterElement = $("#flagsCounter");
            flagsCounterElement.text(parseInt(flagsCounterElement.text()) + 1);
        });
    };

    self.removeFlag = function (element, questionId, underscoreJoinedQuestionRosterVector) {
        var commandName = config.commands.removeFlagFromAnswer;

        var question = {
            id: questionId,
            rosterVector: parseRosterVector(underscoreJoinedQuestionRosterVector)
    }

        var command = datacontext.getCommand(commandName, question);
        self.SendCommand(command, function () {
            var flagIndicator = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector, "flagIndicator"));
            var setFlagMenuItem = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector, "setFlagMenuItem"));
            var removeFlagMenuItem = $('#' + getInterviewItemIdWithPostfix(questionId, underscoreJoinedQuestionRosterVector, "removeFlagMenuItem"));

            $(flagIndicator).addClass("hidden");
            $(removeFlagMenuItem).addClass("hidden");
            $(setFlagMenuItem).removeClass("hidden");

            var flagsCounterElement = $("#flagsCounter");
            flagsCounterElement.text(parseInt(flagsCounterElement.text()) - 1);
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

    function getInterviewItemIdWithPostfix(questionId, rosterVector, postfix) {
        return questionId + "_" + rosterVector + "_" + postfix;
    }

    function parseRosterVector(rosterVector) {
        if (rosterVector == "")
            return [];

        return rosterVector.split('_');
    }
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.Details, Supervisor.VM.BasePage);