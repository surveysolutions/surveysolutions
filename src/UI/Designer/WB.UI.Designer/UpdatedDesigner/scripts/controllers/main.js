'use strict';

angular.module('designerApp')
    .controller('MainCtrl', function ($scope, $routeParams, $location, $route, questionnaireService) {

        $scope.chapters = [];

        $scope.items = [];

        $scope.item = null;

        $scope.questionnaire = null;

        $scope.setItem = function (item) {
            $location.path('/' + $routeParams.questionnaireId + '/chapter/' + $scope.currentChapterId + '/item/' + item.Id);
            $scope.currentItemId = item.Id;
        };

        $scope.submit = function() {
            console.log('submit');
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
        $scope.$on('$locationChangeSuccess', function() {
            $route.current = lastRoute;
        });

        function loadChapterDetails(questionnaireId, chapterId) {
            questionnaireService.getChapterById(questionnaireId, chapterId)
                .success(function(result) {
                    $scope.items = result.Items;
                    $scope.currentChapter = result;
                });
        };
    });