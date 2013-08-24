define('dataservice', ['amplify', 'input'],
    function (amplify, input) {
        var init = function () {
            amplify.request.define('sendCommand', 'ajax', {
                url: input.commandExecutionUrl,
                dataType: 'json',
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                decoder: function (data, status, xhr, success, error) {
                    if (data == null) {
                        error({ error: input.settings.messages.unhandledExceptionMessage }, status);
                    } else {
                        if (data.IsSuccess) {
                            success(data, status);
                        } else {
                            if (!data.HasPermissions) {
                                window.location.href = input.url.lackOfPermitsUrl;
                            } else {
                                error(data, status);
                            }
                            
                        }
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