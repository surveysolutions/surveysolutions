SummaryListViewModel = function(listViewUrl) {
    var self = this;

    self.ListView = new ListViewModel(listViewUrl);
    self.ToggleFilter = function () {
        self.ListView.ToggleFilter();
    };
    self.Templates = ko.observableArray([]);
    self.SelectedTemplate = ko.observable('');

    self.load = function() {
        self.SelectedTemplate.subscribe(self.ListView.filter);

        self.ListView.GetFilterMethod = function() {
            return { TemplateId: self.SelectedTemplate };
        };
        self.ListView.search();
    };
};