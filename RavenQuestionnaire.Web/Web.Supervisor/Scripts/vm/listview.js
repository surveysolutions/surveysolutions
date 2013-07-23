ListViewModel = function (url) {
    var self = this;

    self.IsPageLoaded = ko.observable(false);
    
    self.IsAjaxComplete = ko.observable(true);

    self.ServiceUrl = url;

    self.GetFilterMethod = function () {
        return null;
    };
    self.IsFilterOpen = ko.observable(true);
    self.ToggleFilter = function () {
        if (self.IsFilterOpen()) {
            $('#wrapper').addClass('menu-hidden');
        } else {
            $('#wrapper').removeClass('menu-hidden');
        }
        self.IsFilterOpen(!self.IsFilterOpen());
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

    self.IsAjaxComplete.subscribe(function (isLoaded) {
        if (isLoaded) {
            $('#umbrella').hide();
        }
        if (isLoaded == false) {
            $('#umbrella').show();
        }
    });

    self.SortOrder = ko.observable("");
    self.SortDirection = ko.observable(false);
    self.OrderBy = function () {
        return self.SortOrder() == "" ? [] : [{ Direction: self.SortDirection() == false ? 0 : 1, Field: self.SortOrder() }];
    };

    self.sort = function (so) {
        if ((so || "") != "") {
            if (self.SortOrder() == so) {
                self.SortDirection(!self.SortDirection());
            } else {
                self.SortDirection(false);
            }
            self.SortOrder(so);

            self.search();
        }
    };

    self.filter = function () {
        if (self.Pager().CurrentPage() == 1) {
            self.search();
        } else {
            self.Pager().CurrentPage(1);
        }
    };

    self.mappingOptions = {};

    self.search = function () {

        self.IsAjaxComplete(false);
        
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
                ko.mapping.fromJS(data, self.mappingOptions, self);
                self.ItemsSummary(data.ItemsSummary);
                self.IsPageLoaded(true);
                self.IsAjaxComplete(true);
            });
    };

    self.onBeforeRequest = function () {
    };

    var setMinHeight = function () {
        var windowHeight = $(window).height();
        var navigationHeight = $('.navbar.navbar-fixed-top').height();
        $('#content').css('min-height', (windowHeight - navigationHeight) + 'px');
        $('#wrapper').css('margin-top', navigationHeight + 'px');
        $('#umbrella').css('top', navigationHeight + 'px');

    };

    $(document).ready(function () {
        setMinHeight();
        $(window).resize(function () {
            setMinHeight();
        });
    });
};

ko.bindingHandlers.sortby = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var listView = bindingContext.$root.ListView;
        
        var value = valueAccessor();

        var sort = "sorting";
        var sortAsc = "sorting-up";
        var sortDesc = "sorting-down";

        var refreshUI = function () {

            $(element).addClass(sort);
            
            $(element).removeClass(sortAsc);
            $(element).removeClass(sortDesc);

            if (listView.SortOrder() == value) {
                if (listView.SortDirection()) {
                    $(element).addClass(sortAsc);
                } else {
                    $(element).addClass(sortDesc);
                }
            }
        };

        listView.SortOrder.subscribe(refreshUI);
        listView.SortDirection.subscribe(refreshUI);

        $(element).click(function () {
            listView.sort(value);
        });

        refreshUI();
    }
};

Array.prototype.joinArrayOfObjects = function (key, value) {
    var ret = '';
    this.forEach(function (e) { ret = ret.concat(e[key](), ':', e[value](), ', '); });
    return ret.substring(0, ret.length - 2);
}