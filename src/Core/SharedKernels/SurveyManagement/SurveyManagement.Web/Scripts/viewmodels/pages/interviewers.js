Supervisor.VM.Interviewers = function (listViewUrl, interviewersPageUrl, supervisorsUrl, commandExecutionUrl) {
    Supervisor.VM.Interviewers.superclass.constructor.apply(this, arguments);

    var archiveUserCommad = "ArchiveUserCommad";
    var unArchiveUserCommad = "UnarchiveUserCommand";
    var self = this;

    self.Url = new Url(interviewersPageUrl);

    self.SupervisorUrl = supervisorsUrl;
    self.IsSupervisorsLoading = ko.observable(false);
    self.Supervisors = function (query, sync, pageSize) {
        self.IsSupervisorsLoading(true);
        self.SendRequest(self.SupervisorUrl, { query: query, pageSize: pageSize }, function (response) {
            sync(response.Users, response.TotalCountByQuery);
        }, true, true, function () {
            self.IsSupervisorsLoading(false);
        });
    }
    self.SelectedSupervisor = ko.observable();

    self.ShowOnlyArchived = ko.observable();
    self.ShowOnlyNotConnectedToDevice = ko.observable('');
    self.SearchBy = ko.observable('');

    self.load = function() {
        self.search();
    };

    self.archiveUser = function (userViewItem) {
        self.sendUserCommands([userViewItem], archiveUserCommad);
    };

    self.archiveSupervisor = function (userViewItem) {
        self.AskConfirmationAndRunActionIfTrue(function (filteredItems) {
            self.SendCommand({ supervisorId: userViewItem.UserId() }, function () {
                setTimeout(function() { self.search(); }, 100);
            });
        }, [userViewItem]);
    };

    self.unarchiveUser = function (userViewItem) {
        self.sendUserCommands([userViewItem], unArchiveUserCommad);
    };

    self.unarchiveInterviewers = function () {
        self.sendUserCommands(self.GetSelectedItemsAfterFilter(function (item) { return true; }), unArchiveUserCommad);
    };

    self.archiveInterviewers = function () {
        self.sendUserCommands(self.GetSelectedItemsAfterFilter(function (item) { return true; }), archiveUserCommad);
    };

    self.sendUserCommands = function(filteredItems, commandName) {
        self.AskConfirmationAndRunActionIfTrue(function(filteredItems) {
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
        }, filteredItems);
    };

    self.AskConfirmationAndRunActionIfTrue = function (action, filteredItems) {
        var messageHtml = self.getBindedHtmlTemplate("#confirm-delete-template", filteredItems);

        if (filteredItems.length === 0) {
            bootbox.alert(messageHtml);
            return;
        }

        messageHtml += $("#confirm-continue-message-template").html();

        bootbox.confirm(messageHtml, function (result) {
            if (result) {
                action(filteredItems);
            }
        });
    };

    self.GetFilterMethod = function () {

        var supervisorId = _.isUndefined(self.SelectedSupervisor()) ? null : self.SelectedSupervisor().UserId

        //self.Url.query['supervisorId'] = supervisorId;
        //self.Url.query['showOnlyArchived'] = self.ShowOnlyArchived() || "";
        //self.Url.query['showOnlyNotConnectedToDevice'] = self.ShowOnlyNotConnectedToDevice() || "";
        //self.Url.query['searchBy'] = self.SearchBy() || "";

        //if (Modernizr.history) {
        //    window.history.pushState({}, "Interviews", self.Url.toString());
        //}

        return {
            SupervisorId: supervisorId,
            ShowOnlyArchived: self.ShowOnlyArchived,
            ShowOnlyNotConnectedToDevice: self.ShowOnlyNotConnectedToDevice,
            SearchBy: self.SearchBy
        };
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.Interviewers, Supervisor.VM.ListView);