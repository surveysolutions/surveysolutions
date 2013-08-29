define('route-mediator',
[ 'config'],
    function (config) {
        var
            canleaveCallback,
            self = this,

            viewModelActivated = function (options) {
                canleaveCallback = options && options.canleaveCallback;
            },

            canLeave = function () {
                // Check the active view model to see if we can leave it
                var
                    val = canleaveCallback ? canleaveCallback() : true,
                    response = { val: val };
                return response;
            },

            init = function () {
            };

        init();

        return {
            canLeave: canLeave
        };
    });
