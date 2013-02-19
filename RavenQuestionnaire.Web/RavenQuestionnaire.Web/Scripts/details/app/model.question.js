define('model.question',
    ['ko'],
    function (ko) {
        var Question = function () {
            var self = this;
            self.id = ko.observable();
            self.title = ko.observable();
            self.isNullo = false;
            return self;
        };

        Question.Nullo = new Question().id(0).title('');
        Question.Nullo.isNullo = true;

        return Question;
});
