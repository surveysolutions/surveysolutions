define('model.questionnaire',
    ['ko', 'config', 'model.sharePerson'],
    function(ko, config, sharePerson) {
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
                
                self.currentUserName = ko.observable();
                self.sharePersons = ko.observableArray([]);

                self.addSharePerson = function() {
                    var person = new sharePerson().id(Math.uuid()).userName(self.currentUserName());

                    var result = ko.validation.group(person, { deep: true });
                    if (!person.isValid()) {
                        result.showAllMessages(true);
                    } else {
                        self.sharePersons.push(person);
                        self.currentUserName('');
                        $('#currentUserName').focus();
                    }
                };
                self.removeSharePerson = function (person) {
                    self.sharePersons.remove(person);
                };

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
