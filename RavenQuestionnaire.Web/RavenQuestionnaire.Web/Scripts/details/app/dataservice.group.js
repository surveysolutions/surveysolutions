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
                url: '/api/persons',
                dataType: 'json',
                type: 'PUT',
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

        updateGroup = function(callbacks, data) {
            return amplify.request({
                resourceId: 'updateGroup',
                data: data,
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


