define('model.question',
    ['ko','config', 'model.answerOption'],
    function (ko, config, answerOption) {
        
        var _dc = null,
          Question = function () {
              var self = this;
              self.id = ko.observable();
              self.title = ko.observable();
              self.alias = ko.observable();
              self.type = ko.observable();
              self.isHead = ko.observable();
              self.isFeatured = ko.observable();
              self.isMandatory = ko.observable();
              self.scope = ko.observable();
              self.condition = ko.observable();
              self.validationExpression = ko.observable();
              self.validationMessage = ko.observable();
              self.instruction = ko.observable();
            
              self.answerOrder = ko.observable();
              self.answerOptions = ko.observableArray();
              self.cards = ko.observableArray();
            
              self.maxValue = ko.observable();
              self.triggers = ko.observableArray();

              // UI stuff
              self.addAnswer = function() {
                  var answer = new answerOption().id('id').title(self.currentAnswerTitle()).value(self.currentAnswerValue());
                  self.answerOptions.push(answer);
                  self.currentAnswerTitle('');
                  self.currentAnswerValue('');
              };
              self.removeAnswer = function(answer) {
                  self.answerOptions.remove(answer);
              };
              self.currentAnswerValue  = ko.observable();
              self.currentAnswerTitle = ko.observable();
              
              self.typeOptions = config.questionTypes;
              self.scopeOptions = config.questionScopes;
              self.orderOptions = config.answerOrders;
            
              self.getHref = function () {
                  return config.hashes.detailsQuestion + "/" + self.id();
              };
              self.template = "QuestionView";
              self.isNullo = false;
              self.dirtyFlag = new ko.DirtyFlag([self.title, self.type]);

              return self;
          };

        Question.Nullo = new Question().id(0).title('');
        Question.Nullo.isNullo = true;
        Question.Nullo.dirtyFlag().reset();


        Question.datacontext = function (dc) {
            if (dc) {
                _dc = dc;
            }
            return _dc;
        };

        Question.prototype = function () {
            var dc = Question.datacontext,
                children = function () {
                };
            return {
                isNullo: false,
                children: children
            };
        }();

        return Question;
});
