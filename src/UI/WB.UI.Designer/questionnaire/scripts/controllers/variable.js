angular.module('designerApp')
    .controller('VariableCtrl',
        function ($scope, $rootScope, $state, questionnaireService, commandService, hotkeys, utilityService) {
            $scope.currentChapterId = $state.params.chapterId;

            var markFormAsChanged = function () {
                if ($scope.variableForm) {
                    $scope.variableForm.$setDirty();
                }
            }

            var bindVariable = function (variable) {
                $scope.activeVariable = $scope.activeVariable || {};
                $scope.activeVariable.breadcrumbs = variable.breadcrumbs;

                $scope.activeVariable.itemId = $state.params.itemId;
                $scope.activeVariable.variable = variable.variable;
                $scope.activeVariable.expression = variable.expression;
                $scope.activeVariable.type = variable.type;

                $scope.activeVariable.typeOptions = variable.typeOptions;
                $scope.activeVariable.typeName = _.find($scope.activeVariable.typeOptions, { value: variable.type }).text;
                $scope.activeVariable.label = variable.label;
            };

            $scope.setType = function (type) {
                $scope.activeVariable.type = type;
                $scope.activeVariable.typeName = _.find($scope.activeVariable.typeOptions, { value: type }).text;
                $rootScope.addOrUpdateLocalVariable($scope.activeVariable.itemId, $scope.activeVariable.variable, type);

                markFormAsChanged();
            };

            $scope.saveVariable = function(callback) {
                if ($scope.variableForm.$valid) {
                    commandService.updateVariable($state.params.questionnaireId, $scope.activeVariable).then(function () {
                        $scope.initialVariable = angular.copy($scope.activeVariable);
                        $rootScope.$emit('variableUpdated', {
                            itemId: $scope.activeVariable.itemId,
                            name: $scope.activeVariable.variable,
                            label: $scope.activeVariable.label,
                            type: $scope.activeVariable.type
                    });
                        if (_.isFunction(callback)) {
                            callback();
                        }
                    });
                }
            }

            $scope.cancel = function () {
                var temp = angular.copy($scope.initialVariable);
                bindVariable(temp);
            };

            var saveCombination = 'ctrl+s';
            if (hotkeys.get(saveCombination) !== false) {
                hotkeys.del(saveCombination);
            }

            hotkeys.bindTo($scope)
               .add({
                   combo: saveCombination,
                   description: 'Save changes',
                   allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                   callback: function (event) {
                       if ($scope.questionnaire !== null && !$scope.questionnaire.isReadOnlyForUser) {
                           if ($scope.variableForm.$dirty) {
                               $scope.saveVariable();
                               $scope.variableForm.$setPristine();
                           }
                           event.preventDefault();
                       }
                   }
               });

            var loadVariable = function() {
                questionnaireService.getVariableDetailsById($state.params.questionnaireId, $state.params.itemId)
                    .then(function(result) {
                        var data = result.data;
                        $scope.initialVariable = angular.copy(data);
                        bindVariable(data);

                        var focusId = null;
                        switch ($state.params.property) {
                            case 'VariableName':
                                focusId = 'edit-question-variable-name';
                                break;
                            case 'VariableContent':
                                focusId = "edit-group-condition";
                                break;
                            case 'VariableLabel':
                                focusId = "edit-variable-title-highlight";
                                break;
                            default:
                                break;
                        }

                        utilityService.setFocusIn(focusId);
                    });
            };

            $scope.$on('verifing', function (scope, params) {
                if ($scope.variableForm.$dirty)
                    $scope.saveVariable(function () {
                        $scope.variableForm.$setPristine();
                    });
            });

            loadVariable();
        }
    );
