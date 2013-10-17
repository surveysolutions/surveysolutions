Model = function () {
    var model = {
        User: function () {
            var self = this;
            self.id = ko.observable();
            self.name = ko.observable();
            self.email = ko.observable();
            return self;
        },
        Question: function () {
            var self = this;
            self.title = ko.observable();
            self.id = ko.observable();
            self.type = ko.observable();
            self.variable = ko.observable();
            self.options = ko.observableArray([]);
            self.instructions = ko.observable();
            self.selectedOption = ko.observable();
            self.selectedOptions = ko.observableArray([]);
            self.settings = ko.observableArray([]);
            self.isSingleOption = ko.computed(function () {
                return self.type() === Supervisor.Config.QuestionType.SingleOption;
            });
            self.isMultyOption = ko.computed(function () {
                return self.type() === Supervisor.Config.QuestionType.MultyOption;
            });
            self.hasOptions = ko.computed(function () {
                return self.isSingleOption() || self.isMultyOption();
            });
            self.errors = ko.validation.group(self);

            return self;
        },
        Option: function () {
            var self = this;
            self.id = ko.observable();
            self.questionId = ko.observable();
            self.title = ko.observable();
            self.value = ko.observable();
            self.isSelected = ko.observable(false);
            self.optionFor = ko.computed(function () {
                return "optionFor" + "_" + self.questionId() + "_" + self.id();
            });
            return self;
        }
    };
    return model;
};