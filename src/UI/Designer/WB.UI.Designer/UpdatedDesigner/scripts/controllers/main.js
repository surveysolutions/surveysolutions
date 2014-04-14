'use strict';

angular.module('pocAngularApp')
  .controller('MainCtrl', function ($scope, $http, $routeParams) {

    $scope.documents = [];

    $scope.chapters = [];

    $scope.item = null;

    $scope.isFolded = false;

    $scope.setItem = function (item) {
      $scope.item = item;
    };

    $scope.changeChapter = function (chapter) {
        $scope.currentChapter = chapter;
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
        });

      //$http.get('UpdatedDesigner/data/data.json')
      //  .then(function(result) {
      //    $scope.documents = result.data;
      //    $scope.chapters = _.map(result.data.Chapters, function(chapter){
      //      return _.findWhere(result.data.Groups, {Id: chapter.Id});
      //    });
      //  });
  })
  .filter("truncateFilter", function(){
    return function(input, source){
        if(input.Type == 1)
          return _.findWhere(source.Questions, {Id: input.Id}).Title;
        else
          return _.findWhere(source.Groups, {Id: input.Id}).Title;
    };
  })
  .filter("truncateFilter2", function() {
    return function(input, source) {
        return _.findWhere(source.Groups, { Id: input.Id }).Children;
    };
});