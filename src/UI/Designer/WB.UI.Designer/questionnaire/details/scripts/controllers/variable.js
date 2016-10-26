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
                $scope.activeVariable.variable = variable.name;
                $scope.activeVariable.expression = variable.expression;
                $scope.activeVariable.type = variable.type;

                $scope.activeVariable.typeOptions = variable.typeOptions;
                $scope.activeVariable.typeName = _.find($scope.activeVariable.typeOptions, { value: variable.type }).text;
            };

            $scope.setType = function (type) {
                $scope.activeVariable.type = type;
                $scope.activeVariable.typeName = _.find($scope.activeVariable.typeOptions, { value: type }).text;

                markFormAsChanged();
            };

            $scope.saveVariable = function(callback) {
                if ($scope.variableForm.$valid) {
                    commandService.updateVariable($state.params.questionnaireId, $scope.activeVariable).success(function () {
                        $scope.initialVariable = angular.copy($scope.activeVariable);
                        $rootScope.$emit('variableUpdated', {
                            itemId: $scope.activeVariable.itemId,
                            name: $scope.activeVariable.variable
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
                           $scope.saveVariable();
                           $scope.variableForm.$setPristine();
                           event.preventDefault();
                       }
                   }
               });

            var loadVariable = function() {
                questionnaireService.getVariableDetailsById($state.params.questionnaireId, $state.params.itemId)
                    .success(function(result) {
                        $scope.initialVariable = angular.copy(result);
                        bindVariable(result);

                        var focusId = null;
                        switch ($state.params.property) {
                            case 'VariableName':
                                focusId = 'edit-question-variable-name';
                                break;
                            case 'VariableContent':
                                focusId = "edit-group-condition";
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
