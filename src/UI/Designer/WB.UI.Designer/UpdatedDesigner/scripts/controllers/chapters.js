'use strict';

angular.module('designerApp')
    .controller('ChaptersCtrl', function ($scope, $routeParams, $location, $route, commandService) {

        $scope.chapters = [];

        $scope.isFolded = false;

        $scope.unfold = function () {
            $scope.isFolded = true;
        };

        $scope.foldback = function () {
            $scope.isFolded = false;
        };

        $scope.editChapter = function (chapter) {
            console.log(chapter);
        };

        $scope.addNewChapter = function () {
            var newId = guid();

            var newChapter = {
                Title: 'New Chapter',
                ChapterId: newId,
                QuestionsCount: 0,
                GroupsCount: 0,
                RostersCount: 0
            };

            commandService.addChapter($routeParams.questionnaireId, newChapter).success(function () {
                $scope.questionnaire.Chapters.push(newChapter);
            });
        };

        $scope.cloneChapter = function (chapter) {
            var newId = guid();
            var chapterDescription = "";

            commandService.cloneGroupWithoutChildren($routeParams.questionnaireId, newId, chapter, chapterDescription).success(function () {
                var newChapter = {
                    Title: chapter.Title,
                    ChapterId: newId,
                    QuestionsCount: 0,
                    GroupsCount: 0,
                    RostersCount: 0
                };
                $scope.questionnaire.Chapters.push(newChapter);
            });
        };

        $scope.deleteChapter = function (chapter) {
            if (confirm("Are you sure want to delete?")) {
                commandService.deleteGroup($routeParams.questionnaireId, chapter).success(function () {
                    var index = $scope.questionnaire.Chapters.indexOf(chapter);
                    if (index > -1) {
                        $scope.questionnaire.Chapters.splice(index, 1);
                    }
                });
            }
        };

        function guid() {
            function s4() {
                return Math.floor((1 + Math.random()) * 0x10000)
                    .toString(16)
                    .substring(1);
            }

            return s4() + s4() + s4() + s4() +
                s4() + s4() + s4() + s4();
        };
    });