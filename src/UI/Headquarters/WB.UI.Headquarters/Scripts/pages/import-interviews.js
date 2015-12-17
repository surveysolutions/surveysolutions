Supervisor.VM.ImportInterviews = function (questionnaireId, questionnaireVersion, importInterviewsStatusUrl, importInterviewsUrl) {
    Supervisor.VM.ImportInterviews.superclass.constructor.apply(this, arguments);

    var self = this;
    self.fileWithInterviews = ko.observable().extend({ required: true });
    self.isViewModelValid = function () { return true };
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

    self.load = function (isViewModelValid) {
        self.isViewModelValid = isViewModelValid;
        self.updateStatusByInterviewsImport();
    };

    self.updateStatusByInterviewsImport = function() {
        self.SendRequest(importInterviewsStatusUrl, {}, function (data) {
            self.status.isInProgress(data.IsInProgress);
            self.status.createdInterviewsCount(data.CreatedInterviewsCount);
            self.status.totalInterviewsCount(data.TotalInterviewsCount);
            self.status.elapsedTime(data.ElapsedTime);
            self.status.estimatedTime(data.EstimatedTime);
            self.status.questionnaireTitle(data.QuestionnaireTitle);

            self.isStatusLoaded(true);

            _.delay(self.updateStatusByInterviewsImport, 3000);
        }, true, true);
    };

    self.importInterviews = function () {
        if (!self.isViewModelValid())
            return;

        var request = {
            questionnaireId: questionnaireId, 
            questionnaireVersion: questionnaireVersion, 
            fileWithInterviews: $("#importByPrefilledQuestionsForm").find('[name="uploadedFiles"]')[0].files[0]
        };

        self.SendRequestWithFiles(importInterviewsUrl, request, function(response) {

        });
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ImportInterviews, Supervisor.VM.BasePage);