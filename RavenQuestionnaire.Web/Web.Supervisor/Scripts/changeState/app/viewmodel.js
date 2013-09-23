define('app/viewmodel', ['amplify', 'input', 'knockout'], function (amplify, input, ko) {
    var host = input.url,
        errors = ko.observableArray(),
        init = function() {

            amplify.request.define('sendCommand', 'ajax', {
                url: input.commandExecutionUrl,
                dataType: 'json',
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                decoder: function(data, status, xhr, success, error) {
                    if (xhr.status == 500) {
                        error({ error: "Unexpected error occured. Try to refresh page to continue. If this problem persists, please contact support." }, status);
                    } else if (status === "success") {
                        var result = JSON.parse(xhr.responseText);
                        if (result.error == null) {
                            success(data, status);
                        } else {
                            error(result, status);
                        }
                    } else {
                        error(status, xhr);
                    }
                }
            });
        },
        showErrors = function (response) {
            errors.removeAll();
            errors.push({
                error: response.error
            });
            $('body').addClass('output-visible');
        },
         hideOutput = function () {
             $('body').removeClass('output-visible');
         },
        sendCommand = function(callbacks, command) {
            return amplify.request({
                resourceId: 'sendCommand',
                data: ko.toJSON(command),
                success: callbacks.success,
                error: callbacks.error
            });
        },
        savingMessage = ko.observable('Loading, please wait'),
        isSaving = ko.observable(false),
        comment = ko.observable(''),
        approve = function() {
            savingMessage('Approving this interview, please wait');
            changeState("ApproveInterviewCommand", comment().trim(), "approved");
        },
        reject = function() {
            savingMessage('Rejecting this interview, please wait');
            changeState("RejectInterviewCommand", comment().trim(), 'rejected');
        },
        changeState = function(commandName, comment, status) {
            var command = {
                type: commandName,
                command: ko.toJSON({
                    interviewId: input.interviewId,
                    userId: input.currentUser.UserId,
                    comment: comment
                })
            };
            isSaving(true);
            sendCommand({
                success: function(response) {
                    var backUrl = input.backUrl;
                    backUrl = backUrl.replace("_______", status);
                    window.location = backUrl;
                    isSaving(false);
                },
                error: function(response) {
                    showErrors(response);
                    isSaving(false);
                }
            }, command);
        };
    init();
    return {
        comment: comment,
        approve : approve,
        reject: reject,
        isSaving: isSaving,
        savingMessage : savingMessage,
        init: init,
        hideOutput: hideOutput,
        errors: errors
    };
});