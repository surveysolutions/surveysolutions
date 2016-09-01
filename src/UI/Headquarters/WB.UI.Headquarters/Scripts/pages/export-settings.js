Supervisor.VM.ExportSettings = function ($dataUrl, $changeStateUrl, $regenPasswordUrl, $messageUrl) {
    Supervisor.VM.ExportSettings.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = $dataUrl;

    self.changeStateUrl = $changeStateUrl;
    self.regenPasswordUrl = $regenPasswordUrl;

    self.isEnabled = ko.observable(false);
    self.password = ko.observable('');
    self.message = ko.observable('');
    self.messageUpdated = ko.observable(false);

    self.load = function () {
        self.SendRequest(self.Url, {}, function (data) {
            self.isEnabled(data.IsEnabled);
            self.password(data.Password);
        }, true, true);

        self.SendRequest($messageUrl,
            {},
            function(data) {
                self.message(data.Message);
            }, true, true);
    };

    self.changeState = function () {
        self.SendRequest(self.changeStateUrl,
            { EnableState: self.isEnabled() },
            function (data) {
                self.isEnabled(data.IsEnabled);
                self.password(data.Password);
            });
        return true;
    };

    self.updateMessage = function() {
        self.SendRequest($messageUrl,
            { message: self.message() },
            function() {
                self.messageUpdated(true);
            },
            true,
            false);
    };

    self.regeneratePass = function () {
        var confirmRegenerateHtml = self.getBindedHtmlTemplate("#confirm-regenerate-password");

        bootbox.dialog({
            message: confirmRegenerateHtml,
            buttons: {
                cancel: {
                    label: "No"
                },
                success: {
                    label: "Yes",
                    callback: function () {
                        self.SendRequest(self.regenPasswordUrl,
                                        {}, function (data) {
                                            self.isEnabled(data.IsEnabled);
                                            self.password(data.Password);
                                        });
                    }
                }
            }
        });

        return true;
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ExportSettings, Supervisor.VM.BasePage);