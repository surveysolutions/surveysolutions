Supervisor.VM.ExportData = function (templates, $dataUrl, $historyUrl, $exportFromats, $deleteDataExportProcessUrl, $updateDataUrl, $updateApprovedDataUrl) {
    Supervisor.VM.ExportData.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = $dataUrl;
    self.HistoryUrl = $historyUrl;
    self.DeleteDataExportProcessUrl = $deleteDataExportProcessUrl;
    self.UpdateDataUrl = $updateDataUrl;
    self.UpdateApprovedDataUrl = $updateApprovedDataUrl;
    self.Templates = templates;


    self.ParaDataTabularReference = ko.observableArray();
    self.DataTabularReference = ko.observableArray();
    self.DataBinaryReference = ko.observableArray();
    self.ApprovedDataTabularReference = ko.observableArray();
    self.DataSTATAReference = ko.observableArray();
    self.ApprovedDataSTATAReference = ko.observableArray();
    self.DataSPSSReference = ko.observableArray();
    self.ApprovedDataSPSSReference = ko.observableArray();

    self.RunningProcesses = ko.observableArray([]);
    self.exportFromats = $exportFromats;

    self.selectedTemplate = ko.observable();

    self.selectedTemplateId = ko.computed(function () {
        return self.selectedTemplate() && self.selectedTemplate().id;
    });

    self.selectedTemplateVersion = ko.computed(function () {
        return self.selectedTemplate() && self.selectedTemplate().version;
    });

    self.selectedTemplateTitle = ko.computed(function () {
        return self.selectedTemplate() && self.selectedTemplate().title;
    });

    self.updateDataExportInfo = function (runRecursively) {
        if (self.selectedTemplate() == null) {
            _.delay(function () {
                self.updateDataExportInfo(true);
            }, 3000);
            return;
        }
        var filter = {
            questionnaireId: self.selectedTemplateId(),
            questionnaireVersion: self.selectedTemplate().version
        };
        self.sendWebRequest(self.Url, filter, function(data) {
            ko.mapping.fromJS(data, self.mappingOptions, self);
            if (runRecursively===true) {
                _.delay(function () {
                    self.updateDataExportInfo(true);
                }, 3000);
            }
        });
    };
    self.stopExportProcess = function (runningExport) {
        self.sendWebRequest(self.DeleteDataExportProcessUrl + "/" + runningExport.DataExportProcessId());
    }
    self.requestParaDataUpdate = function(format) {
        return function() {
            self.sendWebRequest(self.HistoryUrl,
                [],
                function (data) {
                    self.updateDataExportInfo();
                });
        }
    };

    self.requestDataUpdate = function(format) {
        var questionnaireId = self.selectedTemplateId();
        var questionnaireVersion = self.selectedTemplate().version;
        return function() {
            self.sendWebRequest(self.UpdateDataUrl + "?questionnaireId=" + questionnaireId + "&questionnaireVersion=" + questionnaireVersion + "&format=" + format,
                [],
                function (data) {
                    self.updateDataExportInfo();
                });
        }
    };

    self.requestApprovedDataUpdate = function(format) {
        var questionnaireId = self.selectedTemplateId();
        var questionnaireVersion = self.selectedTemplate().version;
        return function() {
            self.sendWebRequest(self.UpdateApprovedDataUrl + "?questionnaireId=" + questionnaireId + "&questionnaireVersion=" + questionnaireVersion + "&format=" + format,
                [],
                function(data) {
                    self.updateDataExportInfo();
                });
        }
    };

    self.sendWebRequest = function (url, args, onSuccess) {
        var requestHeaders = {};
        requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;

        $.ajax({
            url: url,
            type: 'post',
            headers: requestHeaders,
            data: args,
            dataType: 'json'
        }).done(function (data) {
            if (!Supervisor.Framework.Objects.isUndefined(onSuccess)) {
                onSuccess(data);
            }
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

        });
    }

    self.lastUpdateDate = function(type, format) {
        var dataReference = self.getDataReference(type, format);

        if (dataReference == null || _.isUndefined(dataReference().LastUpdateDate) || dataReference().LastUpdateDate() === null)
            return "Never";
        return self.formatDate(dataReference().LastUpdateDate());
    }

    self.showDownloadButton = function (type, format) {
        var dataReference = self.getDataReference(type, format);
        if (dataReference == null || _.isUndefined(dataReference().HasDataToExport))
            return false;
        return dataReference().HasDataToExport();
    }
    self.showRefreshButton = function (type, format) {
        var dataReference = self.getDataReference(type, format);
        if (dataReference == null || _.isUndefined(dataReference().CanRefreshBeRequested))
            return true;
        return dataReference().CanRefreshBeRequested();
    }

    self.getDataReference = function (type, format)
    {
        var referenceName = type + format + "Reference";
        if (_.isUndefined(self[referenceName]))
            return null;
        return self[referenceName];
    }

    self.exportFormatName = function (runningExport) {
        return self.exportFromats[runningExport.Format()];
    }
    self.formatDate=function(date) {
        return moment(date).format("MM/DD/YYYY HH:mm:ss");
    }

    self.updateDataExportInfo(true);
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ExportData, Supervisor.VM.BasePage);