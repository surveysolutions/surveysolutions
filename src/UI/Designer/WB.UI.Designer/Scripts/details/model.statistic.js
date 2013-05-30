define('model.statistic',
    ['ko', 'config'],
    function (ko) {
        var _dc = null,
            Statistic = function () {
                var self = this;

                self.questions = ko.observable();
                self.groups = ko.observable();
                self.unsavedQuestion = ko.observable();
                self.unsavedGroups = ko.observable();
                self.hasUnsaved = ko.computed(function () {
                    return (self.unsavedQuestion() + self.unsavedGroups()) > 0;
                });
                self.unsavedWarningMessage = ko.computed(function() {
                    var message = "You have ";
                    if (self.unsavedQuestion() > 0) {
                        message += self.unsavedQuestion() + " unsaved" + (self.unsavedQuestion() > 1 ? " questions" : " question");
                    }
                    if (self.unsavedQuestion() > 0 && self.unsavedGroups() > 0) {
                        message += " and ";
                    }
                    if (self.unsavedGroups() > 0) {
                        message += self.unsavedGroups() + " unsaved " + (self.unsavedGroups() > 1 ? "groups" : "group");
                    }
                    return message;
                });

                return self;
            };

        Statistic.datacontext = function (dc) {
            if (dc) {
                _dc = dc;
            }
            return _dc;
        };

        Statistic.prototype = function () {
            var dc = Statistic.datacontext;

            return {
                isNullo: false
            };
        }();

        return Statistic;
    });
