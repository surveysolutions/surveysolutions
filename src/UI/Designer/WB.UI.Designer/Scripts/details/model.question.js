define('model.question',
    ['ko', 'config', 'utils', 'model.answerOption'],
    function (ko, config, utils, answerOption) {

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
                      message: "Valid variable name should contain only letters, digits and the underscore character and should not start with a digit",
                      params: '^[_A-Za-z][_A-Za-z0-9]*$'
                  },
                  notEqual: 'this'
              });

              self.type = ko.observable("QuestionView"); // Object type
              self.template = "QuestionView"; // tempate id in html file


              self.isHead = ko.observable(false);
              self.isFeatured = ko.observable(false);
              self.isMandatory = ko.observable(false);
              
              self.isSupervisorQuestion = ko.observable();
              
              self.qtype = ko.observable("Text").extend({
                  validation: [{
                      validator: function (val) {
                          if (self.isFeatured() == true && val == "GpsCoordinates") return false;
                          return true;
                      },
                      message: 'Geo Location question cannot be pre-filled'
                  },
                  {
                      validator: function (val) {
                          if (self.isSupervisorQuestion() == true) {
                              switch (val) {
                                  case "Numeric":
                                  case "Text":
                                      return true;
                                  case "SingleOption":
                                  case "MultyOption":;
                                      if (self.isLinked() == 1) {
                                          return false;
                                      }
                                      return true;
                                  case "DateTime":
                                  case "AutoPropagate":
                                  case "GpsCoordinates":
                                      return false;
                              }
                          }
                          return true;
                      },
                      message: 'Date, Auto propagate, Linked categorical and Geo Location questions cannot be filled by supervisor. '
                  }]
              }); // Questoin type
              self.isSupervisorQuestion = ko.observable(false);

              self.scope = ko.computed({
                  read: function () {
                      return this.isSupervisorQuestion() ? config.questionScopes.supervisor : config.questionScopes.interviewer;
                  },
                  write: function (value) {
                      if (_.isUndefined(value) || _.isNull(value)) {
                          this.isSupervisorQuestion(false);
                      }
                      if (value == config.questionScopes.supervisor) {
                          this.isSupervisorQuestion(true);
                          return;
                      }
                      this.isSupervisorQuestion(false);
                  },
                  owner: self
              });

              self.scope(config.questionScopes.interviewer);

              self.condition = ko.observable('').extend({
                  validation: [{
                      validator: function (val) {
                          if (_.isUndefined(val) || _.isNull(val)) {
                              return true;
                          }
                          if (val.indexOf("[this]") != -1) return false;
                          var variable = self.alias();
                          if (_.isUndefined(variable) || _.isNull(variable) || _.isEmpty(variable)) {
                              return true;
                          }
                          variable = "[" + variable + "]";
                          if (val.indexOf(variable) != -1) return false;
                          return true;
                      },
                      message: 'You cannot use self-reference in conditions'
                  }]
              });
              self.validationExpression = ko.observable('');
              self.validationMessage = ko.observable('');
              self.instruction = ko.observable('');
              self.isInteger = ko.observable('');
              self.isLinked = ko.observable(0);
              self.isLinkedDurty = ko.computed(function () {
                  return self.isLinked() == 1;
              });
              self.selectedLinkTo = ko.observable().extend({
                  required: {
                      onlyIf: function () {
                          return self.isLinked() == 1;
                      }
                  }
              });
              self.localQuestionsFromProragatedGroups = ko.observableArray();
              self.questionsFromProragatedGroups = ko.computed(function () {
                  return _.filter(self.localQuestionsFromProragatedGroups(), function (item) {
                      return item.id() != self.id();
                  }).map(function (item) {
                      return { questionId: item.id(), title: item.alias() + ": " + item.title() };
                  });
              });

              self.answerOrder = ko.observable();
              self.answerOptions = ko.observableArray([]).extend({
                  required: {
                      onlyIf: function () {
                          return self.isLinked() == 0 && (self.qtype() === "SingleOption" || self.qtype() === "MultyOption");
                      }
                  },
                  minLength: {
                      params: 2,
                      onlyIf: function () {
                          return self.isLinked() == 0 && (self.qtype() === "SingleOption" || self.qtype() === "MultyOption");
                      }
                  }
              });
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
                  var answer = new answerOption().id(Math.uuid()).title('').value('');

                  answer.errors();

                  self.answerOptions.push(answer);
              };
              self.removeAnswer = function (answer) {
                  self.answerOptions.remove(answer);
              };

              self.addTrigger = function () {
                  self.triggers.push(self.currentTrigger());
              };
              self.removeTrigger = function (trigger) {
                  self.triggers.remove(trigger);
              };

              self.typeOptions = config.questionTypeOptions;
              self.scopeOptions = config.questionScopes;
              self.orderOptions = config.answerOrders;

              self.getHref = function () {
                  return config.hashes.detailsQuestion + "/" + self.id();
              };
              self.isSelected = ko.observable();
              self.isNullo = false;
              self.cloneSource = ko.observable();

              self.dirtyFlag = new ko.DirtyFlag([self.title, self.alias, self.qtype, self.isHead, self.isFeatured, self.isMandatory, self.scope, self.condition, self.validationExpression, self.validationMessage, self.instruction, self.answerOrder, self.answerOptions, self.maxValue, self.triggers, self.selectedLinkTo, self.isLinkedDurty, self.isInteger]);
              self.dirtyFlag().reset();
              self.errors = ko.validation.group(self);
              this.cache = function () { };
              self.canUpdate = ko.observable(true);

              self.isFeatured.subscribe(function (value) {
                  if (value && _.isEmpty(self.condition()) == false) {
                      var weWillClearCondition = config.warnings.weWillClearCondition;
                      bootbox.confirm(weWillClearCondition.message,
                          weWillClearCondition.cancelBtn,
                          weWillClearCondition.okBtn,
                          function (result) {
                              if (result == false) {
                                  self.isFeatured(false);
                                  return;
                              }
                              self.condition('');
                          });

                  }
                  if (value && self.isSupervisorQuestion()) {
                      var weWillClearSupervisorFlag = config.warnings.weWillClearSupervisorFlag;
                      bootbox.confirm(weWillClearSupervisorFlag.message,
                          weWillClearSupervisorFlag.cancelBtn,
                          weWillClearSupervisorFlag.okBtn,
                          function (result) {
                              if (result == false) {
                                  self.isFeatured(false);
                                  return;
                              }
                              self.scope("Interviewer");
                          });
                  }
              });
              
              self.isSupervisorQuestion.subscribe(function (value) {
                  if (value && (_.isEmpty(self.condition()) == false || _.isEmpty(self.validationExpression()) == false)) {
                      var weWillClearConditionAndValidation = config.warnings.weWillClearConditionAndValidation;
                      
                      bootbox.confirm(weWillClearConditionAndValidation.message,
                          weWillClearConditionAndValidation.cancelBtn,
                          weWillClearConditionAndValidation.okBtn,
                          function (result) {
                              if (result == false) {
                                  self.isSupervisorQuestion(false);
                                  return;
                              }
                              self.condition('');
                              self.validationExpression('');
                          });
                  }
                  if (self.isHead()) {
                      var weWillClearSupervisorFlag = config.warnings.weWillClearHeadFlag;
                      bootbox.confirm(weWillClearSupervisorFlag.message,
                          weWillClearSupervisorFlag.cancelBtn,
                          weWillClearSupervisorFlag.okBtn,
                          function (result) {
                              if (result == false) {
                                  self.isFeatured(false);
                                  return;
                              }
                              self.scope("Interviewer");
                          });
                  }
              });

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
                },
                index = function () {
                    if (this.hasParent()) {
                        var parent = this.parent();
                        var item = utils.findById(parent.childrenID(), this.id());
                        return item.index;
                    }
                    return 0;
                },
                hasParent = function () {
                    if (_.isNull(this.parent()) || _.isUndefined(this.parent())) {
                        return false;
                    }
                    return true;
                },
                clone = function () {
                    var item = new Question();
                    item.title(this.title());
                    item.qtype(this.qtype());
                    item.scope(this.scope());
                    item.answerOrder(this.answerOrder());

                    item.answerOptions(_.map(this.answerOptions(), function (answer) {
                        return new answerOption().id(answer.id()).title(answer.title()).value(answer.value());
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

                    if (this.isClone() && this.isNew()) {
                        item.cloneSource(this.cloneSource());
                    } else {
                        item.cloneSource(this);
                    }

                    item.dirtyFlag().reset();

                    item.alias('');
                    item.alias.valueHasMutated();

                    item.isLinked(this.isLinked());
                    item.selectedLinkTo(this.selectedLinkTo());

                    return item;
                };


            return {
                isNullo: false,
                children: children,
                clone: clone,
                index: index,
                hasParent: hasParent
            };
        }();

        ko.utils.extend(Question.prototype, {
            update: function (data) {
                this.title(data.title);
                this.alias(data.alias);
                this.qtype(data.qtype);
                this.isHead(data.isHead);
                this.isFeatured(data.isFeatured);
                this.isMandatory(data.isMandatory);
                this.scope(data.scope);
                this.condition(data.condition);
                this.validationExpression(data.validationExpression);
                this.validationMessage(data.validationMessage);
                this.instruction(data.instruction);
                this.answerOrder(data.answerOrder);
                this.maxValue(data.maxValue);

                this.answerOptions(_.map(data.answerOptions, function (answer) {
                    return new answerOption().id(answer.id).title(answer.title).value(answer.value);
                }));

                this.triggers(_.map(data.triggers, function (trigger) {
                    return { key: trigger.key, value: trigger.value };
                }));

                this.isLinked(data.isLinked);
                this.selectedLinkTo(data.selectedLinkTo);

                //save off the latest data for later use
                this.cache.latestData = data;
            },
            revert: function () {
                this.update(this.cache.latestData);
            },
            commit: function () {
                this.cache.latestData = ko.toJS(this);
            }
        });

        return Question;
    });