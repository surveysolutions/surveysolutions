'use strict';

angular.module('designerApp')
    .controller('ChaptersCtrl', [
        '$scope', '$routeParams', '$location', '$route', 'commandService', 'utilityService', 'navigationService',
        function($scope, $routeParams, $location, $route, commandService, math, navigationService) {

            $scope.chapters = [];

            $scope.isFolded = false;

            $scope.unfold = function() {
                $scope.isFolded = true;
            };

            $scope.foldback = function() {
                $scope.isFolded = false;
            };

            $scope.openMenu = function(chapter) {
                chapter.isMenuOpen = true;
            };

            $scope.editChapter = function(chapter) {
                console.log(chapter);
                chapter.isMenuOpen = false;
                chapter.itemId = chapter.chapterId;
                $scope.setItem(chapter);
                //navigationService.editChapter($routeParams.questionnaireId, chapter.chapterId);
            };

            $scope.addNewChapter = function() {
                var newId = math.guid();

                var newChapter = {
                    title: 'New Chapter',
                    chapterId: newId,
                    questionsCount: 0,
                    groupsCount: 0,
                    rostersCount: 0
                };

                commandService.addChapter($routeParams.questionnaireId, newChapter).success(function() {
                    $scope.questionnaire.chapters.push(newChapter);
                });
            };

            $scope.cloneChapter = function(chapter) {
                chapter.isMenuOpen = false;
                var newId = math.guid();
                var chapterDescription = "";

                commandService.cloneGroupWithoutChildren($routeParams.questionnaireId, newId, chapter, chapterDescription).success(function() {
                    var newChapter = {
                        title: chapter.title,
                        chapterId: newId,
                        questionsCount: 0,
                        groupsCount: 0,
                        rostersCount: 0
                    };
                    $scope.questionnaire.chapters.push(newChapter);
                });
            };

            $scope.deleteChapter = function(chapter) {
                chapter.isMenuOpen = false;
                if (confirm("Are you sure want to delete?")) {
                    commandService.deleteGroup($routeParams.questionnaireId, chapter).success(function() {
                        var index = $scope.questionnaire.chapters.indexOf(chapter);
                        if (index > -1) {
                            $scope.questionnaire.chapters.splice(index, 1);
                        }
                    });
                }
            };
        }
    ]);