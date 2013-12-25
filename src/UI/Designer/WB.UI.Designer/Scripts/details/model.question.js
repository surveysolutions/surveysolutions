﻿define('model.question',
   ['ko', 'config', 'utils', 'model.answerOption', 'validator'],
   function (ko, config, utils, answerOption, validator) {

       var _dc = null,
           Question = function () {
               var self = this;
               self.id = ko.observable(Math.uuid());
               self.isNew = ko.observable(true);
               self.isClone = ko.observable(false);


               self.title = ko.observable('New Question').extend({ required: true });
               self.parent = ko.observable();
               self.alias = ko.observable('');

               self.type = ko.observable("QuestionView"); // Object type
               self.template = "QuestionView"; // tempate id in html file

               self.isFeatured = ko.observable(false);
               self.isMandatory = ko.observable(false);

               self.qtype = ko.observable("Text"); // Questoin type

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

               self.condition = ko.observable('');
               self.validationExpression = ko.observable('');
               self.validationMessage = ko.observable('');
               self.instruction = ko.observable('');
               self.isInteger = ko.observable(1);
               self.countOfDecimalPlaces = ko.observable('').extend({
                   digit: true,
                   min: {
                       params: 1,
                       onlyIf: function () {
                           return self.qtype() == config.questionTypes.Numeric && self.isInteger() == false;
                       }
                   },
                   max: {
                       params: 16,
                       onlyIf: function () {
                           return self.qtype() == config.questionTypes.Numeric && self.isInteger() == false;
                       }
                   }
               });
               self.isLinked = ko.observable(0);
               self.isLinkedAsBool = ko.computed(function () {
                   return self.isLinked() == 1;
               });
               self.selectedLinkTo = ko.observable();
               self.localQuestionsFromProragatedGroups = ko.observableArray();
               self.questionsFromProragatedGroups = ko.computed(function () {
                   return _.filter(self.localQuestionsFromProragatedGroups(), function (item) {
                       return item.id() != self.id();
                   }).map(function (item) {
                       return { questionId: item.id(), title: item.alias() + ": " + item.title() };
                   });
               });

               self.areAnswersOrdered = ko.observable(false);
               self.maxAllowedAnswers = ko.observable('').extend({
                   digit: true,
                   min: 1,
                   validation: [
                       {
                           validator: function (val) {
                               var parsedMaxAllowedAnswers = parseInt(val);
                               if (self.isLinked() == 1 || isNaN(parsedMaxAllowedAnswers)) {
                                   return true;
                               }
                               return parsedMaxAllowedAnswers <= self.answerOptions().length;
                           },
                           message: 'could not be more than categories count'
                       }]
               });
               self.answerOptions = ko.observableArray([]);

               self.maxValue = ko.observable();

               // UI stuff
               self.addAnswer = function () {
                   var answer = new answerOption().id(Math.uuid()).title('').value('');

                   answer.errors();

                   self.answerOptions.push(answer);
               };
               self.removeAnswer = function (answer) {
                   self.answerOptions.remove(answer);
               };

               self.typeOptions = config.questionTypeOptions;
               self.scopeOptions = config.questionScopes;

               self.getHref = function () {
                   return utils.questionUrl(self.id());
               };
               self.isSelected = ko.observable();
               self.isNullo = false;
               self.cloneSource = ko.observable();

               self.dirtyFlag = new ko.DirtyFlag([self.title, self.alias, self.qtype,
                   self.isFeatured, self.isMandatory, self.scope, self.condition, self.validationExpression,
                   self.validationMessage, self.instruction, self.answerOptions, self.maxValue,
                   self.selectedLinkTo, self.isLinkedAsBool, self.isInteger, self.countOfDecimalPlaces,
                   self.areAnswersOrdered, self.maxAllowedAnswers]);
               self.dirtyFlag().reset();
               self.errors = ko.validation.group(self);
               this.cache = function () {
               };

               self.canUpdate = ko.observable(true);

               self.hasErrors = ko.computed(function () {
                   return self.errors().length > 0;
               });

               self.wasValidationAttached = false;

               self.attachValidation = function () {
                   if (self.wasValidationAttached)
                       return;

                   self.maxValue.extend({
                       digit: {
                           params: true,
                           onlyIf: function () {
                               return self.isInteger();
                           }
                       }
                   });

                   self.alias.extend({
                       validatable: true,
                       required: true,
                       maxLength: 32,
                       pattern: {
                           message: "Valid variable name should contain only letters, digits and the underscore character and should not start with a digit",
                           params: '^[_A-Za-z][_A-Za-z0-9]*$'
                       },
                       notEqual: 'this'
                   });

                   self.qtype.extend({
                       validatable: true,
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
                                           case "MultyOption":
                                               ;
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

                   self.selectedLinkTo.extend({
                       validatable: true,
                       required: {
                           onlyIf: function () {
                               return self.isLinked() == 1;
                           }
                       }
                   });

                   self.answerOptions.extend({
                       validatable: true,
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
                                   self.scope(config.questionScopes.interviewer);
                                   self.qtype.valueHasMutated();
                               });
                       }
                   });

                   self.isSupervisorQuestion.subscribe(function (value) {
                       if (value && (_.isEmpty(self.validationExpression()) == false)) {
                           var weWillClearValidation = config.warnings.weWillClearValidation;
                           bootbox.confirm(weWillClearValidation.message,
                               weWillClearValidation.cancelBtn,
                               weWillClearValidation.okBtn,
                               function (result) {
                                   if (result == false) {
                                       self.isSupervisorQuestion(false);
                                       return;
                                   }
                                   self.validationExpression('');
                               });
                       }
                   });

                   self.validationExpression.extend({
                       validatable: true,
                       validation: [{
                           validator: function (val) {
                               var validationResult = validator.isValidExpression(val);
                               if (validationResult.isValid) {
                                   return true;
                               }
                               this.message = validationResult.errorMessage;
                               return false;
                           },
                           message: 'Error'
                       }],
                       throttle: 1000
                   });

                   self.condition.extend({
                       validatable: true,
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
                       },
                       {
                           validator: function (val) {
                               var validationResult = validator.isValidExpression(val);
                               if (validationResult.isValid) {
                                   return true;
                               }
                               this.message = validationResult.errorMessage;
                               return false;
                           },
                           message: 'Error'
                       }]
                   });

                   self.title.extend({
                       validatable: true,
                       validation: [{
                           validator: function (val) {
                               var validationResult = validator.isValidQuestionTitle(val, self);

                               if (validationResult.errorMessage != null)
                                   this.message = validationResult.errorMessage;

                               return validationResult.isValid;
                           },
                           message: 'Question title is invalid.'
                       }]
                   });

                   self.alias.valueHasMutated();
                   self.qtype.valueHasMutated();
                   self.selectedLinkTo.valueHasMutated();
                   self.answerOptions.valueHasMutated();
                   self.validationExpression.valueHasMutated();
                   self.condition.valueHasMutated();
                   self.title.valueHasMutated();

                   self.errors = ko.validation.group(self);

                   self.wasValidationAttached = true;
               };
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

                   item.answerOptions(_.map(this.answerOptions(), function (answer) {
                       return new answerOption().id(answer.id()).title(answer.title()).value(answer.value());
                   }));

                   item.isFeatured(this.isFeatured());
                   item.isMandatory(this.isMandatory());
                   item.condition(this.condition());
                   item.instruction(this.instruction());
                   item.maxValue(this.maxValue());
                   item.isInteger(this.isInteger());
                   item.countOfDecimalPlaces(this.countOfDecimalPlaces());

                   item.areAnswersOrdered(this.areAnswersOrdered());
                   item.maxAllowedAnswers(this.maxAllowedAnswers());

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
               this.isFeatured(data.isFeatured);
               this.isMandatory(data.isMandatory);
               this.scope(data.scope);
               this.condition(data.condition);
               this.validationExpression(data.validationExpression);
               this.validationMessage(data.validationMessage);
               this.instruction(data.instruction);
               this.maxValue(data.maxValue);
               this.isInteger(data.isInteger);
               this.countOfDecimalPlaces(data.countOfDecimalPlaces);

               this.areAnswersOrdered(data.areAnswersOrdered);
               this.maxAllowedAnswers(data.maxAllowedAnswers);

               this.answerOptions(_.map(data.answerOptions, function (answer) {
                   return new answerOption().id(answer.id).title(answer.title).value(answer.value);
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