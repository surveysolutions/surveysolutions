Supervisor.VM.ExportSettings = function($dataUrl, $changeStateUrl, $regenPasswordUrl) {
    Supervisor.VM.ExportSettings.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = $dataUrl;

    self.changeStateUrl = $changeStateUrl;
    self.regenPasswordUrl = $regenPasswordUrl;

    self.isEnabled = ko.observable(false);
    self.password = ko.observable('');

    self.load = function() {
        self.SendRequest(self.Url, {}, function(data) {
            self.isEnabled(data.IsEnabled);
            self.password(data.Password);
        }, true, true);
    };

    self.changeState = function () {
        self.SendRequest(self.changeStateUrl,
        { EnableState: self.isEnabled() }, function (data) {
            self.isEnabled(data.IsEnabled);
            self.password(data.Password);
        })
        return true;
    };

    self.regeneratePass = function() {
        self.SendRequest(self.regenPasswordUrl,
        {}, function (data) {
            self.isEnabled(data.IsEnabled);
            self.password(data.Password);
        });
        return true;
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ExportSettings, Supervisor.VM.BasePage);