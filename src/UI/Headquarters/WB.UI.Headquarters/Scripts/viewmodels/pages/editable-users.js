Supervisor.VM.EditableUsers = function (listViewUrl, $archiveUsersUrl, ajax) {
    Supervisor.VM.EditableUsers.superclass.constructor.apply(this, arguments);

    var self = this;

    self.SelectedItems = ko.observableArray([]);

    self.IsNothingSelected = ko.computed(function () {
        return $(self.SelectedItems()).length === 0;
    });

    self.onTableInitCompleteExtra = function() {};

    self.getConfirmAndSendRequest = function (notifier, confirmTitle, confirmMessage, operationFaildTitle, archive) {
        notifier.confirm(confirmTitle,
            confirmMessage,
            // confirm
            function () {

                var request = {
                    userIds: self.SelectedItems(),
                    archive: archive
                };

                ajax.sendRequest($archiveUsersUrl,
                    "post",
                    request,
                    false,
                    // onSuccess
                    function (data) {
                        var failedCommands = ko.utils.arrayFilter(data.CommandStatuses,
                            function (cmd) {
                                return !cmd.IsSuccess;
                            });

                        if (failedCommands.length > 0) {
                            var failedDomainExceptions = ko.utils.arrayMap(failedCommands,
                                function (failedCommand) {
                                    if (!Supervisor.Framework.Objects
                                        .isUndefined(failedCommand.DomainException) &&
                                        failedCommand.DomainException !== null)
                                        return failedCommand.DomainException;
                                    else {
                                        return input.settings.messages.unhandledExceptionMessage;
                                    }
                                });
                            $.each(failedDomainExceptions,
                                function (index, message) {
                                    notifier.showError(operationFaildTitle, message);
                                });
                        }

                        self.reloadDataTable();
                        
                    });
            },
            // cancel
            function () { });
    };

    self.onDataTableDataReceived = function (data) {
        self.UsersCount(data.recordsTotal);
        self.SelectedItems.removeAll();
    };

    self.onTableInitComplete = function () {
        $('.dataTables_filter label')
                    .on('click',
                        function (e) {
                            if (e.target !== this)
                                return;
                            if ($(this).hasClass("active")) {
                                $(this).removeClass("active");
                            } else {
                                $(this).addClass("active");
                                $(this)
                                    .children("input[type='search']")
                                    .delay(200)
                                    .queue(function () {
                                        $(this).focus();
                                        $(this).dequeue();
                                    });
                            }
                        });

        // Handle click on "Select all" control
        $('#check-all')
            .on('click',
                function () {
                    // Get all rows with search applied
                    var rows = self.Datatable.rows({ 'search': 'applied' }).nodes();
                    // Check/uncheck checkboxes for all rows in the table

                    var checkboxes = $('input[type="checkbox"]', rows);
                    var newState = this.checked;

                    _.forEach(checkboxes,
                        function (checkbox) {
                            if (newState) {
                                if (!checkbox.checked) {
                                    self.SelectedItems.push(checkbox.value);
                                }
                            } else {
                                if (checkbox.checked) {
                                    var indexOfSupervisor = self.SelectedItems.indexOf(checkbox.value);
                                    self.SelectedItems.splice(indexOfSupervisor, 1);
                                }
                            }
                        });

                    if (newState === true) {
                        self.Datatable.rows().select();
                    } else {
                        self.Datatable.rows().deselect();
                    }
                    checkboxes.prop('checked', newState);
                });

        // Handle click on checkbox to set state of "Select all" control
        $('#data_holder tbody')
        .on('change',
            'input[type="checkbox"]',
            function () {
                // If checkbox is not checked
                if (!this.checked) {
                    var indexOfSupervisor = self.SelectedItems.indexOf(this.value);
                    self.SelectedItems.splice(indexOfSupervisor, 1);

                    var el = $('#check-all').get(0);
                    // If "Select all" control is checked and has 'indeterminate' property
                    if (el && el.checked && ('indeterminate' in el)) {
                        // Set visual state of "Select all" control
                        // as 'indeterminate'
                        el.indeterminate = true;
                    }
                } else {
                    self.SelectedItems.push(this.value);
                }
            });

        self.onTableInitCompleteExtra();
    };
};

Supervisor.Framework.Classes.inherit(Supervisor.VM.EditableUsers, Supervisor.VM.Users);