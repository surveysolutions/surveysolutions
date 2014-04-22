'use strict';

angular.module('designerApp')
    .controller('MainCtrl', function($scope, $http, $routeParams, $location, $route, commandService) {

        $scope.chapters = [];

        $scope.items = [];

        $scope.item = null;

        $scope.questionnaire = null;

        $scope.verificationStatus = {
            errorsCount: 8,
            errors: []
        };

        $scope.isFolded = false;

        $scope.changeChapter = function(chapter) {
            $location.path('/' + $routeParams.questionnaireId + '/chapter/' + chapter.ChapterId);
            $scope.currentChapterId = chapter.ChapterId;
            loadChapterDetails($routeParams.questionnaireId, $scope.currentChapterId);
        };

        $scope.setItem = function (item) {
            $location.path('/' + $routeParams.questionnaireId + '/chapter/' + $scope.currentChapterId + '/item/' + item.Id);
            $scope.currentItemId = item.Id;
        };

        $scope.submit = function() {
            console.log('submit');
        };

        $scope.unfold = function() {
            $scope.isFolded = true;
        };

        $scope.foldback = function() {
            $scope.isFolded = false;
        };

        $scope.verify = function() {
            $http.get('api/questionnaire/verify/' + $routeParams.questionnaireId)
                .success(function(result) {
                    $scope.verificationStatus.errors = result.errors;
                    $scope.verificationStatus.errorsCount = result.errors.length;
                });
        };

        $scope.addNewChapter = function() {
            var newId = guid();

            var newChapter = {
                Title: 'New Chapter',
                ChapterId: newId,
                QuestionsCount: 0,
                GroupsCount: 0,
                RostersCount: 0
            };

            commandService.addGroup($routeParams.questionnaireId, newChapter).success(function () {
                $scope.questionnaire.Chapters.push(newChapter);
            });
        };

        $scope.cloneChapter = function(chapter) {
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

        $scope.deleteChapter = function(chapter) {
            if (confirm("Are you sure want to delete?")) {
                commandService.deleteGroup($routeParams.questionnaireId, chapter).success(function () {
                    var index = $scope.questionnaire.Chapters.indexOf(chapter);
                    if (index > -1) {
                        $scope.questionnaire.Chapters.splice(index, 1);
                    }
                });
            }
        };

        $scope.editChapter = function(chapter) {
            console.log(chapter);
        };

        $http.get('api/questionnaire/get/' + $routeParams.questionnaireId)
            .success(function(result) {
                if (result == 'null') {
                    alert('Questionnaire not found');
                } else {
                    $scope.questionnaire = result;

                    if ($routeParams.chapterId) {
                        $scope.currentChapterId = $routeParams.chapterId;
                        loadChapterDetails($routeParams.questionnaireId, $scope.currentChapterId);
                    } else {
                        $scope.currentChapter = result.Chapters[0];
                        $scope.currentChapterId = $scope.currentChapter.ChapterId;
                        loadChapterDetails($routeParams.questionnaireId, $scope.currentChapter.ChapterId);
                    }
                    if ($routeParams.itemId) {
                        $scope.currentItemId = $routeParams.itemId;
                    }
                }
            });

        //do not reload views, change url only
        var lastRoute = $route.current;
        $scope.$on('$locationChangeSuccess', function(event) {
            $route.current = lastRoute;
        });

        function loadChapterDetails(questionnaireId, chapterId) {
            $http.get('api/questionnaire/chapter/' + questionnaireId + "?chapterId=" + chapterId)
                .success(function(result) {
                    $scope.items = result.Items;
                    $scope.currentChapter = result;
                });
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