angular.module('designerApp')
    .controller('QuestionCtrl',
        function ($rootScope, $scope, $state, utilityService, questionnaireService, commandService, $log, confirmService) {
            $scope.currentChapterId = $state.params.chapterId;
            var dictionnaires = {};
            var dataBind = function (result) {
                $scope.activeQuestion = $scope.activeQuestion || {};
                $scope.activeQuestion.breadcrumbs = result.breadcrumbs;

                $scope.activeQuestion.itemId = $state.params.itemId;

                $scope.activeQuestion.variable = result.variableName || result.variable;
                $scope.activeQuestion.variableLabel = result.variableLabel;
                $scope.activeQuestion.mask = result.mask;
                $scope.activeQuestion.questionTypeOptions = result.questionTypeOptions;
                $scope.activeQuestion.title = result.title;
                $scope.activeQuestion.isMandatory = result.isMandatory;
                $scope.activeQuestion.enablementCondition = result.enablementCondition;
                $scope.activeQuestion.validationExpression = result.validationExpression;
                $scope.activeQuestion.validationMessage = result.validationMessage;
                $scope.activeQuestion.allQuestionScopeOptions = result.allQuestionScopeOptions;
                dictionnaires.allQuestionScopeOptions = result.allQuestionScopeOptions;
                $scope.activeQuestion.notPrefilledQuestionScopeOptions = result.notPrefilledQuestionScopeOptions;
                $scope.activeQuestion.instructions = result.instructions;
                $scope.activeQuestion.maxAnswerCount = result.maxAnswerCount;
                $scope.activeQuestion.maxAllowedAnswers = result.maxAllowedAnswers;
                $scope.activeQuestion.areAnswersOrdered = result.areAnswersOrdered;
                $scope.activeQuestion.isFilteredCombobox = result.isFilteredCombobox;

                var options = result.options || [];
                _.each(options, function (option) {
                    option.id = utilityService.guid();
                });

                $scope.activeQuestion.options = options;
                $scope.activeQuestion.optionsCount = result.optionsCount || 0;

                $scope.activeQuestion.wereOptionsTruncated = result.wereOptionsTruncated || false;
                $scope.activeQuestion.isInteger = result.isInteger;
                $scope.activeQuestion.maxValue = result.maxValue;
                $scope.activeQuestion.countOfDecimalPlaces = result.countOfDecimalPlaces;

                $scope.activeQuestion.questionScope = result.isPreFilled ? 'Prefilled' : result.questionScope;

                $scope.sourceOfLinkedQuestions = result.sourceOfLinkedQuestions;
                $scope.sourceOfSingleQuestions = result.sourceOfSingleQuestions;

                $scope.setQuestionType(result.type);

                $scope.setLinkSource(result.linkedToQuestionId);
                $scope.setCascadeSource(result.cascadeFromQuestionId);

                $scope.activeQuestion.shouldUserSeeReloadDetailsPromt = false;

                $scope.questionForm.$setPristine();
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
                    var shouldGetOptionsOnServer = wasThereOptionsLooseWhileChanginQuestionProperties($scope.initialQuestion, $scope.activeQuestion);
                    commandService.sendUpdateQuestionCommand($state.params.questionnaireId, $scope.activeQuestion, shouldGetOptionsOnServer).success(function () {
                        $scope.initialQuestion = angular.copy($scope.activeQuestion);

                        $rootScope.$emit('questionUpdated', {
                            itemId: $scope.activeQuestion.itemId,
                            title: $scope.activeQuestion.title,
                            variable: $scope.activeQuestion.variable,
                            type: $scope.activeQuestion.type,
                            linkedToQuestionId: $scope.activeQuestion.linkedToQuestionId
                        });

                        if ($scope.activeQuestion.type === "SingleOption" && !$scope.activeQuestion.isFilteredCombobox && !_.isEmpty($scope.activeQuestion.cascadeFromQuestionId)) {
                            $scope.activeQuestion.optionsCount = $scope.activeQuestion.options.length;
                        }

                        $scope.questionForm.$setPristine();
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

                if ((wasItCascade && actualQuestion.isFilteredCombobox) || (
                    wasItFiltered && !_.isEmpty(actualQuestion.cascadeFromQuestionId))) {
                    return true;
                }

                return false;
            };

            $scope.setQuestionType = function (type) {
                $scope.activeQuestion.type = type;
                $scope.activeQuestion.typeName = _.find($scope.activeQuestion.questionTypeOptions, { value: type }).text;
                $scope.activeQuestion.allQuestionScopeOptions = dictionnaires.allQuestionScopeOptions;

                if (type === 'DateTime') {
                    $scope.activeQuestion.allQuestionScopeOptions = _.filter($scope.activeQuestion.allQuestionScopeOptions, function (val) {
                        return val.value !== 'Supervisor';
                    });
                    if ($scope.activeQuestion.questionScope === 'Supervisor') {
                        $scope.activeQuestion.questionScope = 'Interviewer';
                    }
                }
                if (type === 'GpsCoordinates' && $scope.activeQuestion.questionScope === 'Prefilled') {
                    $scope.activeQuestion.questionScope = 'Interviewer';
                }
                if (type !== "SingleOption" && type !== "MultyOption") {
                    $scope.setLinkSource(null);
                }
            };

            $scope.cancelQuestion = function () {
                var temp = angular.copy($scope.initialQuestion);
                dataBind(temp);
                $scope.questionForm.$setPristine();
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

            $scope.loadQuestion();
        }
    );
