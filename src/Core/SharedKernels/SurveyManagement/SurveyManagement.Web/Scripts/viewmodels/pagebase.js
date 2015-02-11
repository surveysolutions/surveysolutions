Supervisor.VM.BasePage = function(commandExecutionUrl) {

    var self = this;

    self.IsPageLoaded = ko.observable(false);
    self.IsAjaxComplete = ko.observable(true);
    self.IsFilterOpen = ko.observable(true);
    self.Errors = ko.observableArray([]);
    self.QueryString = location.queryString;
    self.IsNotifyVisible = ko.observable(false);
    self.NotifyTitle = ko.observable(input.settings.messages.notifyDialogTitle);
    self.NotifyText = ko.observable(input.settings.messages.notifyDialogText);
    self.IsShowRequestIndicator = ko.observable(true);

    self.ToggleFilter = function() {
        if (self.IsFilterOpen()) {
            $('body').addClass('menu-hidden');

            $('#content').removeClass('col-sm-9');
            $('#content').removeClass('col-sm-offset-3');
            $('#content').removeClass('col-md-10');
            $('#content').removeClass('col-md-offset-2');
            $('#content').addClass('col-sm-12');
            $('#content').addClass('col-md-12');
        } else {
            $('body').removeClass('menu-hidden');

            $('#content').addClass('col-sm-9');
            $('#content').addClass('col-sm-offset-3');
            $('#content').addClass('col-md-10');
            $('#content').addClass('col-md-offset-2');
            $('#content').removeClass('col-sm-12');
            $('#content').removeClass('col-md-12');
        }
        self.IsFilterOpen(!self.IsFilterOpen());
    };

    self.HideOutput = function() {
        $('body').removeClass('output-visible');
    };

    self.ShowOutput = function() {
        $('body').addClass('output-visible');
    };

    self.ShowError = function(message) {
        self.ShowErrors([message]);
    };

    self.ShowErrors = function(arrayOfMessages) {
        self.Errors.removeAll();
        $.each(arrayOfMessages, function(index, message) {
            self.Errors.push({ error: message });
        });
        self.ShowOutput();
    };

    self.IsAjaxComplete.subscribe(function(isLoaded) {
        if (isLoaded) {
            $('#umbrella').hide();
        } else if (self.IsShowRequestIndicator()) {
            setTimeout(function() {
                if (!self.IsAjaxComplete()) {
                    $('#umbrella').show();
                }
            }, 500);
        }
    });

    var notifyHandler;
    self.ShowNotification = function(title, text) {

        self.NotifyTitle(title || input.settings.messages.notifyDialogTitle);
        self.NotifyText(text || input.settings.messages.notifyDialogText);

        self.IsNotifyVisible(true);

        notifyHandler = setTimeout(function() {
            self.HideNotification();
        }, 3000);
    };

    self.HideNotification = function() {
        if (!Supervisor.Framework.Objects.isUndefined(notifyHandler)) {
            clearTimeout(notifyHandler);
        }
        self.IsNotifyVisible(false);
    };

    self.CheckForRequestComplete = function() {
        if (!self.IsAjaxComplete()) {
            self.ShowNotification(input.settings.messages.notifyDialogTitle, input.settings.messages.notifyDialogText);
        }
    };

    self.SendRequest = function(requestUrl, args, onSuccess, skipInProgressCheck, allowGet) {

        if (!skipInProgressCheck && !self.IsAjaxComplete()) {
            self.CheckForRequestComplete();
            return;
        }

        self.IsAjaxComplete(false);

        var requestHeaders = {};
        requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;

        $.ajax({
            url: requestUrl,
            type: allowGet === true ? 'get' : 'post',
            data: args,
            headers: requestHeaders,
            dataType: 'json'
        }).done(function(data) {
            if (!Supervisor.Framework.Objects.isUndefined(onSuccess)) {
                onSuccess(data);
            }
        }).fail(function() {
            self.ShowError(input.settings.messages.unhandledExceptionMessage);
        }).always(function() {
            self.IsPageLoaded(true);
            self.IsAjaxComplete(true);
        });
    };

    self.SendCommand = function(command, onSuccess) {
        self.SendRequest(commandExecutionUrl, command, function(data) {
            if (data.IsSuccess) {
                if (!Supervisor.Framework.Objects.isUndefined(data.DomainException) && data.DomainException != null) {
                    self.ShowError(data.DomainException);
                } else if (!Supervisor.Framework.Objects.isUndefined(onSuccess)) {
                    onSuccess(data);
                }
            } else {
                self.ShowError(input.settings.messages.unhandledExceptionMessage);
            }
        });
    };

    self.SendCommands = function(commands, onSuccess) {
        self.SendRequest(commandExecutionUrl, commands, function(data) {
            var failedCommands = ko.utils.arrayFilter(data.CommandStatuses, function(cmd) {
                return !cmd.IsSuccess;
            });

            if (failedCommands.length > 0) {
                var failedDomainExceptions = ko.utils.arrayMap(failedCommands, function(failedCommand) {
                    return failedCommand.DomainException;
                });
                self.ShowErrors(failedDomainExceptions);
            } else {
                if (!Supervisor.Framework.Objects.isUndefined(onSuccess)) {
                    onSuccess();
                }
            }
        });
    };

    self.load = function() {
    };
};