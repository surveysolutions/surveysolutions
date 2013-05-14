define('model.question',
    ['ko', 'config', 'model.answerOption'],
    function (ko, config, answerOption) {

        var _dc = null,
          Question = function () {
              var self = this;
              self.id = ko.observable(Math.uuid());
              self.isNew = ko.observable(true);
              self.isClone = ko.observable(false);
              
              
              self.title = ko.observable('New Question').extend({ required: true });
              self.parent = ko.observable();
              self.alias = ko.observable('').extend({
                  required: true, maxLength: 32,
                  pattern: {
                      message: "Valid variable name should contains only letters, digits and underscore character and shouldn't starts with digit",
                      params: '^[_A-Za-z][_A-Za-z0-9]*$'
                  }
              });
              
              self.type = ko.observable("QuestionView"); // Object type
              self.template = "QuestionView"; // tempate id in html file

              self.qtype = ko.observable("Text"); // Questoin type
              self.isHead = ko.observable(false);
              self.isFeatured = ko.observable(false);
              self.isMandatory = ko.observable(false);
              self.scope = ko.observable();
              self.condition = ko.observable('');
              self.validationExpression = ko.observable('');
              self.validationMessage = ko.observable('');
              self.instruction = ko.observable('');

              self.answerOrder = ko.observable();
              self.answerOptions = ko.observableArray([]);
              self.cards = ko.observableArray([]);

              self.maxValue = ko.observable();
              self.triggers = ko.observableArray([]);

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
                  var answer = new answerOption().id(Math.uuid()).title(self.currentAnswerTitle()).value(self.currentAnswerValue());
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

              self.typeOptions = _.keys(config.questionTypes);
              self.scopeOptions = config.questionScopes;
              self.orderOptions = config.answerOrders;

              self.getHref = function () {
                  return config.hashes.detailsQuestion + "/" + self.id();
              };
              self.isSelected = ko.observable();
              self.isNullo = false;
              self.cloneSource = ko.observable();

              self.dirtyFlag = new ko.DirtyFlag([self.title, self.alias, self.qtype, self.isHead, self.isFeatured, self.isMandatory, self.scope, self.condition, self.validationExpression, self.validationMessage, self.instruction, self.answerOrder, self.answerOptions, self.maxValue, self.triggers]);
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
                children = function() {
                },
                clone = function () {
                    var item = new Question();
                    item.title(this.title());
                    item.qtype(this.qtype());
                    item.scope(this.scope());
                    item.answerOrder(this.answerOrder());

                    item.answerOptions(_.map(this.answerOptions(), function (answer) {
                        return new model.AnswerOption().id(answer.id()).title(answer.title()).value(answer.value());
                    }));
                    
                    item.triggers(_.map(this.triggers(), function (trigger) {
                        return { key: trigger.key, value: trigger.value };
                    }));

                    item.isHead(this.isHead());
                    item.isFeatured(this.isFeatured());
                    item.isMandatory(this.isMandatory());
                    item.condition(this.condition());
                    item.instruction(this.instruction());
                    item.maxValue(this.maxValue());

                    item.validationExpression(this.validationExpression());
                    item.validationMessage(this.validationMessage());

                    item.parent(this.parent());
                    item.id(Math.uuid());
                    item.isNew(true);
                    item.isClone(true);
                    item.cloneSource(this);
                    item.dirtyFlag().reset();
                    
                    item.alias('');
                    item.alias.valueHasMutated();

                    return item;
                };
            return {
                isNullo: false,
                children: children,
                clone : clone
            };
        }();

        return Question;
    });