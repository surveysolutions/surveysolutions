define('model.answerOption',
    ['ko','config'],
    function (ko, config) {
        var AnswerOption = function () {
            var self = this;
            
            self.id = ko.observable(Math.uuid());
            self.title = ko.observable();
            self.value = ko.observable();
            
            self.image = ko.observable();
            
            self.isNullo = false;
            return self;
        };

        AnswerOption.Nullo = new AnswerOption().id(0).title('');
        AnswerOption.Nullo.isNullo = true;

        return AnswerOption;
});
