'use strict';

angular.module('designerApp')
    .controller('QuestionCtrl', [
        '$scope', '$routeParams',
        function ($scope, $routeParams) {
            console.log($scope.activeQuestion);

            $scope.$watch('activeQuestion', function (newVal) {
                console.log($scope.activeQuestion);
            });

            $scope.saveQuestion = function() {
                console.log('save question');
            };
        }
    ]);