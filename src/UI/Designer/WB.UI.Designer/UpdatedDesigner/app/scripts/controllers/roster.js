(function() {
    'use strict';

    angular.module('designerApp')
        .controller('RosterCtrl', [
            '$rootScope', '$scope', '$stateParams', 'questionnaireService', 'commandService', 'utilityService', 'confirmService', '$log',
            function($rootScope, $scope, $stateParams, questionnaireService, commandService, utilityService, confirmService, $log) {

                $scope.rosterTypes = {
                    'Numeric-template.html': 'Answer to numeric question',
                    'List-template.html': 'Answer to list question',
                    'FixedTitles-template.html': 'Manual list'
                };

                var dataBind = function(result) {
                    $scope.activeRoster = result;
                    $scope.activeRoster.itemId = $stateParams.itemId;

                    $scope.activeRoster.numerics = utilityService.union(_.toArray(result.numericIntegerQuestions));
                    $scope.activeRoster.lists = utilityService.union(_.toArray(result.textListsQuestions));
                    $scope.activeRoster.multiOption = utilityService.union(_.toArray(result.notLinkedMultiOptionQuestions));

                    $scope.getRosterTemplate();
                };

                $scope.getSelected = function(collection, id) {
                    var current = _.find(collection, { 'id': id });
                    if (!_.isUndefined(current)) {
                        return current.title;
                    }
                    return '';
                };

                $scope.getRosterTemplate = function() {
                    if ($scope.activeRoster !== undefined && $scope.activeRoster.roster.rosterSizeSourceType !== undefined) {
                        if ($scope.activeRoster.roster.rosterSizeSourceType === 'FixedTitles') {
                            $scope.activeRoster.rosterTemplate = 'FixedTitles-template.html';
                        }
                        if ($scope.activeRoster.roster.rosterSizeSourceType === 'Question') {
                            if ($scope.activeRoster.roster.rosterTitleQuestionId === null) {
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
                        $scope.activeRoster.roster.rosterSizeSourceType = 'FixedTitles';
                        $scope.activeRoster.roster.rosterSizeQuestionId = null;
                    } else {
                        $scope.activeRoster.roster.rosterSizeSourceType = 'Question';
                        $scope.activeRoster.roster.rosterFixedTitles = null;
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
                    commandService.updateRoster($stateParams.questionnaireId, $scope.activeRoster).success(function(result) {
                        $scope.initialRoster = angular.copy($scope.activeRoster);

                        $rootScope.$emit('rosterUpdated', {
                            itemId: $scope.activeRoster.itemId,
                            title: $scope.activeRoster.roster.title
                        });
                    });
                };

                $scope.deleteRoster = function() {
                    var modalInstance = confirmService.open($scope.activeRoster.roster);

                    modalInstance.result.then(function(confirmResult) {
                        if (confirmResult === 'ok') {
                            commandService.deleteGroup($stateParams.questionnaireId, $stateParams.itemId).success(function(result) {
                                if (result.IsSuccess) {
                                    var itemIdToDelete = $stateParams.itemId;
                                    questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                                    $scope.resetSelection();
                                }
                            });
                        }
                    });
                };

                $scope.cancelRoster = function() {
                    var temp = angular.copy($scope.initialRoster);
                    dataBind(temp);
                };

                $scope.loadRoster();
            }
        ]);
}());