define('dataservice', ['amplify'],
    function (amplify) {
        var init = function () {

            amplify.request.define('updateGroup', 'ajax', {
                url: '/Command/Execute',
                dataType: 'json',
                type: 'POST',
                contentType: 'application/json; charset=utf-8'
            });
        },
             getGroup = function (callbacks, id) {
                 return amplify.request({
                     resourceId: 'group',
                     data: { id: id },
                     success: callbacks.success,
                     error: callbacks.error
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