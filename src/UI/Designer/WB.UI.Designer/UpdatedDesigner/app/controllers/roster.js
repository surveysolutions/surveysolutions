(function() {
    'use strict';

    angular.module('designerApp')
        .controller('RosterCtrl', [
            '$scope', '$routeParams', 'questionnaireService', 'commandService', 'utilityService','$log',
            function($scope, $routeParams, questionnaireService, commandService, utilityService, $log) {

                var dataBind = function(result) {
                    $scope.activeRoster.breadcrumbs = result.breadcrumbs;
                    $scope.activeRoster.numerics = utilityService.union(_.toArray(result.numericIntegerQuestions));
                    $scope.activeRoster.lists = utilityService.union(_.toArray(result.textListsQuestions));
                    $scope.activeRoster.multiOption = utilityService.union(_.toArray(result.notLinkedMultiOptionQuestions));

                    var roster = result.roster;
                    $scope.activeRoster.title = roster.title;
                    $scope.activeRoster.description = roster.description;
                    $scope.activeRoster.enablementCondition = roster.enablementCondition;
                    $scope.activeRoster.rosterSizeSourceType = roster.rosterSizeSourceType;
                    $scope.activeRoster.rosterFixedTitles = roster.rosterFixedTitles;
                    $scope.activeRoster.rosterSizeQuestionId = roster.rosterSizeQuestionId;
                    $scope.activeRoster.rosterTitleQuestionId = roster.rosterTitleQuestionId;

                    $scope.getRosterTemplate();
                };

                $scope.getRosterTemplate = function () {
                    if ($scope.activeRoster !== undefined && $scope.activeRoster.rosterSizeSourceType !== undefined) {
                        if ($scope.activeRoster.rosterSizeSourceType === 'FixedTitles') {
                            $scope.activeRoster.rosterTemplate = 'FixedTitles-template.html';
                        }
                        if ($scope.activeRoster.rosterSizeSourceType === 'Question') {
                            if ($scope.activeRoster.rosterTitleQuestionId === null) {
                                $scope.activeRoster.rosterTemplate = 'List-template.html';
                            } else {
                                $scope.activeRoster.rosterTemplate = 'Numeric-template.html';
                            }
                        }
                    }
                };

                $scope.updateRosterType = function() {
                    if ($scope.activeRoster.rosterTemplate === 'FixedTitles-template.html') {
                        $scope.activeRoster.rosterSizeSourceType = 'FixedTitles';
                        $scope.activeRoster.rosterSizeQuestionId = null;
                    } else {
                        $scope.activeRoster.rosterSizeSourceType = 'Question';
                        $scope.activeRoster.rosterFixedTitles = null;
                    }
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