Supervisor.VM.ControlPanel.InterviewDetails = function (apiUrl) {
    Supervisor.VM.ControlPanel.InterviewDetails.superclass.constructor.apply(this, arguments);

    var self = this;

    self.apiUrl = apiUrl;

    self.messages = ko.observableArray([]);
    self.errors = ko.observableArray([]);

    self.load = function () {
        self.SendRequest(self.apiUrl, {}, function(data) {
            self.messages(data.Messages);
            self.errors(data.Errors);
        });
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ControlPanel.InterviewDetails, Supervisor.VM.BasePage);