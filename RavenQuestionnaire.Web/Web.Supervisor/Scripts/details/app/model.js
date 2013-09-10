define('app/model', ['knockout', 'knockout.validation'],
function (ko) {
    var QuestionModel = function () {
        var self = this;
        self.uiId = ko.observable();
        self.id = ko.observable();
        self.isCapital = ko.observable();
        self.comments = ko.observableArray();
        self.isEnabled = ko.observable();
        self.isFeatured = ko.observable();
        self.isFlagged = ko.observable();
        self.isMandatory = ko.observable();
        self.propagationVector = ko.observable();
        self.questionType = ko.observable();
        self.title = ko.observable();
        self.isValid = ko.observable(true);
        self.isVisible = ko.observable(true);
        self.isSelected = ko.observable(false);
        self.isAnswered = ko.observable(false);
        self.validationMessage = ko.observable('');
        self.validationExpression = ko.observable('');
        self.scope = ko.observable();
        self.markerStyle = ko.computed(function () {
            if (self.isValid() == false) {
                return "invalid";
            }
            if (self.scope() == 1) {
                return "supervisor";
            }
            return "";
        });
        self.matchFilter = function (filter) {
            switch (filter) {
                case "all": self.isVisible(true); break;
                case "flaged": self.isVisible(self.isFlagged()); break;
                case "commented": self.isVisible(self.comments.length > 0); break;
                case "answered": self.isVisible(self.isAnswered()); break;
                case "invalid": self.isVisible(self.isValid()==false); break;
                case "supervisor": self.isVisible(self.scope() == 1); break;
                case "enabled": self.isVisible(self.isEnabled()); break;
            }
        };
        return self;
    };
    var model = {
        GpsQuestion: function () {
            var self = this;
            ko.utils.extend(self, new QuestionModel());
            self.lat = ko.observable();
            self.lon = ko.observable();
            return self;
        },
        TextQuestion: function () {
            var self = this;
            ko.utils.extend(self, new QuestionModel());
            self.answer = ko.observable();
            return self;
        },
        NumericQuestion: function () {
            var self = this;
            ko.utils.extend(self, new QuestionModel());
            self.answer = ko.observable();
            return self;
        },
        DateTimeQuestion: function () {
            var self = this;
            ko.utils.extend(self, new QuestionModel());
            self.answer = ko.observable();
            return self;
        },
        SingleOptionQuestion: function () {
            var self = this;
            ko.utils.extend(self, new QuestionModel());
            self.selectedOption = ko.observable();
            self.options = ko.observableArray();
            return self;
        },
        MultyOptionQuestion: function () {
            var self = this;
            ko.utils.extend(self, new QuestionModel());
            self.selectedOptions = ko.observableArray();
            self.options = ko.observableArray();
            return self;
        },
        Group: function () {
            var self = this;
            
            self.uiId = ko.observable();
            self.id = ko.observable();
            self.depth = ko.observable();
            self.css = ko.computed(function () {
                return "level" + self.depth();
            });
            self.href = ko.computed(function () {
                return "#group/" + self.uiId();
            });
            self.title = ko.observable();
            self.parentId = ko.observable();
            self.propagationVector = ko.observable();
            self.questions = ko.observableArray();
            self.isVisible = ko.observable(true);
            self.visibleQuestionsCount = ko.computed(function() {
                return _.reduce(self.questions(), function (count, question) {
                    return count + (question.isVisible() ? 1 : 0);
                }, 0);
            });
            return self;
        },
        Interview: function () {
            var self = this;
            self.id = ko.observable();
            self.title = ko.observable();
            self.questionnaireId = ko.observable();
            self.status = ko.observable();
            self.responsible = ko.observable();
            return self;
        },
        User: function () {
            var self = this;
            self.id = ko.observable();
            self.name = ko.observable();
            return self;
        }
    };
    return model;
});