define('model.sharePerson',
    ['ko', 'config'],
    function(ko) {
        var SharePerson = function () {
            var self = this;

            self.userEmail = ko.observable('').extend({ required: true, email: true, validateUserEmailAsync: self });
            
            self.errors = ko.validation.group(self);
            self.isValidating = function () {
                return self.userEmail.isValidating();
            },
            self.check = function (checkCallback) {
                if (self.isValidating()) {
                    setTimeout(function() {
                        self.check(checkCallback);
                    }, 50);
                    return false;
                }

                if (!self.isValid()) {
                    self.errors.showAllMessages();
                    return false;
                }

                checkCallback(self);
            };
            self.isNullo = false;
            return self;
        };

        return SharePerson;
    });

ko.validation.rules['validateUserEmailAsync'] = {
    async: true,
    validator: function(val, parms, callback) {
        var options = {
            url: '/designer/account/findbyemail',
            type: 'POST',
            success: callback,
            data: { email: val }
        };
        
        $.ajax(options).done(function (data) {
            if (data.isUserExist) {
                callback(true);
            } else {
                callback(false);
            }
        }).fail(function() {
            callback(false); 
        });
    },
    message: "user doesn't exists"
};

ko.validation.registerExtenders();
