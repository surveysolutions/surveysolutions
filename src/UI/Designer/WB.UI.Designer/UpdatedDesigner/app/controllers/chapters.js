(function() {
    'use strict';

    angular.module('designerApp')
        .controller('ChaptersCtrl', [
            '$scope', '$location', 'commandService', 'utilityService', 'navigationService',
            function($scope, $location, commandService, math, navigationService) {

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
                $scope.changeChapter = function (chapter) {
                    navigationService.openChapter($routeParams.questionnaireId, chapter.itemId);
                    $scope.$parent.currentChapterId = chapter.itemId;
                    $scope.$parent.loadChapterDetails($routeParams.questionnaireId, $scope.currentChapterId);
                    $scope.foldback();
                };

                $scope.editChapter = function(chapter) {
                    console.log(chapter);
                    chapter.isMenuOpen = false;
                    chapter.itemId = chapter.itemId;
                    $scope.setItem(chapter);
                    //navigationService.editChapter($routeParams.questionnaireId, chapter.itemId);
                };

                $scope.addNewChapter = function() {
                    var newId = math.guid();

                    var newChapter = {
                        title: 'New Chapter',
                        itemId: newId
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
                            itemId: newId
                        };
                        $scope.questionnaire.chapters.push(newChapter);
                        navigationService.openChapter($routeParams.questionnaireId, newId);
                    });
                };

                $scope.deleteChapter = function(chapter) {
                    chapter.isMenuOpen = false;
                    if (confirm("Are you sure want to delete?")) {
                        commandService.deleteGroup($routeParams.questionnaireId, chapter.itemId).success(function() {
                            var index = $scope.questionnaire.chapters.indexOf(chapter);
                            if (index > -1) {
                                $scope.questionnaire.chapters.splice(index, 1);
                            }
                        });
                    }
                };
            }
        ]);
}());