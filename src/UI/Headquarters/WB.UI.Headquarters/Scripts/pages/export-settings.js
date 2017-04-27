﻿Supervisor.VM.ExportSettings = function (ajax, notifier, $dataUrl, $changeStateUrl, $regenPasswordUrl, $messageUrl) {
    Supervisor.VM.ExportSettings.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = $dataUrl;

    self.changeStateUrl = $changeStateUrl;
    self.regenPasswordUrl = $regenPasswordUrl;

    self.isEnabled = ko.observable(false);
    self.password = ko.observable('');
    self.message = ko.observable('');
    self.messageUpdated = ko.observable(false);

    self.load = function () {
        self.SendRequest(self.Url, {}, function (data) {
            self.isEnabled(data.IsEnabled);
            self.password(data.Password);
        }, true, true);

        self.SendRequest($messageUrl,
            {},
            function(data) {
                self.message(data.Message);
            }, true, true);
    };

    self.changeState = (function () {
        var confirmChangeSstateHtml = self.getBindedHtmlTemplate("#confirm-change-state-password");
        var newCheckState = self.isEnabled();
        bootbox.dialog({
            closeButton: false,
            message: confirmChangeSstateHtml,
            buttons: {
                cancel: {
                    label: "No",
                    callback: function() {
                        self.isEnabled(!newCheckState);
                    }
                },
                success: {
                    label: "Yes",
                    callback: function () {
                        ajax.sendRequest(self.changeStateUrl, "POST", { EnableState: newCheckState }, false,
                            //onSuccess
                            function (data) {
                                self.isEnabled(data.IsEnabled);
                                self.password(data.Password);
                            });
                    }
                }
            }
        });

        return true;
    });

    self.updateMessage = function() {
        ajax.sendRequest($messageUrl, "POST", { message: self.message() }, false,
            //onSuccess
            function() {
                self.messageUpdated(true);
                location.reload();
            });
    };

    self.clearMessage = function () {
        var confirmNoteClearing = self.getBindedHtmlTemplate("#confirm-note-clearing");

        bootbox.dialog({
            closeButton: false,
            message: confirmNoteClearing,
            buttons: {
                cancel: {
                    label: "No"
                },
                success: {
                    label: "Yes",
                    callback: function () {
                        self.message("");
                        self.updateMessage();
                    }
                }
            }
        });
    };


    self.onLogoSubmit = function() {
        //check whether browser fully supports all File API
        if (window.File && window.FileReader && window.FileList && window.Blob) {
            //get the file size and file type from file input field
            var fsize = $('#companyLogo')[0].files[0].size;

            if (fsize > 1024*1024*10) //do something if file size more than 10 mb
            {
                alert('Logo image size should be less than 10mb');
                return false;
            } else {
                return true;
            }
        }
    };

    self.regeneratePass = function () {
        var confirmRegenerateHtml = self.getBindedHtmlTemplate("#confirm-regenerate-password");

        bootbox.dialog({
            closeButton: false,
            message: confirmRegenerateHtml,
            buttons: {
                cancel: {
                    label: "No"
                },
                success: {
                    label: "Yes",
                    callback: function () {
                        ajax.sendRequest(self.regenPasswordUrl, "POST", { }, false,
                            //onSuccess
                            function (data) {
                                self.isEnabled(data.IsEnabled);
                                self.password(data.Password);
                            });
                    }
                }
            }
        });

        return true;
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ExportSettings, Supervisor.VM.BasePage);