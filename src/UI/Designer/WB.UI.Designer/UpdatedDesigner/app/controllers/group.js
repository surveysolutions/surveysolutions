(function() {
    'use strict';

    angular.module('designerApp')
        .controller('GroupCtrl', [
            '$scope', '$stateParams', 'questionnaireService', 'commandService', 'utilityService',
            function($scope, $stateParams, questionnaireService, commandService, math) {

                $scope.loadGroup = function() {
                    questionnaireService.getGroupDetailsById($stateParams.questionnaireId, $stateParams.itemId).success(function(result) {
                            $scope.activeChapter = result;
                            $scope.activeChapter.group.itemId = $stateParams.itemId;
                        }
                    );
                };

                $scope.saveChapter = function() {
                    $("#edit-chapter-save-button").popover('destroy');
                    commandService.updateGroup($stateParams.questionnaireId, $scope.activeChapter.group).success(function (result) {
                        if (!result.IsSuccess) {
                            $("#edit-chapter-save-button").popover({
                                content: result.Error,
                                placement: top,
                                animation: true
                            }).popover('show');
                        }
                    });
                };

                $scope.deleteChapter = function() {
                    if (confirm("Are you sure want to delete?")) {
                        commandService.deleteGroup($stateParams.questionnaireId, $stateParams.itemId).success(function(result) {
                            $("#edit-chapter-save-button").popover('destroy');
                            if (result.IsSuccess) {
                                if ($scope.isChapter($scope.activeChapter)) {
                                    var index = $scope.questionnaire.chapters.indexOf($scope.activeChapter.group);
                                    if (index > -1) {
                                        $scope.questionnaire.chapters.splice(index, 1);
                                    }
                                } else {
                                    $scope.activeChapter.isDeleted = true;
                                }
                            } else {
                                $("#edit-chapter-save-button").popover({
                                    content: result.Error,
                                    placement: top,
                                    animation: true
                                }).popover('show');
                            }
                        });
                    }
                };

                $scope.cloneChapter = function() {
                    var newId = math.guid();
                    var chapterDescription = "";

                    commandService.cloneGroupWithoutChildren($stateParams.questionnaireId, newId, $scope.activeChapter.group, chapterDescription).success(function(result) {
                        $("#edit-chapter-save-button").popover('destroy');
                        if (result.IsSuccess) {
                            var newChapter = {
                                title: $scope.activeChapter.title,
                                itemId: newId,
                                description: chapterDescription
                            };
                            $scope.questionnaire.chapters.push(newChapter);
                            $scope.close();
                        } else {
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