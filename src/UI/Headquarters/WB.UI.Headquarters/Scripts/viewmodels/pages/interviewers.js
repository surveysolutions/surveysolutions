Supervisor.VM.Interviewers = function (listViewUrl, commandExecutionUrl, interviewersPageUrl, supervisorsUrl) {
    Supervisor.VM.Interviewers.superclass.constructor.apply(this, arguments);

    var archiveUserCommad = "ArchiveUserCommad";
    var unArchiveUserCommad = "UnarchiveUserCommand";
    var self = this;

    self.Url = new Url(interviewersPageUrl);

    self.SupervisorUrl = supervisorsUrl;
    self.IsSupervisorsLoading = ko.observable(false);
    self.Supervisors = function (query, sync, pageSize) {
        self.IsSupervisorsLoading(true);
        self.SendRequest(self.SupervisorUrl, { query: query, pageSize: pageSize, showLocked: true }, function (response) {
            sync(response.Users, response.TotalCountByQuery);
        }, true, true, function () {
            self.IsSupervisorsLoading(false);
        });
    }
    self.SelectedSupervisor = ko.observable();

    self.Archived = ko.observable(false);
    self.ConnectedToDevice = ko.observable('');
    self.SearchBy = ko.observable('');

    self.archiveUser = function (userViewItem) {
        self.sendUserCommands("#confirm-archive-template", "#confirm-archive-continue-message-template", [userViewItem], archiveUserCommad);
    };

    self.unarchiveUser = function (userViewItem) {
        self.sendUserCommands("#confirm-unarchive-template", "#confirm-unarchive-continue-message-template", [userViewItem], unArchiveUserCommad);
    };

    self.unarchiveInterviewers = function () {
        self.sendUserCommands("#confirm-unarchive-template", "#confirm-unarchive-continue-message-template", self.GetSelectedItemsAfterFilter(function (item) { return true; }), unArchiveUserCommad);
    };

    self.archiveInterviewers = function () {
        self.sendUserCommands("#confirm-archive-template", "#confirm-archive-continue-message-template", self.GetSelectedItemsAfterFilter(function (item) { return true; }), archiveUserCommad);
    };

    self.sendUserCommands = function (confirm_template, confirm_continue_template, filteredItems, commandName) {
        self.AskConfirmationAndRunActionIfTrue(confirm_template, confirm_continue_template,
            function (filteredItems) {
                var commands = ko.utils.arrayMap(filteredItems, function (rawItem) {
                    return ko.toJSON({ userId: rawItem.UserId() });
                });

                var command = {
                    type: commandName,
                    commands: commands
                };
                self.SendCommands(command, function () {
                    setTimeout(function () { self.search(); }, 100);
                });
            },
            filteredItems);
    };

    self.AskConfirmationAndRunActionIfTrue = function (confirm_template, confirm_continue_template, action, filteredItems) {
        var messageHtml = self.getBindedHtmlTemplate(confirm_template, filteredItems);

        if (filteredItems.length === 0) {
            bootbox.alert(messageHtml);
            return;
        }

        messageHtml += $(confirm_continue_template).html();

        bootbox.confirm(messageHtml, function (result) {
            if (result) {
                action(filteredItems);
            }
        });
    };

    self.GetFilterMethod = function () {

        var supervisorName = _.isUndefined(self.SelectedSupervisor()) ? null : self.SelectedSupervisor().UserName

        self.Url.query['supervisor'] = supervisorName || "";
        self.Url.query['archived'] = self.Archived() || "";
        self.Url.query['connectedToDevice'] = self.ConnectedToDevice() || "";
        self.Url.query['searchBy'] = self.SearchBy() || "";

        if (Modernizr.history) {
            window.history.pushState({}, "Interviews", self.Url.toString());
        }

        return {
            SupervisorName: supervisorName,
            Archived: self.Archived,
            ConnectedToDevice: self.ConnectedToDevice,
            SearchBy: self.SearchBy
        };
    };

    self.load = function () {

        if (self.QueryString['supervisor'] != null) {
            self.SelectedSupervisor({ UserName: self.QueryString['supervisor'] });
        }

        self.SearchBy(decodeURIComponent(self.QueryString['searchBy'] || ""));
        self.Archived(self.QueryString['archived']);
        self.ConnectedToDevice(self.QueryString['connectedToDevice']);

        self.Url.query['supervisor'] = self.QueryString['supervisor'] || "";
        self.Url.query['archived'] = self.QueryString['archived'] || "";
        self.Url.query['connectedToDevice'] = self.QueryString['connectedToDevice'] || "";
        self.Url.query['searchBy'] = self.QueryString['searchBy'] || "";

        self.SelectedSupervisor.subscribe(self.filter);
        self.Archived.subscribe(self.filter);
        self.ConnectedToDevice.subscribe(self.filter);

        self.search();
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.Interviewers, Supervisor.VM.ListView);