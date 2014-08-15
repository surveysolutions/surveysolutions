(function() {
    'use strict';

    angular.module('designerApp')
        .controller('QuestionCtrl', [
            '$rootScope', '$scope', '$state', 'utilityService', 'questionnaireService', 'commandService', '$log',
            function ($rootScope, $scope, $state, utilityService, questionnaireService, commandService, $log) {
                $scope.currentChapterId = $state.params.chapterId;

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
                    $scope.activeQuestion.notPrefilledQuestionScopeOptions = result.notPrefilledQuestionScopeOptions;
                    $scope.activeQuestion.instructions = result.instructions;
                    $scope.activeQuestion.maxAnswerCount = result.maxAnswerCount;
                    $scope.activeQuestion.maxAllowedAnswers = result.maxAllowedAnswers;
                    $scope.activeQuestion.areAnswersOrdered = result.areAnswersOrdered;
                    $scope.activeQuestion.isFilteredCombobox = result.isFilteredCombobox;

                    var options = result.options || [];
                    _.each(options, function(option) {
                        option.id = utilityService.guid();
                    });

                    $scope.activeQuestion.options = options;
                    $scope.activeQuestion.isInteger = result.isInteger;
                    $scope.activeQuestion.maxValue = result.maxValue;
                    $scope.activeQuestion.countOfDecimalPlaces = result.countOfDecimalPlaces;

                    $scope.activeQuestion.questionScope = result.isPreFilled ? 'Prefilled' : result.questionScope;


                    $scope.sourceOfLinkedQuestions = result.sourceOfLinkedQuestions;
                    $scope.setQuestionType(result.type);
                    $scope.setLinkSource(result.linkedToQuestionId);

                    $scope.questionForm.$setPristine();
                };

                var isQuestionSaving = false;

                $scope.loadQuestion = function() {
                    questionnaireService.getQuestionDetailsById($state.params.questionnaireId, $state.params.itemId)
                        .success(function (result) {
                            $scope.initialQuestion = angular.copy(result);
                            dataBind(result);

                        });
                };

                $scope.saveQuestion = function () {
                    if ($scope.questionForm.$valid) {
                        commandService.sendUpdateQuestionCommand($state.params.questionnaireId, $scope.activeQuestion).success(function(result) {

                            $scope.initialQuestion = angular.copy($scope.activeQuestion);
                            $rootScope.$emit('questionUpdated', {
                                itemId: $scope.activeQuestion.itemId,
                                title: $scope.activeQuestion.title,
                                variable: $scope.activeQuestion.variable,
                                type: $scope.activeQuestion.type,
                                linkedToQuestionId: $scope.activeQuestion.linkedToQuestionId
                            });
                            $scope.questionForm.$setPristine();
                        });
                    }
                };

                $scope.setQuestionType = function(type) {
                    $scope.activeQuestion.type = type;
                    $scope.activeQuestion.typeName = _.find($scope.activeQuestion.questionTypeOptions, { value: type }).text;
                    if (type == 'GpsCoordinates' && $scope.activeQuestion.questionScope == 'Prefilled') {
                        $scope.activeQuestion.questionScope = 'Interviewer';
                    }
                    if (type != "SingleOption" && type != "MultyOption") {
                        $scope.setLinkSource(null);
                    }
                };

                $scope.cancelQuestion = function () {
                    var temp = angular.copy($scope.initialQuestion);
                    dataBind(temp);
                    $scope.questionForm.$setPristine();
                };

                $scope.addOption = function() {
                    $scope.activeQuestion.options.push({
                        "value": null,
                        "title": '',
                        "id": utilityService.guid()
                    });
                };

                $scope.editFilteredComboboxOptions = function () {
                    var a = window.open("../../questionnaire/editoptions/" + $scope.activeQuestion.itemId, "Edit options", "resizable: no;center : yes; modal:yes");
                };

                $scope.removeOption = function(index) {
                    $scope.activeQuestion.options.splice(index, 1);
                };

                $scope.changeQuestionScope = function(scope) {
                    $scope.activeQuestion.questionScope = scope.text;
                    if ($scope.activeQuestion.questionScope == 'Prefilled') {
                        $scope.activeQuestion.enablementCondition = '';
                    }
                };

                $scope.$watch('activeQuestion.isLinked', function(newValue) {
                    if (!newValue && $scope.activeQuestion) {
                        $scope.activeQuestion.linkedToQuestionId = null;
                    }
                });

                $scope.setLinkSource = function (itemId) {
                    $scope.activeQuestion.isLinked = !_.isEmpty(itemId);
                    if (itemId) {
                        $scope.activeQuestion.linkedToQuestionId = itemId;
                        $scope.activeQuestion.linkedToQuestion = _.find($scope.sourceOfLinkedQuestions, { id: $scope.activeQuestion.linkedToQuestionId });
                    }
                };

                $scope.loadQuestion();
            }
        ]);
}());