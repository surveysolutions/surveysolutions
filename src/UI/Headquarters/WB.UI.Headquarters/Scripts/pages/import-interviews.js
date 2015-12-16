Supervisor.VM.ImportInterviews = function (questionnaireId, questionnaireVersion, importInterviewsStatusUrl) {
    Supervisor.VM.ImportInterviews.superclass.constructor.apply(this, arguments);

    var self = this;
    self.status = {
        createdInterviewsCount: ko.observable(0),
        totalInterviewsCount: ko.observable(0),
        elapsedTime: ko.observable("-"),
        estimatedTime: ko.observable("-"),
        isInProgress: ko.observable(false),
        questionnaireTitle : ko.observable('')
    };
    self.isStatusLoaded = ko.observable(false);
    self.canImportInterviews = ko.computed(function() {
        return self.isStatusLoaded() && !self.status.isInProgress();
    });

    self.load = function () {
        self.SendRequest(importInterviewsStatusUrl, {}, function (data) {
            self.status.isInProgress(data.IsInProgress);
            self.status.createdInterviewsCount(data.CreatedInterviewsCount);
            self.status.totalInterviewsCount(data.TotalInterviewsCount);
            self.status.elapsedTime(data.ElapsedTime);
            self.status.estimatedTime(data.EstimatedTime);
            self.status.questionnaireTitle(data.QuestionnaireTitle);

            self.isStatusLoaded(true);

            _.delay(self.load, 3000);
        }, true, true);
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ImportInterviews, Supervisor.VM.BasePage);