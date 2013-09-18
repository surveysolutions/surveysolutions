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


    self.ToggleFilter = function () {
        self.ListView.ToggleFilter();
    };
    self.Statuses = ko.observableArray([]);
    self.SelectedStatus = ko.observable('');

    self.load = function () {
        self.SelectedStatus.subscribe(self.ListView.filter);

        self.ListView.GetFilterMethod = function () {
            return { StatusId: self.SelectedStatus };
        };
        self.ListView.search();
    };
};