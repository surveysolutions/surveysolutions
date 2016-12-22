﻿Supervisor.VM.BasePage = function(commandExecutionUrl) {

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

    self.HideAllAlerts = function() {
        $("#alerts").empty();
    }

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

    self.SendRequest = function(requestUrl, args, onSuccess, skipInProgressCheck, allowGet, onDone) {

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
        }).fail(function (jqXhr, textStatus, errorThrown) {
            if (jqXhr.status === 403) {
                if ((!jqXhr.responseText || 0 === jqXhr.responseText.length)) {
                    self.ShowError(input.settings.messages.forbiddenMessage);
                }
                else {
                    var isJson = true;
                    var json;
                    try {
                        json = $.parseJSON(jqXhr.responseText);
                    }
                    catch (err) {
                        isJson = false;
                    }

                    if (isJson) {
                        self.ShowError(json.Message);
                    } else {
                        self.ShowError(jqXhr.responseText);
                    }
                }
            } else {
                self.ShowError(input.settings.messages.unhandledExceptionMessage);
            }
        }).always(function() {
            self.IsPageLoaded(true);
            self.IsAjaxComplete(true);
            if (!_.isUndefined(onDone)) {
                onDone();
            }
        });
    };

    self.SendRequestWithFiles = function (requestUrl, args, onSuccess, onFail, onDone) {

        if (!self.IsAjaxComplete()) {
            self.CheckForRequestComplete();
            return;
        }

        self.IsAjaxComplete(false);

        var requestHeaders = {};
        requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;

        var formData = new FormData();
        for (var argumentName in args) {
            formData.append(argumentName, args[argumentName]);
        }

        $.ajax({
            url: requestUrl,
            type: 'post',
            data: formData,
            headers: requestHeaders,
            cache: false,
            contentType: false,
            processData: false,
        }).done(function (data) {
            if (!Supervisor.Framework.Objects.isUndefined(onSuccess)) {
                onSuccess(data);
            }
        }).fail(function (jqXhr, textStatus, errorThrown) {
            if (!_.isUndefined(onFail)) {
                onFail(jqXhr);
                return;
            }
            if (jqXhr.status === 403) {
                if ((!jqXhr.responseText || 0 === jqXhr.responseText.length)) {
                    self.ShowError(input.settings.messages.forbiddenMessage);
                }
                else {
                    self.ShowError(jqXhr.responseText);
                }
            } else {
                self.ShowError(input.settings.messages.unhandledExceptionMessage);
            }
        }).always(function () {
            self.IsPageLoaded(true);
            self.IsAjaxComplete(true);
            if (!_.isUndefined(onDone)) {
                onDone();
            }
        });
    };

    self.SendCommand = function(command, onSuccess, onDone) {
        self.SendRequest(commandExecutionUrl, command, function (data) {
            self.HideAllAlerts();
            if (data.IsSuccess) {
                if (!Supervisor.Framework.Objects.isUndefined(onSuccess))
                  onSuccess(data);
            } else {
                if (!Supervisor.Framework.Objects.isUndefined(data.DomainException) && data.DomainException != null) {
                    self.ShowError(data.DomainException);
                }
                else
                    self.ShowError(input.settings.messages.unhandledExceptionMessage);
            }
        }, false, false, onDone);
    };

    self.SendCommands = function (commands, onSuccess, skipInProgressCheck) {
        self.SendRequest(commandExecutionUrl, commands, function (data) {
            self.HideAllAlerts();
            var failedCommands = ko.utils.arrayFilter(data.CommandStatuses, function(cmd) {
                return !cmd.IsSuccess;
            });

            if (failedCommands.length > 0) {
                var failedDomainExceptions = ko.utils.arrayMap(failedCommands, function(failedCommand) {
                    if (!Supervisor.Framework.Objects.isUndefined(failedCommand.DomainException) && failedCommand.DomainException != null)
                        return failedCommand.DomainException;
                    else {
                        return input.settings.messages.unhandledExceptionMessage;
                    }
                });
                self.ShowErrors(failedDomainExceptions);
            } else {
                if (!Supervisor.Framework.Objects.isUndefined(onSuccess)) {
                    onSuccess();
                }
            }
        }, skipInProgressCheck);
    };

    self.load = function() {
    };

    self.getBindedHtmlTemplate = function (templateId, bindObject) {
        var messageTemplate = $("<div/>").html($(templateId).html())[0];
        ko.applyBindings(bindObject, messageTemplate);
        var html = $(messageTemplate).html();
        return html;
    }

    self.CreateUsersViewModel = function (usersToAssignUrl) {
        var users = {};
        users.IsAssignToLoading = ko.observable(false);
        users.UsersToAssignUrl = usersToAssignUrl;
        users.LoadUsers = function (query, sync, pageSize) {
            users.IsAssignToLoading(true);
            self.SendRequest(users.UsersToAssignUrl, { query: query, pageSize: pageSize }, function (response) {
                sync(response.Users, response.TotalCountByQuery);
            }, true, true, function () {
                users.IsAssignToLoading(false);
            });
        }
        users.StoreInteviewer = function() {};
        users.ClearAssignTo = function () {
            users.AssignTo(undefined);
        }
        users.AssignTo = ko.observable();
        return users;
    }
};