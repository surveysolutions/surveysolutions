angular.module('designerApp')
    .controller('GroupCtrl',
        function ($rootScope, $scope, $state, $timeout, questionnaireService, commandService, hotkeys, utilityService) {
            $scope.currentChapterId = $state.params.chapterId;

            var saveGroup = 'ctrl+s';
            if (hotkeys.get(saveGroup) !== false) {
                hotkeys.del(saveGroup);
            }

            hotkeys.bindTo($scope)
                .add({
                    combo: saveGroup,
                    description: 'Save changes',
                    allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                    callback: function(event) {
                        if ($scope.questionnaire !== null && !$scope.questionnaire.isReadOnlyForUser) {
                            $scope.saveGroup();
                            $scope.groupForm.$setPristine();
                            event.preventDefault();
                        }
                    }
                });
            
            var dataBind = function (group) {
                $scope.activeGroup = group;

                $scope.activeGroup.isChapter = ($state.params.itemId === $state.params.chapterId);
                $scope.activeGroup.itemId = $state.params.itemId;
                $scope.activeGroup.variableName = $state.params.variableName;

                if (!_.isNull($scope.groupForm) && !_.isUndefined($scope.groupForm)) {
                    $scope.groupForm.$setPristine();
                }
            };

            $scope.loadGroup = function () {
                questionnaireService.getGroupDetailsById($state.params.questionnaireId, $state.params.itemId)
                    .success(function(result) {
                            dataBind(result.group);
                            $scope.activeGroup.breadcrumbs = result.breadcrumbs;
                            $scope.initialGroup = angular.copy($scope.activeGroup);
                            
                            var focusId = null;
                            switch ($state.params.property) {
                                case 'Title':
                                    focusId = 'edit-group-title';
                                    break;
                                case 'EnablingCondition':
                                    focusId = 'edit-group-condition';
                                    break;
                                    
                                default:
                                    break;
                            }
                            utilityService.setFocusIn(focusId);
                        }
                    );
            };

            $scope.saveGroup = function (callback) {
                if ($scope.groupForm.$valid) {
                    commandService.updateGroup($state.params.questionnaireId, $scope.activeGroup).success(function () {
                        $scope.initialGroup = angular.copy($scope.activeGroup);
                        $rootScope.$emit('groupUpdated', {
                            itemId: $scope.activeGroup.itemId,
                            title: $scope.activeGroup.title,
                            hasCondition: ($scope.activeGroup.enablementCondition !== null && /\S/.test($scope.activeGroup.enablementCondition))
                        });
                        if (_.isFunction(callback)) {
                            callback();
                        }
                    });
                }
            };

            $scope.$on('verifing', function (scope, params) {
                if ($scope.groupForm.$dirty)
                    $scope.saveGroup(function() {
                        $scope.groupForm.$setPristine();
                    });
            });

            $scope.cancelGroup = function () {
                var temp = angular.copy($scope.initialGroup);
                dataBind(temp);
            };

            $scope.deleteItem = function () {
                if ($scope.activeGroup.isChapter) {
                    $rootScope.$emit('deleteChapter', {
                        chapter: $scope.activeGroup
                    });
                } else {
                    $scope.deleteGroup($scope.activeGroup);
                }
            };

            if ($scope.questionnaire) {
                $scope.loadGroup();
            } else {
                $rootScope.$on('questionnaireLoaded', function () {
                    $scope.loadGroup();

                });
            }

        }
    );
