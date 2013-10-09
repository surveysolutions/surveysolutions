StatusListViewModel = function (listViewUrl) {
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
    self.Statuses = ko.observableArray([]);
    self.SelectedStatus = ko.observable('');

    self.load = function () {
        
        self.SelectedStatus.subscribe(self.ListView.filter);
     
        self.ListView.GetFilterMethod = function () {
            
            self.Url.query['status'] = self.SelectedStatus() || "";

            if (Modernizr.history) {
                window.history.pushState({}, "Status", self.Url.toString());
            }
            return { StatusId: self.SelectedStatus };
        };
        
        self.Url.query['status'] = location.queryString['status'] || "";

        self.ListView.search();
    };
};