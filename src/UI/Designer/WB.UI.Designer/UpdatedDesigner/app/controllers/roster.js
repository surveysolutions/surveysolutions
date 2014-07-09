(function() {
    'use strict';

    angular.module('designerApp')
        .controller('RosterCtrl', [
            '$scope', '$stateParams', 'questionnaireService', 'commandService', 'utilityService', '$log', '$modal',
            function($scope, $stateParams, questionnaireService, commandService, utilityService, $log, $modal) {

                $scope.rosterTypes = {
                    'Numeric-template.html': 'Answer to numeric question',
                    'List-template.html': 'Answer to list question',
                    'FixedTitles-template.html': 'Manual list'
                };

                var dataBind = function(result) {
                    $scope.activeRoster = $scope.activeRoster || {};

                    $scope.activeRoster.itemId = $stateParams.itemId;

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
                    $scope.activeRoster.variableName = roster.variableName;
                    $scope.getRosterTemplate();
                };

                $scope.getSelected = function (collection, id) {
                    var current = _.find(collection, { 'id': id });
                    if (!_.isUndefined(current)) {
                        return current.title;
                    }
                    return '';
                };

                $scope.getRosterTemplate = function() {
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

                $scope.updateRosterType = function(i) {
                    $scope.activeRoster.rosterTemplate = (_.invert($scope.rosterTypes))[i];

                    if ($scope.activeRoster.rosterTemplate === 'FixedTitles-template.html') {
                        $scope.activeRoster.rosterSizeSourceType = 'FixedTitles';
                        $scope.activeRoster.rosterSizeQuestionId = null;
                    } else {
                        $scope.activeRoster.rosterSizeSourceType = 'Question';
                        $scope.activeRoster.rosterFixedTitles = null;
                    }
                };

                $scope.loadRoster = function() {
                    questionnaireService.getRosterDetailsById($stateParams.questionnaireId, $stateParams.itemId).success(function(result) {
                            $scope.initialRoster = angular.copy(result);
                            dataBind(result);
                        }
                    );
                };

                $scope.saveRoster = function() {
                    $("#edit-roster-save-button").popover('destroy');
                    commandService.updateRoster($stateParams.questionnaireId, $scope.activeRoster).success(function(result) {
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

                $scope.deleteRoster = function () {
                    var modalInstance = $modal.open({
                        templateUrl: 'app/views/confirm.html',
                        controller: 'confirmCtrl',
                        windowClass: 'confirm-window',
                        resolve:
                        {
                            item: function () {
                                return $scope.activeRoster;
                            }
                        }
                    });

                    modalInstance.result.then(function (confirmResult) {
                        if (confirmResult === 'ok') {
                            commandService.deleteGroup($stateParams.questionnaireId, $stateParams.itemId).success(function (result) {
                                if (result.IsSuccess) {
                                    var itemIdToDelete = $stateParams.itemId;
                                    questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                                    $scope.resetSelection();
                                }
                            });
                        }
                    });
                };

                $scope.resetRoster = function() {
                    dataBind($scope.initialRoster);
                };

                $scope.loadRoster();
            }
        ]);
}());