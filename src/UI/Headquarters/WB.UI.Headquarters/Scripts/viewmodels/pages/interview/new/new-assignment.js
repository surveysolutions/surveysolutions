Supervisor.VM.NewAssignment = function (questionnaire, assignmentListUrl, supervisorsUrl, assignmentsApiUrl) {
    Supervisor.VM.NewAssignment.superclass.constructor.apply(this);

    var self = this;
    
    var datacontext = new DataContext(new Mapper(new Model()));
    datacontext.parseData(questionnaire);

    self.assignmentListUrl = assignmentListUrl;
    self.submitting = false;
    self.submitSuccess = false;
    self.questionnaire = ko.observable();
    self.responsible = ko.observable().extend({ required: true });
    self.quantity = ko.observable(1).extend({ digit: true }).extend({ min: 1 });
    self.questions = ko.observableArray();
    self.isSupervisorsLoading = ko.observable(false);
    self.supervisors = function (query, sync, pageSize) {
        self.isSupervisorsLoading(true);
        self.SendRequest(supervisorsUrl, { query: query, pageSize: pageSize }, function (response) {
            sync(response.options, response.total);
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
            if (!self.submitting && !self.submitSuccess) {
                self.submitting = true;

                var request = {
                        responsibleId: self.responsible().key,
                        questionnaireId: self.questionnaire().templateId,
                        questionnaireVersion: self.questionnaire().templateVersion,
                        answersToFeaturedQuestions: ko.toJSON(datacontext.prepareQuestion()),
                        quantity: self.quantity()
                };

                self.SendRequest(assignmentsApiUrl, request, function () {
                    self.submitSuccess = true;
                    window.location = self.assignmentListUrl + "?templateId=" +
                        datacontext.questionnaire.templateId + "&templateVersion=" +
                        datacontext.questionnaire.templateVersion;
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
        }
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.NewAssignment, Supervisor.VM.BasePage);
