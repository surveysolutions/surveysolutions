define('model.questionnaire',
    ['ko', 'config'],
    function(ko, config) {
        var _dc = null,
            Questionnaire = function() {
                var self = this;
                self.id = ko.observable();
                self.title = ko.observable();
                self.childrenID = ko.observableArray();
                
                self.isNullo = false;
                self.dirtyFlag = new ko.DirtyFlag([self.title, self.type]);

                return self;
            };

        Questionnaire.Nullo = new Questionnaire().id(0).title('');
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
