angular.module('designerApp')
    .controller('QuestionCtrl',
        function ($rootScope, $scope, $state, $timeout, utilityService, questionnaireService, commandService, $log, confirmService, hotkeys, optionsService) {
            $scope.currentChapterId = $state.params.chapterId;
            var dictionnaires = {};

            var saveQuestion = 'ctrl+s';
          
            
            if (hotkeys.get(saveQuestion) !== false) {
                hotkeys.del(saveQuestion);
            }

            var markFormAsChanged = function () {
                if ($scope.questionForm) {
                    $scope.questionForm.$setDirty();
                }
            }

            hotkeys.bindTo($scope)
                .add({
                    combo: saveQuestion,
                    description: 'Save changes',
                    allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                    callback: function(event) {
                        if ($scope.questionnaire !== null && !$scope.questionnaire.isReadOnlyForUser) {
                            $scope.saveQuestion();
                            $scope.questionForm.$setPristine();
                            event.preventDefault();
                        }
                    }
                });


            $scope.onKeyPressInOptions = function(keyEvent) {
                if (keyEvent.which === 13) {
                    keyEvent.preventDefault();

                    var targetDomElement = keyEvent.target ? keyEvent.target : keyEvent.srcElement;

                    utilityService.moveFocusAndAddOptionIfNeeded(
                        targetDomElement,
                        ".question-options-editor",
                        ".question-options-editor input.question-option-value-editor",
                        $scope.activeQuestion.options,
                        function() { return $scope.addOption(); },
                        "option");
                }
            };

            var bindQuestion = function(question) {
                $scope.activeQuestion = $scope.activeQuestion || {};
                $scope.activeQuestion.breadcrumbs = question.breadcrumbs;

                $scope.activeQuestion.itemId = $state.params.itemId;

                $scope.activeQuestion.variable = question.variableName || question.variable;
                $scope.activeQuestion.variableLabel = question.variableLabel;
                $scope.activeQuestion.mask = question.mask;
                $scope.activeQuestion.questionTypeOptions = question.questionTypeOptions;
                $scope.activeQuestion.title = question.title;
                
                $scope.activeQuestion.enablementCondition = question.enablementCondition;
                $scope.activeQuestion.validationExpression = question.validationExpression;
                $scope.activeQuestion.hideIfDisabled = question.hideIfDisabled;
                $scope.activeQuestion.validationMessage = question.validationMessage;
                $scope.activeQuestion.allQuestionScopeOptions = question.allQuestionScopeOptions;
                $scope.activeQuestion.instructions = question.instructions;
                $scope.activeQuestion.hideInstructions = question.hideInstructions;
                $scope.activeQuestion.useFormatting = question.useFormatting;
                $scope.activeQuestion.maxAnswerCount = question.maxAnswerCount;
                $scope.activeQuestion.maxAllowedAnswers = question.maxAllowedAnswers;
                $scope.activeQuestion.areAnswersOrdered = question.areAnswersOrdered;
                $scope.activeQuestion.yesNoView = question.yesNoView;
                $scope.activeQuestion.isFilteredCombobox = question.isFilteredCombobox;
                $scope.activeQuestion.optionsFilterExpression = question.optionsFilterExpression;

                $scope.activeQuestion.validationConditions = question.validationConditions;

                $scope.activeQuestion.isTimestamp = question.isTimestamp;

                var options = question.options || [];
                _.each(options, function(option) {
                    option.id = utilityService.guid();
                });
                
                $scope.activeQuestion.useListAsOptionsEditor = true;
                $scope.activeQuestion.options = options;
                $scope.activeQuestion.optionsCount = question.optionsCount || 0;

                $scope.activeQuestion.wereOptionsTruncated = question.wereOptionsTruncated || false;
                $scope.activeQuestion.isInteger = (question.type === 'Numeric') ? question.isInteger : true;
                $scope.activeQuestion.countOfDecimalPlaces = question.countOfDecimalPlaces;

                $scope.activeQuestion.questionScope = question.isPreFilled ? 'Prefilled' : question.questionScope;

                $scope.setQuestionType(question.type);

                $scope.setLinkSource(question.linkedToEntityId, question.linkedFilterExpression);
                $scope.setCascadeSource(question.cascadeFromQuestionId);

                $scope.activeQuestion.shouldUserSeeReloadDetailsPromt = false;

                if (!_.isNull($scope.questionForm) && !_.isUndefined($scope.questionForm)) {
                    $scope.questionForm.$setPristine();
                }
            };

            $scope.MAX_OPTIONS_COUNT = 200;

            var dataBind = function (result) {
                dictionnaires.allQuestionScopeOptions = result.allQuestionScopeOptions;

                $scope.sourceOfLinkedEntities = result.sourceOfLinkedEntities;
                $scope.sourceOfSingleQuestions = result.sourceOfSingleQuestions;
                
                bindQuestion(result);
            };

            $scope.loadQuestion = function () {
                questionnaireService.getQuestionDetailsById($state.params.questionnaireId, $state.params.itemId)
                    .success(function (result) {
                        $scope.initialQuestion = angular.copy(result);
                        dataBind(result);
                        utilityService.scrollToValidationCondition($state.params.validationIndex);


                        var focusId = null;
                        switch ($state.params.property) {
                            case 'Title':
                                focusId = 'edit-question-title';
                                break;
                            case 'VariableName':
                                focusId = 'edit-question-variable-name';
                                break;
                            case 'EnablingCondition':
                                focusId = "edit-question-enablement-condition";
                                break;
                            case 'ValidationExpression':
                                focusId = 'validation-expression-' + $state.params.validationIndex;
                                break;
                            case 'ValidationMessage':
                                focusId = 'validationMessage' + $state.params.validationIndex;
                                break;
                            case 'Option':
                                focusId = 'option-title-' + $state.params.validationIndex;
                                break;
                            case 'OptionsFilter':
                                focusId = 'optionsFilterExpression';
                                break;
                            default:
                                break;
                        }

                        utilityService.setFocusIn(focusId);
                    });
            };

            var hasQuestionEnablementConditions = function(question) {
                return $scope.doesQuestionSupportEnablementConditions() &&
                    question.enablementCondition !== null &&
                    /\S/.test(question.enablementCondition);
            };

            var hasQuestionValidations = function(question) {
                return $scope.doesQuestionSupportValidations() && question.validationConditions.length > 0;
            };

            $scope.saveQuestion = function (callback) {
                if ($scope.questionForm.$valid) {
                    $scope.showOptionsInList();
                    var shouldGetOptionsOnServer = wasThereOptionsLooseWhileChanginQuestionProperties($scope.initialQuestion, $scope.activeQuestion) && $scope.activeQuestion.isCascade;
                    commandService.sendUpdateQuestionCommand($state.params.questionnaireId, $scope.activeQuestion, shouldGetOptionsOnServer).success(function () {
                        $scope.initialQuestion = angular.copy($scope.activeQuestion);

                        $rootScope.$emit('questionUpdated', {
                            itemId: $scope.activeQuestion.itemId,
                            type: $scope.activeQuestion.type,
                            linkedToEntityId: $scope.activeQuestion.linkedToEntityId,
                            linkedFilterExpression: $scope.activeQuestion.linkedFilterExpression,
                            hasCondition: hasQuestionEnablementConditions($scope.activeQuestion),
                            hasValidation: hasQuestionValidations($scope.activeQuestion),
                            title: $scope.activeQuestion.title,
                            variable: $scope.activeQuestion.variable,
                            hideIfDisabled: $scope.activeQuestion.hideIfDisabled
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
                if (actualQuestion.type !== "SingleOption" || actualQuestion.type !== "MultyOption")
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

                var isQuestionScopeSupervisorOrPrefilled = $scope.activeQuestion.questionScope === 'Supervisor' || $scope.activeQuestion.questionScope === 'Prefilled';
                if (type === 'TextList' && isQuestionScopeSupervisorOrPrefilled) {
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
                if (type === 'GpsCoordinates' && $scope.activeQuestion.questionScope === 'Supervisor') {
                    $scope.activeQuestion.questionScope = 'Interviewer';
                }

                if (type === 'MultyOption' && $scope.activeQuestion.questionScope === 'Prefilled') {
                    $scope.activeQuestion.questionScope = 'Interviewer';
                }

                if (type !== "SingleOption" && type !== "MultyOption") {
                    $scope.setLinkSource(null,null);
                }

                markFormAsChanged();
            };

            $scope.cancelQuestion = function () {
                var temp = angular.copy($scope.initialQuestion);
                bindQuestion(temp);
            };

            $scope.addOption = function () {
                if ($scope.activeQuestion.optionsCount >= $scope.MAX_OPTIONS_COUNT)
                    return;

                $scope.activeQuestion.options.push({
                    "value": null,
                    "title": '',
                    "id": utilityService.guid()
                });
                $scope.activeQuestion.optionsCount += 1;
                markFormAsChanged();
            };

            $scope.editFilteredComboboxOptions = function () {
                if ($scope.questionForm.$dirty) {
                    var modalInstance = confirmService.open({
                        title: "To open options editor all unsaved changes must be saved. Should we save them now?",
                        okButtonTitle: "Save",
                        cancelButtonTitle: "No, later",
                        isReadOnly: $scope.questionnaire.isReadOnlyForUser
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
                        cancelButtonTitle: "No, later",
                        isReadOnly: $scope.questionnaire.isReadOnlyForUser
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
                $scope.activeQuestion.optionsCount = $scope.activeQuestion.options.length;
                markFormAsChanged();
            };

            $scope.removeValidationCondition = function(index) {
                $scope.activeQuestion.validationConditions.splice(index, 1);
                markFormAsChanged();
            }

            $scope.addValidationCondition = function() {
                $scope.activeQuestion.validationConditions.push({
                    expression: '',
                    message: ''
                });
                markFormAsChanged();
                _.defer(function () {
                    $(".question-editor .form-holder").scrollTo({ top: '+=200px', left: "+=0" }, 250);
                });
            }

            $scope.showOptionsInTextarea = function () {
                $scope.activeQuestion.stringifiedOptions = optionsService.stringifyOptions($scope.activeQuestion.options);
                $scope.activeQuestion.useListAsOptionsEditor = false;
            };

            $scope.showOptionsInList = function () {
                if ($scope.activeQuestion.useListAsOptionsEditor) {
                    return;
                }
                if (_.isUndefined($scope.questionForm.stringifiedOptions) || !$scope.questionForm.stringifiedOptions.$valid) {
                    return;
                }
                if ($scope.activeQuestion.stringifiedOptions) {
                    $scope.activeQuestion.options = optionsService.parseOptions($scope.activeQuestion.stringifiedOptions);
                }
                $scope.activeQuestion.useListAsOptionsEditor = true;
            };

            $scope.changeQuestionScope = function (scope) {
                $scope.activeQuestion.questionScope = scope.text;
                if ($scope.activeQuestion.questionScope === 'Prefilled') {
                    $scope.activeQuestion.enablementCondition = '';
                }
                markFormAsChanged();
            };

            $scope.getQuestionScopes = function (currentQuestion) {
                if (!currentQuestion)
                    return [];
                var allScopes = currentQuestion.allQuestionScopeOptions;
                if (!currentQuestion.isCascade && !currentQuestion.isLinked && $.inArray(currentQuestion.type, ['TextList', 'QRBarcode', 'Multimedia', 'GpsCoordinates', 'MultyOption']) < 0)
                    return allScopes;

                return allScopes.filter(function (o) {
                    if (currentQuestion.type == 'MultyOption')
                        return o.value !== 'Prefilled';

                    if (currentQuestion.type == 'GpsCoordinates')
                        return o.value !== 'Supervisor';

                    return o.value !== 'Prefilled' && o.value !== 'Supervisor';
                });
            };

            $scope.$watch('activeQuestion.isLinked', function(newValue) {
                if (!$scope.activeQuestion) {
                    return;
                }
                if (newValue) {
                    $scope.activeQuestion.yesNoView = false;
                    $scope.activeQuestion.optionsFilterExpression = null;
                } else {
                    $scope.activeQuestion.linkedToEntityId = null;
                    $scope.activeQuestion.linkedToEntity = null;
                }
            });
            $scope.$watch('activeQuestion.yesNoView', function (newValue) {
                if (newValue && $scope.activeQuestion) {
                    $scope.activeQuestion.isLinked = false;
                }
            });

            $scope.$watch('activeQuestion.isCascade', function (newValue) {
                if ($scope.activeQuestion) {
                    if (newValue) {
                        if ($scope.activeQuestion.questionScope !== 'Interviewer' && $scope.activeQuestion.questionScope !== 'Hidden') {
                            $scope.activeQuestion.questionScope = 'Interviewer';
                            $scope.activeQuestion.optionsFilterExpression = null;
                        }
                    } else {
                        $scope.activeQuestion.cascadeFromQuestionId = null;
                        $scope.activeQuestion.cascadeFromQuestion = null;
                    }
                }
            });

            $scope.$on('verifing', function (scope, params) {
                if ($scope.questionForm.$dirty)
                    $scope.saveQuestion(function() {
                        $scope.questionForm.$setPristine();
                    });
            });

            $scope.setLinkSource = function (itemId, linkedFilterExpression) {
                $scope.activeQuestion.isLinked = !_.isEmpty(itemId);

                if (itemId) {
                    $scope.activeQuestion.linkedToEntityId = itemId;
                    $scope.activeQuestion.linkedToEntity = _.find($scope.sourceOfLinkedEntities, { id: $scope.activeQuestion.linkedToEntityId });
                    $scope.activeQuestion.linkedFilterExpression = linkedFilterExpression;
                    markFormAsChanged();
                } 
            };

            $scope.setCascadeSource = function (itemId) {
                $scope.activeQuestion.isCascade = !_.isEmpty(itemId);

                if (itemId) {
                    $scope.activeQuestion.cascadeFromQuestionId = itemId;
                    $scope.activeQuestion.cascadeFromQuestion = _.find($scope.sourceOfSingleQuestions, { id: $scope.activeQuestion.cascadeFromQuestionId });
                    markFormAsChanged();
                }
            };

            var questionTypesDoesNotSupportValidations = ["Multimedia"];
            
            $scope.doesQuestionSupportValidations = function () {
                return $scope.activeQuestion && !_.contains(questionTypesDoesNotSupportValidations, $scope.activeQuestion.type)
                    && !($scope.activeQuestion.isCascade && $scope.activeQuestion.cascadeFromQuestionId);
            };

            $scope.doesQuestionSupportEnablementConditions = function () {
                return $scope.activeQuestion && ($scope.activeQuestion.questionScope != 'Prefilled')
                    && !($scope.activeQuestion.isCascade && $scope.activeQuestion.cascadeFromQuestionId);
            };

            $scope.isIntegerChange = function () {
                $scope.activeQuestion.countOfDecimalPlaces = null;
            };

            $scope.loadQuestion();
        }
    );
