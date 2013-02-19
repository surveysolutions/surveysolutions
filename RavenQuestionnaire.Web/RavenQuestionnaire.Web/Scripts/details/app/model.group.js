define('model.group',
    ['ko'],
    function (ko) {
        var Group = function () {
            var self = this;
            self.id = ko.observable();
            self.title = ko.observable();
            self.isNullo = false;
            return self;
        };

        Group.Nullo = new Group().id(0).title('Not a room');
        Group.Nullo.isNullo = true;

        return Group;
});
