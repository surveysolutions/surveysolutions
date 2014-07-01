(function() {
    'use strict';

    angular.module('designerApp')
        .controller('QuestionCtrl', [
            '$scope', '$routeParams', 'questionnaireService', 'commandService', 'navigationService', '$log',
            function($scope, $routeParams, questionnaireService, commandService, navigationService, $log) {

                var dataBind = function(result) {
                    $scope.activeQuestion.breadcrumbs = result.breadcrumbs;

                    $scope.activeQuestion.type = result.type;
                    $scope.activeQuestion.variable = result.variableName;
                    $scope.activeQuestion.variableLabel = result.variableLabel;
                    $scope.activeQuestion.questionTypeOptions = result.questionTypeOptions;
                    $scope.activeQuestion.title = result.title;
                    $scope.activeQuestion.isMandatory = result.isMandatory;
                    $scope.activeQuestion.enablementCondition = result.enablementCondition;
                    $scope.activeQuestion.validationExpression = result.validationExpression;
                    $scope.activeQuestion.validationMessage = result.validationMessage;
                    $scope.activeQuestion.questionScopeOptions = result.questionScopeOptions;
                    $scope.activeQuestion.instructions = result.instructions;
                    $scope.activeQuestion.options = result.options;
                    $scope.activeQuestion.isInteger = result.isInteger;
                    $scope.activeQuestion.maxValue = result.maxValue;
                    $scope.activeQuestion.countOfDecimalPlaces = result.countOfDecimalPlaces;

                    if (result.isPreFilled) {
                        $scope.activeQuestion.questionScope = 'Headquarter';
                    } else {
                        $scope.activeQuestion.questionScope = 'Interviewer';
                    }
                };

                $scope.loadQuestion = function() {
                    questionnaireService.getQuestionDetailsById($routeParams.questionnaireId, $scope.activeQuestion.itemId)
                        .success(function(result) {
                            $scope.initialQuestion = angular.copy(result);
                            dataBind(result);
                        });
                };

                $scope.saveQuestion = function() {
                    commandService.sendUpdateQuestionCommand($routeParams.questionnaireId, $scope.activeQuestion).success(function(result) {
                        if (!result.IsSuccess) {
                            $log.error(result.Error);
                        }
                    });
                };

                $scope.setQuestionType = function(type) {
                    $scope.activeQuestion.type = type;
                };

                $scope.deleteQuestion = function() {
                    if (confirm("Are you sure want to delete question?")) {
                        commandService.deleteQuestion($routeParams.questionnaireId, $scope.activeQuestion.itemId).success(function(result) {
                            if (result.IsSuccess) {
                                $scope.activeQuestion.isDeleted = true;
                                navigationService.openQuestionnaire($routeParams.questionnaireId);
                            } else {
                                $log.error(result);
                            }
                        });
                    }
                };

                $scope.moveToChapter = function(chapterId) {
                    questionnaireService.moveQuestion($scope.activeQuestion.itemId, 0, chapterId, $routeParams.questionnaireId);

                    var removeFrom = $scope.activeQuestion.getParentItem() || $scope;
                    removeFrom.items.splice(_.indexOf(removeFrom.items, $scope.activeQuestion), 1);
                    $scope.resetSelection();
                };

                $scope.resetQuestion = function() {
                    dataBind($scope.initialQuestion);
                };

                $scope.addOption = function() {
                    $scope.activeQuestion.options.push({
                        "value": null,
                        "title": ''
                    });;
                };

                $scope.removeOption = function(index) {
                    $scope.activeQuestion.options.splice(index, 1);
                };

                $scope.changeQuestionScope = function() {
                    if ($scope.activeQuestion.questionScope == 'Headquarter') {
                        $scope.activeQuestion.enablementCondition = '';
                    }
                };

                $scope.loadQuestion();

                $scope.$watch('activeQuestion', function() {
                    $scope.loadQuestion();
                });
            }
        ]);
}());