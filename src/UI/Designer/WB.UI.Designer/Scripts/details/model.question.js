define('model.question',
    ['ko', 'config', 'model.answerOption'],
    function (ko, config, answerOption) {

        var _dc = null,
          Question = function () {
              var self = this;
              self.id = ko.observable(Math.uuid());
              self.isNew = ko.observable(true);
              
              self.title = ko.observable('New Question').extend({ minLength: 3, require: true });
              self.parent = ko.observable();
              self.alias = ko.observable(Math.uuid(5)).extend({
                  require: true, maxLength: 32,
                  pattern: {
                      message: "Valid variable name should contains only letters, digits and underscore character and shouldn't starts with digit",
                      params: '^[_A-Za-z][_A-Za-z0-9]*$'
                  }
              });
              
              self.type = ko.observable("QuestionView"); // Object type
              self.template = "QuestionView"; // tempate id in html file

              self.qtype = ko.observable("Text"); // Questoin type
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
              self.currentTrigger = ko.observable();
              self.localPropagatedGroups = ko.observableArray();
              self.propagatedGroups = ko.computed(function () {
                  return _.filter(self.localPropagatedGroups(), function (item) {
                      var trigger = _.find(self.triggers(), function (t) { return t.key == item.id(); });
                      if (!_.isUndefined(trigger)) {
                          return false;
                      }
                      return true;
                  }).map(function (item) {
                      return { key: item.id(), value: item.title() };
                  });
              }).extend({ throttle: 500 });
              self.hasPropagatedGroups = ko.computed(function () {
                  return self.propagatedGroups().length != 0;
              });
              self.addAnswer = function () {
                  var answer = new answerOption().id('id').title(self.currentAnswerTitle()).value(self.currentAnswerValue());
                  self.answerOptions.push(answer);
                  self.currentAnswerTitle('');
                  self.currentAnswerValue('');
                  $('#currentAnswerValue').focus();
              };
              self.removeAnswer = function (answer) {
                  self.answerOptions.remove(answer);
              };

              self.addTrigger = function () {
                  self.triggers.push(self.currentTrigger());
              };
              self.removeTrigger = function (trigger) {
                  self.triggers.remove(trigger);
                  //self.triggers.valueHasMutated();
              };

              self.currentAnswerValue = ko.observable();
              self.currentAnswerTitle = ko.observable();

              self.typeOptions = config.questionTypes;
              self.scopeOptions = config.questionScopes;
              self.orderOptions = config.answerOrders;

              self.getHref = function () {
                  return config.hashes.detailsQuestion + "/" + self.id();
              };
              self.isSelected = ko.observable();
              self.isNullo = false;

              self.dirtyFlag = new ko.DirtyFlag([self.title, self.type]);
              self.dirtyFlag().reset();
              self.errors = ko.validation.group(self);
              
              return self;
          };

        Question.Nullo = new Question().id(0).title('Title').type('QuestionView');
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
