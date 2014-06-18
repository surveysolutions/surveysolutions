(function() {
    'use strict';

    angular.module('designerApp')
        .controller('QuestionCtrl', [
            '$scope', '$routeParams', 'questionnaireService', 'commandService', '$log',
            function($scope, $routeParams, questionnaireService, commandService, $log) {

                var dataBind = function(result) {
                    $scope.activeQuestion.breadcrumbs = result.breadcrumbs;

                    //console.log(JSON.stringify(result));

                    $scope.activeQuestion.type = result.type;
                    $scope.activeQuestion.questionTypeOptions = result.questionTypeOptions;
                    $scope.activeQuestion.variableName = result.variableName;
                    $scope.activeQuestion.title = result.title;
                    $scope.activeQuestion.isPreFilled = result.isPreFilled;
                    $scope.activeQuestion.isMandatory = result.isMandatory;
                    $scope.activeQuestion.enablementCondition = result.enablementCondition;
                    $scope.activeQuestion.validationExpression = result.validationExpression;
                    $scope.activeQuestion.validationMessage = result.validationMessage;
                    $scope.activeQuestion.questionScopeOptions = result.questionScopeOptions;
                }

                $scope.loadQuestion = function () {
                    questionnaireService.getQuestionDetailsById($routeParams.questionnaireId, $scope.activeQuestion.itemId)
                        .success(function (result) {
                            $scope.initialQuestion = angular.copy(result);
                            dataBind(result);
                        });
                };

                $scope.saveQuestion = function() {
                    commandService.sendUpdateQuestionCommand($routeParams.questionnaireId, $scope.activeQuestion).success(function (result) {
                        if (!result.IsSuccess) {
                            $log.error(result.Error);
                        }
                    });
                };

                $scope.moveToChapter = function(chapterId) {
                    questionnaireService.moveQuestion(questionId, 0, chapterId, questionnaireId);
                    $scope.resetSelection();
                    questionnaireService.removeItem($scope.items, questionId);
                };

                $scope.resetQuestion = function () {
                    dataBind($scope.initialQuestion);
                };

                $scope.loadQuestion();

                $scope.$watch('activeQuestion', function() {
                    $scope.loadQuestion();
                });
            }
        ]);
}());