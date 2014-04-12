'use strict';

angular.module('pocAngularApp')
  .controller('MainCtrl', function ($scope, $http, $routeParams) {

    $scope.documents = [];

    $scope.chapters = [];

    $scope.item = null;

    $scope.setItem = function (item) {
      $scope.item = item;
      console.log($scope.item);
    };

    $scope.changeChapter = function (chapter) {
        $scope.currentChapter = chapter;
        console.log($scope.currentChapter);
    };

    $scope.submit = function () {
      console.log('submit');
    };

    $scope.unfold = function () {
        if(!$('.chapter-panel').hasClass("unfolded")){
            $('.chapter-panel').addClass("unfolded");
        }
    };

    $scope.foldback = function () {
        $('.chapter-panel').removeClass('unfolded');
    };

    //$http.get('UpdatedDesigner/data/data.json')
    //  .then(function(result) {
    //    $scope.documents = result.data;
    //    $scope.chapters = _.map(result.data.Chapters, function(chapter){
    //      return _.findWhere(result.data.Groups, {Id: chapter.Id});
    //    });
    //  });

      $http.get('api/questionnaire/get/' + $routeParams.questionnaireId)
        .success(function (result) {
            $scope.questionnaire = result;
            $scope.currentChapter = result.Chapters[0];
      });
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