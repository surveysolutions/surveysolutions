Supervisor.VM.NewInterview = function (commandExecutionUrl, questionnaire, interviewListUrl) {
    Supervisor.VM.NewInterview.superclass.constructor.apply(this, [commandExecutionUrl]);
    
    var self = this;
    
    var datacontext = new DataContext(new Mapper(new Model()));
    datacontext.parseData(questionnaire);
    
    self.InterviewListUrl = interviewListUrl;

    self.questionnaire = ko.observable();
    self.responsible = ko.observable();
    self.questions = ko.observableArray();
    self.supervisors = ko.observableArray();

    self.isSaveEnable = ko.computed(function() {
        var answersAreInvalid = ko.utils.arrayFilter(self.questions(), function (question) {
            return question.errors().length > 0;
        });
        return $(answersAreInvalid).length == 0;
    });
    
    self.saveCommand = function() {
        var command = {
            type: "CreateInterviewCommand",
            command: ko.toJSON({
                interviewId: self.questionnaire().id,
                supervisorId: self.responsible().id(),
                questionnaireId: self.questionnaire().templateId,
                answersToFeaturedQuestions: datacontext.prepareQuestion()
            })
        };
        self.SendCommand(command, function(data) {
            window.location = self.InterviewListUrl.concat("?templateId=",
                datacontext.questionnaire.templateId, "&templateVersion=",
                datacontext.questionnaire.templateVersion);
        });
    },
    self.load = function () {
        self.IsAjaxComplete(false);
        self.questionnaire(datacontext.questionnaire);
        self.questions(datacontext.questions.getAllLocal());
        self.supervisors(datacontext.supervisors.getAllLocal());
        self.IsAjaxComplete(true);
        self.IsPageLoaded(true);
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.NewInterview, Supervisor.VM.BasePage);
