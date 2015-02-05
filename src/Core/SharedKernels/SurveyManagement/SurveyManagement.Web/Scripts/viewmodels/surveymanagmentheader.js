Supervisor.VM.SurveyManagmentHeader = function (updateIncomingPackagesQueueApiUrl, holderId) {
    Supervisor.VM.SurveyManagmentHeader.superclass.constructor.apply(this, arguments);
    var self = this;
    self.holder = $(holderId);
    self.holder.html('-');
    self.holder.parent.tooltip();
    self.load = function () {
        self.updateIncomingPackagesQueue();
    };

    self.updateIncomingPackagesQueue = function () {
        self.SendRequest(updateIncomingPackagesQueueApiUrl, {}, function (data) {
            self.holder.html(data);
            setInterval(self.updateIncomingPackagesQueue, 3000);
        }, true, true);
    };
}


Supervisor.Framework.Classes.inherit(Supervisor.VM.SurveyManagmentHeader, Supervisor.VM.BasePage);