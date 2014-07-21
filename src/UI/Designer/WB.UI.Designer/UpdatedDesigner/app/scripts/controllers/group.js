(function() {
    'use strict';

    angular.module('designerApp')
        .controller('GroupCtrl', [
            '$rootScope', '$scope', '$stateParams', 'questionnaireService', 'commandService', 'utilityService', '$log',
            function($rootScope, $scope, $stateParams, questionnaireService, commandService, utilityService, $log) {

                var dataBind = function(result) {
                    $scope.activeGroup = result;
                    $scope.activeGroup.isChapter = ($stateParams.itemId == $stateParams.chapterId);
                    $scope.activeGroup.group.itemId = $stateParams.itemId;
                    $scope.activeGroup.group.variableName = $stateParams.variableName;
                };

                $scope.loadGroup = function() {
                    questionnaireService.getGroupDetailsById($stateParams.questionnaireId, $stateParams.itemId).success(function(result) {
                            dataBind(result);
                            $scope.initialGroup = angular.copy(result);
                            utilityService.focus('focusGroup');
                        }
                    );
                };

                $scope.saveGroup = function () {
                    commandService.updateGroup($stateParams.questionnaireId, $scope.activeGroup.group).success(function(result) {
                        $scope.initialGroup = angular.copy($scope.activeGroup);
                        $rootScope.$emit('groupUpdated', {
                            itemId: $scope.activeGroup.group.itemId,
                            title: $scope.activeGroup.group.title
                        });
                    });
                };

                $scope.cancelGroup = function() {
                    var temp = angular.copy($scope.initialGroup);
                    dataBind(temp);
                    $scope.groupform.$setPristine();
                };

                $scope.deleteItem = function() {
                    if ($scope.activeGroup.isChapter) {
                        $rootScope.$emit('deleteChapter', {
                            chapter: $scope.activeGroup.group
                        });
                    } else {
                        $scope.deleteGroup($scope.activeGroup.group);
                    }
                };

                $scope.loadGroup();
            }
        ]);
}());