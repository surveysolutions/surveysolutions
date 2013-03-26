define('dataservice.questionnaire',
    ['amplify'],
    function(amplify) {
        var init = function() {

            amplify.request.define('questionnaire', 'ajax', {
                url: '/designer/api/questionnaire/{id}',
                dataType: 'json',
                type: 'GET'
                //cache:
            });

            amplify.request.define('updateQuestionnaire', 'ajax', {
                url: '/designer/api/questionnaire',
                dataType: 'json',
                type: 'PUT',
                contentType: 'application/json; charset=utf-8'
            });
        },
            getQuestionnaire = function (callbacks, id) {
                return amplify.request({
                    resourceId: 'questionnaire',
                    data: { id: id },
                    success: callbacks.success,
                    error: callbacks.error
                });
            };

        updateQuestionnaire = function (callbacks, data) {
            return amplify.request({
                resourceId: 'updateQuestionnaire',
                data: data,
                success: callbacks.success,
                error: callbacks.error
            });
        };

        init();

        return {
            getQuestionnaire: getQuestionnaire,
            updateQuestionnaire: updateQuestionnaire
        };
    });


