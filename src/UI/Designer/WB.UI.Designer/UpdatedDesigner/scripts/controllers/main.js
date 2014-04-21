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
          var newId = guid();
          console.log(newId);
          var newChapter = {
              Title: 'New Chapter',
              GroupId: newId,
              QuestionsCount: 0,
              GroupsCount: 0,
              RostersCount: 0
          };
          $scope.currentChapterId = newId;
          $scope.questionnaire.Chapters.push(newChapter);

          $http({
              method: 'POST',
              url: 'command/execute',
              data: {
                  "type": "AddGroup",
                  "command": "{\"questionnaireId\":\"7c97b192-5b02-44b7-82ed-6741a5035fae\",\"groupId\":\"" + newChapter.GroupId + "\",\"title\":\""+ newChapter.Title +"\",\"description\":\"\",\"condition\":\"\",\"isRoster\":false,\"rosterSizeQuestionId\":null,\"rosterSizeSource\":\"Question\",\"rosterFixedTitles\":null,\"rosterTitleQuestionId\":null,\"parentGroupId\":null}"
              },
              headers: { 'Content-Type': 'application/json; ' }
          }).success();
      };

      $scope.editChapter = function (chapter) {
          console.log(chapter);
      };

      $scope.cloneChapter = function (chapter) {
          $scope.questionnaire.Chapters.push(chapter);
      };

      $scope.deleteChapter = function (chapter) {
          $http({
              method: 'POST',
              url: 'command/execute',
              data: {
                  "type": "DeleteGroup",
                  "command": "{\"questionnaireId\":\"7c97b192-5b02-44b7-82ed-6741a5035fae\",\"groupId\":\"" + chapter.ChapterId + "\"}"
              },
              headers: { 'Content-Type': 'application/json;' }
          }).success(function() {
              var index = $scope.questionnaire.Chapters.indexOf(chapter);
              if (index > -1) {
                  $scope.questionnaire.Chapters.splice(index, 1);
              }
          });
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
                    $scope.items = result.Items;
                    $scope.currentChapter = result;
                    //console.log(JSON.stringify($scope.currentChapter));
          });
      };

      function guid() {
          function s4() {
              return Math.floor((1 + Math.random()) * 0x10000)
                         .toString(16)
                         .substring(1);
          }
          return s4() + s4() + s4()+ s4()+
                 s4() + s4() + s4() + s4();
      };
});