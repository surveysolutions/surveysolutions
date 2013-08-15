define('model.questionnaire',
    ['ko', 'config', 'model.sharePerson', 'input'],
    function(ko, config, sharePerson, input) {
        var _dc = null,
            Questionnaire = function() {
                var self = this;
                self.id = ko.observable();
                self.title = ko.observable();
                self.childrenID = ko.observableArray();
                self.getHref = function () {
                    return config.hashes.detailsQuestionnaire + "/" + self.id();
                };

                self.canUpdate = ko.observable(true);
                self.isPublic = ko.observable(false);
                self.isSelected = ko.observable();
                self.isNullo = false;
                self.canUpdate = ko.observable(true);
                self.dirtyFlag = new ko.DirtyFlag([self.title, self.isPublic]);
                self.dirtyFlag().reset();

                self.errors = ko.validation.group(self);

                self.addSharedPerson = function() {
                    self.sharePersons.push(new sharePerson().userEmail(self.currentUserForSharing().userEmail()));
                    self.currentUserForSharing(new sharePerson());
                    $('#currentUserEmail').focus();
                };

                self.removeSharedPerson = function (person) {
                    self.sharePersons.remove(person);
                };

                self.currentUserForSharing = ko.observable(new sharePerson());
                self.sharePersons = ko.observableArray([]);
                input.sharedPersons.forEach(function (entry) {
                    self.sharePersons.push((new sharePerson().userEmail(entry)));
                });

                return self;
            };

        Questionnaire.Nullo = new Questionnaire().id(0).title('').isPublic(false);
        Questionnaire.Nullo.isNullo = true;
        Questionnaire.Nullo.dirtyFlag().reset();


        Questionnaire.datacontext = function(dc) {
            if (dc) {
                _dc = dc;
            }
            return _dc;
        };

        Questionnaire.prototype = function() {
            var dc = Questionnaire.datacontext,
                children = function() {
                };
            return {
                isNullo: false,
                children: children
            };
        }();

        return Questionnaire;
    });