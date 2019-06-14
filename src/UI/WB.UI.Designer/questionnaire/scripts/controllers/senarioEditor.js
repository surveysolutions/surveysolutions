angular.module('designerApp')
    .controller('scenarioEditorCtrl',
        function ($state, $scope, $http, isReadOnlyForUser, scenarioId, webTesterService) {
            $scope.scenario = {
            }

            $scope.aceLoaded = function (_editor) {
                // Options
                if (isReadOnlyForUser) {
                    _editor.setReadOnly(true);
                }
                _editor.setOptions({
                    maxLines: 35
                });
                _editor.setShowPrintMargin(false);
            };

            $scope.onSave = function () {
                return webTesterService.setScenarioSteps($state.params.questionnaireId, 
                    scenarioId,
                    {steps: $scope.scenario.steps});
            };

            webTesterService.getScenarioSteps($state.params.questionnaireId, scenarioId)
                .then(function (response) {
                    $scope.scenario.steps = JSON3.stringify(response.data, null, 4);
                });
        });