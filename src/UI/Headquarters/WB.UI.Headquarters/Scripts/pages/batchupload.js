Supervisor.VM.BatchUpload = function (interviewId, questionnaireId, questionnaireVersion, responsibles, importDataUrl, successUploadUrl) {
    Supervisor.VM.BatchUpload.superclass.constructor.apply(this, arguments);

    var self = this;

    self.Responsibles = responsibles;
    self.SelectedResponsible = ko.observable();

    self.IsSupervisorSelected = ko.computed(function() {
        return !_.isUndefined(self.SelectedResponsible());
    });

    self.load = function () {
    };

    self.uploadSample = function() {
        var request = {
            interviewId: interviewId,
            questionnaireId: questionnaireId,
            questionnaireVersion: questionnaireVersion,
            responsibleSupervisor: _.isUndefined(self.SelectedResponsible()) ? null : self.SelectedResponsible().UserId
        };

        self.SendRequest(importDataUrl, request, function () {
            window.location = successUploadUrl;
        });
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.BatchUpload, Supervisor.VM.BasePage);