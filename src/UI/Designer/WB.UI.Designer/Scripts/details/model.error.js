define('model.error',
    ['ko', 'config'],
    function (ko) {
        var _dc = null,
            error = function () {
                var self = this;

                self.message = ko.observable();
                self.references = ko.observableArray();
                
                return self;
            };

        error.datacontext = function (dc) {
            if (dc) {
                _dc = dc;
            }
            return _dc;
        };

        return error;
    });
