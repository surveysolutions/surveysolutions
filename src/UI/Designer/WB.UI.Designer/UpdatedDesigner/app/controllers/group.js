(function() {
    'use strict';

    angular.module('designerApp')
        .controller('GroupCtrl', [
            '$scope', '$stateParams', 'questionnaireService', 'commandService', '$log',
            function($scope, $stateParams, questionnaireService, commandService, $log) {

                $scope.loadGroup = function() {
                    questionnaireService.getGroupDetailsById($stateParams.questionnaireId, $stateParams.itemId).success(function(result) {
                            $scope.activeChapter = result;
                            $scope.activeChapter.group.itemId = $stateParams.itemId;
                            $scope.activeChapter.group.variableName = $stateParams.variableName;
                        }
                    );
                };

                $scope.saveChapter = function () {
                    commandService.updateGroup($stateParams.questionnaireId, $scope.activeChapter.group);
                };

                $scope.moveToChapter = function(chapterId) {
                    questionnaireService.moveGroup($scope.activeChapter.itemId, 0, chapterId, $stateParams.questionnaireId);
                    var removeFrom = $scope.activeChapter.getParentItem() || $scope;
                    removeFrom.items.splice(_.indexOf(removeFrom.items, $scope.activeChapter), 1);
                    $scope.resetSelection();
                };

                $scope.loadGroup();
            }
        ]);
}());