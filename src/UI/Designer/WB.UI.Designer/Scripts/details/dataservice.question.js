define('dataservice.question',
    ['amplify'],
    function (amplify) {
        var
            init = function () {

                amplify.request.define('question', 'ajax', {
                    url: '/api/persons/{id}',
                    dataType: 'json',
                    type: 'GET'
                    //cache:
                });

                amplify.request.define('questionUpdate', 'ajax', {
                    url: '/api/persons',
                    dataType: 'json',
                    type: 'PUT',
                    contentType: 'application/json; charset=utf-8'
                });
            },

            getQuestion = function (callbacks, id) {
                return amplify.request({
                    resourceId: 'question',
                    data: { id: id },
                    success: callbacks.success,
                    error: callbacks.error
                });
            };

            updateQuestion = function (callbacks, data) {
                return amplify.request({
                    resourceId: 'questionUpdate',
                    data: data,
                    success: callbacks.success,
                    error: callbacks.error
                });
            };

        init();
  
    return {
        getQuestion: getQuestion,
        updateQuestion: updateQuestion
    };
});


