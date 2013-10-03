Supervisor.VM.TeamsAndStatuses = function (listViewUrl) {
    Supervisor.VM.TeamsAndStatuses.superclass.constructor.apply(this, arguments);
    
    var self = this;
    self.SelectedTemplate = ko.observable('');

    self.GetFilterMethod = function () {
        var selectedTemplate = Supervisor.Framework.Objects.isEmpty(self.SelectedTemplate())
             ? { templateId: '', version: '' }
             : JSON.parse(self.SelectedTemplate());
        return {
            TemplateId: selectedTemplate.templateId,
            TemplateVersion: selectedTemplate.version
        };
    };

    self.load = function () {
        self.SelectedTemplate.subscribe(self.filter);
        self.search();
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.TeamsAndStatuses, Supervisor.VM.ListView);