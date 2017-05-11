Supervisor.VM.Supervisors = function (listViewUrl, archiveUsersUrl, ajax, notifier) {
    Supervisor.VM.Supervisors.superclass.constructor.apply(this, arguments);

    var self = this;
    
    self.SelectedItems = ko.observableArray([]);
    self.SupervisorsCount = ko.observable(0);

    self.getConfirmAndSendRequest = function (confirmMessage, archive) {
        notifier.confirm("@Pages.ConfirmationNeededTitle",
            confirmMessage,
            // confirm
            function () {

                var request = {
                    userIds: self.SelectedItems(),
                    archive: archive
                };

                ajax.sendRequest(archiveUsersUrl, "post", request, false,
                    // onSuccess
                    function (data) {
                        var failedCommands = ko.utils.arrayFilter(data.CommandStatuses, function (cmd) {
                            return !cmd.IsSuccess;
                        });

                        if (failedCommands.length > 0) {
                            var failedDomainExceptions = ko.utils.arrayMap(failedCommands, function (failedCommand) {
                                if (!Supervisor.Framework.Objects.isUndefined(failedCommand.DomainException) && failedCommand.DomainException !== null)
                                    return failedCommand.DomainException;
                                else {
                                    return input.settings.messages.unhandledExceptionMessage;
                                }
                            });
                            $.each(failedDomainExceptions, function (index, message) {
                                notifier.showError("@Pages.OperationFailedTitle", message);
                            });
                        }

                        self.updater();
                    });
            },
            // cancel
            function () { });
    }


    self.unarchiveSupervisors = function () {
        self.getConfirmAndSendRequest("@Archived.UnarchiveSupervisorWarning \r\n @Pages.Supervisors_UnarchiveSupervisorsConfirm", false);
    };

    self.archiveSupervisors = function () {
        self.getConfirmAndSendRequest("@Pages.Supervisors_ArchiveSupervisorsConfirmMessage", true);
    }
    
    self.load = function () {
        self.SupervisorsCount.subscribe(reloadDataTable);
    };

    self.IsNothingSelected = ko.computed(function () {
        return $(self.SelectedItems()).length === 0;
    });





    self.table = null;

    var supervisorsListUrl = '@Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "UsersApi", action = "AllSupervisors" })';
    var $impersonateUrl = '@Url.Action("ObservePerson", "Account", new { area = string.Empty })';
    var $archiveUsersUrl = '@Url.RouteUrl("DefaultApiWithAction", new {httproute = "", controller = "UsersApi", action = "ArchiveUsers"})';

    var model = new Supervisor.VM.Supervisors(
        supervisorsListUrl,
        $archiveUsersUrl,
        ajax,
        notifier);

    ko.applyBindings(model);

    var requestHeaders = {};
    requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;

    $.fn.dataTable.ext.errMode = 'none';

    var selectRowAndGetData = function (selectedItem) {
        var rowIndex = selectedItem.parent().children().index(selectedItem);
        self.table.row(rowIndex).select();
        var selectedRows = self.table.rows({ selected: true }).data()[0];
        return selectedRows;
    }
            @if (authorizedUser.IsObserver)
    {
        <text>

    var buildMenuItem = function (selectedRow) {
        var items = {}

        var impersonateAsUser = function (key, opt) {
            var selectedRow = selectRowAndGetData(opt.$trigger);
            window.location.href = $impersonateUrl + '?personName=' + selectedRow.userName;
        };
        items["impersonate"] =
        {
            name: "@Users.ImpersonateAsUser",
            callback: impersonateAsUser
        };

        return items;
    }

        self.contextMenuOptions = {
            selector: "table#supervisors .with-context-menu",
            autoHide: false,
            build: function ($trigger, e) {
                var selectedRow = selectRowAndGetData($trigger);
                var items = buildMenuItem(selectedRow);
                return { items: items };
            },
            trigger: 'left'
        };
        </text>
    }
    var onTableInitComplete = function () {
        $('.dataTables_filter label').on('click', function (e) {
            if (e.target !== this)
                return;
            if ($(this).hasClass("active")) {
                self.table.search( '' ).draw();
                $(this).removeClass("active");
            } else {
                $(this).addClass("active");
                $(this).children("input[type='search']").delay(200).queue(function () { $(this).focus(); $(this).dequeue(); });
            }
        });
        @if (authorizedUser.IsObserver)
        {
            <text>
            $.contextMenu(self.contextMenuOptions);
            </text>
        }
    };

    self.table = $('table#supervisors')
        .on('init.dt', onTableInitComplete)
        .on('xhr.dt', function (e, settings, json, xhr) {
            if (xhr.status === 401)
                location.reload();
            else if (xhr.status != undefined && xhr.status !== 200)
                notifier.showError("@Pages.OperationFailedTitle", "@Pages.OperationFailedDescription");
        })
        .DataTable({
            processing: true,
            language:
            {
                "url": window.input.settings.config.dataTableTranslationsUrl,
                searchPlaceholder: "@Pages.Search"
            },
            serverSide: true,
            ajax: {
                url: supervisorsListUrl,
                type: "POST",
                headers: requestHeaders,
                data: function(d) {
                    model.SelectedItems.removeAll(); // move to before filter event
                },
                dataSrc: function (json) {
                    model.SupervisorsCount(json.recordsTotal);
                    return json.data;
                }
            },
            "createdRow": function (row, data, dataIndex) {
                $(row).addClass('with-context-menu');
            },
            columns: [
                @if (isAdmin)
            {
                    <text>
                        {
                            "orderable": false,
                            render: function(data, type, row) {
                                return "<input class=\"checkbox-filter\" id=\"cbx_" + row.userId + "\" value=\"" + row.userId +
                                    "\" type=\"checkbox\"> <label for=\"cbx_" + row.userId + "\"><span class=\"tick\"></span></label> ";
                            },
                            "class": "checkbox-cell"
                        },
                    </text>
                        }
                        {
                            data: "userName",
                            name: "UserName", // case-sensitive!
                            render: function (data, type, row) {
                                return !row.isArchived
                                    ? "<a href='@Url.Action("Edit", "Supervisor")/" + row.userId + "'>" + data + "</a>"
                                    : data;
                            }
                        },
        {
            data: "creationDate",
            name: "CreationDate", // case-sensitive! should be DB name here from Designer DB questionnairelistviewitems? to sort column
            "class": "date"
        },
                {
                    data: "email",
                    name: "Email", // case-sensitive! should be DB name here from Designer DB questionnairelistviewitems? to sort column
                    render: function (data, type, row) {
                        return data ? "<a href='mailto:" + data + "'>" + data + "</a>" : "";
                    },
                    "class": "date"
                },
                {
                    data: "isArchived",
                    name: "IsArchived", // case-sensitive!
                    render: function (data, type, row) {
                        return data ? "@Common.Yes" : "@Common.No";
                    }
                }
    ],
    select: {
        style: 'multi',
        selector: 'td:first-child input[type="checkbox"]'
    },
    searchHighlight: true,
    rowId: 'userId',
    pagingType: "full_numbers",
    lengthChange: false, // do not show page size selector
    pageLength: 50, // page size
    order: [[1, 'asc']],
    conditionalPaging: true
});

var updater = function () {
    self.table.ajax.reload();
}

model.load(updater);

// Handle click on "Select all" control
$('#check-all')
    .on('click',
        function () {
            // Get all rows with search applied
            var rows = self.table.rows({ 'search': 'applied' }).nodes();
            // Check/uncheck checkboxes for all rows in the table

            var checkboxes = $('input[type="checkbox"]', rows);
            var newState = this.checked;

            _.forEach(checkboxes,
                function (checkbox) {
                    if (newState) {
                        if (!checkbox.checked) {
                            model.SelectedItems.push(checkbox.value);
                        }
                    } else {
                        if (checkbox.checked) {
                            var indexOfSupervisor = model.SelectedItems.indexOf(checkbox.value);
                            model.SelectedItems.splice(indexOfSupervisor, 1);
                        }
                    }
                });

            if (newState == true) {
                self.table.rows().select();
            } else {
                self.table.rows().deselect();
            }
            checkboxes.prop('checked', newState);
        });

// Handle click on checkbox to set state of "Select all" control
$('#supervisors tbody')
    .on('change',
        'input[type="checkbox"]',
        function () {
            // If checkbox is not checked
            if (!this.checked) {
                var indexOfSupervisor = model.SelectedItems.indexOf(this.value);
                model.SelectedItems.splice(indexOfSupervisor, 1);

                var el = $('#check-all').get(0);
                // If "Select all" control is checked and has 'indeterminate' property
                if (el && el.checked && ('indeterminate' in el)) {
                    // Set visual state of "Select all" control
                    // as 'indeterminate'
                    el.indeterminate = true;
                }
            } else {
                model.SelectedItems.push(this.value);
            }
        });
return self;








};

Supervisor.Framework.Classes.inherit(Supervisor.VM.Supervisors, Supervisor.VM.ListView);