SurveyListViewModel = function (listViewUrl) {
    var self = this;

    self.ListView = new ListViewModel(listViewUrl);
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