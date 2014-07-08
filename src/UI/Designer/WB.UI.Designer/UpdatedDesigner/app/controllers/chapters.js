(function() {
    'use strict';

    angular.module('designerApp')
        .controller('ChaptersCtrl', [
            '$rootScope', '$scope', '$state', 'commandService', 'utilityService', 
            function ($rootScope, $scope, $state, commandService, math) {

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
                    chapter.itemId = chapter.itemId;
                    $scope.setItem(chapter);
                };

                $scope.addNewChapter = function() {
                    var newId = math.guid();

                    var newChapter = {
                        title: 'New Chapter',
                        itemId: newId
                    };

                    commandService.addChapter($state.params.questionnaireId, newChapter).success(function() {
                        $scope.questionnaire.chapters.push(newChapter);
                    });
                };

                $scope.cloneChapter = function(chapter) {
                    chapter.isMenuOpen = false;
                    var newId = math.guid();
                    var chapterDescription = "";

                    commandService.cloneGroupWithoutChildren($state.params.questionnaireId, newId, chapter, chapterDescription).success(function () {
                        var newChapter = {
                            title: chapter.title,
                            itemId: newId
                        };
                        $scope.questionnaire.chapters.push(newChapter);
                        navigationService.openChapter($state.params.questionnaireId, newId);
                    });
                };

                $rootScope.$on('$stateChangeSuccess', function () {
                    $scope.foldback();
                });
            }
        ]);
}());