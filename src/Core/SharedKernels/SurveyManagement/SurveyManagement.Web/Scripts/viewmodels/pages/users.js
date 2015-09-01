﻿Supervisor.VM.Users = function (listViewUrl) {
    Supervisor.VM.Users.superclass.constructor.apply(this, arguments);
    var archiveUserCommad = "ArchiveUserCommad";
    var unArchiveUserCommad = "UnarchiveUserCommand";
    var self = this;

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
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.Users, Supervisor.VM.ListView);