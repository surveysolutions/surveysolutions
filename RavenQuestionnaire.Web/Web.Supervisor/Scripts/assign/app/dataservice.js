define('app/dataservice', ['amplify', 'input','knockout'], function (amplify, input, ko) {

    var host = input.url,
        init = function () {
        amplify.request.define('sendAssingmentData', 'ajax', {
            url: host,
            dataType: 'json',
            type: "post",
            contentType: 'application/json'
        });
    },
        sendAssingmentData = function (callback, data) {
            return amplify.request({
                resourceId: 'sendAssingmentData',
                data: ko.toJSON(data),
                success: callback.success,
                error: callback.error
            });
        };
    init();
    return {
        sendAssingmentData: sendAssingmentData
    };
});