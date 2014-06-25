(function() {
    'use strict';

    angular.module('designerApp')
        .controller('ChapterCtrl', [
            '$scope', '$routeParams', 'questionnaireService', 'commandService', 'utilityService', 'navigationService',
            function($scope, $routeParams, questionnaireService, commandService, math, navigationService) {

                $scope.loadGroup = function() {
                    questionnaireService.getGroupDetailsById($routeParams.questionnaireId, $scope.activeChapter.itemId).success(function(result) {
                            var group = result.group;
                            $scope.activeChapter.title = group.title;
                            $scope.activeChapter.description = group.description;
                            $scope.activeChapter.enablementCondition = group.enablementCondition;
                            $scope.activeChapter.breadcrumbs = result.breadcrumbs;
                        }
                    );
                };

                $scope.saveChapter = function() {
                    $("#edit-chapter-save-button").popover('destroy');
                    commandService.updateGroup($routeParams.questionnaireId, $scope.activeChapter).success(function(result) {
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
                        commandService.deleteGroup($routeParams.questionnaireId, $scope.activeChapter.itemId).success(function(result) {
                            $("#edit-chapter-save-button").popover('destroy');
                            if (result.IsSuccess) {
                                if ($scope.isChapter($scope.activeChapter)) {
                                    var index = $scope.questionnaire.chapters.indexOf($scope.activeChapter);
                                    if (index > -1) {
                                        $scope.questionnaire.chapters.splice(index, 1);
                                    }
                                } else {
                                    $scope.activeChapter.isDeleted = true;
                                }
                                navigationService.openQuestionnaire($routeParams.questionnaireId);
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

                    commandService.cloneGroupWithoutChildren($routeParams.questionnaireId, newId, $scope.activeChapter, chapterDescription).success(function(result) {
                        $("#edit-chapter-save-button").popover('destroy');
                        if (result.IsSuccess) {
                            var newChapter = {
                                title: $scope.activeChapter.title,
                                itemId: newId,
                                description: chapterDescription
                            };
                            $scope.questionnaire.chapters.push(newChapter);
                            $scope.close();
                            navigationService.openChapter($routeParams.questionnaireId, newId);
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
                    questionnaireService.moveGroup($scope.activeChapter.itemId, 0, chapterId, $routeParams.questionnaireId);
                    var removeFrom = $scope.activeChapter.getParentItem() || $scope;
                    removeFrom.items.splice(_.indexOf(removeFrom.items, $scope.activeChapter), 1);
                    $scope.resetSelection();
                };

                $scope.loadGroup();

                $scope.$watch('activeChapter', function() {
                    $scope.loadGroup();
                });
            }
        ]);
}());