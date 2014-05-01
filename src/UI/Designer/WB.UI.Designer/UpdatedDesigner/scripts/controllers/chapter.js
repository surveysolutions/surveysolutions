'use strict';

angular.module('designerApp')
    .controller('ChapterCtrl', [
        '$scope', '$routeParams',
        function($scope, $routeParams) {
            console.log($scope.activeChapter);

            $scope.$watch('activeChapter', function(newVal) {
                console.log($scope.activeChapter);
            });

            $scope.saveChapter = function() {
                console.log('save chapter');
            };
        }
    ]);