Supervisor.VM.BatchUpload = function (interviewId, questionnaireId, questionnaireVersion, responsiblesUrl, importDataUrl, successUploadUrl) {
    Supervisor.VM.BatchUpload.superclass.constructor.apply(this, arguments);

    var self = this;

    self.ResponsiblesUrl = responsiblesUrl;
    self.IsResponsiblesLoading = ko.observable(false);
    self.Responsibles = function (query, sync, pageSize) {
        self.IsResponsiblesLoading(true);
        self.SendRequest(self.ResponsiblesUrl, { query: query, pageSize: pageSize }, function (response) {
            sync(response.Users, response.TotalCountByQuery);
        }, true, true, function() {
            self.IsResponsiblesLoading(false);
        });
    }
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