define('dataservice', ['amplify'],
    function (amplify) {
        var init = function () {

            amplify.request.define('updateGroup', 'ajax', {
                url: '/Command/Execute',
                dataType: 'json',
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                decoder: function (data, status, xhr, success, error) {
                    if (status === "success") {
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
                resourceId: 'updateGroup',
                data: ko.toJSON({ command: commandJSON }),
                success: callbacks.success,
                error: callbacks.error
            });
        };

        init();

        return {
            sendCommand: sendCommand
        };
    });