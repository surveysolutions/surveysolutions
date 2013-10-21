﻿ko.bindingHandlers.sortby = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var listView = bindingContext.$root;

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

Supervisor.VM.ListView = function (serviceUrl, commandExecutionUrl) {
    Supervisor.VM.ListView.superclass.constructor.apply(this, [commandExecutionUrl]);
    
    var self = this;
    
    self.ServiceUrl = serviceUrl;

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
    self.Pager().CurrentPage.subscribe(function() {
        self.search(self.SortOrder);
    });

    self.Pager().CanChangeCurrentPage = ko.computed(function() { return self.IsAjaxComplete(); });

    self.SortOrder = ko.observable("");
    self.SortDirection = ko.observable(false);
    self.OrderBy = function () {
        return self.SortOrder() == "" ? [] : [{ Direction: self.SortDirection() == false ? 0 : 1, Field: self.SortOrder() }];
    };

    self.sort = function (so) {
        
        if (!self.IsAjaxComplete()) {
            self.CheckForRequestComplete();
            return;
        }
        
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

    self.search = function() {
        var params = {
            Pager: {
                Page: self.Pager().CurrentPage(),
                PageSize: self.Pager().PageSize()
            },
            SortOrder: self.OrderBy(),
            Request: self.Filter()
        };

        self.SendRequest(self.ServiceUrl, params, function(data) {
            ko.mapping.fromJS(data, self.mappingOptions, self);
            self.ItemsSummary(data.ItemsSummary);
        });
    };
    
    self.SelectedItems = ko.computed(function () {
        return ko.utils.arrayFilter(self.Items(), function (item) {
            var value = item.IsSelected();
            return !isNaN(value) && value;
        });
    });

    self.IsNothingSelected = ko.computed(function () {
        return $(self.SelectedItems()).length == 0;
    });

    self.IsOnlyOneSelected = ko.computed(function () {
        return $(self.SelectedItems()).length == 1;
    });
    
    var myChildModel = function (data) {
        ko.mapping.fromJS(data, {}, this);

        this.IsSelected = ko.observable(false);
    };
    
    self.mappingOptions = {
        'Items': {
            create: function (item) {
                return new myChildModel(item.data);
            }
        }
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ListView, Supervisor.VM.BasePage);