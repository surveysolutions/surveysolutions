'use strict';

angular.module('designerApp')
    .controller('ChapterCtrl', [
        '$scope', '$routeParams', 'questionnaireService', 'commandService', 'utilityService', 'navigationService',
        function($scope, $routeParams, questionnaireService, commandService, math, navigationService) {

            $scope.isChapter = function(item) {
                return item.hasOwnProperty('chapterId');
            };

            $scope.loadGroup = function() {
                questionnaireService.getGroupDetailsById($routeParams.questionnaireId, $scope.activeChapter.itemId).success(function(result) {
                        var group = result.group;
                        $scope.activeChapter.description = group.description;
                        $scope.activeChapter.enablementCondition = group.enablementCondition;
                        $scope.activeChapter.breadcrumbs = result.breadcrumbs;
                    }
                );
            }

            $scope.loadGroup();

            $scope.$watch('activeChapter', function() {
                $scope.loadGroup();
            });

            $scope.close = function() {
                $scope.closePanel();
            }

            $scope.cancel = function() {
                $scope.close();
            }

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
                var itemId = $scope.activeChapter.itemId;
                if ($scope.isChapter($scope.activeChapter)) {
                    itemId = $scope.activeChapter.chapterId;
                }

                if (confirm("Are you sure want to delete?")) {
                    commandService.deleteGroup($routeParams.questionnaireId, itemId).success(function(result) {
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
                            $scope.close();
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

                if (!$scope.isChapter($scope.activeChapter)) {
                    $scope.activeChapter.chapterId = $scope.activeChapter.itemId;
                }

                commandService.cloneGroupWithoutChildren($routeParams.questionnaireId, newId, $scope.activeChapter, chapterDescription).success(function(result) {
                    $("#edit-chapter-save-button").popover('destroy');
                    if (result.IsSuccess) {
                        var newChapter = {
                            title: $scope.activeChapter.title,
                            chapterId: newId,
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
        }
    ]);