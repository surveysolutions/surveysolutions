DocumentListViewModel = function (listViewUrl) {
    var self = this;

    self.ListView = new ListViewModel(listViewUrl);
        
    self.Templates = ko.observableArray([]);
    self.Responsibles = ko.observableArray([]);
    self.Statuses = ko.observableArray([]);

    self.SelectedTemplate = ko.observable('');
    self.SelectedResponsible = ko.observable('');
    self.SelectedStatus = ko.observable('');
    self.OnlyAssigned = ko.observable(false);

    self.load = function () {
        self.ListView.GetFilterMethod = function () {
            return {
                TemplateId: self.SelectedTemplate,
                ResponsibleId: self.SelectedResponsible,
                StatusId: self.SelectedStatus,
                OnlyAssigned: self.OnlyAssigned
            };
        };
        
        self.SelectedTemplate(location.queryString['templateid']);
        self.SelectedStatus(location.queryString['status']);

        self.SelectedTemplate.subscribe(self.ListView.filter);
        self.SelectedResponsible.subscribe(self.ListView.filter);
        self.SelectedStatus.subscribe(self.ListView.filter);
        self.OnlyAssigned.subscribe(self.ListView.filter);

        self.ListView.search();
    };
};