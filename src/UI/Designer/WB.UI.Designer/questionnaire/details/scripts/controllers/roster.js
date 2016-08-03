(function() {
    'use strict';

    angular.module('designerApp')
        .controller('RosterCtrl', 
            function ($rootScope, $scope, $stateParams, questionnaireService, commandService, confirmService, $log, utilityService, hotkeys, optionsService) {
                $scope.currentChapterId = $stateParams.chapterId;
                $scope.selectedNumericQuestion = null;
                $scope.selectedMultiQuestion = null;
                $scope.selectedListQuestion = null;
                $scope.selectedTitleQuestion = null;

                var saveRoster = 'ctrl+s';

                if (hotkeys.get(saveRoster) !== false) {
                    hotkeys.del(saveRoster);
                }

                hotkeys.bindTo($scope)
                    .add({
                        combo: saveRoster,
                        description: 'Save changes',
                        allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                        callback: function(event) {
                            if ($scope.questionnaire !== null && !$scope.questionnaire.isReadOnlyForUser) {
                                $scope.saveRoster();
                                $scope.editRosterForm.$setPristine();
                                event.preventDefault();
                            }
                        }
                    });

                $scope.onKeyPressInOptions = function(keyEvent) {
                    if (keyEvent.which === 13) {
                        keyEvent.preventDefault();
                        utilityService.moveFocusAndAddOptionIfNeeded(
                            event.target ? event.target : event.srcElement,
                            ".fixed-roster-titles-editor",
                            ".fixed-roster-titles-editor input.fixed-roster-value-editor",
                            $scope.activeRoster.fixedRosterTitles,
                            function() { return $scope.addFixedTitle(); },
                            "title");
                    }
                };

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

                    $scope.activeRoster.useListAsRosterTitleEditor = true;

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

                $scope.removeFixedTitle = function (index) {
                    $scope.activeRoster.fixedRosterTitles.splice(index, 1);
                    $scope.editRosterForm.$setDirty();
                };

                $scope.showOptionsInTextarea = function () {
                    $scope.activeRoster.stringifiedRosterTitles = optionsService.stringifyOptions($scope.activeRoster.fixedRosterTitles);
                    $scope.activeRoster.useListAsRosterTitleEditor = false;
                };

                $scope.showRosterTitlesInList = function () {
                    if ($scope.activeRoster.useListAsRosterTitleEditor) {
                        return;
                    }
                    if (_.isUndefined($scope.editRosterForm.stringifiedRosterTitles) || !$scope.editRosterForm.stringifiedRosterTitles.$valid) {
                        return;
                    }
                    if ($scope.activeRoster.stringifiedRosterTitles) {
                        $scope.activeRoster.fixedRosterTitles = optionsService.parseOptions($scope.activeRoster.stringifiedRosterTitles);
                    }
                    $scope.activeRoster.useListAsRosterTitleEditor = true;
                };
                
                $scope.addFixedTitle = function () {
                    $scope.activeRoster.fixedRosterTitles.push({
                        "value": null,
                        "title": ''
                    });
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

                $scope.saveRoster = function (callback) {
                    if ($scope.editRosterForm.$valid) {
                  
                        commandService.updateRoster($stateParams.questionnaireId, $scope.activeRoster).success(function () {
                            $scope.initialRoster = angular.copy($scope.activeRoster);

                            $rootScope.$emit('rosterUpdated', {
                                itemId: $scope.activeRoster.itemId,
                                variable: $scope.activeRoster.variableName,
                                title: $scope.activeRoster.title,
                                hasCondition: ($scope.activeRoster.enablementCondition !== null && /\S/.test($scope.activeRoster.enablementCondition))
                            });
                            if (_.isFunction(callback)) {
                                callback();
                            }
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
                                $rootScope.$emit('rosterDeleted', itemIdToDelete);
                            });
                        }
                    });
                };

                $scope.$on('verifing', function (scope, params) {
                    if ($scope.editRosterForm.$dirty) {
                        $scope.saveRoster(function() {
                            $scope.editRosterForm.$setPristine();
                        });
                    }
                });

                $scope.cancelRoster = function() {
                    var temp = angular.copy($scope.initialRoster);
                    dataBind(temp);
                };

                $scope.loadRoster();
            }
        );
}());