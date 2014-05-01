'use strict';

angular.module('designerApp')
    .controller('QuestionCtrl', [
        '$scope', '$routeParams', 'questionnaireService', 'commandService',
        function($scope, $routeParams, questionnaireService, commandService) {

            var questionId = $scope.activeQuestion.ItemId;
            var questionnaireId = $routeParams.questionnaireId;
            var questionBrief = $scope.activeQuestion;

            console.log($scope.activeQuestion);

            $scope.$watch('activeQuestion', function (newVal) {
                console.log($scope.activeQuestion);
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

            questionnaireService.getQuestionDetailsById(questionnaireId, questionId)
                .success(function(result) {
                    if (result == 'null') {
                        alert('Questionnaire not found');
                    } else {
                        //console.log(result);
                        $scope.activeQuestion = result.question;
                        $scope.activeQuestion.questionScopes = result.questionScopeOptions;
                        $scope.activeQuestion.questionTypes = result.questionTypeOptopns;
                    }
                });
        }
    ]);