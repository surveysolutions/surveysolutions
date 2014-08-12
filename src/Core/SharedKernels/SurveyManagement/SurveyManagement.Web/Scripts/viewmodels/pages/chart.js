Supervisor.VM.ChartPage = function (serviceUrl, interviewDetailsUrl, users, commandExecutionUrl) {
    Supervisor.VM.ChartPage.superclass.constructor.apply(this, [serviceUrl, commandExecutionUrl]);

    var self = this;

    self.load = function () {
        self.search();
    };

};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ChartPage, Supervisor.VM.ListView);