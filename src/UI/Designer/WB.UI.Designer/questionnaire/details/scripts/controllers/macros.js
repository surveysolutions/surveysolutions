angular.module('designerApp')
    .controller('MacrosCtrl',
        function ($rootScope, $scope, $state, hotkeys, commandService, utilityService, confirmService) {
            'use strict';

            var hideMacrosPane = 'ctrl+m';

            if (hotkeys.get(hideMacrosPane) !== false) {
                hotkeys.del(hideMacrosPane);
            }

            hotkeys.add(hideMacrosPane, 'Close macros panel', function (event) {
                event.preventDefault();
                $scope.foldback();
            });

            $scope.macros = [];

            var dataBind = function (macro, macroDto) {
                macro.initialMacro = angular.copy(macroDto);

                macro.itemId = macroDto.itemId;
                macro.name = macroDto.name;
                macro.description = macroDto.description;
                macro.content = macroDto.content;
                macro.isDescriptionVisible = !_.isEmpty(macro.description);
            };

            $scope.loadMacros = function () {
                if ($scope.questionnaire === null || $scope.questionnaire.macros === null)
                    return;

                _.each($scope.questionnaire.macros, function (macroDto) {
                    var macro = {};
                    if (!_.any($scope.macros, function( elem ) {return elem.itemId === macroDto.itemId;})) {
                        dataBind(macro, macroDto);
                        $scope.macros.push(macro);
                    }
                });
            };

            $scope.isFolded = false;

            $scope.unfold = function () {
                $scope.isFolded = true;
            };

            $scope.addNewMacro = function () {
                var newId = utilityService.guid();

                var newMacros = {
                    itemId: newId
                };

                commandService.addMacro($state.params.questionnaireId, newMacros).success(function () {
                    var macros = {};
                    dataBind(macros, newMacros);
                    $scope.macros.push(macros);
                });
            };

            $scope.saveMacro = function (macro) {
                commandService.updateMacro($state.params.questionnaireId, macro).success(function () {
                    macro.initialMacro = angular.copy(macro);
                    macro.form.$setPristine();
                });
            };

            $scope.cancel = function (macro) {
                var temp = angular.copy(macro.initialMacro);
                dataBind(macro, temp);
                macro.form.$setPristine();
            };

            $scope.deleteMacro = function (index) {
                var macro = $scope.macros[index];
                var macroName = macro.name || "macro with no name";
                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(macroName));

                modalInstance.result.then(function (confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteMacros($state.params.questionnaireId, macro.itemId).success(function () {
                            $scope.macros.splice(index, 1);
                        });
                    }
                });
            };

            $scope.isDescriptionEmpty = function (macro) {
                return _.isEmpty(macro.description);
            };
            $scope.toggleDescription = function (macro) {
                macro.isDescriptionVisible = !macro.isDescriptionVisible;
            };

            $scope.aceLoaded = function (editor) {
                var expressionEditorPlaceholder = "content";

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
                    enableLiveAutocompletion: false
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

            $scope.foldback = function () {
                $scope.isFolded = false;
                $rootScope.$broadcast("closeMacrosList", {});
            };

            $scope.$on('openMacrosList', function (scope, params) {
                $scope.unfold();
                if (!_.isUndefined(params) && !_.isUndefined(params.focusOn)) {
                    setTimeout(function () { utilityService.focus("focusMacro" + params.focusOn); }, 500);
                }
            });

            $scope.$on('closeMacrosListRequested', function () {
                $scope.foldback();
            });

            $rootScope.$on('questionnaireLoaded', function () {
                $scope.loadMacros();
            });

            $scope.$on('verifing', function (scope, params) {
                for (var i = 0; i < $scope.macros.length; i++) {
                    var macro = $scope.macros[i];
                    if (macro.form.$dirty) {
                        $scope.saveMacro(macro);
                    }
                }
            });
        });
