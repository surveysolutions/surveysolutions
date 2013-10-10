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

        if (!self.IsAjaxComplete()) {
            self.CheckForRequestComplete();
            return;
        }

        self.IsAjaxComplete(false);

        var request = { questionnaireId: questionnaireViewItem.Id() };
        
        $.post(self.getQuestionnaireUrl, request, null, "json")
            .done(function(data) {
                if (data.IsSuccess) {
                    self.onQuestionnaireImported();
                } else {
                    self.showImportQuestionnaireError();
                }
            }).fail(function() {
                self.showImportQuestionnaireError();
            }).always(function() {
                self.IsAjaxComplete(true);
            });

    };

    self.showImportQuestionnaireError = function() {
        self.ShowError(input.settings.messages.unhandledExceptionMessage);
    };

    self.onQuestionnaireImported = function() {

    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.DesignerQuestionnaires, Supervisor.VM.ListView);