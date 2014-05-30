'use strict';

angular.module('designerApp')
    .controller('RosterCtrl', [
        '$scope', '$routeParams', 'questionnaireService', 'commandService', 'utilityService', 'navigationService',
        function($scope, $routeParams, questionnaireService, commandService, math, navigationService) {
            $scope.loadRoster = function () {
                questionnaireService.getGroupDetailsById($routeParams.questionnaireId, $scope.activeRoster.itemId).success(function (result) {
                    var roster = result.group;
                    $scope.activeRoster.description = roster.description;
                    $scope.activeRoster.enablementCondition = roster.enablementCondition;
                    $scope.activeRoster.breadcrumbs = result.breadcrumbs;
                }
                );
            }

            $scope.loadRoster();
        }
    ])