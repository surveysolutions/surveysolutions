angular.module('designerApp')
    .controller('QuestionCtrl',
        function ($rootScope, $scope, $state, utilityService, questionnaireService, commandService, $log, confirmService, hotkeys) {
            $scope.currentChapterId = $state.params.chapterId;
            var dictionnaires = {};
            hotkeys.bindTo($scope)
              .add({
                  combo: 'ctrl+s',
                  description: 'Save current question',
                  allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                  callback: function (event) {
                      $scope.saveQuestion();
                      $scope.questionForm.$setPristine();
                      event.preventDefault();
                  }
              });
            var bindQuestion = function(question) {
                $scope.activeQuestion = $scope.activeQuestion || {};
                $scope.activeQuestion.breadcrumbs = question.breadcrumbs;

                $scope.activeQuestion.itemId = $state.params.itemId;

                $scope.activeQuestion.variable = question.variableName || question.variable;
                $scope.activeQuestion.variableLabel = question.variableLabel;
                $scope.activeQuestion.mask = question.mask;
                $scope.activeQuestion.questionTypeOptions = question.questionTypeOptions;
                $scope.activeQuestion.title = question.title;
                $scope.activeQuestion.isMandatory = question.isMandatory;
                $scope.activeQuestion.enablementCondition = question.enablementCondition;
                $scope.activeQuestion.validationExpression = question.validationExpression;
                $scope.activeQuestion.validationMessage = question.validationMessage;
                $scope.activeQuestion.allQuestionScopeOptions = question.allQuestionScopeOptions;
                $scope.activeQuestion.notPrefilledQuestionScopeOptions = question.notPrefilledQuestionScopeOptions;
                $scope.activeQuestion.instructions = question.instructions;
                $scope.activeQuestion.maxAnswerCount = question.maxAnswerCount;
                $scope.activeQuestion.maxAllowedAnswers = question.maxAllowedAnswers;
                $scope.activeQuestion.areAnswersOrdered = question.areAnswersOrdered;
                $scope.activeQuestion.isFilteredCombobox = question.isFilteredCombobox;

                var options = question.options || [];
                _.each(options, function(option) {
                    option.id = utilityService.guid();
                });

                $scope.activeQuestion.options = options;
                $scope.activeQuestion.optionsCount = question.optionsCount || 0;

                $scope.activeQuestion.wereOptionsTruncated = question.wereOptionsTruncated || false;
                $scope.activeQuestion.isInteger = (question.type === 'Numeric') ? question.isInteger : true;
                $scope.activeQuestion.maxValue = question.maxValue;
                $scope.activeQuestion.countOfDecimalPlaces = question.countOfDecimalPlaces;

                $scope.activeQuestion.questionScope = question.isPreFilled ? 'Prefilled' : question.questionScope;

                $scope.setQuestionType(question.type);

                $scope.setLinkSource(question.linkedToQuestionId);
                $scope.setCascadeSource(question.cascadeFromQuestionId);

                $scope.activeQuestion.shouldUserSeeReloadDetailsPromt = false;
            };

            var dataBind = function (result) {
                dictionnaires.allQuestionScopeOptions = result.allQuestionScopeOptions;

                $scope.sourceOfLinkedQuestions = result.sourceOfLinkedQuestions;
                $scope.sourceOfSingleQuestions = result.sourceOfSingleQuestions;
                
                bindQuestion(result);
            };

            $scope.loadQuestion = function () {
                questionnaireService.getQuestionDetailsById($state.params.questionnaireId, $state.params.itemId)
                    .success(function (result) {
                        $scope.initialQuestion = angular.copy(result);
                        dataBind(result);
                    });
            };

            $scope.saveQuestion = function (callback) {
                if ($scope.questionForm.$valid) {
                    var shouldGetOptionsOnServer = wasThereOptionsLooseWhileChanginQuestionProperties($scope.initialQuestion, $scope.activeQuestion) && $scope.activeQuestion.isCascade;
                    commandService.sendUpdateQuestionCommand($state.params.questionnaireId, $scope.activeQuestion, shouldGetOptionsOnServer).success(function () {
                        $scope.initialQuestion = angular.copy($scope.activeQuestion);

                        $rootScope.$emit('questionUpdated', {
                            itemId: $scope.activeQuestion.itemId,
                            title: $scope.activeQuestion.title,
                            variable: $scope.activeQuestion.variable,
                            type: $scope.activeQuestion.type,
                            linkedToQuestionId: $scope.activeQuestion.linkedToQuestionId
                        });

                        var notIsFilteredCombobox = !$scope.activeQuestion.isFilteredCombobox;
                        var notIsCascadingCombobox = _.isEmpty($scope.activeQuestion.cascadeFromQuestionId);

                        if ($scope.activeQuestion.type === "SingleOption" && notIsFilteredCombobox && notIsCascadingCombobox) {
                            $scope.activeQuestion.optionsCount = $scope.activeQuestion.options.length;
                        }

                        if (_.isFunction(callback)) {
                            callback();
                        }

                        if (shouldGetOptionsOnServer) {
                            $scope.loadQuestion();
                        }
                    });
                }
            };

            var wasThereOptionsLooseWhileChanginQuestionProperties = function(initialQuestion, actualQuestion) {
                if (actualQuestion.type != "SingleOption")
                    return false;

                if ((actualQuestion.wereOptionsTruncated || false) === false)
                    return false;

                var wasItFiltered = initialQuestion.isFilteredCombobox || false;
                var wasItCascade = !_.isEmpty(initialQuestion.cascadeFromQuestionId);

                if (
                    (wasItCascade && actualQuestion.isFilteredCombobox) ||
                    (wasItCascade && !_.isEmpty(initialQuestion.cascadeFromQuestionId)) ||
                    (wasItFiltered && !_.isEmpty(actualQuestion.cascadeFromQuestionId)) 
                    ){
                    return true;
                }

                return false;
            };

            $scope.setQuestionType = function (type) {
                $scope.activeQuestion.type = type;
                $scope.activeQuestion.typeName = _.find($scope.activeQuestion.questionTypeOptions, { value: type }).text;
                $scope.activeQuestion.allQuestionScopeOptions = dictionnaires.allQuestionScopeOptions;


                if (type === 'TextList') {
                    $scope.activeQuestion.questionScope = 'Interviewer';
                }

                if (type === 'DateTime') {
                    $scope.activeQuestion.allQuestionScopeOptions = _.filter($scope.activeQuestion.allQuestionScopeOptions, function (val) {
                        return val.value !== 'Supervisor';
                    });
                    if ($scope.activeQuestion.questionScope === 'Supervisor') {
                        $scope.activeQuestion.questionScope = 'Interviewer';
                    }
                }
                if (type === 'GpsCoordinates') {
                    $scope.activeQuestion.questionScope = 'Interviewer';
                }
                if (type !== "SingleOption" && type !== "MultyOption") {
                    $scope.setLinkSource(null);
                }
            };

            $scope.cancelQuestion = function () {
                var temp = angular.copy($scope.initialQuestion);
                bindQuestion(temp);
            };

            $scope.addOption = function () {
                $scope.activeQuestion.options.push({
                    "value": null,
                    "title": '',
                    "id": utilityService.guid()
                });
                $scope.activeQuestion.optionsCount += 1;
            };

            $scope.editFilteredComboboxOptions = function () {
                if ($scope.questionForm.$dirty) {
                    var modalInstance = confirmService.open({
                        title: "To open options editor all unsaved changes must be saved. Should we save them now?",
                        okButtonTitle: "Save",
                        cancelButtonTitle: "No, later"
                    });

                    modalInstance.result.then(function (confirmResult) {
                        if (confirmResult === 'ok') {
                            $scope.saveQuestion(function () {
                                openOptionsEditor();
                            });
                        }
                    });
                } else {
                    openOptionsEditor();
                }
            };

            $scope.editCascadingComboboxOptions = function () {
                var wasCascadeFromQuestionIdChanged = ($scope.activeQuestion.cascadeFromQuestionId != $scope.initialQuestion.cascadeFromQuestionId);
                if ($scope.questionForm.$dirty || wasCascadeFromQuestionIdChanged) {
                    var modalInstance = confirmService.open({
                        title: "To open options editor all unsaved changes must be saved. Should we save them now?",
                        okButtonTitle: "Save",
                        cancelButtonTitle: "No, later"
                    });

                    modalInstance.result.then(function (confirmResult) {
                        if (confirmResult === 'ok') {
                            $scope.saveQuestion(function () {
                                openCascadeOptionsEditor();
                            });
                        }
                    });
                } else {
                    openCascadeOptionsEditor();
                }
            };

            var openOptionsEditor = function () {
                $scope.activeQuestion.shouldUserSeeReloadDetailsPromt = true;

                window.open("../../questionnaire/editoptions/" + $state.params.questionnaireId + "?questionid=" + $scope.activeQuestion.itemId,
                  "", "scrollbars=yes, center=yes, modal=yes, width=960", true);
            };

            var openCascadeOptionsEditor = function () {
                $scope.activeQuestion.shouldUserSeeReloadDetailsPromt = true;

                window.open("../../questionnaire/editcascadingoptions/" + $state.params.questionnaireId + "?questionid=" + $scope.activeQuestion.itemId,
                  "", "scrollbars=yes, center=yes, modal=yes, width=960", true);
            };

            $scope.removeOption = function (index) {
                $scope.activeQuestion.options.splice(index, 1);
                $scope.activeQuestion.optionsCount -= 1;
            };

            $scope.changeQuestionScope = function (scope) {
                $scope.activeQuestion.questionScope = scope.text;
                if ($scope.activeQuestion.questionScope === 'Prefilled') {
                    $scope.activeQuestion.enablementCondition = '';
                }
            };

            $scope.$watch('activeQuestion.isLinked', function (newValue) {
                if (!newValue && $scope.activeQuestion) {
                    $scope.activeQuestion.linkedToQuestionId = null;
                    $scope.activeQuestion.linkedToQuestion = null;
                }
            });

            $scope.$watch('activeQuestion.isCascade', function (newValue) {
                if ($scope.activeQuestion) {
                    if (newValue) {
                        $scope.activeQuestion.questionScope = 'Interviewer';
                    } else {
                        $scope.activeQuestion.cascadeFromQuestionId = null;
                        $scope.activeQuestion.cascadeFromQuestion = null;
                    }
                }
            });

            $scope.setLinkSource = function (itemId) {
                $scope.activeQuestion.isLinked = !_.isEmpty(itemId);

                if (itemId) {
                    $scope.activeQuestion.linkedToQuestionId = itemId;
                    $scope.activeQuestion.linkedToQuestion = _.find($scope.sourceOfLinkedQuestions, { id: $scope.activeQuestion.linkedToQuestionId });
                } 
            };

            $scope.setCascadeSource = function (itemId) {
                $scope.activeQuestion.isCascade = !_.isEmpty(itemId);

                if (itemId) {
                    $scope.activeQuestion.cascadeFromQuestionId = itemId;
                    $scope.activeQuestion.cascadeFromQuestion = _.find($scope.sourceOfSingleQuestions, { id: $scope.activeQuestion.cascadeFromQuestionId });
                }
            };

            var questionTypesDoesNotSupportValidations = [
                "TextList",
                "QRBarcode",
                "Multimedia",
                "GpsCoordinates"];
            
            $scope.doesQuestionSupportValidations = function () {
                return $scope.activeQuestion && !_.contains(questionTypesDoesNotSupportValidations, $scope.activeQuestion.type)
                    && !($scope.activeQuestion.isCascade && $scope.activeQuestion.cascadeFromQuestionId);
            };

            $scope.doesQuestionSupportEnablementConditions = function () {
                return $scope.activeQuestion && ($scope.activeQuestion.questionScope != 'Prefilled')
                    && !($scope.activeQuestion.isCascade && $scope.activeQuestion.cascadeFromQuestionId);
            };

            $scope.loadQuestion();
        }
    );
