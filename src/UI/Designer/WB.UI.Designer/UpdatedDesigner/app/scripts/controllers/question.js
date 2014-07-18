(function() {
    'use strict';

    angular.module('designerApp')
        .controller('QuestionCtrl', [
            '$rootScope', '$scope', '$state', 'utilityService', 'questionnaireService', 'commandService', '$log',
            function ($rootScope, $scope, $state, utilityService, questionnaireService, commandService, $log) {
                var dataBind = function(result) {
                    $scope.activeQuestion = $scope.activeQuestion || {};
                    $scope.activeQuestion.breadcrumbs = result.breadcrumbs;

                    $scope.activeQuestion.itemId = $state.params.itemId;
                    $scope.activeQuestion.type = result.type;
                    $scope.activeQuestion.variable = result.variableName;
                    $scope.activeQuestion.variableLabel = result.variableLabel;
                    $scope.activeQuestion.mask = result.mask;
                    $scope.activeQuestion.questionTypeOptions = result.questionTypeOptions;
                    $scope.activeQuestion.title = result.title;
                    $scope.activeQuestion.isMandatory = result.isMandatory;
                    $scope.activeQuestion.enablementCondition = result.enablementCondition;
                    $scope.activeQuestion.validationExpression = result.validationExpression;
                    $scope.activeQuestion.validationMessage = result.validationMessage;
                    $scope.activeQuestion.questionScopeOptions = result.questionScopeOptions;
                    $scope.activeQuestion.instructions = result.instructions;
                    var options = result.options || [];
                    _.each(options, function(option) {
                        option.id = utilityService.guid();
                    });

                    $scope.activeQuestion.options = options;
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
                    questionnaireService.getQuestionDetailsById($state.params.questionnaireId, $state.params.itemId)
                        .success(function(result) {
                            $scope.initialQuestion = angular.copy(result);
                            dataBind(result);
                        });
                };

                $scope.saveQuestion = function() {
                    commandService.sendUpdateQuestionCommand($state.params.questionnaireId, $scope.activeQuestion).success(function(result) {
                        $scope.initialQuestion = angular.copy($scope.activeQuestion);
                        $rootScope.$emit('questionUpdated', {
                            itemId: $scope.activeQuestion.itemId,
                            title: $scope.activeQuestion.title,
                            variable: $scope.activeQuestion.variable,
                            type: $scope.activeQuestion.type
                        });
                    });
                };

                $scope.setQuestionType = function(type) {
                    $scope.activeQuestion.type = type;
                };

                $scope.cancelQuestion = function() {
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

                $scope.removeOption = function(index) {
                    $scope.activeQuestion.options.splice(index, 1);
                };

                $scope.changeQuestionScope = function(scope) {
                    $scope.activeQuestion.questionScope = scope.text;
                    if ($scope.activeQuestion.questionScope == 'Headquarter') {
                        $scope.activeQuestion.enablementCondition = '';
                    }
                };

                $scope.loadQuestion();
            }
        ]);
}());