Supervisor.VM.BatchUserCreation = function (allUserBatchUploadProcessesUrl, deleteUserBatchUploadProcessUrl, confirmTemplateId) {
    Supervisor.VM.BatchUserCreation.superclass.constructor.apply(this, arguments);

    var self = this;

    self.load = function () {
        self.search();
    };

    self.deleteUserBatchUploadProcess = function (processItem) {
        var confirmMessageHtml = self.getBindedHtmlTemplate(confirmTemplateId, processItem);

        bootbox.dialog({
            message: confirmMessageHtml,
            buttons: {
                cancel: {
                    label: "No"
                },
                success: {
                    label: "Yes",
                    callback: function() {
                        var requestHeaders = {};
                        requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;

                        $.ajax({
                            url: deleteUserBatchUploadProcessUrl + "/" + processItem.ProcessId(),
                            type: 'post',
                            headers: requestHeaders,
                            dataType: 'json'
                        }).done(function (data) {
                            self.search();
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
                            self.search();
                        }).always(function () {
                            self.IsPageLoaded(true);
                            self.IsAjaxComplete(true);
                        });
                    }
                }
            }
        });
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.BatchUserCreation, Supervisor.VM.ListView);