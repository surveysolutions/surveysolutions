(function() {
    'use strict';

    angular.module('designerApp')
        .controller('GroupCtrl', [
            '$scope', '$stateParams', 'questionnaireService', 'commandService', 'utilityService', '$modal',
            function($scope, $stateParams, questionnaireService, commandService, math, $modal) {

                $scope.loadGroup = function() {
                    questionnaireService.getGroupDetailsById($stateParams.questionnaireId, $stateParams.itemId).success(function(result) {
                            $scope.activeChapter = result;
                            $scope.activeChapter.group.itemId = $stateParams.itemId;
                        }
                    );
                };

                $scope.saveChapter = function() {
                    $("#edit-chapter-save-button").popover('destroy');
                    commandService.updateGroup($stateParams.questionnaireId, $scope.activeChapter.group).success(function(result) {
                        if (!result.IsSuccess) {
                            $("#edit-chapter-save-button").popover({
                                content: result.Error,
                                placement: top,
                                animation: true
                            }).popover('show');
                        }
                    });
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