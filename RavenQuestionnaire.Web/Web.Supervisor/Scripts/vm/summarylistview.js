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
            var selectedTemplate = _.isEmpty(self.SelectedTemplate())
                 ? { templateId: '', version: '' }
                 : JSON.parse(self.SelectedTemplate());
            return {
                TemplateId: selectedTemplate.templateId,
                TemplateVersion: selectedTemplate.version
            };
        };
        self.ListView.search();
    };
};