Supervisor.VM.TeamsAndStatuses = function (listViewUrl) {
    Supervisor.VM.TeamsAndStatuses.superclass.constructor.apply(this, arguments);
    
    var self = this;
    self.Url = new Url(window.location.href);
    self.SelectedTemplate = ko.observable('');
    self.TotalRow = ko.observable(null);
    this.QuestionnaireName = ko.observable();

    self.GetFilterMethod = function () {
        var selectedTemplate = Supervisor.Framework.Objects.isEmpty(self.SelectedTemplate())
             ? { templateId: '', version: '' }
             : JSON.parse(self.SelectedTemplate());
        self.Url.query['templateId'] = selectedTemplate.templateId;
        self.Url.query['templateVersion'] = selectedTemplate.version;
        if (Modernizr.history) {
            window.history.pushState({}, "Summary", self.Url.toString());
        }

        return {
            TemplateId: selectedTemplate.templateId,
            TemplateVersion: selectedTemplate.version
        };
    };

    self.load = function () {
        self.SelectedTemplate("{\"templateId\": \"" + self.QueryString['templateId'] + "\",\"version\": \"" + self.QueryString['templateVersion'] + "\"}");

        updateQuestionnaireName(self.SelectedTemplate());

        self.SelectedTemplate.subscribe(function (value) {
            updateQuestionnaireName(self.SelectedTemplate());
            self.filter();
        });

        self.search();
    };

    var updateQuestionnaireName = function (value) {
        self.QuestionnaireName($("#questionnaireSelector option[value='" + value + "']").text());
    }
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.TeamsAndStatuses, Supervisor.VM.ListView);