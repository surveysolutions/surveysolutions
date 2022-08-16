angular.module('designerApp')
    .controller('scenarioEditorCtrl',
        function ($state, $scope, $http, isReadOnlyForUser, scenarioId, scenarioTitle, webTesterService, commandService) {
            $scope.scenario = {}

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

            webTesterService.getScenarioSteps($state.params.questionnaireId, scenarioId, scenarioTitle)
                .then(function (response) {
                    $scope.scenario.steps = JSON3.stringify(response.data, null, 4);
                    $scope.scenario.title = scenarioTitle;
                });
        });
