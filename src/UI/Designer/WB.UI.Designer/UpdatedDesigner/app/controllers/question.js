(function() {
    'use strict';

    angular.module('designerApp')
        .controller('QuestionCtrl', [
            '$scope', '$routeParams', 'questionnaireService', 'commandService',
            function($scope, $routeParams, questionnaireService, commandService) {

                var questionnaireId = $routeParams.questionnaireId;
                var questionId;
                var questionBrief;

                questionId = $scope.activeQuestion.itemId;
                questionBrief = $scope.activeQuestion;

                questionnaireService.getQuestionDetailsById(questionnaireId, questionId)
                    .success(function(result) {
                        $scope.activeQuestion = result;
                        $scope.activeQuestion.optionValue = "";
                        $scope.activeQuestion.optionTitle = "";
                    });

                $scope.addOption = function() {
                    $scope.activeQuestion.options.push({
                        value: $scope.activeQuestion.optionValue,
                        title: $scope.activeQuestion.optionTitle
                    });
                };

                $scope.saveQuestion = function() {
                    $('#edit-question-save-button').popover('destroy');
                    commandService.sendUpdateQuestionCommand(questionnaireId, $scope.activeQuestion).success(function(result) {
                        if (result.IsSuccess) {
                            questionBrief.title = $scope.activeQuestion.title;
                            questionBrief.type = $scope.activeQuestion.type;
                            questionBrief.variable = $scope.activeQuestion.variableName;
                        } else {
                            $('#edit-question-save-button').popover({
                                content: result.Error,
                                placement: top,
                                animation: true
                            }).popover('show');
                        }
                    });
                };

                $scope.moveToChapter = function (chapterId) {
                    questionnaireService.moveQuestion(questionId, 0, chapterId, questionnaireId);
                    $scope.resetSelection();
                    questionnaireService.removeItem($scope.items, questionId);
                };
            }
        ]);
}());