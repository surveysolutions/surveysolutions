SurveyListViewModel = function (listViewUrl) {
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
    self.Users = ko.observableArray([]);

    self.SelectedUser = ko.observable('');

    self.load = function () {
        
        self.ListView.GetFilterMethod = function () {
            
            self.Url.query['interviewerId'] = self.SelectedUser() || "";

            if (Modernizr.history) {
                window.history.pushState({}, "interviewerId", self.Url.toString());
            }
            return { UserId: self.SelectedUser };
        };

        self.SelectedUser(location.queryString['interviewerId']);
        
        self.SelectedUser.subscribe(self.ListView.filter);
        
        self.Url.query['interviewerId'] = location.queryString['interviewerId'] || "";
        
        self.ListView.search();
    };
};