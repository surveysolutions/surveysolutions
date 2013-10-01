SummaryListViewModel = function (listViewUrl) {
    var self = this;
    self.ListView = new ListViewModel(listViewUrl);
    self.HideOutput = function () {
        self.ListView.HideOutput();
    };

    self.ShowOutput = function () {
        self.ListView.ShowOutput();
    };

    self.Errors = ko.computed(function () {
        return self.ListView.Errors();
    });

    self.Url = new Url(window.location.href);
    
    self.ToggleFilter = function () {
        self.ListView.ToggleFilter();
    };
    self.Templates = ko.observableArray([]);
    self.SelectedTemplate = ko.observable('');

    self.load = function () {
        
        self.SelectedTemplate("{\"templateId\": \"" + location.queryString['templateId'] + "\",\"version\": \"" + location.queryString['templateVersion'] + "\"}");
        
        self.SelectedTemplate.subscribe(self.ListView.filter);

        self.ListView.GetFilterMethod = function () {
            var selectedTemplate = _.isEmpty(self.SelectedTemplate())
                 ? { templateId: '', version: '' }
                 : JSON.parse(self.SelectedTemplate());
            
            self.Url.query['templateId'] = selectedTemplate.templateId;
            self.Url.query['templateVersion'] = selectedTemplate.version;
            
            window.history.pushState({}, "Summary", self.Url.toString());
            
            return {
                TemplateId: selectedTemplate.templateId,
                TemplateVersion: selectedTemplate.version
            };
        };
        
        

        self.ListView.search();
    };
};