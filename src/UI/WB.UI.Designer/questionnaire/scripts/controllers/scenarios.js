angular.module('designerApp')
    .controller('ScenariosCtrl',
    function ($rootScope, $scope, $state, $i18next, hotkeys, commandService, utilityService, confirmService, webTesterService) {
            'use strict';

            var hideScenariosPane = 'ctrl+r';

            if (hotkeys.get(hideScenariosPane) !== false) {
                hotkeys.del(hideScenariosPane);
            }

            hotkeys.add(hideScenariosPane, $i18next.t('HotkeysCloseScenarios'), function (event) {
                event.preventDefault();
                $scope.foldback();
            });

            $scope.scenarios = [];

            var dataBind = function (scenario, scenarioDto) {
                scenario.id = scenarioDto.id;
                scenario.title = scenarioDto.title;
            };

            $scope.loadScenarios = function () {
                if ($scope.questionnaire === null || $scope.questionnaire.scenarios === null)
                    return;

                _.each($scope.questionnaire.scenarios, function (scenarioDto) {
                    var scenario = { checkpoint: {} };
                    if (!_.any($scope.scenarios, function (elem) { return elem.id === scenarioDto.id; })) {
                        dataBind(scenario, scenarioDto);
                        dataBind(scenario.checkpoint, scenarioDto);
                        $scope.scenarios.push(scenario);
                    }
                });
            };

            $scope.isFolded = false;

            $scope.unfold = function () {
                $scope.isFolded = true;
            };

            $scope.runScenario = function (scenario) {
                var webTesterWindow = window.open("about:blank", '_blank');

                webTesterService.run($state.params.questionnaireId).then(function (response) {
                    webTesterWindow.location.href = response.data + "?scenarioId=" + scenario.id;
                });
            };

            $scope.saveScenario = function (scenario) {
                commandService.updateScenario($state.params.questionnaireId, scenario).then(function () {
                    dataBind(scenario.checkpoint, scenario);
                    scenario.form.$setPristine();
                });
            };

            $scope.cancel = function (scenario) {
                dataBind(scenario, scenario.checkpoint);
                scenario.form.$setPristine();
            };

            $scope.deleteScenario = function (index) {
                var scenario = $scope.scenarios[index];
                var scenarioName = scenario.title || $i18next.t("SideBarScenarioNoName");
                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(scenarioName));

                modalInstance.result.then(function (confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteScenario($state.params.questionnaireId, scenario.id).then(function () {
                            $scope.scenarios.splice(index, 1);
                        });
                    }
                });
            };

            $scope.foldback = function () {
                $scope.isFolded = false;
                $rootScope.$broadcast("closeScenariosList", {});
            };

            $scope.$on('openScenariosList', function (scope, params) {
                $scope.unfold();
                if (!_.isUndefined(params) && !_.isUndefined(params.focusOn)) {
                    setTimeout(function () { utilityService.focus("focusScenario" + params.focusOn); }, 500);
                }
            });

            $scope.$on('closeScenariosListRequested', function () {
                $scope.foldback();
            });

            $rootScope.$on('questionnaireLoaded', function () {
                $scope.loadScenarios();
            });

            $scope.$on('verifing', function () {
                for (var i = 0; i < $scope.scenarios.length; i++) {
                    var scenario = $scope.scenarios[i];
                    if (scenario.form.$dirty) {
                        $scope.saveScenario(scenario);
                    }
                }
            });
        });
