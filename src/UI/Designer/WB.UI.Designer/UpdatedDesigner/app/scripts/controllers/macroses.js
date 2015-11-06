angular.module('designerApp')
    .controller('MacrosesCtrl',
        function ($rootScope, $scope, $state, hotkeys, commandService, utilityService) {
            'use strict';

            var hideMacrosesPane = 'ctrl+m';

            if (hotkeys.get(hideMacrosesPane) !== false) {
                hotkeys.del(hideMacrosesPane);
            }

            hotkeys.add(hideMacrosesPane, 'Close macroses panel', function (event) {
                event.preventDefault();
                $scope.foldback();
            });

            $scope.macroses = [];

            var dataBind = function (macros, macrosDto) {
                macros.initialMacros = angular.copy(macrosDto);

                macros.itemId = macrosDto.itemId;
                macros.name = macrosDto.name;
                macros.description = macrosDto.description;
                macros.expression = macrosDto.expression;
                macros.isDescriptionVisible = !_.isEmpty(macros.description);
            };

            $scope.loadMacroses = function () {
                if ($scope.questionnaire == null || $scope.questionnaire.macroses == null)
                    return;

                _.each($scope.questionnaire.macroses, function (macrosDto) {
                    var macros = {};
                    dataBind(macros, macrosDto);
                    $scope.macroses.push(macros);
                });
            };

            $scope.isFolded = false;

            $scope.unfold = function () {
                $scope.isFolded = true;
            };

            $scope.addNewMacros = function () {
                var newId = utilityService.guid();

                var newMacros = {
                    itemId: newId
                };

                commandService.addMacros($state.params.questionnaireId, newMacros).success(function () {
                    var macros = {};
                    dataBind(macros, newMacros);
                    $scope.macroses.push(macros);
                });
            };

            $scope.saveMacros = function (macros, form) {
                commandService.updateMacros($state.params.questionnaireId, macros).success(function () {
                    form.$setPristine();
                });
            }

            $scope.cancel = function (macros, form) {
                var temp = angular.copy(macros.initialMacros);
                dataBind(macros, temp);
                form.$setPristine();
            }

            $scope.deleteMacros = function (index) {
                var macros = $scope.macroses[index];
                commandService.deleteMacros($state.params.questionnaireId, macros.itemId).success(function () {
                    $scope.macroses.splice(index, 1);
                });
            }
            
            $scope.getDescriptionBtnName = function (macros) {
                return macros.isDescriptionVisible? "hide" : "add";
            }
            $scope.isDescriptionBtnVisible = function (macros) {
                return _.isEmpty(macros.description);
            }
            $scope.toggleDescription = function (macros) {
                macros.isDescriptionVisible = !macros.isDescriptionVisible;
            }

            $scope.aceLoaded = function (editor) {
                var expressionEditorPlaceholder = "expression";

                // Editor part
                var renderer = editor.renderer;

                // Options
                editor.setOptions({
                    maxLines: Infinity,
                    mode: "ace/mode/csharp",
                    fontSize: 16,
                    highlightActiveLine: false,
                    theme: "ace/theme/github"
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
                setTimeout(update, 100);
            };

            $scope.foldback = function () {
                $scope.isFolded = false;
                $rootScope.$broadcast("closeMacrosesList", {});
            };

            $scope.$on('openMacrosesList', function (scope, params) {
                $scope.unfold();
                if (!_.isUndefined(params) && !_.isUndefined(params.focusOn)) {
                    utilityService.focus("focusMacros" + params.focusOn);
                }
            });

            $scope.$on('closeMacrosesListRequested', function () {
                $scope.foldback();
            });

            $rootScope.$on('questionnaireLoaded', function () {
                $scope.loadMacroses();
            });
        });
