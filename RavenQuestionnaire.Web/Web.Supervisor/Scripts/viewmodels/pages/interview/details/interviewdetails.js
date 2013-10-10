Supervisor.VM.InterviewDetails = function(commandExecutionUrl, inputQuestionnaire, userName) {
    Supervisor.VM.InterviewDetails.superclass.constructor.apply(this, arguments);

    var self = this,
        config = new Config(),
        model = new Model(),
        mapper = new Mapper(model, config),
        datacontext = new DataContext(mapper, config);
    
    self.filter = ko.observable('all');
    self.questionnaire = ko.observable();
    self.groups = ko.observableArray();
    self.questions = ko.observableArray();
    self.currentQuestion = ko.observable();
    self.currentComment = ko.observable('');

    self.closeDetails = function() {
        $('#content').removeClass('details-visible');
    };
    self.showDetails = function(question, event) {
        event.stopPropagation();
        if (_.isNull(self.currentQuestion()) == false &&
            _.isUndefined(self.currentQuestion()) == false) {
            self.currentQuestion().isSelected(false);
        }
        self.currentQuestion(question);
        self.currentQuestion().isSelected(true);
        $('#content').addClass('details-visible');
    };

    self.flagedCount = ko.computed(function() {
        return _.reduce(self.questions(), function(count, question) {
            return count + (question.isFlagged() ? 1 : 0);
        }, 0);
    });
    self.answeredCount = ko.computed(function() {
        return _.reduce(self.questions(), function(count, question) {
            return count + (question.isAnswered() ? 1 : 0);
        }, 0);
    });
    self.commentedCount = ko.computed(function() {
        return _.reduce(self.questions(), function(count, question) {
            return count + (question.comments().length > 0 ? 1 : 0);
        }, 0);
    });
    self.invalidCount = ko.computed(function() {
        return _.reduce(self.questions(), function(count, question) {
            return count + (question.isValid() ? 0 : 1);
        }, 0);
    });
    self.editableCount = ko.computed(function() {
        return _.reduce(self.questions(), function(count, question) {
            return count + (question.scope() == "Supervisor" ? 1 : 0);
        }, 0);
    });
    self.enabledCount = ko.computed(function() {
        return _.reduce(self.questions(), function(count, question) {
            return count + (question.isEnabled() ? 1 : 0);
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
            questionId: self.currentQuestion().uiId()
        });
        self.SendCommand(command, function () {
            var comment = new model.Comment();
            comment.text(self.currentComment());
            comment.date(new Date());
            comment.userName(userName);
            self.currentQuestion().comments.push(comment);
            self.currentComment('');
        });
    };
    self.flagAnswer = function(question) {
        var commandName = question.isFlagged()
            ? config.commands.removeFlagFromAnswer
            : config.commands.setFlagToAnswer;

        var command = datacontext.getCommand(commandName, { questionId: question.uiId() });
        self.SendCommand(command, function () {
            question.isFlagged(!question.isFlagged());
        });
    };
    self.saveAnswer = function(question) {
        var commandName = "";
        switch (question.questionType()) {
        case "Text":
            commandName = config.commands.answerTextQuestionCommand;
            break;
        case "AutoPropagate":
            commandName = config.commands.answerNumericQuestionCommand;
            break;
        case "Numeric":
            commandName = config.commands.answerNumericQuestionCommand;
            break;
        case "DateTime":
            commandName = config.commands.answerDateTimeQuestionCommand;
            break;
        case "GpsCoordinates":
            commandName = config.commands.answerGeoLocationQuestionCommand;
            break;
        case "SingleOption":
            commandName = config.commands.answerSingleOptionQuestionCommand;
            break;
        case "MultyOption":
            commandName = config.commands.answerMultipleOptionsQuestionCommand;
            break;
        }

        var command = datacontext.getCommand(commandName, { questionId: question.uiId() });
        self.SendCommand(command);
    };
    self.filterClick = function(ui, e) {
        //console.log(ui);
        //console.log(e);
    };
    self.load = function () {
        self.IsAjaxComplete(false);
        datacontext.parseData(inputQuestionnaire);
        self.questionnaire(datacontext.questionnaire);
        self.groups(datacontext.groups.getAllLocal());
        self.questions(datacontext.questions.getAllLocal());
        
        Router({
            '/group/:groupId': function(groupId) {
                self.applyQuestionFilter('all');
                var visibleGroupsIds = [groupId];
                $.each(self.groups(), function(index, group) {
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
            '/:filter': function(f) {
                self.applyQuestionFilter(f);
            }
        }).init();
        self.IsAjaxComplete(true);
        self.IsPageLoaded(true);
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.InterviewDetails, Supervisor.VM.BasePage);