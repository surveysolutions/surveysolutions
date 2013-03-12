define('dataservice.group',
    ['amplify'],
    function(amplify) {
        var init = function() {

            amplify.request.define('group', 'ajax', {
                url: '/api/persons/{id}',
                dataType: 'json',
                type: 'GET'
                //cache:
            });

            amplify.request.define('updateGroup', 'ajax', {
                url: '/Command/Execute',
                dataType: 'json',
                type: 'POST',
                contentType: 'application/json; charset=utf-8'
            });
        },
            getGroup = function(callbacks, id) {
                return amplify.request({
                    resourceId: 'group',
                    data: { id: id },
                    success: callbacks.success,
                    error: callbacks.error
                });
            };

        updateGroup = function (callbacks, data) {
            var command =  { text: "text" };

            return amplify.request({
                resourceId: 'updateGroup',
                data: ko.toJSON( { command:  ko.toJSON(command) }),
                success: callbacks.success,
                error: callbacks.error
            });
        };

        init();

        return {
            getGroup: getGroup,
            updateGroup: updateGroup
        };
    });


