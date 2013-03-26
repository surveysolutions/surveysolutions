define('dataservice', ['amplify', 'input'],
    function (amplify, input) {
        var init = function () {
            amplify.request.define('updateGroup', 'ajax', {
                url: input.commandExecutionUrl,
                dataType: 'json',
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                decoder: function (data, status, xhr, success, error) {
                    if (xhr.status == 500) {
                        error({error : "Unexpected error occured. Try to refresh page to continue. If this problem persists, plese contact support."}, status);
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
        };

        sendCommand = function (callbacks, commandJSON) {
            return amplify.request({
                resourceId: 'updateGroup',
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