angular.module('designerApp')
    .controller('QuestionCtrl',
        function ($rootScope, $scope, $state, $i18next, $timeout, utilityService, questionnaireService, commandService, $log, confirmService, hotkeys, optionsService, alertService) {
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
                    description: $i18next.t('Save'),
                    allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                    callback: function(event) {
                        if ($scope.questionnaire !== null && !$scope.questionnaire.isReadOnlyForUser) {
                            if ($scope.questionForm.$dirty) {
                                $scope.saveQuestion();
                                $scope.questionForm.$setPristine();
                            }
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
                $scope.activeQuestion.geometryTypeOptions = question.geometryTypeOptions;
                $scope.activeQuestion.geometryType = question.geometryType;

                var options = question.options || [];
                _.each(options, function(option) {
                    option.id = utilityService.guid();
                });
                
                $scope.activeQuestion.useListAsOptionsEditor = true;
                $scope.activeQuestion.options = options;
                $scope.activeQuestion.optionsCount = question.optionsCount || 0;

                $scope.activeQuestion.wereOptionsTruncated = question.wereOptionsTruncated || false;
                $scope.activeQuestion.isInteger = (question.type === 'Numeric') ? question.isInteger : true;
                $scope.activeQuestion.isSignature = (question.type === 'Multimedia') ? question.isSignature : false;

                $scope.activeQuestion.countOfDecimalPlaces = question.countOfDecimalPlaces;

                $scope.activeQuestion.questionScope = question.isPreFilled
                    ? 'Identifying' 
                    : question.questionScope;

                $scope.setLinkSource(question.linkedToEntityId, question.linkedFilterExpression);
                $scope.setCascadeSource(question.cascadeFromQuestionId);
                $scope.setQuestionType(question.type);

                $rootScope.updateVariableTypes($scope.activeQuestion);
                
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
                    .then(function (result) {
                        var data = result.data;
                        $scope.initialQuestion = angular.copy(data);
                        dataBind(data);
                        
                        
                        var focusId = null;
                        switch ($state.params.property) {
                            case 'Title':
                                focusId = 'edit-question-title-highlight';
                                break;
                            case 'VariableName':
                                focusId = 'edit-question-variable-name';
                                break;
                            case 'EnablingCondition':
                                focusId = "edit-question-enablement-condition";
                                break;
                            case 'ValidationExpression':
                                focusId = 'validation-expression-' + $state.params.indexOfEntityInProperty;
                                break;
                            case 'ValidationMessage':
                                focusId = 'validation-message-' + $state.params.indexOfEntityInProperty;
                                break;
                            case 'Option':
                                focusId = 'option-title-' + $state.params.indexOfEntityInProperty;
                                break;
                            case 'OptionsFilter':
                                focusId = 'optionsFilterExpression';
                                break;
                            case 'Instructions':
                                focusId = 'edit-question-instructions';
                            default:
                                break;
                        }

                        if (!_.isNull($state.params.indexOfEntityInProperty))
                            utilityService.scrollToValidationCondition($state.params.indexOfEntityInProperty);
                        else {
                            utilityService.scrollToElement(".question-editor .form-holder", "#" + focusId);
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
                    $scope.trimEmptyOptions();
                    var shouldGetOptionsOnServer = wasThereOptionsLooseWhileChanginQuestionProperties($scope.initialQuestion, $scope.activeQuestion) && $scope.activeQuestion.isCascade;
                    commandService.sendUpdateQuestionCommand($state.params.questionnaireId, $scope.activeQuestion, shouldGetOptionsOnServer).then(function () {
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
                            hideIfDisabled: $scope.activeQuestion.hideIfDisabled,
                            yesNoView: $scope.activeQuestion.yesNoView,
                            isInteger: $scope.activeQuestion.isInteger,
                            linkedToType: $scope.activeQuestion.linkedToEntity == null ? null : $scope.activeQuestion.linkedToEntity.type
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

            var questionsWithOnlyInterviewerScope = ['Multimedia', 'Audio', 'Area', 'QRBarcode'];

            $scope.setQuestionType = function (type) {
                $scope.activeQuestion.type = type;
                $scope.activeQuestion.typeName = _.find($scope.activeQuestion.questionTypeOptions, { value: type }).text;
                $scope.activeQuestion.allQuestionScopeOptions = dictionnaires.allQuestionScopeOptions;

                var isQuestionScopeSupervisorOrPrefilled = $scope.activeQuestion.questionScope === 'Supervisor' || $scope.activeQuestion.questionScope === 'Identifying';
                if (type === 'TextList' && isQuestionScopeSupervisorOrPrefilled) {
                    $scope.changeQuestionScope($scope.getQuestionScopeByValue('Interviewer'));
                }

                if (type === 'DateTime') {
                    $scope.activeQuestion.allQuestionScopeOptions = _.filter($scope.activeQuestion.allQuestionScopeOptions, function (val) {
                        return val.value !== 'Supervisor';
                    });
                    if ($scope.activeQuestion.questionScope === 'Supervisor') {
                        $scope.changeQuestionScope($scope.getQuestionScopeByValue('Interviewer'));
                    }
                }
                if (_.contains(questionsWithOnlyInterviewerScope, type)) {
                    $scope.changeQuestionScope($scope.getQuestionScopeByValue('Interviewer'));
                }

                if (type === 'GpsCoordinates' && $scope.activeQuestion.questionScope === 'Supervisor') {
                    $scope.changeQuestionScope($scope.getQuestionScopeByValue('Interviewer'));
                }

                if (type === 'MultyOption' && $scope.activeQuestion.questionScope === 'Identifying') {
                    $scope.changeQuestionScope($scope.getQuestionScopeByValue('Interviewer'));
                }

                if (type !== "SingleOption" && type !== "MultyOption") {
                    $scope.setLinkSource(null, null);
                }

                if (type === 'MultyOption' || type === "SingleOption") {
                    if ($scope.activeQuestion.options.length === 0) {
                        $scope.addOption();
                    }
                }

                if (!$scope.doesQuestionSupportOptionsFilters()) {
                    $scope.activeQuestion.optionsFilterExpression = '';
                    $scope.activeQuestion.linkedFilterExpression = '';
                }

                if (type === "Area") {
                    if($scope.activeQuestion.geometryType === null)
                        $scope.activeQuestion.geometryType = $scope.activeQuestion.geometryTypeOptions[0].value;
                }
                else
                    $scope.activeQuestion.geometryType = null;

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
                        title: $i18next.t('QuestionOpenEditorConfirm'),
                        okButtonTitle: $i18next.t('Save'),
                        cancelButtonTitle: $i18next.t('Cancel'),
                        isReadOnly: $scope.questionnaire.isReadOnlyForUser
                    });

                    modalInstance.result.then(function (confirmResult) {
                        if (confirmResult === 'ok') {
                            $scope.saveQuestion(function () {
                                var alertInstance = alertService.open({
                                    title: $i18next.t('QuestionOpenEditorSaved'),
                                    okButtonTitle: $i18next.t('Ok'),
                                    isReadOnly: $scope.questionnaire.isReadOnlyForUser
                                });

                                alertInstance.result.then(function(confirmResult) {
                                    openOptionsEditor();
                                });
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
                        title: $i18next.t('QuestionOpenEditorConfirm'),
                        okButtonTitle: $i18next.t('Save'),
                        cancelButtonTitle: $i18next.t('Cancel'),
                        isReadOnly: $scope.questionnaire.isReadOnlyForUser
                    });

                    modalInstance.result.then(function (confirmResult) {
                        if (confirmResult === 'ok') {
                            $scope.saveQuestion(function () {
                                var alertInstance = alertService.open({
                                    title: $i18next.t('QuestionOpenEditorSaved'),
                                    okButtonTitle: $i18next.t('Ok'),
                                    isReadOnly: $scope.questionnaire.isReadOnlyForUser
                                });

                                alertInstance.result.then(function (confirmResult) {
                                    openCascadeOptionsEditor();
                                });
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
                    "", "scrollbars=yes, center=yes, modal=yes, width=960, height=500, left=100, top=100", true);
            };

            var openCascadeOptionsEditor = function () {
                $scope.activeQuestion.shouldUserSeeReloadDetailsPromt = true;

                window.open("../../questionnaire/editcascadingoptions/" + $state.params.questionnaireId + "?questionid=" + $scope.activeQuestion.itemId,
                    "", "scrollbars=yes, center=yes, modal=yes, width=960, height=500, left=100, top=100", true);
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
                    $scope.activeQuestion.optionsCount = $scope.activeQuestion.options.length;
                }
                $scope.activeQuestion.useListAsOptionsEditor = true;
            };

            $scope.trimEmptyOptions = function () {
                var notEmptyOptions = _.filter($scope.activeQuestion.options, function (o) {
                    return !_.isNull(o.value || null) || !_.isEmpty(o.title || '');
                });
                $scope.activeQuestion.options = notEmptyOptions;
            }

            $scope.changeQuestionScope = function (scope) {
                $scope.activeQuestion.questionScope = scope.value;
                if ($scope.activeQuestion.questionScope === 'Identifying') {
                    $scope.activeQuestion.enablementCondition = '';
                }
                markFormAsChanged();
            };

            $scope.changeGeometryType = function (geometry) {
                $scope.activeQuestion.geometryType = geometry;
                
                markFormAsChanged();
            };


            $scope.getQuestionScopes = function (currentQuestion) {
                if (!currentQuestion)
                    return [];
                var allScopes = currentQuestion.allQuestionScopeOptions;

                if (_.contains(questionsWithOnlyInterviewerScope, currentQuestion.type)) {
                    return allScopes.filter(function (o) {
                        return o.value === 'Interviewer';
                    });
                }

                if (!currentQuestion.isCascade && !currentQuestion.isLinked &&
                    $.inArray(currentQuestion.type, ['TextList', 'GpsCoordinates', 'MultyOption']) < 0)
                    return allScopes;

                return allScopes.filter(function (o) {
                    if (currentQuestion.type === 'MultyOption')
                        return o.value !== 'Identifying';

                    if (currentQuestion.type === 'GpsCoordinates')
                        return o.value !== 'Supervisor';

                    return o.value !== 'Identifying' && o.value !== 'Supervisor';
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
                            $scope.changeQuestionScope($scope.getQuestionScopeByValue('Interviewer'));
                            $scope.activeQuestion.optionsFilterExpression = null;
                        }
                    } else {
                        $scope.activeQuestion.cascadeFromQuestionId = null;
                        $scope.activeQuestion.cascadeFromQuestion = null;
                    }
                }
            });

            $scope.getQuestionScopeByValue = function(value) {
                return _.find($scope.activeQuestion.allQuestionScopeOptions, { value: value });
            }
            $scope.$on('verifing', function () {
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

            var questionTypesDoesNotSupportValidations = ["Multimedia", "Audio"];
            
            $scope.doesQuestionSupportValidations = function () {
                return $scope.activeQuestion &&
                    !_.contains(questionTypesDoesNotSupportValidations, $scope.activeQuestion.type);

            };

            $scope.doesQuestionSupportOptionsFilters = function () {
                if ($scope.activeQuestion) {
                    if ($scope.activeQuestion.type === 'MultyOption' || $scope.activeQuestion.type === 'SingleOption') {
                        return true;
                    }
                }

                return false;
            };

            $scope.doesQuestionSupportEnablementConditions = function () {
                return $scope.activeQuestion && ($scope.activeQuestion.questionScope != 'Identifying')
                    && !($scope.activeQuestion.isCascade && $scope.activeQuestion.cascadeFromQuestionId);
            };

            $scope.isIntegerChange = function () {
                $scope.activeQuestion.countOfDecimalPlaces = null;
            };

            $scope.loadQuestion();
        }
    );
