'use strict';

angular.module('designerApp')
    .controller('ChapterCtrl', [
        '$scope', '$routeParams',
        function($scope, $routeParams) {
            $scope.saveChapter = function() {
                console.log('save chapter');
            };
        }
    ]);