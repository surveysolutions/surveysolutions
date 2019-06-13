angular.module('designerApp')
    .controller('ScenariosCtrl',
    function ($rootScope, $scope, $state, $i18next, hotkeys, commandService, utilityService, confirmService) {
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
                scenario.itemId = scenarioDto.itemId;
                scenario.name = scenarioDto.name;
                scenario.description = scenarioDto.description;
                scenario.content = scenarioDto.content;
                scenario.isDescriptionVisible = !_.isEmpty(scenario.description);
            };

            $scope.loadScenarios = function () {
                if ($scope.questionnaire === null || $scope.questionnaire.scenarios === null)
                    return;

                _.each($scope.questionnaire.scenarios, function (scenarioDto) {
                    var scenario = { checkpoint: {} };
                    if (!_.any($scope.scenarios, function (elem) { return elem.itemId === scenarioDto.itemId; })) {
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
                //commandService.runScenario();
                alert("run run run");
            };

            $scope.cancel = function (scenario) {
                dataBind(scenario, scenario.checkpoint);
                scenario.form.$setPristine();
            };

            $scope.deleteScenario = function (index) {
                var scenario = $scope.scenarios[index];
                var scenarioName = scenario.name || $i18next.t("SideBarScenarioNoName");
                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(scenarioName));

                modalInstance.result.then(function (confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteScenarios($state.params.questionnaireId, scenario.itemId).then(function () {
                            $scope.scenarios.splice(index, 1);
                        });
                    }
                });
            };

            $scope.isDescriptionEmpty = function (scenario) {
                return _.isEmpty(scenario.description);
            };
            $scope.toggleDescription = function (scenario) {
                scenario.isDescriptionVisible = !scenario.isDescriptionVisible;
            };

            $scope.aceLoaded = function (editor) {
                var expressionEditorPlaceholder = $i18next.t('SideBarScenarioContent');

                // Editor part
                var renderer = editor.renderer;

                // Options
                editor.setOptions({
                    maxLines: Infinity,
                    mode: "ace/mode/csharp-extended",
                    fontSize: 16,
                    highlightActiveLine: false,
                    theme: "ace/theme/github-extended",
                    enableBasicAutocompletion: true,
                    enableLiveAutocompletion: true
                });
                editor.$blockScrolling = Infinity;

                $scope.aceEditorUpdateMode(editor);

                $rootScope.$on('variablesChanged', function () {
                    $scope.aceEditorUpdateMode(editor);
                });

                renderer.setShowGutter(false);
                renderer.setPadding(0);

                function update() {
                    var shouldShow = !editor.session.getValue().length;
                    var node = editor.renderer.emptyMessageNode;
                    if (!shouldShow && node) {
                        editor.renderer.scroller.removeChild(editor.renderer.emptyMessageNode);
                        editor.renderer.emptyMessageNode = null;
                    } else if (shouldShow && !node) {
                        node = editor.renderer.emptyMessageNode = document.createElement("div");
                        node.textContent = expressionEditorPlaceholder;
                        node.className = "ace_invisible ace_emptyMessage";
                        node.style.padding = "0";
                        editor.renderer.scroller.appendChild(node);
                    }
                }
                editor.on("input", update);

                editor.commands.addCommand({
                    name: "replace",
                    bindKey: { win: "Tab|Shift-Tab", mac: "Tab" },
                    exec: function (editor) {
                        editor.blur();
                    }
                });

                setTimeout(update, 100);
            };

            $scope.getVariablesNames = function() {
                return _($rootScope.variableNames);
            };

            $scope.aceEditorUpdateMode = function (editor) {
                if (editor) {
                    var CSharpExtendableMode = window.ace.require("ace/mode/csharp-extended").Mode;
                    editor.getSession().setMode(new CSharpExtendableMode(function () {
                        return _.pluck($rootScope.variableNames, "name");
                    }));

                    var variablesCompletor =
                    {
                        getCompletions: function (editor, session, pos, prefix, callback) {
                            var i = 0;
                            callback(null,
                                $scope.getVariablesNames()
                                .sortBy('name')
                                .reverse()
                                .map(function (variable) {
                                    return { name: variable.name, value: variable.name, score: i++, meta: variable.type }
                                }));
                        },

                        identifierRegexps: [/[@a-zA-Z_0-9\$\-\u00A2-\uFFFF]/]
                    };

                    var lang_tool = ace.require("ace/ext/language_tools");
                    lang_tool.setCompleters([variablesCompletor]);
                }
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
