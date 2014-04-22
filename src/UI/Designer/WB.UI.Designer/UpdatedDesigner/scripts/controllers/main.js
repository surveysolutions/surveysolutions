'use strict';

angular.module('designerApp')
    .controller('MainCtrl', function ($scope, $routeParams, $location, $route, questionnaireService, verificationService) {

        $scope.chapters = [];

        $scope.items = [];

        $scope.item = null;

        $scope.questionnaire = null;

        $scope.verificationStatus = {
            errorsCount: 8,
            errors: []
        };

        $scope.setItem = function (item) {
            $location.path('/' + $routeParams.questionnaireId + '/chapter/' + $scope.currentChapterId + '/item/' + item.Id);
            $scope.currentItemId = item.Id;
        };

        $scope.submit = function() {
            console.log('submit');
        };

        $scope.verify = function() {
            verificationService.verify($routeParams.questionnaireId).success(function (result) {
                    $scope.verificationStatus.errors = result.errors;
                    $scope.verificationStatus.errorsCount = result.errors.length;
                });
        };

        $scope.editChapter = function(chapter) {
            console.log(chapter);
        };

        questionnaireService.getQuestionnaireById($routeParams.questionnaireId)
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
            questionnaireService.getChapterById(questionnaireId, chapterId)
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