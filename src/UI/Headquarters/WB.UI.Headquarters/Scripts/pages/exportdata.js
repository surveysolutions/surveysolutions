Supervisor.VM.ExportData = function (templates, statuses, $dataUrl, $exportFormats, $deleteDataExportProcessUrl, $updateDataUrl, $exportToExternalStorageUrl, storages) {
    Supervisor.VM.ExportData.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = $dataUrl;
    self.DeleteDataExportProcessUrl = $deleteDataExportProcessUrl;
    self.UpdateDataUrl = $updateDataUrl;
    self.Templates = templates;
    self.Statuses = statuses;

    self.DataExports = ko.observableArray([]);
    self.RunningDataExportProcesses = ko.observableArray([]);
    self.exportFormats = $exportFormats;

    self.selectedTemplate = ko.observable();
    self.selectedStatus = ko.observable({ status: 'null' });
    
    self.fromDateSelected = ko.observable();
    self.toDateSelected = ko.observable();

    self.fromDateSelected.subscribe(function(newValue) {
        if (newValue == null) return;

        var picker = $("#to-date").data('datetimepickr_inst');
        picker.config.minDate = newValue;

        self.updateDataExportInfo(false);
    });

    self.toDateSelected.subscribe(function(newValue) {
        if (newValue == null) return;

        var picker = $("#from-date").data('datetimepickr_inst');
        picker.config.maxDate = newValue;

        self.updateDataExportInfo(false);
    });

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
    
    self.getRequestQuery = function (format) {
        var request = {
            id: self.selectedTemplateId(),
            version: self.selectedTemplate().version,
            status: self.selectedStatus().status
        };
        if (format)
            request["format"] = format;

        if (self.fromDateSelected() != undefined)
            request["from"] = self.fromDateSelected().toJSON();

        if (self.toDateSelected() != undefined)
            request["to"] = moment(self.toDateSelected()).add(1, 'days').utc().toISOString();

        return $.param(request);
    };

    self.updateDataExportInfo = function (runRecursively) {
        if (self.selectedTemplate() == null) {
            _.delay(function () {
                self.updateDataExportInfo(true);
            }, 3000);
            return;
        }

        self.sendWebRequest(self.Url + "?" + self.getRequestQuery(), {}, function (data) {
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

    self.selectStorage = function () {
        var dialogHtml = $("#storages-dialog").html();

        bootbox.dialog({
            message: dialogHtml,
            buttons: {
                cancel: {
                    label: "Cancel"
                },
                success: {
                    label: "Select",
                    callback: function () {
                        if (storages.selectedStorage().is_local) {
                            var requestUpdate = self.requestDataUpdate(4 /* binary export*/, false);
                            requestUpdate();
                        } else {
                            var state = {
                                questionnaireIdentity: {
                                    questionnaireId: self.selectedTemplateId(),
                                    version: self.selectedTemplate().version
                                },
                                interviewStatus: self.selectedStatus().status,
                                fromDate: self.fromDateSelected(),
                                toDate: self.toDateSelected() === undefined ? undefined : moment(self.toDateSelected()).add(1, 'days').utc(),
                                type: storages.selectedStorage().type
                            };

                            var request = {
                                response_type: storages.response_type,
                                redirect_uri: encodeURIComponent(storages.redirect_uri),
                                client_id: storages.selectedStorage().client_id,
                                state: window.btoa(window.location.href + ";" + $exportToExternalStorageUrl + ";" + JSON.stringify(state)),
                                scope: storages.selectedStorage().scope
                            };

                            window.location = storages.selectedStorage().authorization_uri + "?" + decodeURIComponent($.param(request));
                        }
                    }
                }
            }
        });

        ko.applyBindings(storages, $(".options-group")[0]);
    }

    self.requestDataUpdate = function (format, shouldShowStorageSelector) {
        return function () {
            if (storages && format === 4 /*binary export*/ && shouldShowStorageSelector === true) {
                self.selectStorage();
                return;
            }
            self.sendWebRequest(self.UpdateDataUrl + "?" + self.getRequestQuery(format),
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
            if (jqXhr.status === 401) {
                location.reload();
            }
            else if (jqXhr.status === 403) {
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
        return self.formatDate(dataReference.LastUpdateDate());
    }

    self.fileSize = function (type, format) {
        var dataReference = self.getDataReference(type, format);
        if (dataReference == null) return "Please wait...";
        if (_.isUndefined(dataReference.FileSize) || dataReference.FileSize() === null)
            return "No exported data";
        return dataReference.FileSize();
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

    self.hasAnyDataToBePrepared = function (type, format) {
        var dataReference = self.getDataReference(type, format);
        if (dataReference == null) return false;
        if (_.isUndefined(dataReference.HasAnyDataToBePrepared))
            return true;
        return dataReference.HasAnyDataToBePrepared();
    }
    
    self.requestStopExportProcess = function (type, format) {

        var confirmMessageHtml = self.getBindedHtmlTemplate("#confirm-stop-export");

        var dataReference = self.getDataReference(type, format);
        if (dataReference == null) return;
        if (_.isUndefined(dataReference.CanRefreshBeRequested))
            return;
        
        bootbox.dialog({
            message: confirmMessageHtml,
            buttons: {
                cancel: {
                    label: "No"
                },
                success: {
                    label: "Yes",
                    callback: function () {
                        self.sendWebRequest(self.DeleteDataExportProcessUrl + "/" + dataReference.DataExportProcessId());
                    }
                }
            }
        });
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
        return self.exportFormats[runningExport.Format()];
    }
    self.formatDate=function(date) {
        return moment(date).format("MMM DD, YYYY HH:mm");
    }

    self.exportFormatProgress = function (progress) {
        return progress === 0 ? "Enqueued" : progress + "%";
    }

    self.getProgressInPercents = function (type, format) {
        var dataReference = self.getDataReference(type, format);
        if (dataReference == null || _.isUndefined(dataReference.HasDataToExport))
            return false;
        return dataReference.ProgressInPercents();
    }

    self.isInQueue = function (type, format) {
        var dataReference = self.getDataReference(type, format);
        if (dataReference == null) return false;

        return dataReference.StatusOfLatestExportProcess() === 2;
    }

    self.isRunning = function (type, format) {
        var dataReference = self.getDataReference(type, format);
        if (dataReference == null) return false;

        return dataReference.StatusOfLatestExportProcess() === 3;
    }

    self.isCompessing = function (type, format) {
        var dataReference = self.getDataReference(type, format);
        if (dataReference == null) return false;

        return dataReference.StatusOfLatestExportProcess() === 4;
    }

    self.updateProcess = setInterval(function() { self.updateDataExportInfo(false); }, 2000);
    
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ExportData, Supervisor.VM.BasePage);
