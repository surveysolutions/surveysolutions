define('dataprimer',
    ['ko', 'datacontext', 'config'],
    function (ko, datacontext, config) {

        var fetch = function () {
                
                return $.Deferred(function (def) {
                    $.when()
                        .pipe(function() { })
                        .fail(function() { def.reject(); })
                        .done(function() { def.resolve(); });

                }).promise();
            };

        return {
            fetch: fetch
        };
    });