var Notifier = function () {
    var self = this;
    var loadingIndicator = null;
    var stack_modal = { "dir1": "down", "dir2": "right", "push": "center", "modal": true, "overlay_close": false };
    var loadingIndicatorOptions = {
        title: "Loading, please wait",
        text: false,
        addclass: "stack-modal loading-indicator",
        stack: stack_modal,
        type: "info",
        hide: false,
        icon: false,
        auto_display: false,
        buttons: {
            sticker: false,
            closer: false
        }
    };
    PNotify.prototype.options.styling = "bootstrap3";

    self.showError = function (title, message) {
        new PNotify({ title: title, text: message });
    };

    self.showNotification = function (title, message) {
        new PNotify({ title: title, text: message });
    };

    self.modal = function (title, message) {
        var modal = new PNotify({
            title: title,
            text: message,
            hide: false,
            type: "info",
            tag: "modal",
            insert_brs: false,
            addclass: "centered-modal",
            history: {
                history: false
            },
            buttons: {
                closer: false,
                sticker: false
            },
            stack: stack_modal
        });
        modal.get();
        return modal;
    };

    self.confirm = function (title, message, confirmCallback, cancelCallback) {
        return (new PNotify({
            title: title,
            text: message,
            hide: false,
            type: "notice",
            insert_brs: false,
            addclass: "centered-modal",
            confirm: {
                confirm: true
            },
            buttons: {
                sticker: false
            },
            history: {
                history: false
            },
            stack: stack_modal
        })).get()
            .on('pnotify.confirm', confirmCallback)
            .on('pnotify.cancel', cancelCallback);
    };

    self.alert = function (title, message, callback) {
        (new PNotify({
            title: title,
            text: message,
            hide: false,
            insert_brs: false,
            addclass: "centered-modal",
            confirm: {
                confirm: true,
                buttons: [{
                    text: 'Ok',
                    addClass: 'ui-pnotify-action-button btn btn-default',
                    click: function (notice) {
                        notice.remove();
                    }
                },
                    null]
            },
            buttons: {
                closer: false,
                sticker: false
            },
            history: {
                history: false
            },
            stack: stack_modal
        })).get()
            .on('pnotify.confirm', callback)
            .on('pnotify.cancel', callback);
    };

    var openPnotifyIfExists = function (pnotify) {
        if (!isNull(loadingIndicator)) {
            pnotify.open();
        }
    };

    self.showLoadingIndicator = function () {
        if (isNull(loadingIndicator)) {
            loadingIndicator = new PNotify(loadingIndicatorOptions);
            delay(openPnotifyIfExists, 500, loadingIndicator);
        }
    };


    self.hideLoadingIndicator = function () {
        if (!isNull(loadingIndicator)) {
            loadingIndicator.remove();
            loadingIndicator = null;
        }
    };

    return self;
};

var Ajax = function (notifier) {
    var self = this;

    self.isAjaxComplete = ko.observable(true);

    self.checkForRequestComplete = function () {
        if (!self.IsAjaxComplete()) {
            notifier.showNotification(input.settings.messages.notifyDialogTitle, input.settings.messages.notifyDialogText);
        }
    };

    self.sendRequest = function (requestUrl, method, args, skipInProgressCheck, onSuccess, onDone, onError) {
        if (!skipInProgressCheck && !self.isAjaxComplete()) {
            self.checkForRequestComplete();
            return null;
        }

        self.isAjaxComplete(false);
        if (!skipInProgressCheck) {
            notifier.showLoadingIndicator();
        }

        var requestHeaders = {};
        //requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;

        return $.ajax({
            url: requestUrl,
            type: method,
            data: args,
            headers: requestHeaders,
            dataType: 'json'
        }).done(function (data) {
            if (!isUndefined(onSuccess)) {
                onSuccess(data);
            }
        }).fail(function (jqXhr, textStatus, errorThrown) {
            if (jqXhr.status === 403) {
                if ((!jqXhr.responseText || 0 === jqXhr.responseText.length)) {
                    notifier.showError(input.settings.messages.forbiddenMessage);
                }
                else {
                    notifier.showError("Error", jqXhr.responseJSON.Message || jqXhr.responseText);
                }
            } else {
                notifier.showError(input.settings.messages.unhandledExceptionMessage);
            }

            if (!isUndefined(onError)) {
                onError(jqXhr, textStatus, errorThrown);
            }
        }).always(function () {
            self.isAjaxComplete(true);
            notifier.hideLoadingIndicator();

            if (!isUndefined(onDone)) {
                onDone();
            }
        });
    };
};
