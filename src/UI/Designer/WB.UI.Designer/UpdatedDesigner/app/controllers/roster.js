'use strict';

angular.module('designerApp')
    .controller('RosterCtrl', [
        '$scope', '$routeParams', 'questionnaireService',
        function($scope, $routeParams, questionnaireService) {

            $scope.loadRoster = function() {
                questionnaireService.getRosterDetailsById($routeParams.questionnaireId, $scope.activeRoster.itemId).success(function(result) {
                        $scope.activeRoster.breadcrumbs = result.breadcrumbs;
                        $scope.activeRoster.numerics = result.numericIntegerQuestions["new Chapter"];

                        var roster = result.roster;
                        $scope.activeRoster.description = roster.description;
                        $scope.activeRoster.enablementCondition = roster.enablementCondition;
                        $scope.activeRoster.rosterSizeSourceType = roster.rosterSizeSourceType;
                        $scope.activeRoster.rosterFixedTitles = roster.rosterFixedTitles;
                    }
                );
            }

            $scope.loadRoster();

            $scope.$watch('activeRoster', function() {
                $scope.loadRoster();
            });
        }
    ])