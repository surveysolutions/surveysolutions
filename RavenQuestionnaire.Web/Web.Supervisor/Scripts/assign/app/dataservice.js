define('app/dataservice', ['amplify', 'input','knockout'], function (amplify, input, ko) {

    var host = input.url,
        init = function () {
        amplify.request.define('sendAssingmentData', 'ajax', {
            url: host,
            dataType: 'json',
            type: "post",
            contentType: 'application/json'
        });
        amplify.request.define('sendCommand', 'ajax', {
            url: input.commandExecutionUrl,
            dataType: 'json',
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            decoder: function (data, status, xhr, success, error) {
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
        sendAssingmentData = function (callback, data) {
            return amplify.request({
                resourceId: 'sendAssingmentData',
                data: ko.toJSON(data),
                success: callback.success,
                error: callback.error
            });
        },
        sendCommand = function (callbacks, commandJSON) {
            return amplify.request({
                resourceId: 'sendCommand',
                data: ko.toJSON(commandJSON),
                success: callbacks.success,
                error: callbacks.error
            });
        };
    init();
    return {
        sendAssingmentData: sendAssingmentData,
        sendCommand: sendCommand
    };
});