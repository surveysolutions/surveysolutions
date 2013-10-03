Supervisor.VM.InterviewsBase = function (serviceUrl, interviewDetailsUrl, users, commandExecutionUrl) {
    Supervisor.VM.InterviewsBase.superclass.constructor.apply(this, [serviceUrl, commandExecutionUrl]);
    
    var self = this;
    
    self.Url = new Url(interviewDetailsUrl);
    self.Users = users;

    self.Templates = ko.observableArray([]);
    self.Responsibles = ko.observableArray([]);
    self.Statuses = ko.observableArray([]);

    self.SelectedTemplate = ko.observable('');
    self.SelectedResponsible = ko.observable('');
    self.SelectedStatus = ko.observable('');
    
    self.GetFilterMethod = function () {

        var selectedTemplate = Supervisor.Framework.Objects.isEmpty(self.SelectedTemplate())
            ? { templateId: '', version: '' }
            : JSON.parse(self.SelectedTemplate());

        self.Url.query['templateId'] = selectedTemplate.templateId;
        self.Url.query['templateVersion'] = selectedTemplate.version;
        self.Url.query['status'] = self.SelectedStatus() || "";
        self.Url.query['interviewerId'] = self.SelectedResponsible() || "";

        window.history.pushState({}, "Interviews", self.Url.toString());

        return {
            TemplateId: selectedTemplate.templateId,
            TemplateVersion: selectedTemplate.version,
            ResponsibleId: self.SelectedResponsible,
            Status: self.SelectedStatus
        };
    };

    self.load = function () {
        self.SelectedTemplate("{\"templateId\": \"" + self.QueryString['templateId'] + "\",\"version\": \"" + self.QueryString['templateVersion'] + "\"}");
        self.SelectedStatus(self.QueryString['status']);
        self.SelectedResponsible(self.QueryString['interviewerId']);

        self.Url.query['templateId'] = self.QueryString['templateId'] || "";
        self.Url.query['templateVersion'] = self.QueryString['templateVersion'] || "";
        self.Url.query['status'] = self.QueryString['status'] || "";
        self.Url.query['interviewerId'] = self.QueryString['interviewerId'] || "";

        self.SelectedTemplate.subscribe(self.filter);
        self.SelectedResponsible.subscribe(self.filter);
        self.SelectedStatus.subscribe(self.filter);

        self.search();
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.InterviewsBase, Supervisor.VM.ListView);