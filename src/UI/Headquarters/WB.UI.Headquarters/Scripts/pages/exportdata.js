Supervisor.VM.ExportData = function (templates, statuses, $dataUrl, $historyUrl, $exportFromats, $deleteDataExportProcessUrl, $updateDataUrl) {
    Supervisor.VM.ExportData.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = $dataUrl;
    self.HistoryUrl = $historyUrl;
    self.DeleteDataExportProcessUrl = $deleteDataExportProcessUrl;
    self.UpdateDataUrl = $updateDataUrl;
    self.Templates = templates;
    self.Statuses = statuses;

    self.DataExports = ko.observableArray([]);
    self.RunningDataExportProcesses = ko.observableArray([]);
    self.exportFromats = $exportFromats;

    self.selectedTemplate = ko.observable();
    self.selectedStatus = ko.observable({status : 'null'});

    self.selectedTemplateId = ko.computed(function () {
        return self.selectedTemplate() && self.selectedTemplate().id;
    });

    self.selectedTemplateVersion = ko.computed(function () {
        return self.selectedTemplate() && self.selectedTemplate().version;
    });

    self.selectedTemplateTitle = ko.computed(function () {
        return self.selectedTemplate() && self.selectedTemplate().title;
    });

    self.selectedStatus.subscribe(function() {
        self.updateDataExportInfo(false);
    });

    self.updateDataExportInfo = function (runRecursively) {
        if (self.selectedTemplate() == null) {
            _.delay(function () {
                self.updateDataExportInfo(true);
            }, 3000);
            return;
        }
        var questionnaireId = self.selectedTemplateId();
        var questionnaireVersion = self.selectedTemplate().version;
        var status = self.selectedStatus().status;

        self.sendWebRequest(self.Url + "?questionnaireId=" + questionnaireId + "&questionnaireVersion=" + questionnaireVersion + "&status=" + status, {}, function (data) {
            ko.mapping.fromJS(data, self.mappingOptions, self);
            if (runRecursively===true) {
                _.delay(function () {
                    self.updateDataExportInfo(true);
                }, 3000);
            }
        });
    };

    self.stopExportProcess = function (runningExport) {
        runningExport.exportFormatName = self.exportFormatName;
        var confirmMessageHtml = self.getBindedHtmlTemplate("#confirm-delete-template", runningExport);

        bootbox.dialog({
            message: confirmMessageHtml,
            buttons: {
                cancel: {
                    label: "No"
                },
                success: {
                    label: "Yes",
                    callback: function () {
                        self.sendWebRequest(self.DeleteDataExportProcessUrl + "/" + runningExport.DataExportProcessId());
                    }
                }
            }
        });
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
        var status = self.selectedStatus().status;

        return function() {
            self.sendWebRequest(self.UpdateDataUrl + "?questionnaireId=" + questionnaireId + "&questionnaireVersion=" + questionnaireVersion + "&format=" + format + "&status=" + status,
                [],
                function (data) {
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
        if (dataReference == null) return "Please wait...";
        if (_.isUndefined(dataReference.LastUpdateDate) || dataReference.LastUpdateDate() === null)
            return "No exported data";
        return "Last updated: " + self.formatDate(dataReference.LastUpdateDate());
    }

    self.showDownloadButton = function (type, format) {
        var dataReference = self.getDataReference(type, format);
        if (dataReference == null || _.isUndefined(dataReference.HasDataToExport))
            return false;
        return dataReference.HasDataToExport();
    }
    self.showRefreshButton = function (type, format) {
        var dataReference = self.getDataReference(type, format);
        if (dataReference == null) return false;
        if (_.isUndefined(dataReference.CanRefreshBeRequested))
            return true;
        return dataReference.CanRefreshBeRequested();
    }

    self.getDataReference = function (type, format) {

        var existingDataExport = _.find(self.DataExports(), function (dataExports) {
            return dataExports.DataExportType() == type && dataExports.DataExportFormat() == format;
        });

        if (_.isUndefined(existingDataExport)) {
            return null;
        }
        return existingDataExport;
    }

    self.exportFormatName = function (runningExport) {
        return self.exportFromats[runningExport.Format()];
    }
    self.formatDate=function(date) {
        return moment(date).format("YYYY-MM-DD HH:mm:ss");
    }

    self.exportFormatProgress = function (progress) {
        return progress === 0 ? "Enqueued" : progress + "%";
    }

    self.updateDataExportInfo(true);
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ExportData, Supervisor.VM.BasePage);