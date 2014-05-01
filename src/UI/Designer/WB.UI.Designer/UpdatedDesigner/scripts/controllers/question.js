'use strict';

angular.module('designerApp')
    .controller('QuestionCtrl', [
        '$scope', '$routeParams', 'questionnaireService', 'commandService',
        function($scope, $routeParams, questionnaireService, commandService) {

            var questionnaireId = $routeParams.questionnaireId;
            var questionId = null;
            var questionBrief = null;
            //console.log($scope.activeQuestion);

            //$scope.$watch('activeQuestion', function(newVal) {
                
            //});

            questionId = $scope.activeQuestion.itemId;
            questionBrief = $scope.activeQuestion;

            questionnaireService.getQuestionDetailsById(questionnaireId, questionId)
                .success(function (result) {
                    if (result == 'null') {
                        alert('Questionnaire not found');
                    } else {
                        console.log(result);
                        $scope.activeQuestion = result.question;
                        $scope.activeQuestion.questionScopes = result.questionScopeOptions;
                        $scope.activeQuestion.questionTypes = result.questionTypeOptopns;
                        $scope.activeQuestion.breadcrumbs = result.breadcrumbs;
                    }
                });

            $scope.saveQuestion = function() {
                //console.log(questionBrief);
                $('#edit-question-save-button').popover('destroy');
                commandService.sendUpdateQuestionCommand(questionnaireId, $scope.activeQuestion).success(function(result) {
                    if (result.IsSuccess) {
                        questionBrief.Title = $scope.activeQuestion.title;
                        questionBrief.Type = $scope.activeQuestion.type;
                        questionBrief.Variable = $scope.activeQuestion.variableName;
                    } else {
                        // console.log(result);
                        $('#edit-question-save-button').popover({
                            content: result.Error,
                            placement: top,
                            animation: true
                        }).popover('show');
                    }
                });
            };
        }
    ]);