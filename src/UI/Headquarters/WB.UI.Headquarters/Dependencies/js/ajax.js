var Notifier = function () {
    var self = this;
    
    PNotify.prototype.options.styling = "bootstrap3";

    self.showError = function (title, message) {
        new PNotify({ title: title, text: message });
    };

    self.showNotification = function (title, message) {
        new PNotify({ title: title, text: message });
    };

    return self;
};

var Ajax = function (notifier) {
    var self = this;

    notifier.showNotification("Hello", "World");

    self.isAjaxComplete = ko.observable(true);

    self.checkForRequestComplete = function () {
        if (!self.IsAjaxComplete()) {
            notifier.showNotification(input.settings.messages.notifyDialogTitle, input.settings.messages.notifyDialogText);
        }
    };

    self.sendRequest = function (requestUrl, method, args, skipInProgressCheck, onSuccess, onDone, onError) {
        notifier.showError("Hello1", "World1");
        if (!skipInProgressCheck && !self.isAjaxComplete()) {
            self.checkForRequestComplete();
            return;
        }

        self.isAjaxComplete(false);

        var requestHeaders = {};
        requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;

        $.ajax({
            url: requestUrl,
            type: method,
            data: args,
            headers: requestHeaders,
            dataType: 'json'
        }).done(function (data) {
            if (!_.isUndefined(onSuccess)) {
                onSuccess(data);
            }
        }).fail(function (jqXhr, textStatus, errorThrown) {
            if (jqXhr.status === 403) {
                if ((!jqXhr.responseText || 0 === jqXhr.responseText.length)) {
                    notifier.showError(input.settings.messages.forbiddenMessage);
                }
                else {
                    notifier.showError(jqXhr.responseText);
                }
            } else {
                notifier.showError(input.settings.messages.unhandledExceptionMessage);
            }

            if (!_.isUndefined(onError)) {
                onError(jqXhr, textStatus, errorThrown);
            }
        }).always(function () {
            self.isAjaxComplete(true);
            if (!_.isUndefined(onDone)) {
                onDone();
            }
        });
    };
};