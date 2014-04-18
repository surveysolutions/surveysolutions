'use strict';

angular.module('pocAngularApp')
  .controller('MainCtrl', function ($scope, $http, $routeParams, $location, $route) {

      $scope.chapters = [];

      $scope.items = [];

      $scope.item = null;

      $scope.questionnaire = null;

      $scope.verificationStatus = {
          errorsCount: 8,
          errors: []
      };

      $scope.isFolded = false;

      $scope.changeChapter = function (chapter) {
          $location.path('/' + $routeParams.questionnaireId + '/chapter/' + chapter.ChapterId);
          $scope.currentChapterId = chapter.ChapterId;
          loadChapterDetails($routeParams.questionnaireId, $scope.currentChapterId);
      };

      $scope.setItem = function (group, question) {
          $location.path('/' + $routeParams.questionnaireId + '/chapter/' + $scope.currentChapterId + '/item/' + question.QuestionId);
          $scope.currentItemId = question.QuestionId;
      };

      $scope.setGroup = function (group) {
          $location.path('/' + $routeParams.questionnaireId + '/chapter/' + $scope.currentChapterId + '/item/' + group.GroupId);
          $scope.currentItemId = group.GroupId;
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

      $scope.verify = function () {
          $http.get('api/questionnaire/verify/' + $routeParams.questionnaireId)
              .success(function (result) {
                  $scope.verificationStatus.errors = result.errors;
                  $scope.verificationStatus.errorsCount = result.errors.length;
              });
      };

      $scope.addNewChapter = function () {
          var newChapter = {
              Title: 'New Chapter',
              GroupId: "6e240642274c4bdea937baa78cd4ad6f",
              QuestionsCount: 0,
              GroupsCount: 0,
              RostersCount: 0
          };
          $scope.questionnaire.Chapters.push(newChapter);
      };

      $http.get('api/questionnaire/get/' + $routeParams.questionnaireId)
          .success(function (result) {
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

      var lastRoute = $route.current;
      $scope.$on('$locationChangeSuccess', function (event) {
          $route.current = lastRoute;
      });

      function loadChapterDetails(questionnaireId, chapterId) {
          $http.get('api/questionnaire/chapter/' + questionnaireId + "?chapterId=" + chapterId)
                .success(function (result) {
                    $scope.items = result.Groups;
                    $scope.currentChapter = result;
                    console.log(JSON.stringify($scope.currentChapter));
          });
      };
  });