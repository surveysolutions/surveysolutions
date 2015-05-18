Supervisor.VM.ExportData = function (templates) {
    Supervisor.VM.ExportData.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Templates = templates;

self.selectedTemplate = ko.observable();

self.selectedTemplateId = ko.computed(function () {
    return self.selectedTemplate() && self.selectedTemplate().id;
});

self.selectedTemplateVersion = ko.computed(function () {
    return self.selectedTemplate() && self.selectedTemplate().version;
});

self.selectedTemplateTitle = ko.computed(function () {
    return self.selectedTemplate() && self.selectedTemplate().title;
});
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ExportData, Supervisor.VM.BasePage);