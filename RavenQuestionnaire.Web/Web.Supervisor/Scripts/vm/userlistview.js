UserListViewModel = function (listViewUrl) {
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
    
    self.load = function() {
        self.ListView.search();
    };
};