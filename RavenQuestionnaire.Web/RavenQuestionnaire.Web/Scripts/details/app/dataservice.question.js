define('dataservice.person',
    ['amplify'],
    function (amplify) {
        var
            init = function () {

                amplify.request.define('speakers', 'ajax', {
                    url: '/api/speakers',
                    dataType: 'json',
                    type: 'GET'
                    //cache: true
                }),

                amplify.request.define('persons', 'ajax', {
                    url: '/api/persons',
                    dataType: 'json',
                    type: 'GET'
                    //cache:
                });

                amplify.request.define('person', 'ajax', {
                    url: '/api/persons/{id}',
                    dataType: 'json',
                    type: 'GET'
                    //cache:
                });

                amplify.request.define('personUpdate', 'ajax', {
                    url: '/api/persons',
                    dataType: 'json',
                    type: 'PUT',
                    contentType: 'application/json; charset=utf-8'
                });
            },

            getSpeakers = function (callbacks) {
                return amplify.request({
                    resourceId: 'speakers',
                    success: callbacks.success,
                    error: callbacks.error
                });
            },

            getPersons = function (callbacks) {
                return amplify.request({
                    resourceId: 'persons',
                    success: callbacks.success,
                    error: callbacks.error
                });
            },

            getPerson = function (callbacks, id) {
                return amplify.request({
                    resourceId: 'person',
                    data: { id: id },
                    success: callbacks.success,
                    error: callbacks.error
                });
            };

            updatePerson = function (callbacks, data) {
                return amplify.request({
                    resourceId: 'personUpdate',
                    data: data,
                    success: callbacks.success,
                    error: callbacks.error
                });
            };

        init();
  
    return {
        getPerson: getPerson,
        getPersons: getPersons,
        getSpeakers: getSpeakers,
        updatePerson: updatePerson
    };
});


