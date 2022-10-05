angular.module('designerApp')
    .controller('QuestionCtrl',
        function ($rootScope, $scope, $state, $i18next, $timeout, utilityService, questionnaireService, commandService, $log, confirmService, 
            hotkeys, optionsService, alertService, $uibModal) {
            $scope.currentChapterId = $state.params.chapterId;
            var dictionnaires = {
                categoricalMultiKinds:
                [
                    { value: 1, text: $i18next.t('QuestionCheckboxes') },
                    { value: 2, text: $i18next.t('QuestionYesNoMode') },
                    { value: 3, text: $i18next.t('QuestionComboBox') }
                ]
            };

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
                        if ($scope.questionnaire !== null && !$scope.questionnaire.isReadOnlyForUser
                            && !($scope.activeQuestion.parentIsCover && !$scope.questionnaire.isCoverPageSupported)) {
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


            // https://developer.mozilla.org/en-US/docs/Web/API/Broadcast_Channel_API
            // Automatically reload window on popup close. If supported by browser
            $scope.openEditor = null
            if('BroadcastChannel' in window){
                $scope.bcChannel = new BroadcastChannel("editcategory")
                $scope.bcChannel.onmessage = function(ev) {
                    console.log(ev.data)
                    if(ev.data === 'close#' + $scope.openEditor) {
                        $scope.loadQuestion()
                    }
                }
            }

            var bindQuestion = function(question) {
                $scope.activeQuestion = $scope.activeQuestion || {};
                $scope.activeQuestion.breadcrumbs = question.breadcrumbs;

                $scope.activeQuestion.itemId = $state.params.itemId;

                $scope.activeQuestion.chapterId = question.chapterId;

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
                $scope.activeQuestion.defaultDate = question.defaultDate;
                $scope.activeQuestion.categoricalMultiKinds = dictionnaires.categoricalMultiKinds;

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

                $scope.setLinkSource(question.linkedToEntityId, question.linkedFilterExpression, question.optionsFilterExpression);
                $scope.setCascadeSource(question.cascadeFromQuestionId);
                $scope.setQuestionType(question.type);

                $rootScope.updateVariableTypes($scope.activeQuestion);
                
                $scope.activeQuestion.shouldUserSeeReloadDetailsPromt = false;

                $scope.activeQuestion.showAsList = question.showAsList;
                $scope.activeQuestion.showAsListThreshold = question.showAsListThreshold;

                $scope.activeQuestion.isLinkedToReusableCategories = !_.isEmpty(question.categoriesId);
                $scope.activeQuestion.categoriesId = question.categoriesId;

                $scope.activeQuestion.parentIsCover = $scope.questionnaire
                    ? _.find($scope.questionnaire.chapters, { itemId: $scope.currentChapterId, isCover: true }) != null
                    : false;
                $scope.activeQuestion.isReadOnly = $scope.questionnaire
                    ? _.find($scope.questionnaire.chapters, { itemId: $scope.currentChapterId, isReadOnly: true }) != null
                    : false;


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
                                break;
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

            $scope.showAddClassificationModal = function() {
                var showModal = function() {
                    var modalInstance = $uibModal.open({
                        templateUrl: 'views/add-classification.html',
                        backdrop: false,
                        windowClass: "add-classification-modal dragAndDrop",
                        controller: 'addClassificationCtrl',
                        resolve: {
                            isReadOnlyForUser: $scope.questionnaire.isReadOnlyForUser || $scope.currentChapter.isReadOnly || false,
                            hasOptions: $scope.activeQuestion.optionsCount > 0
                        }
                    });

                    modalInstance.result.then(function(selectedClassification) {
                            if (_.isNull(selectedClassification) || _.isUndefined(selectedClassification))
                                return;

                            var questionTitle = $scope.activeQuestion.title || $i18next.t('UntitledQuestion');
                            var replaceOptions = function() {
                                
                                var optionsToInsertCount = selectedClassification.categoriesCount;

                                if (optionsToInsertCount > $scope.MAX_OPTIONS_COUNT) {
                                    if ($scope.activeQuestion.type !== "SingleOption") {

                                        var modalInstance = confirmService.open(
                                            utilityService.willBeTakenOnlyFirstOptionsConfirmationPopup(questionTitle,
                                                $scope.MAX_OPTIONS_COUNT));

                                        modalInstance.result.then(function(confirmResult) {
                                            if (confirmResult === 'ok') {
                                                $scope.activeQuestion.options = selectedClassification.categories;
                                                $scope.activeQuestion.optionsCount = $scope.activeQuestion.options.length;
                                                markFormAsChanged();
                                            }
                                        });
                                    } else {
                                        commandService.replaceOptionsWithClassification(
                                            $state.params.questionnaireId,
                                            $scope.activeQuestion.itemId,
                                            selectedClassification.id);

                                        $scope.activeQuestion.isFilteredCombobox = true;
                                        $scope.activeQuestion.options = selectedClassification.categories;
                                        $scope.activeQuestion.optionsCount = $scope.activeQuestion.options.length;
                                        markFormAsChanged();
                                    }
                                } else {
                                    if ($scope.activeQuestion.isFilteredCombobox) {
                                        commandService.replaceOptionsWithClassification(
                                            $state.params.questionnaireId,
                                            $scope.activeQuestion.itemId,
                                            selectedClassification.id);
                                    }
                                    $scope.activeQuestion.options = selectedClassification.categories;
                                    $scope.activeQuestion.optionsCount = selectedClassification.categories.length;
                                    markFormAsChanged();
                                }
                            };

                            if ($scope.activeQuestion.options.length > 0) {
                                var modalInstance = confirmService.open(utilityService.replaceOptionsConfirmationPopup(questionTitle));
                                modalInstance.result.then(function(confirmResult) {
                                    if (confirmResult === 'ok') {
                                        replaceOptions();
                                    }
                                });
                            } else {
                                replaceOptions();
                            }
                        },
                        function() {

                        });
                };


                if ($scope.questionForm.$dirty) {
                    $scope.saveQuestion(showModal);
                } else {
                    showModal();
                }
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
                            linkedToType: $scope.activeQuestion.linkedToEntity == null ? null : $scope.activeQuestion.linkedToEntity.type,
                            defaultDate: $scope.activeQuestion.defaultDate,
                            categoriesId: $scope.activeQuestion.categoriesId
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
                    $scope.setLinkSource(null, null, null);
                }

                if (type === 'MultyOption' || type === "SingleOption") {
                    if ($scope.activeQuestion.options.length === 0) {
                        $scope.addOption();
                    }
                }

                if (!$scope.doesQuestionSupportOptionsFilters()) {
                    $scope.activeQuestion.optionsFilterExpression = null;
                    $scope.activeQuestion.linkedFilterExpression = null;
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
                        isReadOnly: $scope.questionnaire.isReadOnlyForUser || $scope.currentChapter.isReadOnly
                    });

                    modalInstance.result.then(function (confirmResult) {
                        if (confirmResult === 'ok') {
                            $scope.saveQuestion(function () {
                                var alertInstance = alertService.open({
                                    title: $i18next.t('QuestionOpenEditorSaved'),
                                    okButtonTitle: $i18next.t('Ok'),
                                    isReadOnly: $scope.questionnaire.isReadOnlyForUser || $scope.currentChapter.isReadOnly
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
                        isReadOnly: $scope.questionnaire.isReadOnlyForUser || $scope.currentChapter.isReadOnly
                    });

                    modalInstance.result.then(function (confirmResult) {
                        if (confirmResult === 'ok') {
                            $scope.saveQuestion(function () {
                                var alertInstance = alertService.open({
                                    title: $i18next.t('QuestionOpenEditorSaved'),
                                    okButtonTitle: $i18next.t('Ok'),
                                    isReadOnly: $scope.questionnaire.isReadOnlyForUser || $scope.currentChapter.isReadOnly
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
                $scope.openEditor =  $scope.activeQuestion.itemId
                window.open("../../questionnaire/editoptions/" + $state.params.questionnaireId + "?questionid=" + $scope.activeQuestion.itemId,
                    "", "scrollbars=yes, center=yes, modal=yes, width=960, height=745, top=" + (screen.height - 745) / 4 + ", left= " + (screen.width - 960) / 2, true);
            };

            var openCascadeOptionsEditor = function () {
                $scope.activeQuestion.shouldUserSeeReloadDetailsPromt = true;
                $scope.openEditor =  $scope.activeQuestion.itemId
                window.open("../../questionnaire/editoptions/" + $state.params.questionnaireId 
                    + "?questionid=" + $scope.activeQuestion.itemId
                    + "&cascading=true",
                    "", "scrollbars=yes, center=yes, modal=yes, width=960, height=745, top=" + (screen.height - 745) / 4 + ", left= " + (screen.width - 960) / 2, true);
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

                    if (currentQuestion.type === 'SingleOption')
                        return o.value !== 'Identifying';

                    return o.value !== 'Identifying' && o.value !== 'Supervisor';
                });
            };

            $scope.changeCategoricalKind = function (kind) {
                var isFilteredCombobox = kind.value === 3;
                var yesNoView = kind.value === 2;

                if (isFilteredCombobox === $scope.activeQuestion.isFilteredCombobox &&
                    yesNoView === $scope.activeQuestion.yesNoView) return;

                $scope.activeQuestion.isFilteredCombobox = isFilteredCombobox;
                $scope.activeQuestion.yesNoView = yesNoView;

                if ($scope.activeQuestion.isLinked)
                    $scope.activeQuestion.isLinked = !isFilteredCombobox && !yesNoView;

                markFormAsChanged();
            };
            
            $scope.getCategoricalKind = function () {
                if ($scope.activeQuestion.isFilteredCombobox)
                    return dictionnaires.categoricalMultiKinds[2];
                else if ($scope.activeQuestion.yesNoView)
                    return dictionnaires.categoricalMultiKinds[1];
                else
                    return dictionnaires.categoricalMultiKinds[0];
            };

            $scope.getSourceOfCategories = function() {
                if ($scope.activeQuestion.isLinked)
                    return $i18next.t('RostersQuestion');

                return $scope.activeQuestion.isLinkedToReusableCategories === true
                    ? $i18next.t('ReusableCategories')
                    : $i18next.t('UserDefinedCategories');
            };

            $scope.getSelectedCategories = function() {
                return _.find($scope.getCategoriesList(),
                    function(c) { return c.categoriesId === $scope.activeQuestion.categoriesId; });
            };

            $scope.getCategoriesList = function() {
                return ($scope.questionnaire || {}).categories || [];
            };

            $scope.setCategories = function (categories) {
                if ($scope.activeQuestion.categoriesId === categories.categoriesId) return;

                $scope.activeQuestion.categoriesId = categories.categoriesId;
                markFormAsChanged();
            };

            $scope.getCategoricalSingleDisplayMode = function () {
                if ($scope.activeQuestion.isCascade || ($scope.activeQuestion.cascadeFromQuestionId || '') !== '')
                    return $i18next.t('QuestionCascading');

                if ($scope.activeQuestion.isFilteredCombobox === true)
                    return $i18next.t('QuestionComboBox');

                else return $i18next.t('QuestionRadioButtonList');
            };

            $scope.setIsReusableCategories = function() {
                if ($scope.activeQuestion.isLinkedToReusableCategories === true) return;

                $scope.activeQuestion.isLinked = false;
                $scope.activeQuestion.isLinkedToReusableCategories = true;
                
                markFormAsChanged();
            };

            $scope.setUserDefinedCategories = function() {
                if ($scope.activeQuestion.isLinkedToReusableCategories === false) return;

                $scope.activeQuestion.isLinked = false;
                $scope.activeQuestion.categoriesId = null;
                $scope.activeQuestion.isLinkedToReusableCategories = false;
                
                markFormAsChanged();
            };

            $scope.setIsLinkedQuestion = function () {
                if ($scope.activeQuestion.isLinked === true) return;

                $scope.activeQuestion.isLinked = true;
                $scope.activeQuestion.isCascade = false;
                $scope.activeQuestion.isLinkedToReusableCategories = null;
                $scope.activeQuestion.categoriesId = null;

                markFormAsChanged();
            };

            $scope.setQuestionAsRadioButtons = function() {

                $scope.activeQuestion.isCascade = false;
                $scope.activeQuestion.isLinked = false;
                $scope.activeQuestion.isFilteredCombobox = false;

                markFormAsChanged();
            };

            $scope.setQuestionAsCombobox = function() {
                if ($scope.activeQuestion.isFilteredCombobox === true) return;

                $scope.activeQuestion.isCascade = false;
                $scope.activeQuestion.isFilteredCombobox = true;

                markFormAsChanged();
            };

            $scope.setQuestionAsCascading = function () {
                if ($scope.activeQuestion.isCascade === true) return;

                $scope.activeQuestion.isLinked = false;
                $scope.activeQuestion.isFilteredCombobox = false;
                $scope.activeQuestion.isCascade = true;

                markFormAsChanged();
            };

            $scope.$on('updateCategories', function (scope, categoryInfo) {
                if ($scope.activeQuestion.isLinkedToReusableCategories === true) {
                    if (categoryInfo.oldCategoriesId != null && $scope.activeQuestion.categoriesId == categoryInfo.oldCategoriesId) {
                        $scope.activeQuestion.categoriesId = categoryInfo.categoriesId;
                    }
                    else if ($scope.activeQuestion.categoriesId !== null && $scope.getSelectedCategories() === undefined) {
                        $scope.activeQuestion.isLinkedToReusableCategories = null;
                        $scope.activeQuestion.categoriesId = null;
                    }
                }
            });

            $scope.$watch('activeQuestion.isLinked',
                function(newValue) {
                    if (!$scope.activeQuestion) {
                        return;
                    }
                    if (newValue) {
                        // nothing
                    } else {
                        $scope.activeQuestion.linkedToEntityId = null;
                        $scope.activeQuestion.linkedToEntity = null;
                    }
                });

            $scope.$watch('activeQuestion.isCascade', function (newValue) {
                if ($scope.activeQuestion) {
                    if (newValue) {
                        if ($scope.activeQuestion.questionScope !== 'Interviewer'
                            && $scope.activeQuestion.questionScope !== 'Hidden'
                            && $scope.activeQuestion.questionScope !== 'Supervisor'
                            && $scope.activeQuestion.questionScope !== 'Identifying') {
                            $scope.changeQuestionScope($scope.getQuestionScopeByValue('Interviewer'));
                        }
                        $scope.activeQuestion.optionsFilterExpression = null;
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

            $scope.setLinkSource = function (itemId, linkedFilterExpression, optionsFilterExpression) {
                $scope.activeQuestion.isLinked = !_.isEmpty(itemId);

                if (itemId) {
                    $scope.activeQuestion.linkedToEntityId = itemId;
                    $scope.activeQuestion.linkedToEntity = _.find($scope.sourceOfLinkedEntities, { id: $scope.activeQuestion.linkedToEntityId });


                    var filter = linkedFilterExpression || optionsFilterExpression;
                    if ($scope.activeQuestion.linkedToEntity !== undefined && $scope.activeQuestion.linkedToEntity.type === 'textlist') {
                        $scope.activeQuestion.linkedFilterExpression = null;
                        $scope.activeQuestion.optionsFilterExpression = filter;
                    } else {
                        $scope.activeQuestion.linkedFilterExpression = filter;
                        $scope.activeQuestion.optionsFilterExpression = null;
                    }

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

            $scope.doesQuestionSupportQuestionScope = function () {
                return $scope.activeQuestion &&
                    $scope.activeQuestion.allQuestionScopeOptions &&
                    $scope.activeQuestion.allQuestionScopeOptions.length > 0 &&
                    $scope.questionnaire &&
                    (!$scope.questionnaire.isCoverPageSupported || !$scope.activeQuestion.parentIsCover);
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
                return $scope.activeQuestion
                    && ($scope.activeQuestion.questionScope != 'Identifying')
                    && !($scope.activeQuestion.isCascade && $scope.activeQuestion.cascadeFromQuestionId)
                    && !$scope.activeQuestion.parentIsCover;
            };

            $scope.isIntegerChange = function () {
                $scope.activeQuestion.countOfDecimalPlaces = null;
            };

            $scope.showAsListChange = function () {
                $scope.activeQuestion.showAsListThreshold = null;
            };
            
            $scope.loadQuestion();
        }
    );
