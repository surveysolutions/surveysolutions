Supervisor.VM.BasePage = function (commandExecutionUrl) {

    var self = this;

    self.IsPageLoaded = ko.observable(false);
    self.IsAjaxComplete = ko.observable(true);
    self.IsFilterOpen = ko.observable(true);
    self.Errors = ko.observableArray([]);
    self.QueryString = location.queryString;
    self.IsNotifyVisible = ko.observable(false);
    self.NotifyTitle = ko.observable(input.settings.messages.notifyDialogTitle);
    self.NotifyText = ko.observable(input.settings.messages.notifyDialogText);

    self.ToggleFilter = function() {
        if (self.IsFilterOpen()) {
            $('#wrapper').addClass('menu-hidden');
        } else {
            $('#wrapper').removeClass('menu-hidden');
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
        } else {
            setTimeout(function() {
                if (!self.IsAjaxComplete()) {
                    $('#umbrella').show();
                }
            }, 500);
        }
    });

    var notifyHandler;
    self.ShowNotification = function() {
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
            self.ShowNotification();
        }
    };

    self.SendRequest = function (requestUrl, args, onSuccess) {
        if (!self.IsAjaxComplete()) {
            self.CheckForRequestComplete();
            return;
        }

        self.IsAjaxComplete(false);
        $.post(requestUrl, args, null, "json").done(function (data) {
            if (!Supervisor.Framework.Objects.isUndefined(onSuccess)) {
                onSuccess(data);
            }
        }).fail(function () {
            self.ShowError(input.settings.messages.unhandledExceptionMessage);
        }).always(function () {
            self.IsPageLoaded(true);
            self.IsAjaxComplete(true);
        });
    };

    self.SendCommand = function(command, onSuccess) {
        self.SendRequest(commandExecutionUrl, command, function(data) {
            if (data.IsSuccess) {
                if (!Supervisor.Framework.Objects.isUndefined(onSuccess)) {
                    if (!Supervisor.Framework.Objects.isUndefined(data.DomainException)) {
                        self.ShowError(data.DomainException);
                    } else {
                        onSuccess();
                    }
                }
            } else {
                self.ShowError(input.settings.messages.unhandledExceptionMessage);
            }
        });
    };

    self.SendCommands = function (commands, onSuccess) {
        self.SendRequest(commandExecutionUrl, commands, function (data) {
            var failedCommands = ko.utils.arrayFilter(data.CommandStatuses, function (cmd) {
                return !cmd.IsSuccess;
            });

            var failedCommandIds = ko.utils.arrayMap(failedCommands, function (failedCommand) {
                return failedCommand.CommandId;
            });

            if (failedCommandIds.length > 0) {
                var failedDomainExceptions = ko.utils.arrayMap(failedCommands, function(failedCommand) {
                    return failedCommand.DomainException;
                });
                self.ShowErrors(failedDomainExceptions);
            } else {
                if (!Supervisor.Framework.Objects.isUndefined(onSuccess)) {
                    onSuccess(failedCommandIds);
                }
            }
        });
    };
    
    self.load = function() {
    };
}