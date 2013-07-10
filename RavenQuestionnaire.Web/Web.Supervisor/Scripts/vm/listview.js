ListViewModel = function (url) {
    var self = this;

    self.IsPageLoaded = ko.observable(false);

    self.ServiceUrl = url;

    self.GetFilterMethod = function () {
        return null;
    };

    self.Items = ko.observableArray([]);
    self.ItemsSummary = ko.observable(null);

    self.Filter = function () {
        return self.GetFilterMethod ? self.GetFilterMethod.apply() : null;
    };

    // holds the total item count
    self.TotalCount = ko.observable();

    // actual pager, used to bind to the pager's template
    // first parameter must be an observable or function which returns the current 'total item count'.
    // it is wrapped in a ko.computed inside the pager.
    self.Pager = ko.pager(self.TotalCount);

    self.Pager().PageSize(20);

    // Subscribe to current page changes.
    self.Pager().CurrentPage.subscribe(function () {
        self.search(self.SortOrder);
    });

    self.SortOrder = "";
    self.SortDirection = false;
    self.OrderBy = function () {
        return self.SortOrder == "" ? [] : [{ Direction: self.SortDirection == false ? 0 : 1, Field: self.SortOrder }];
    };

    self.sort = function (so) {
        if ((so || "") != "") {
            if (self.SortOrder == so) {
                self.SortDirection = !self.SortDirection;
            }
            self.SortOrder = so;

            self.search();
        }
    };


    self.search = function () {

        self.onBeforeRequest();

        var params = {
            Pager: {
                Page: self.Pager().CurrentPage(),
                PageSize: self.Pager().PageSize()
            },
            SortOrder: self.OrderBy(),
            Request: self.Filter()
        };

        $.post(self.ServiceUrl, params, null, "json")
            .done(function (data) {
                ko.mapping.fromJS(data, {}, self);
                self.ItemsSummary(data.ItemsSummary);
                self.IsPageLoaded(true);
            });
    };

    self.onBeforeRequest = function () {
    };
};