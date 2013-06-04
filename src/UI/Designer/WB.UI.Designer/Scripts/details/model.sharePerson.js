define('model.sharePerson',
    ['ko','config'],
    function (ko) {
        var SharePerson = function () {
            var self = this;

            self.id = ko.observable(Math.uuid());
            self.userName = ko.observable().extend({ required: true });
            
            self.isNullo = false;
            return self;
        };

        SharePerson.Nullo = new SharePerson().id(0).userName('');
        SharePerson.Nullo.isNullo = true;

        return SharePerson;
});
