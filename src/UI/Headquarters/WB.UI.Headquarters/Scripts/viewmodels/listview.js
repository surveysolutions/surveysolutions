Supervisor.VM.ListView = function (serviceUrl, commandExecutionUrl, useGetRequests) {
    Supervisor.VM.ListView.superclass.constructor.apply(this, [commandExecutionUrl]);
    
    var self = this;

    self.useGetRequests = useGetRequests === true;
    
    self.ServiceUrl = serviceUrl;

    self.Items = ko.observableArray([]);
    self.SearchBy = ko.observable('');

    self.GetFilterMethod = function () {
        return null;
    };

    self.Filter = function () {
        return self.GetFilterMethod ? self.GetFilterMethod.apply() : undefined;
    };

    // holds the total item count
    self.TotalCount = ko.observable();

    // actual pager, used to bind to the pager's template
    // first parameter must be an observable or function which returns the current 'total item count'.
    // it is wrapped in a ko.computed inside the pager.
    self.Pager = ko.pager(self.TotalCount);

    self.Pager().PageSize(20);

    self.isNeedFireEventOnChangeCurrentPage = true;

    // Subscribe to current page changes.
    self.Pager().CurrentPage.subscribe(function () {
        if (self.isNeedFireEventOnChangeCurrentPage)
            self.search(self.SortOrder);
    });
    self.SetCurrentPageWithoutRunSubscribers = function(value) {
        self.isNeedFireEventOnChangeCurrentPage = false;
        self.Pager().CurrentPage(value);
        self.isNeedFireEventOnChangeCurrentPage = true;
    };

    self.Pager().CanChangeCurrentPage = ko.computed(function() { return self.IsAjaxComplete(); });

    self.SortOrder = ko.observable("");
    self.SortDirection = ko.observable(false);
    self.OrderBy = function () {
        return self.SortOrder() === "" ? [] : [{ Direction: self.SortDirection() == false ? 0 : 1, Field: self.SortOrder() }];
    };

    self.sort = function (so) {
        
        if (!self.IsAjaxComplete()) {
            self.CheckForRequestComplete();
            return;
        }
        
        if ((so || "") !== "") {
            if (self.SortOrder() === so) {
                self.SortDirection(!self.SortDirection());
            } else {
                self.SortDirection(false);
            }
            self.SortOrder(so);

            self.search();
        } 
    };

    self.filter = function (onSuccess, onDone) {
        self.SearchBy(self.SearchBy().trim());

        if (self.Pager().CurrentPage() !== 1) 
            self.SetCurrentPageWithoutRunSubscribers(1);

        if (jQuery.isFunction(onSuccess) && jQuery.isFunction(onDone)) {
            self.search(onSuccess, onDone);
        } else {
            self.search();
        }
    };

    
    self.mappingOptions = {};

    self.search = function(onSuccess, onDone) {
        var filter = {
            PageIndex: self.Pager().CurrentPage(),
            PageSize: self.Pager().PageSize(),
            SortOrder: self.OrderBy(),
            SearchBy: self.SearchBy()
        };

        var request = self.Filter() || {};
        
        $.extend(request, filter);

        self.SendRequest(self.ServiceUrl, request, function (data) {
            self.setExportUrls();
            ko.mapping.fromJS(data, self.mappingOptions, self);

            if (self.Items().length > 0)
                self.datatableColumnLength(Object.keys(self.Items()[0]).length);

            if (!_.isUndefined(onSuccess) && !_.isNull(onSuccess) && _.isFunction(onSuccess)) {
                onSuccess();
            }

            $('.table').unhighlight();

            if (self.Items().length > 0) {
                $('.table').highlight(self.SearchBy());
            }

        }, true, self.useGetRequests, onDone);
    };
    self.clear = function() {
        self.SearchBy("");

        if (self.Pager().CurrentPage() !== 1) 
            self.SetCurrentPageWithoutRunSubscribers(1);

        self.search();
    };

    self.SelectedItems = ko.computed(function () {
        return ko.utils.arrayFilter(self.Items(), function (item) {
            var value = item.IsSelected();
            return !isNaN(value) && value;
        });
    });

    self.GetSelectedItemsAfterFilter = function (selectedRowAsArray, filterFunc) {
        var allItems = _.isArray(selectedRowAsArray) ? selectedRowAsArray : self.SelectedItems();
        var filteredItems = ko.utils.arrayFilter(allItems, filterFunc);
        return filteredItems;
    }

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

    self.selectAll = function (checkbox) {
        var isCheckboxSelected = $(checkbox).is(":checked");
        ko.utils.arrayForEach(self.Items(), function (item) {
            item.IsSelected(isCheckboxSelected);
        });
    };

    self.unselectAll = function () {
        ko.utils.arrayForEach(self.Items(), function (item) {
            item.IsSelected(false);
        });
    };

    self.ExportToExcelUrl = ko.observable("");
    self.ExportToCsvUrl = ko.observable("");
    self.ExportToTabUrl = ko.observable("");

    self.setExportUrls = function()
    {
        var request = self.Filter() || {};

        if ((self.Datatable || null) !=null)
            $.extend(request, self.Datatable.ajax.params());

        var requestUrl = self.ServiceUrl + "?" + decodeURIComponent($.param(request));

        self.ExportToExcelUrl(requestUrl + "&exportType=excel");
        self.ExportToCsvUrl(requestUrl + "&exportType=csv");
        self.ExportToTabUrl(requestUrl + "&exportType=tab");
    };

    self.datatableColumnLength = ko.observable(0);
    
    self.initDataTable = function (onDataReceivedCallback, onTableInitComplete) {
        $.fn.dataTable.ext.errMode = 'none';

        var tableColumns = self.getDataTableColumns();

        self.datatableColumnLength(tableColumns.length);

        var tableColumnDefs = [];
        for (var columnIndex = 0; columnIndex < tableColumns.length; columnIndex ++) {
            tableColumnDefs.push({
                "targets": columnIndex,
                "createdCell": function (td, cellData, rowData, row, col) {
                    var $td = $(td);
                    var $th = $("table thead tr th").eq($td.index());
                    $td.attr("data-th", $th.text());
                }
            });
        }

        self.Datatable = $('table#data_holder').DataTable(
        {
            language:
            {
                "url": window.input.settings.config.dataTableTranslationsUrl
                /*searchPlaceholder: "@Pages.Search"*/
            },
            ajax: function(data, callback, settings) {
                var request = self.Filter() || {};

                $.extend(request, data);
                _.map(data.columns, function (column) {
                    delete (column.orderable);
                    delete (column.search);
                    delete (column.searchable);
                });

                self.SendRequest(serviceUrl, request, function (d) {
                    self.setExportUrls();

                    if (!_.isUndefined(onDataReceivedCallback))
                        onDataReceivedCallback(d);
                    callback(d);
                }, true, self.useGetRequests);
            },
            columns: tableColumns,
            columnDefs: tableColumnDefs,
            pageLength: 20,
            pagingType: "full_numbers",
            lengthChange: false,
            conditionalPaging: true,
            processing: true,
            serverSide: true,
            deferRender: true,
            searchHighlight: true,
            footerCallback: self.footerCallback,
            "createdRow": function(row, data, dataIndex) {
                $(row).addClass('with-context-menu');
                if (data.isDisabled) {
                    $(row).addClass('disabled');
                }
            },
            order: self.defaultOrder,
            search: {
                caseInsensitive: true
            },
            initComplete: function (settings, json) {
                self.initSearchControl();

                if (!_.isUndefined(onTableInitComplete))
                    onTableInitComplete();

                // Replace throttling with debounce https://github.com/DataTables/DataTables/issues/809#issuecomment-293918587
                var $input = $(this).find("input[type='search']");
                var searchDelay = this.api().settings()[0].searchDelay;

                $input.off().on('keyup cut paste',
                    _.debounce(function() { api.search($input.val()).draw(); },
                    searchDelay));

                
            }
        });
    };

    self.reloadDataTable = function() {
        self.Datatable.ajax.reload();
    };

    self.getDataTableColumns = function () { return []; }

    self.initSearchControl = function() {
        var clearSearchButton = $('<button type="button" class="btn btn-link btn-clear"><span></span></button>');
        var searchInput = $(self.Datatable.table().container()).find('.dataTables_filter label input');

        searchInput.after(clearSearchButton);

        clearSearchButton.on('click', function() {
            searchInput.val('');
            self.Datatable.search('').draw();
        });

        searchInput
            .off()
            .on('keyup', function(e) {
                if((e.ctrlKey && e.keyCode === 86)||(e.key === "Enter")){
                    searchInput.val(searchInput.val().trim());
                }
                self.Datatable.search(searchInput.val().trim()).draw();
            });
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ListView, Supervisor.VM.BasePage);
