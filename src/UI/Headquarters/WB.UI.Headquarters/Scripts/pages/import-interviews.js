Supervisor.VM.ImportInterviews = function (questionnaireId, questionnaireVersion, importInterviewsStatusUrl, importInterviewsUrl, responsiblesUrl) {
    Supervisor.VM.ImportInterviews.superclass.constructor.apply(this, arguments);

    var self = this;
    self.fileWithInterviews = ko.observable().extend({ required: true });
    self.isViewModelValid = function () { return true };
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
    self.selectedResponsible = ko.observable();
    self.isStatusLoaded = ko.observable(false);
    self.canImportInterviews = ko.computed(function() {
        return self.isStatusLoaded() && !self.status.isInProgress();
    });
    self.isNeedShowStatusPanel = function() {
        return self.status.hasErrors()
            && questionnaireId == elf.status.questionnaireId()
            && questionnaireVersion == self.status.questionnaireVersion();
    }

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
            self.status.questionnaireId(data.QuestionnaireId);
            self.status.questionnaireVersion(data.QuestionnaireVersion);
            self.status.hasErrors(data.HasErrors);

            self.isStatusLoaded(true);

            _.delay(self.updateStatusByInterviewsImport, 1000);
        }, true, true);
    };

    self.importInterviews = function () {
        if (!self.isViewModelValid())
            return;

        var fileByPrefilledQuestions = $("#fileByPrefilledQuestions");

        var request = {
            questionnaireId: questionnaireId, 
            questionnaireVersion: questionnaireVersion,
            supervisorId: _.isUndefined(self.selectedResponsible()) ? "" : self.selectedResponsible().UserId,
            fileWithInterviews: fileByPrefilledQuestions[0].files[0]
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
                $("#select-supervisor-dialog").show();
                bootbox.alert({
                    title: "Select supervisor",
                    message: $("#select-supervisor-dialog"),
                    callback: function() {
                        if (!_.isUndefined(self.selectedResponsible())) {
                            self.importInterviews();
                            self.fileWithInterviews('');
                            self.selectedResponsible(undefined);
                        }
                        $("#select-supervisor-dialog").hide().appendTo($('body'));
                    }
                });
            } 
        });
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ImportInterviews, Supervisor.VM.BasePage);