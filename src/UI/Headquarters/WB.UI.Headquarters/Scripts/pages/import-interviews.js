Supervisor.VM.ImportInterviews = function (sampleId, questionnaireId, questionnaireVersion, importInterviewsStatusUrl, importInterviewsUrl, responsiblesUrl) {
    Supervisor.VM.ImportInterviews.superclass.constructor.apply(this, arguments);

    var self = this;
    
    self.status = {
        questionnaireId: ko.observable(),
        questionnaireVersion: ko.observable(),
        createdInterviewsCount: ko.observable(0),
        totalInterviewsCount: ko.observable(0),
        elapsedTime: ko.observable("-"),
        estimatedTime: ko.observable("-"),
        isInProgress: ko.observable(false),
        questionnaireTitle: ko.observable(''),
        hasErrors: ko.observable(false),
        sampleId: ko.observable()
    };
    self.isResponsiblesLoading = ko.observable(false);
    self.responsibles = function (query, sync, pageSize) {
        self.isResponsiblesLoading(true);
        self.SendRequest(responsiblesUrl, { query: query, pageSize: pageSize }, function (response) {
            sync(response.Users, response.TotalCountByQuery);
        }, true, true, function () {
            self.isResponsiblesLoading(false);
        });
    }
    self.selectedResponsible = ko.observable(undefined).extend({ required: { shouldValidateOnStart: false } });
    self.isStatusLoaded = ko.observable(false);

    self.canImportInterviews = ko.computed(function () {
        if (sampleId !== self.status.sampleId())
            return true;
        return self.isStatusLoaded() && !self.status.isInProgress() && self.status.createdInterviewsCount() === 0;
    });
    self.importCompleted = ko.computed(function () {
        return sampleId === self.status.sampleId() && self.isStatusLoaded() && !self.status.isInProgress() && self.status.createdInterviewsCount() === self.status.totalInterviewsCount();
    });

    self.isNeedShowStatusPanel = ko.computed(function() {
        return sampleId === self.status.sampleId();
    });

    self.load = function () {
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
            self.status.questionnaireId(data.QuestionnaireId);
            self.status.questionnaireVersion(data.QuestionnaireVersion);
            self.status.hasErrors(data.HasErrors);
            self.status.sampleId(data.SampleId);
            self.isStatusLoaded(true);

            _.delay(self.updateStatusByInterviewsImport, 3000);
        }, true, true);
    };

    self.importInterviews = function () {
        var request = {
            questionnaireId: questionnaireId, 
            questionnaireVersion: questionnaireVersion,
            supervisorId: _.isUndefined(self.selectedResponsible()) ? "" : self.selectedResponsible().UserId,
            sampleId: sampleId
        };

        self.SendRequestWithFiles(importInterviewsUrl, request, function (response) {
            if (response.RequiredPrefilledQuestions.length > 0) {
                bootbox.alert({
                    message: self.getBindedHtmlTemplate("#required-prefilled-questions-template", response.RequiredPrefilledQuestions),
                    callback: function () {
                        self.fileWithInterviews('');
                    }
                });
            }
            else if (response.IsSupervisorRequired) {
                self.selectedResponsible.isValid();
                $("#dialogSelectSupervisor").modal({
                    "backdrop": "static",
                    "keyboard": true,
                    "show": true
                });
            }
        });
    }

    self.selectSupervisor = function () {
        self.importInterviews();
        $('#dialogSelectSupervisor').modal('hide');
    }

    self.cancelSupervisorSelection = function() {
        self.selectedResponsible(undefined);
    }
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ImportInterviews, Supervisor.VM.BasePage);