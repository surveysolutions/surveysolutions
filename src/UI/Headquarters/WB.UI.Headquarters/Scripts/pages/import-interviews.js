Supervisor.VM.ImportInterviews = function (interviewImportProcessId, questionnaireId, questionnaireVersion, preloadingType, wasResponsibleProvided, importInterviewsStatusUrl, importInterviewsUrl, responsiblesUrl, processUrl) {
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
        interviewImportProcessId: ko.observable(),
        preloadingType: ko.observable(),
        wasResponsibleProvided: ko.observable()
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

    self.IsSupervisorSelected = ko.computed(function () {
        return !_.isUndefined(self.selectedResponsible());
    });

    self.isStatusLoaded = ko.observable(false);

    self.canImportInterviews = ko.computed(function () {
        if (!self.isStatusLoaded())
            return false;

        if (self.status.isInProgress())
            return false;

        if (interviewImportProcessId === self.status.interviewImportProcessId())
            return self.status.totalInterviewsCount() === 0;

        return true;
    });

    self.isRunningProcessExists = ko.computed(function () {
        return self.isStatusLoaded() && self.status.isInProgress() && interviewImportProcessId !== self.status.interviewImportProcessId();
    });

    self.getOtherProcessUrl = ko.computed(function () {
        return processUrl +"/" +self.status.interviewImportProcessId() +"?questionnaireId=" +self.status.questionnaireId() +"&version=" +self.status.questionnaireVersion();
    });

    self.importCompleted = ko.computed(function () {
        return self.isStatusLoaded() && interviewImportProcessId === self.status.interviewImportProcessId() && !self.status.isInProgress() && self.status.createdInterviewsCount() === self.status.totalInterviewsCount() && !self.status.hasErrors();
    });
    self.importCompletedWithError = ko.computed(function () {
        return self.isStatusLoaded() && interviewImportProcessId === self.status.interviewImportProcessId() && !self.status.isInProgress() && self.status.hasErrors();
    });
    self.isNeedShowStatusPanel = ko.computed(function () {
        return self.isStatusLoaded() && interviewImportProcessId === self.status.interviewImportProcessId();
    });

    self.load = function () {
        self.updateStatusByInterviewsImport();
    };

    self.updateStatusByInterviewsImport = function () {
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
            self.status.interviewImportProcessId(data.InterviewImportProcessId);
            self.isStatusLoaded(true);

            if (interviewImportProcessId !== self.status.interviewImportProcessId() || self.status.isInProgress() || self.status.totalInterviewsCount() === 0)
                _.delay(self.updateStatusByInterviewsImport, 3000);

        }, true, true);
    };

    self.importInterviews = function () {
        var request = {
            questionnaireId: questionnaireId,
            questionnaireVersion: questionnaireVersion,
            supervisorId: _.isUndefined(self.selectedResponsible()) ? "" : self.selectedResponsible().UserId,
            interviewImportProcessId: interviewImportProcessId,
            preloadingType: preloadingType,
            wasResponsibleProvided: wasResponsibleProvided
        };

        self.SendRequestWithFiles(importInterviewsUrl, request, function (response) {
            if (response.IsSupervisorRequired) {
                self.selectedResponsible.isValid();
            }
        },
            function (response) {
                self.ShowError(JSON.parse(response.responseText).Message);
            });
    }

    
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ImportInterviews, Supervisor.VM.BasePage);