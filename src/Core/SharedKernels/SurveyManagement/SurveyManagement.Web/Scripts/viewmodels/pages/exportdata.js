Supervisor.VM.ExportData = function (templates, $dataUrl) {
    Supervisor.VM.ExportData.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = $dataUrl;
    self.Templates = templates;

    self.selectedTemplate = ko.observable();
    self.paraDataUpdateDate = ko.observable();

    self.selectedTemplateId = ko.computed(function () {
        return self.selectedTemplate() && self.selectedTemplate().id;
    });
    self.selectedTemplate.subscribe(function () {
        self.search();
    });

    self.selectedTemplateVersion = ko.computed(function () {
        return self.selectedTemplate() && self.selectedTemplate().version;
    });

    self.selectedTemplateTitle = ko.computed(function () {
        return self.selectedTemplate() && self.selectedTemplate().title;
    });

    self.search = function () {
        var filter = {
            questionnaireId: self.selectedTemplateId(),
            questionnaireVersion: self.selectedTemplate().version
        };
        self.SendRequest(self.Url, filter, function (data) {
            ko.mapping.fromJS(data, self.mappingOptions, self);
        }, true);
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ExportData, Supervisor.VM.BasePage);