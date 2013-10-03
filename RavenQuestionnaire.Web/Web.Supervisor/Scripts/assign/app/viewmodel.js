define('app/viewmodel', ['knockout', 'app/datacontext', 'input'],
    function (ko, datacontext, input) {
        var self = this,
            questionnaire = ko.observable(),
            responsible = ko.observable(),
            questions = ko.observableArray(),
            supervisors = ko.observableArray(),
            isSaving = ko.observable(false),
            isSaveEnable = ko.computed(function () {
                var answersAreInvalid = _.any(questions(), function (question) {
                    return question.errors().length > 0;
                });
                return (isSaving() === false) && !answersAreInvalid;
            }),
            saveCommand = function () {
                isSaving(true);
                datacontext.sendCommand("CreateInterviewCommand", ko.toJS(responsible), {
                    success: function (response) {
                        var backUrl = input.backUrl.replace("_______", datacontext.questionnaire.templateId);
                        window.location = backUrl;
                        isSaving(false);
                    },
                    error: function (response) {
                        self.ShowError(response.error);
                        isSaving(false);
                    }
                });
            },
            init = function () {
                questionnaire(datacontext.questionnaire);
                questions(datacontext.questions.getAllLocal());
                supervisors(datacontext.supervisors.getAllLocal());
            };

        self = {
            init: init,
            questions: questions,
            supervisors: supervisors,
            responsible: responsible,
            questionnaire: questionnaire,
            saveCommand: saveCommand,
            isSaveEnable: isSaveEnable,
            isSaving: isSaving
        };

        ko.utils.extend(self, new Supervisor.VM.BasePage());
        
        return self;
    });