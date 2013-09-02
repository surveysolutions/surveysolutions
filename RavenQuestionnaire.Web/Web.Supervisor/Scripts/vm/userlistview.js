UserListViewModel = function (listViewUrl) {
    var self = this;

    self.ListView = new ListViewModel(listViewUrl);
    self.ToggleFilter = function () {
        self.ListView.ToggleFilter();
    };
    
    self.load = function() {
        self.ListView.search();
    };
};