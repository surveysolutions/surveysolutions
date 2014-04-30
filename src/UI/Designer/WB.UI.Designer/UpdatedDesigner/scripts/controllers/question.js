'use strict';

angular.module('designerApp')
    .controller('QuestionCtrl', [
        '$scope', '$routeParams',
        function($scope, $routeParams) {
            $scope.saveQuestion = function() {
                console.log('save question');
            };
        }
    ]);