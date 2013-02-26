define('model.question',
    ['ko','config'],
    function (ko,config) {
        var Question = function () {
            var self = this;
            self.id = ko.observable();
            self.title = ko.observable();
            self.alias = ko.observable();
            self.type = ko.observable();
            self.typeOptions = config.questionTypes;

            self.answerOrder = ko.observable();
            self.answerOptions = ko.observableArray();
            self.isHead = ko.observable();
            self.isFeatured = ko.observable();
            self.isMandatory = ko.observable();
            self.cards = ko.observableArray();
            self.condition = ko.observable();
            self.instruction = ko.observable();
            self.maxValue = ko.observable();
            self.scope = ko.observable();
            
            self.triggers = ko.observableArray();
            self.validationExpression = ko.observable();
            self.validationMessage = ko.observable();

            // UI stuff
            self.getHref = function () {
                return config.hashes.detailsQuestion + "/" + self.id();
            };
            self.template = "QuestionView";
            self.isNullo = false;
            return self;
        };

        Question.Nullo = new Question().id(0).title('');
        Question.Nullo.isNullo = true;

        return Question;
});
