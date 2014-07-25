(function() {
    'use strict';

    angular.module('designerApp')
        .controller('RosterCtrl', [
            '$rootScope', '$scope', '$stateParams', 'questionnaireService', 'commandService', 'utilityService', 'confirmService', '$log',
            function ($rootScope, $scope, $stateParams, questionnaireService, commandService, utilityService, confirmService, $log) {
                $scope.currentChapterId = $stateParams.chapterId;

                var dataBind = function(result) {
                    $scope.activeRoster = result;
                    $scope.activeRoster.variable = result.variableName;
                    $scope.activeRoster.lists = utilityService.union(_.toArray(result.textListsQuestions));
                    $scope.activeRoster.numerics = utilityService.union(_.toArray(result.numericIntegerQuestions));
                    $scope.activeRoster.titles = utilityService.union(_.toArray(result.numericIntegerTitles));
                    $scope.activeRoster.multiOption = utilityService.union(_.toArray(result.notLinkedMultiOptionQuestions));

                    $scope.activeRoster.getTitleForRosterType = function () {
                        return _.find($scope.activeRoster.rosterTypeOptions, { 'value': $scope.activeRoster.type }).text;
                    };

                    $scope.editRosterForm.$setPristine();
                };

                $scope.getSelected = function(collection, id) {
                    var current = _.find(collection, { 'id': id });
                    if (!_.isUndefined(current)) {
                        return current.title;
                    }
                    return '';
                };

                
                $scope.updateRosterType = function (type) {
                    $scope.activeRoster.type = type;
                };

                $scope.loadRoster = function() {
                    questionnaireService.getRosterDetailsById($stateParams.questionnaireId, $stateParams.itemId).success(function(result) {
                            $scope.initialRoster = angular.copy(result);
                            dataBind(result);
                        }
                    );
                };

                $scope.saveRoster = function () {
                    if ($scope.editRosterForm.$valid) {
                        commandService.updateRoster($stateParams.questionnaireId, $scope.activeRoster).success(function() {
                            $scope.initialRoster = angular.copy($scope.activeRoster);

                            $rootScope.$emit('rosterUpdated', {
                                itemId: $scope.activeRoster.itemId,
                                variable: $scope.activeRoster.variableName,
                                title: $scope.activeRoster.title
                            });
                            $scope.editRosterForm.$setPristine();
                        });
                    }
                };

                $scope.deleteRoster = function() {
                    var modalInstance = confirmService.open($scope.activeRoster);

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