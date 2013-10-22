Supervisor.VM.DesignerQuestionnaires = function (listViewUrl, getQuestionnaireUrl) {
    Supervisor.VM.DesignerQuestionnaires.superclass.constructor.apply(this, arguments);
    
    var self = this;
    self.getQuestionnaireUrl = getQuestionnaireUrl;
    self.Query = ko.observable('');
    
    self.GetFilterMethod = function () {
        return {
            Filter: self.Query()
        };
    };
    self.load = function () {
        self.search();
    };

    self.getQuestionnaire = function(questionnaireViewItem) {

        var request = { questionnaireId: questionnaireViewItem.Id() };

        self.SendCommand(request, function() {
            self.onQuestionnaireImported();
        });
    };

    self.onQuestionnaireImported = function() {

    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.DesignerQuestionnaires, Supervisor.VM.ListView);