Supervisor.VM.Questionnaires = function(listViewUrl, deleteQuestionnaireUrl) {
    Supervisor.VM.Questionnaires.superclass.constructor.apply(this, arguments);

    var self = this;

    self.Query = ko.observable('');

    self.GetFilterMethod = function() {
        return {
            Filter: self.Query()
        };
    };
    self.load = function() {
        self.search();
    };

    self.deleteQuestionnaire = function (questionnaireViewItem) {
        if (confirm(input.settings.messages.deleteQuestionnaireConfirmationMessage)) {
            var deleteQuestionnaireCommand = { questionnaireId: questionnaireViewItem.QuestionnaireId(), version: questionnaireViewItem.Version() };

            self.SendCommand(deleteQuestionnaireCommand, function () {
                setTimeout(function() { self.search(); }, 100);
            });   
        }
    };
}
Supervisor.Framework.Classes.inherit(Supervisor.VM.Questionnaires, Supervisor.VM.ListView);