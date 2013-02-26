define('model.menuItem',
    ['ko', 'config'],
    function (ko, config) {
        var _dc = null,
            MenuItem = function() {
                var self = this;
                self.id = ko.observable();
                self.title = ko.observable();
                self.level = ko.observable();
                self.isSelected = ko.observable(false);
                self.getHref = function () {
                    return config.hashes.detailsGroup + "/" + self.id();
                };
                self.isNullo = false;
                self.dirtyFlag = new ko.DirtyFlag([self.title, self.level]);
                return self;
            };

        MenuItem.Nullo = new MenuItem().id(0).title('').level(0);
        MenuItem.Nullo.isNullo = true;
        MenuItem.Nullo.dirtyFlag().reset();


        MenuItem.datacontext = function (dc) {
            if (dc) { _dc = dc; }
            return _dc;
        };
        
        MenuItem.prototype = function () {
            var
                dc = MenuItem.datacontext,
                group = function () {
                    return dc().groups.getLocalById(self.id());
                };
            return {
                isNullo: false,
                group: group
            };
        }();

        return MenuItem;
});
