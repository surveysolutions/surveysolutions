(function() {
    'use strict';

    angular.module('designerApp')
        .controller('ChaptersCtrl', [
            '$scope', '$stateParams', 'commandService', 'utilityService', 
            function ($scope, $stateParams,commandService, math) {

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
                    navigationService.openChapter($stateParams.questionnaireId, chapter.itemId);
                    $scope.$parent.currentChapterId = chapter.itemId;
                    $scope.$parent.loadChapterDetails($stateParams.questionnaireId, $scope.currentChapterId);
                    $scope.foldback();
                };

                $scope.editChapter = function(chapter) {
                    console.log(chapter);
                    chapter.isMenuOpen = false;
                    chapter.itemId = chapter.itemId;
                    $scope.setItem(chapter);
                    //navigationService.editChapter($stateParams.questionnaireId, chapter.itemId);
                };

                $scope.addNewChapter = function() {
                    var newId = math.guid();

                    var newChapter = {
                        title: 'New Chapter',
                        itemId: newId
                    };

                    commandService.addChapter($stateParams.questionnaireId, newChapter).success(function() {
                        $scope.questionnaire.chapters.push(newChapter);
                    });
                };

                $scope.cloneChapter = function(chapter) {
                    chapter.isMenuOpen = false;
                    var newId = math.guid();
                    var chapterDescription = "";

                    commandService.cloneGroupWithoutChildren($stateParams.questionnaireId, newId, chapter, chapterDescription).success(function() {
                        var newChapter = {
                            title: chapter.title,
                            itemId: newId
                        };
                        $scope.questionnaire.chapters.push(newChapter);
                        navigationService.openChapter($stateParams.questionnaireId, newId);
                    });
                };
            }
        ]);
}());