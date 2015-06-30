Supervisor.VM.Users = function (listViewUrl) {
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
        var messageHtml = self.getBindedHtmlTemplate("#confirm-delete-template", filteredItems);

        if (filteredItems.length === 0) {
            bootbox.alert(messageHtml);
            return;
        }

        messageHtml += $("#confirm-continue-message-template").html();

        bootbox.confirm(messageHtml, function(result) {
            if (result) {
                var commands = ko.utils.arrayMap(filteredItems, function(rawItem) {
                    return ko.toJSON({ userId: rawItem.UserId() });
                });

                var command = {
                    type: commandName,
                    commands: commands
                };
                self.SendCommands(command, function() {
                    setTimeout(function() { self.search(); }, 100);
                });
            }
        });
    };
    self.getBindedHtmlTemplate = function (templateId, bindObject) {
        var messageTemplate = $("<div/>").html($(templateId).html())[0];
        ko.applyBindings(bindObject, messageTemplate);
        var html = $(messageTemplate).html();
        return html;
    }
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.Users, Supervisor.VM.ListView);