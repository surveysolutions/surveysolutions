Supervisor.VM.HealthCheck = function (healthCheckUrl) {
    Supervisor.VM.HealthCheck.superclass.constructor.apply(this, arguments);

    var self = this;

    self.apiUrl = healthCheckUrl;

    self.model = ko.observable({});
    self.status = ko.observable(0);

    self.load = function () {
        self.SendRequest(self.apiUrl, {}, function (data) {
            self.model(data);
            self.status(data.Status);
        }, true, true);
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.HealthCheck, Supervisor.VM.BasePage);