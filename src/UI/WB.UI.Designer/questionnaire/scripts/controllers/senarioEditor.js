angular.module('designerApp')
    .controller('scenarioEditorCtrl',
        function ($state, $scope, $http, isReadOnlyForUser, scenarioId, webTesterService, commandService) {
            $scope.scenario = {
            }

            $scope.aceLoaded = function (_editor) {
                // Options
                _editor.setReadOnly(true);
                _editor.setOptions({
                    maxLines: 40
                });
                _editor.setShowPrintMargin(false);
            };

            $scope.onSave = function () {
                return commandService.upadteScenarioSteps($state.params.questionnaireId, 
                    scenarioId,
                    $scope.scenario.steps).then(function(){
                        $scope.frmEditor.$setPristine();
                    });
            };

            webTesterService.getScenarioSteps($state.params.questionnaireId, scenarioId)
                .then(function (response) {
                    $scope.scenario.steps = JSON3.stringify(response.data, null, 4);
                });
        });