(function() {
    'use strict';

    angular.module('designerApp')
        .controller('RosterCtrl', 
            function ($rootScope, $scope, $stateParams, questionnaireService, commandService, confirmService, $log, utilityService, hotkeys) {
                $scope.currentChapterId = $stateParams.chapterId;
                $scope.selectedNumericQuestion = null;
                $scope.selectedMultiQuestion = null;
                $scope.selectedListQuestion = null;
                $scope.selectedTitleQuestion = null;

                hotkeys.bindTo($scope)
                      .add({
                          combo: 'ctrl+s',
                          description: 'Save current roster',
                          allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                          callback: function (event) {
                              $scope.saveRoster();
                              $scope.editRosterForm.$setPristine();
                              event.preventDefault();
                          }
                      });

                var dataBind = function(result) {
                    $scope.activeRoster = result;
                    $scope.activeRoster.variable = result.variableName;
                    $scope.activeRoster.lists = result.textListsQuestions;
                    $scope.activeRoster.numerics = result.numericIntegerQuestions;
                    $scope.activeRoster.titles = result.numericIntegerTitles;
                    $scope.activeRoster.multiOption = result.notLinkedMultiOptionQuestions;

                    $scope.selectedNumericQuestion = getSelected($scope.activeRoster.numerics, $scope.activeRoster.rosterSizeNumericQuestionId);
                    $scope.selectedListQuestion = getSelected($scope.activeRoster.lists, $scope.activeRoster.rosterSizeListQuestionId);
                    $scope.selectedMultiQuestion = getSelected($scope.activeRoster.multiOption, $scope.activeRoster.rosterSizeMultiQuestionId);
                    $scope.selectedTitleQuestion = getSelected($scope.activeRoster.titles, $scope.activeRoster.rosterTitleQuestionId);

                    $scope.activeRoster.getTitleForRosterType = function () {
                        return _.find($scope.activeRoster.rosterTypeOptions, { 'value': $scope.activeRoster.type }).text;
                    };

                    if (!_.isNull($scope.editRosterForm) && !_.isUndefined($scope.editRosterForm)) {
                        $scope.editRosterForm.$setPristine();
                    }
                };

                var getSelected = function (collection, id) {
                    if (_.isNull(id)) return null;

                    var current = _.find(collection, { 'id': id });
                    if (!_.isUndefined(current)) {
                        return current;
                    }
                    return null;
                };

                $scope.selectNumericQuestion = function(numericId) {
                    $scope.activeRoster.rosterSizeNumericQuestionId = numericId;
                    $scope.selectedNumericQuestion = getSelected($scope.activeRoster.numerics, $scope.activeRoster.rosterSizeNumericQuestionId);
                    $scope.editRosterForm.$setDirty();
                };

                $scope.selectListQuestion = function(listId) {
                    $scope.activeRoster.rosterSizeListQuestionId = listId;
                    $scope.selectedListQuestion = getSelected($scope.activeRoster.lists, $scope.activeRoster.rosterSizeListQuestionId);
                    $scope.editRosterForm.$setDirty();
                };

                $scope.selectMultiQuestion = function(multiId) {
                    $scope.activeRoster.rosterSizeMultiQuestionId = multiId;
                    $scope.selectedMultiQuestion = getSelected($scope.activeRoster.multiOption, $scope.activeRoster.rosterSizeMultiQuestionId);
                    $scope.editRosterForm.$setDirty();
                };

                $scope.selectTitleQuestion = function(titleQuestionId) {
                    $scope.activeRoster.rosterTitleQuestionId = titleQuestionId;
                    $scope.selectedTitleQuestion = getSelected($scope.activeRoster.titles, $scope.activeRoster.rosterTitleQuestionId);
                    $scope.editRosterForm.$setDirty();
                };


                $scope.updateRosterType = function (type) {
                    $scope.activeRoster.type = type;
                    $scope.editRosterForm.$setDirty();
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
                        });
                    }
                };

                $scope.deleteRoster = function() {
                    var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup($scope.activeRoster.title));

                    modalInstance.result.then(function(confirmResult) {
                        if (confirmResult === 'ok') {
                            commandService.deleteGroup($stateParams.questionnaireId, $stateParams.itemId).success(function() {
                                var itemIdToDelete = $stateParams.itemId;
                                questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                                $scope.resetSelection();
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
        );
}());