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
                        self.SendRequest(deleteUserBatchUploadProcessUrl + "/" + processItem.ProcessId(), {}, function () {
                            self.search();
                        });
                    }
                }
            }
        });
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.BatchUserCreation, Supervisor.VM.ListView);