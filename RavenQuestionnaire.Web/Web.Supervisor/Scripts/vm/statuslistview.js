StatusListViewModel = function (listViewUrl) {
    var self = this;

    self.ListView = new ListViewModel(listViewUrl);

    self.Statuses = ko.observableArray([]);
    self.SelectedStatus = ko.observable('');

    self.load = function() {
        self.SelectedStatus.subscribe(self.ListView.filter);

        self.ListView.GetFilterMethod = function() {
            return { StatusId: self.SelectedStatus };
        };
        self.ListView.search();
    };
};