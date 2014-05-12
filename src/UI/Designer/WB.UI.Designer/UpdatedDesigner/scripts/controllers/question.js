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
                        $scope.activeQuestion = result;
                        $scope.activeQuestion.optionValue = "";
                        $scope.activeQuestion.optionTitle = "";
                    }
                });

            $scope.addOption = function () {
                $scope.activeQuestion.options.push({
                    value: $scope.activeQuestion.optionValue,
                    title: $scope.activeQuestion.optionTitle
                });
            }

            $scope.saveQuestion = function() {
                //console.log(questionBrief);
                $('#edit-question-save-button').popover('destroy');
                commandService.sendUpdateQuestionCommand(questionnaireId, $scope.activeQuestion).success(function(result) {
                    if (result.IsSuccess) {
                        questionBrief.title = $scope.activeQuestion.title;
                        questionBrief.type = $scope.activeQuestion.type;
                        questionBrief.variable = $scope.activeQuestion.variableName;
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