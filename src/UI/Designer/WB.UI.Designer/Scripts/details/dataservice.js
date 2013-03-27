define('dataservice', ['amplify', 'input'],
    function (amplify, input) {
        var init = function () {
            amplify.request.define('sendCommand', 'ajax', {
                url: input.commandExecutionUrl,
                dataType: 'json',
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                decoder: function (data, status, xhr, success, error) {
                    if (xhr.status == 500) {
                        error({error : "Unexpected error occured. Try to refresh page to continue. If this problem persists, please contact support."}, status);
                    } else if (status === "success") {
                        success(data, status);
                    } else if (status === "fail" || status === "error") {
                        error(JSON.parse(xhr.responseText), status);
                    } else {
                        error(status, xhr);
                    }
                }
            });
        };

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
            sendCommand: sendCommand
        };
    });