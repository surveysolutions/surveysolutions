(function() {
    'use strict';

    angular.module('designerApp')
        .controller('RosterCtrl', [
            '$scope', '$routeParams', 'questionnaireService', 'commandService', '$log',
            function($scope, $routeParams, questionnaireService, commandService, $log) {

                var dataBind = function(result) {
                    $scope.activeRoster.breadcrumbs = result.breadcrumbs;
                    $scope.activeRoster.numerics = result.numericIntegerQuestions["new Chapter"];

                    var roster = result.roster;
                    $scope.activeRoster.title = roster.title;
                    $scope.activeRoster.description = roster.description;
                    $scope.activeRoster.enablementCondition = roster.enablementCondition;
                    $scope.activeRoster.rosterSizeSourceType = roster.rosterSizeSourceType;
                    $scope.activeRoster.rosterFixedTitles = roster.rosterFixedTitles;
                };

                $scope.loadRoster = function() {
                    questionnaireService.getRosterDetailsById($routeParams.questionnaireId, $scope.activeRoster.itemId).success(function(result) {
                            $scope.initialRoster = angular.copy(result);
                            dataBind(result);
                        }
                    );
                };

                $scope.saveRoster = function() {
                    $("#edit-roster-save-button").popover('destroy');
                    commandService.updateRoster($routeParams.questionnaireId, $scope.activeRoster).success(function(result) {
                        if (!result.IsSuccess) {
                            $("#edit-roster-save-button").popover({
                                content: result.Error,
                                placement: top,
                                animation: true
                            }).popover('show');
                            $log.error(result);
                        }
                    });
                };

                $scope.deleteRoster = function() {
                    if (confirm("Are you sure want to delete roster?")) {
                        commandService.deleteGroup($routeParams.questionnaireId, $scope.activeRoster.itemId).success(function(result) {
                            if (result.IsSuccess) {
                                $scope.activeRoster.isDeleted = true;
                            } else {
                                $log.error(result);
                            }
                        });
                    }
                };

                $scope.resetRoster = function() {
                    dataBind($scope.initialRoster);
                };

                $scope.loadRoster();

                $scope.$watch('activeRoster', function() {
                    $scope.loadRoster();
                });
            }
        ]);
}());