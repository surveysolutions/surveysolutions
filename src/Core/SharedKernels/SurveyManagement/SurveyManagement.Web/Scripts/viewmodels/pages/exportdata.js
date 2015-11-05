Supervisor.VM.ExportData = function (templates, $dataUrl, $historyUrl, $exportFromats, $exportTypes, $deleteDataExportProcessUrl, $updateTabularDataUrl, $updateApprovedTabularDataUrl) {
    Supervisor.VM.ExportData.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = $dataUrl;
    self.HistoryUrl = $historyUrl;
    self.DeleteDataExportProcessUrl = $deleteDataExportProcessUrl;
    self.UpdateTabularDataUrl = $updateTabularDataUrl;
    self.UpdateApprovedTabularDataUrl = $updateApprovedTabularDataUrl;
    self.Templates = templates;
    self.ParadataReference = ko.observableArray();
    self.TabularDataReference = ko.observableArray();
    self.TabularApprovedDataReference = ko.observableArray();
    self.RunningProcesses = ko.observableArray([]);
    self.exportFromats = $exportFromats;
    self.exportTypes = $exportTypes;

    self.selectedTemplate = ko.observable();

    self.selectedTemplateId = ko.computed(function () {
        return self.selectedTemplate() && self.selectedTemplate().id;
    });
    self.selectedTemplate.subscribe(function () {
        self.updateDataExportInfo();
    });

    self.selectedTemplateVersion = ko.computed(function () {
        return self.selectedTemplate() && self.selectedTemplate().version;
    });

    self.selectedTemplateTitle = ko.computed(function () {
        return self.selectedTemplate() && self.selectedTemplate().title;
    });

    self.updateDataExportInfo = function () {
        var filter = {
            questionnaireId: self.selectedTemplateId(),
            questionnaireVersion: self.selectedTemplate().version
        };
        self.SendRequest(self.Url, filter, function (data) {
            ko.mapping.fromJS(data, self.mappingOptions, self);
            _.delay(self.updateDataExportInfo, 3000);
        }, true);
    };
    self.stopExportProcess = function (runningExport) {
        self.sendActionRequest(self.DeleteDataExportProcessUrl + "/" + runningExport.DataExportProcessId());
    }
    self.requestParaDataUpdate = function() {
        self.sendActionRequest(self.HistoryUrl);
    };
    self.requestTabularDataUpdate = function () {
        self.sendActionRequest(self.UpdateTabularDataUrl + "?questionnaireId=" + self.selectedTemplateId() + "&questionnaireVersion=" + self.selectedTemplate().version);
    }
    self.requestApprovedTabularDataUpdate = function () {
        self.sendActionRequest(self.UpdateApprovedTabularDataUrl + "?questionnaireId=" + self.selectedTemplateId() + "&questionnaireVersion=" + self.selectedTemplate().version);
    }
    self.sendActionRequest = function (url, args) {
        var requestHeaders = {};
        requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;

        $.ajax({
            url: url,
            type: 'post',
            headers: requestHeaders,
            data: args,
            dataType: 'json'
        }).done(function (data) {
            self.updateDataExportInfo();
        }).fail(function (jqXhr, textStatus, errorThrown) {
            if (jqXhr.status === 403) {
                if ((!jqXhr.responseText || 0 === jqXhr.responseText.length)) {
                    self.ShowError(input.settings.messages.forbiddenMessage);
                } else {
                    self.ShowError(jqXhr.responseText);
                }
            } else {
                var jsonException = $.parseJSON(jqXhr.responseText);
                if (!Supervisor.Framework.Objects.isUndefined(jsonException))
                    self.ShowError(jsonException.Message);
                else
                    self.ShowError(input.settings.messages.unhandledExceptionMessage);
            }

        }).always(function () {
            self.IsPageLoaded(true);
            self.IsAjaxComplete(true);
        });
    }

    self.lastParaDataUpdateDate = function () {
        if (self.ParadataReference() == null || _.isUndefined(self.ParadataReference().LastUpdateDate))
            return "Never";
        return self.ParadataReference().LastUpdateDate();
    }

    self.showParaDataDownloadButton = function () {
        if (self.ParadataReference() == null || _.isUndefined(self.ParadataReference().HasDataToExport))
            return false;
        return self.ParadataReference().HasDataToExport();
    }
    self.showParaDataRefreshButton = function () {
        if (self.ParadataReference() == null || _.isUndefined(self.ParadataReference().CanRefreshBeRequested))
            return true;
        return self.ParadataReference().CanRefreshBeRequested();
    }

    self.lastTabularDataUpdateDate = function () {
        if (self.TabularDataReference() == null || _.isUndefined(self.TabularDataReference().LastUpdateDate))
            return "Never";
        return self.TabularDataReference().LastUpdateDate();
    }

    self.showTabularDataDownloadButton = function () {
        if (self.TabularDataReference() == null || _.isUndefined(self.TabularDataReference().HasDataToExport))
            return false;
        return self.TabularDataReference().HasDataToExport();
    }
    self.showTabularDataRefreshButton = function () {
        if (self.TabularDataReference() == null || _.isUndefined(self.TabularDataReference().CanRefreshBeRequested))
            return true;
        return self.TabularDataReference().CanRefreshBeRequested();
    }

    self.lastApprovedTabularDataUpdateDate = function () {
        if (self.TabularApprovedDataReference() == null || _.isUndefined(self.TabularApprovedDataReference().LastUpdateDate))
            return "Never";
        return self.TabularApprovedDataReference().LastUpdateDate();
    }

    self.showApprovedTabularDataDownloadButton = function () {
        if (self.TabularApprovedDataReference() == null || _.isUndefined(self.TabularApprovedDataReference().HasDataToExport))
            return false;
        return self.TabularApprovedDataReference().HasDataToExport();
    }
    self.showApprovedTabularDataRefreshButton = function () {
        if (self.TabularApprovedDataReference() == null || _.isUndefined(self.TabularApprovedDataReference().CanRefreshBeRequested))
            return true;
        return self.TabularApprovedDataReference().CanRefreshBeRequested();
    }

    self.exportFormatName = function (runningExport) {
        return self.exportFromats[runningExport.Format()];
    }

    self.exportName = function (runningExport) {
        if (runningExport.QuestionnaireTitle() && runningExport.QuestionnaireVersion())
            return runningExport.QuestionnaireTitle() +'-'+ runningExport.QuestionnaireVersion();
        return self.exportTypes[runningExport.Type()];
    }
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ExportData, Supervisor.VM.BasePage);