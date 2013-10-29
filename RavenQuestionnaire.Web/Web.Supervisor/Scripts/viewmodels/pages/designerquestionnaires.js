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

        self.SendCommand(request, function (data) {

            if (!Supervisor.Framework.Objects.isUndefined(data.Errors) && !Supervisor.Framework.Objects.isNull(data.Errors) && data.Errors.length > 0) {
                self.VerificationErrors.removeAll();
                $.each(data.Errors, function (index, error) {

                    var uiReferences = $.map(error.References, function (item) {
                        return { title: item.Title, type: item.Type, id: item.Id };
                    });
                    self.VerificationErrors.push({ message: error.Message, code: error.Code, references: uiReferences });
                });
                self.ShowOutput();
            } else
                self.onQuestionnaireImported();
        });
    };
    self.onQuestionnaireImported = function () {
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.DesignerQuestionnaires, Supervisor.VM.ListView);