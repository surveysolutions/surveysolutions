define('app/model', ['knockout', 'knockout.validation'],
function (ko) {
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
            self.options = ko.observableArray([]);
            self.instructions = ko.observable();
            self.selectedOption = ko.observable().extend({ required: true });
            self.selectedOptions = ko.observableArray([]);
            self.isSingleOption = ko.computed(function () {
                return self.type() === "SingleOption";
            });
            self.isMultyOption = ko.computed(function () {
                return self.type() === "MultyOption";
            });
            self.hasOptions = ko.computed(function () {
                return self.isSingleOption() || self.isMultyOption();
            });
            return self;
        },
        Option: function () {
            var self = this;
            self.id = ko.observable();
            self.title = ko.observable();
            self.value = ko.observable();
            self.isSelected = ko.observable(false);
            self.optionFor = ko.computed(function () {
                return "optionFor" + "_" + self.id();
            });
            return self;
        }
    };
    return model;
});