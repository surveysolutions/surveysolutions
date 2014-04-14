'use strict';

angular.module('pocAngularApp')
  .controller('MainCtrl', function ($scope, $http, $routeParams) {

    $scope.chapters = [];
    $scope.items = [];

    $scope.item = null;

    $scope.isFolded = false;

    $scope.setItem = function (item) {
      $scope.item = item;
    };

    $scope.changeChapter = function (chapter) {
        $scope.currentChapter = chapter;
        loadChapterDetails();
    };

    $scope.submit = function () {
      console.log('submit');
    };

    $scope.unfold = function () {
        $scope.isFolded = true;
    };

    $scope.foldback = function () {
        $scope.isFolded = false;
    };

    $scope.addNewChapter = function() {
        var newChapter = {
            Title: 'New Chapter',
            GroupId: "6e240642274c4bdea937baa78cd4ad6f",
            Statistics: {
                QuestionsCount: 0,
                GroupsCount: 0,
                RostersCount: 0
            }
        };
        $scope.questionnaire.Chapters.push(newChapter);
    }

    $http.get('api/questionnaire/get/' + $routeParams.questionnaireId)
        .success(function (result) {
            $scope.questionnaire = result;
            $scope.currentChapter = result.Chapters[0];
            loadChapterDetails();
        });

    function loadChapterDetails() {
        $http.get('api/questionnaire/chapter/' + $routeParams.questionnaireId + "?chapterId=" + $scope.currentChapter.GroupId)
            .success(function (result) {
                $scope.items = result.Groups;
        });
    }
  });