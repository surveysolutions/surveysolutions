define('dataprimer',
    ['ko', 'datacontext', 'config'],
    function (ko, datacontext, config) {

        var logger = config.logger,
            
            fetch = function () {
                
                return $.Deferred(function (def) {

                    var data = {
                    };

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