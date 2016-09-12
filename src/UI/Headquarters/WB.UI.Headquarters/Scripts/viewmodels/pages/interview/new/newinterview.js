Supervisor.VM.NewInterview = function (commandExecutionUrl, questionnaire, interviewListUrl, supervisorsUrl) {
    Supervisor.VM.NewInterview.superclass.constructor.apply(this, [commandExecutionUrl]);

    var self = this;
    
    var datacontext = new DataContext(new Mapper(new Model()));
    datacontext.parseData(questionnaire);
    
    self.InterviewListUrl = interviewListUrl;

    self.submitting = false;
    self.questionnaire = ko.observable();
    self.responsible = ko.observable().extend({ required: true });
    self.questions = ko.observableArray();
    self.isSupervisorsLoading = ko.observable(false);
    self.supervisors = function (query, sync, pageSize) {
        self.isSupervisorsLoading(true);
        self.SendRequest(supervisorsUrl, { query: query, pageSize: pageSize }, function (response) {
            sync(response.Users, response.TotalCountByQuery);
        }, true, true, function() {
            self.isSupervisorsLoading(false);
        });
    }

    self.isSaveEnable = ko.computed(function() {
        var answersAreInvalid = ko.utils.arrayFilter(self.questions(), function (question) {
            return question.errors().length > 0;
        });
        return $(answersAreInvalid).length == 0 && !_.isUndefined(self.responsible());
    });
    
    self.saveCommand = function() {
            if (!self.isViewModelValid())
                return;
            if (!self.submitting) {
                self.submitting = true;

                var command = {
                    type: "CreateInterviewCommand",
                    command: ko.toJSON({
                        supervisorId: self.responsible().UserId,
                        questionnaireId: self.questionnaire().templateId,
                        questionnaireVersion: self.questionnaire().templateVersion,
                        answersToFeaturedQuestions: datacontext.prepareQuestion()
                    })
                };
                self.SendCommand(command, function(data) {
                    window.location = self.InterviewListUrl.concat("?templateId=",
                        datacontext.questionnaire.templateId, "&templateVersion=",
                        datacontext.questionnaire.templateVersion);
                }, function() {
                    self.submitting = false;
                });
            }
        },
    self.load = function () {
        self.IsAjaxComplete(false);
        self.questionnaire(datacontext.questionnaire);
        self.questions(datacontext.questions.getAllLocal());
        self.IsAjaxComplete(true);
        self.IsPageLoaded(true);
    };

    self.answerTimestampQuestion = function(question) {
        var formattedDate = moment().format('YYYY-MM-DD[T]HH:mm:ssZ');
        question.selectedOption(formattedDate);
    }

    self.errors = ko.validation.group(self);

    self.isViewModelValid = function() {
        if (self.errors().length === 0) {
            return true;
        } else {
            self.errors.showAllMessages();
            return false;
        };
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.NewInterview, Supervisor.VM.BasePage);
