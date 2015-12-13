Supervisor.VM.CreateInterviewer = function (supervisorsUrl) {
    Supervisor.VM.CreateInterviewer.superclass.constructor.apply(this);

    var self = this;

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

    self.load = function () {
      
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.CreateInterviewer, Supervisor.VM.BasePage);