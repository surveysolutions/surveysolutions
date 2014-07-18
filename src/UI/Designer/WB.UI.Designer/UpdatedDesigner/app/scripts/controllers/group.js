(function() {
    'use strict';

    angular.module('designerApp')
        .controller('GroupCtrl', [
            '$rootScope', '$scope', '$stateParams', 'questionnaireService', 'commandService', 'utilityService', '$log',
            function($rootScope, $scope, $stateParams, questionnaireService, commandService, utilityService, $log) {

                var dataBind = function (result) {
                    $scope.activeChapter = result;
                    $scope.activeChapter.isChapter = ($stateParams.itemId == $stateParams.chapterId);
                    $scope.activeChapter.group.itemId = $stateParams.itemId;
                    $scope.activeChapter.group.variableName = $stateParams.variableName;
                };

                $scope.loadGroup = function() {
                    questionnaireService.getGroupDetailsById($stateParams.questionnaireId, $stateParams.itemId).success(function(result) {
                            dataBind(result);
                            $scope.initialGroup = angular.copy(result);
                            utilityService.focus('focusGroup');
                        }
                    );
                };

                $scope.saveChapter = function() {
                    commandService.updateGroup($stateParams.questionnaireId, $scope.activeChapter.group).success(function(result) {
                        $scope.initialGroup = angular.copy($scope.activeChapter);
                        $rootScope.$emit('groupUpdated', {
                            itemId: $scope.activeChapter.group.itemId,
                            title: $scope.activeChapter.group.title
                        });
                    });
                };

                $scope.cancelGroup = function() {
                    var temp = angular.copy($scope.initialGroup);
                    dataBind(temp);
                    $scope.groupform.$setPristine();
                };

                $scope.deleteItem = function () {
                    if ($scope.activeChapter.isChapter) {
                        $rootScope.$emit('deleteChapter', {
                            chapter: $scope.activeChapter.group
                        });
                    } else {
                        $scope.deleteGroup($scope.activeChapter.group);
                    }
                };

                //$scope.moveToChapter = function(chapterId) {
                //    questionnaireService.moveGroup($scope.activeChapter.itemId, 0, chapterId, $stateParams.questionnaireId);
                //    var removeFrom = $scope.activeChapter.getParentItem() || $scope;
                //    removeFrom.items.splice(_.indexOf(removeFrom.items, $scope.activeChapter), 1);
                //    $scope.resetSelection();
                //};

                $scope.loadGroup();
            }
        ]);
}());