Supervisor.VM.Details = function (settings) {
    Supervisor.VM.Details.superclass.constructor.apply(this, [settings.Urls.CommandExecution]);

    var self = this,
        config = new Config(),
        datacontext = new DataContext(config, settings.Interview.InterviewId);

    self.questionnaire = ko.observable();
    self.currentComment = ko.observable('');
    self.changeStateComment = ko.observable('');
    self.changeStateHistory = ko.observable();
    

    self.getImageUrl = function(fileName) {
        return settings.Urls.InterviewFile + "?interviewId=" + settings.Interview.InterviewId + "&fileName=" + fileName;
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

    function getInterviewItemIdWithPostfix(questionId, rosterVector, postfix) {
        return questionId + "_" + rosterVector + "_" + postfix;
    }

    function parseRosterVector(rosterVector) {
        if (rosterVector == "")
            return null;

        return rosterVector.split('_');
    }
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.Details, Supervisor.VM.BasePage);