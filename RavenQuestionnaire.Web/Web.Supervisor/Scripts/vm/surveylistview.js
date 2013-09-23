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


    self.ToggleFilter = function () {
        self.ListView.ToggleFilter();
    };
    self.Users = ko.observableArray([]);

    self.SelectedUser = ko.observable('');

    self.load = function () {

        self.ListView.GetFilterMethod = function () {
            return { UserId: self.SelectedUser };
        };

        self.SelectedUser(location.queryString['interviewerid']);
        self.SelectedUser.subscribe(self.ListView.filter);
        self.ListView.search();
    };
};