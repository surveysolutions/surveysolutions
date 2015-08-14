Supervisor.VM.NewInterview = function (commandExecutionUrl, questionnaire, interviewListUrl) {
    Supervisor.VM.NewInterview.superclass.constructor.apply(this, [commandExecutionUrl]);
    //console.log(questionnaire);
    var self = this;
    
    var datacontext = new DataContext(new Mapper(new Model()));
    datacontext.parseData(questionnaire);
    
    self.InterviewListUrl = interviewListUrl;

    self.submitting = false;
    self.questionnaire = ko.observable();
    self.responsible = ko.observable().extend({ required: true });
    self.questions = ko.observableArray();
    self.supervisors = ko.observableArray();

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
                        interviewId: self.questionnaire().id,
                        supervisorId: self.responsible().id(),
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
    self.load = function (isViewModelValid) {
        self.IsAjaxComplete(false);
        self.questionnaire(datacontext.questionnaire);
        self.questions(datacontext.questions.getAllLocal());
        self.supervisors(datacontext.supervisors.getAllLocal());
        self.IsAjaxComplete(true);
        self.IsPageLoaded(true);

        self.isViewModelValid = isViewModelValid;
    };

    self.isViewModelValid = function() { return true };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.NewInterview, Supervisor.VM.BasePage);
