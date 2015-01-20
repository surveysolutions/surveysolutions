﻿Supervisor.VM.DesignerQuestionnaires = function (listViewUrl, getQuestionnaireUrl) {
    Supervisor.VM.DesignerQuestionnaires.superclass.constructor.apply(this, arguments);
  
    var self = this;

    self.ImportFailMessage = ko.observable();
    self.VerificationErrors = ko.observableArray([]);

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

    var getQuestionnaireRequest = function (questionnaireViewItem, allowCensusMode) {
        var request = { questionnaire: questionnaireViewItem, allowCensusMode: allowCensusMode };

        self.SendCommand(request, function (data) {
            if ((data.ImportError || "") != "") {
                self.ImportFailMessage(data.ImportError);
                self.ShowOutput();
            }
            else if (!Supervisor.Framework.Objects.isUndefined(data.Errors) && !Supervisor.Framework.Objects.isNull(data.Errors) && data.Errors.length > 0) {
                self.VerificationErrors.removeAll();
                self.ImportFailMessage(data.QuestionnaireTitle);
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

    self.getQuestionnaire = function(questionnaireViewItem) {
        getQuestionnaireRequest(questionnaireViewItem, false);
    };

    self.getQuestionnaireCensus = function(questionnaireViewItem) {
        getQuestionnaireRequest(questionnaireViewItem, true);
    };

    self.onQuestionnaireImported = function () {
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.DesignerQuestionnaires, Supervisor.VM.ListView);