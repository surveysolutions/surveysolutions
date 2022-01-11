(function () {
    'use strict';

    angular.module('designerApp')
        .controller('RosterCtrl',
            function ($rootScope, $scope, $state, $i18next, questionnaireService, commandService, confirmService, $log, utilityService, hotkeys, optionsService) {
                $scope.currentChapterId = $state.params.chapterId;
                $scope.selectedNumericQuestion = null;
                $scope.selectedMultiQuestion = null;
                $scope.selectedListQuestion = null;
                $scope.selectedTitleQuestion = null;

                var fixedRosterLimit = 200;
                var saveRoster = 'ctrl+s';

                if (hotkeys.get(saveRoster) !== false) {
                    hotkeys.del(saveRoster);
                }

                hotkeys.bindTo($scope)
                    .add({
                        combo: saveRoster,
                        description: $i18next.t('Save'),
                        allowIn: ['INPUT', 'SELECT', 'TEXTAREA'],
                        callback: function (event) {
                            if ($scope.questionnaire !== null && !$scope.questionnaire.isReadOnlyForUser) {
                                if ($scope.editRosterForm.$dirty) {
                                    $scope.saveRoster();
                                    $scope.editRosterForm.$setPristine();
                                }
                                event.preventDefault();
                            }
                        }
                    });

                $scope.onKeyPressInOptions = function (keyEvent) {
                    if (keyEvent.which === 13) {
                        keyEvent.preventDefault();

                        if ($scope.wasTitlesLimitReached())
                            return;

                        utilityService.moveFocusAndAddOptionIfNeeded(
                            keyEvent.target ? keyEvent.target : keyEvent.srcElement,
                            ".fixed-roster-titles-editor",
                            ".fixed-roster-titles-editor input.fixed-roster-value-editor",
                            $scope.activeRoster.fixedRosterTitles,
                            function () { return $scope.addFixedTitle(); },
                            "title");
                    }
                };

                var dataBind = function (result) {
                    $scope.activeRoster = result;
                    $scope.activeRoster.variable = result.variableName;
                    $scope.activeRoster.lists = result.textListsQuestions;
                    $scope.activeRoster.numerics = result.numericIntegerQuestions;
                    $scope.activeRoster.titles = result.numericIntegerTitles;
                    $scope.activeRoster.multiOption = result.notLinkedMultiOptionQuestions;

                    bindListsWithSelectedElements();

                    $scope.activeRoster.useListAsRosterTitleEditor = true;

                    $scope.activeRoster.getTitleForRosterType = function () {
                        return _.find($scope.activeRoster.rosterTypeOptions, { 'value': $scope.activeRoster.type }).text;
                    };

                    if ($scope.editRosterForm) {
                        $scope.editRosterForm.$setPristine();
                    }
                };

                var getSelected = function (collection, id, errorTitleCode) {
                    if (_.isNull(id)) return null;

                    var current = _.find(collection, { 'id': id });
                    if (!_.isUndefined(current)) {
                        return current;
                    }
                    return {
                        isSectionPlaceHolder: false,
                        id: id,
                        title: $i18next.t(_.isEmpty(errorTitleCode) ? 'SelectedInvalidItem' : errorTitleCode),
                        breadcrumbs: null,
                        type: null,
                        varName: $i18next.t('SelectItemFromTheList'),
                        questionType: null
                    };
                };

                var bindListsWithSelectedElements = function () {
                    $scope.selectedNumericQuestion = getSelected($scope.activeRoster.numerics, $scope.activeRoster.rosterSizeNumericQuestionId);
                    $scope.selectedListQuestion = getSelected($scope.activeRoster.lists, $scope.activeRoster.rosterSizeListQuestionId);
                    $scope.selectedMultiQuestion = getSelected($scope.activeRoster.multiOption, $scope.activeRoster.rosterSizeMultiQuestionId);
                    $scope.selectedTitleQuestion = getSelected($scope.activeRoster.titles, $scope.activeRoster.rosterTitleQuestionId, 'IncorrectRosterTitle');
                };

                $scope.wasTitlesLimitReached = function () {
                    return $scope.activeRoster.fixedRosterTitles.length >= fixedRosterLimit;
                };

                $scope.selectNumericQuestion = function (numericId) {
                    $scope.activeRoster.rosterSizeNumericQuestionId = numericId;
                    $scope.selectedNumericQuestion = getSelected($scope.activeRoster.numerics, $scope.activeRoster.rosterSizeNumericQuestionId);
                    $scope.editRosterForm.$setDirty();
                    questionnaireService.getQuestionsEligibleForNumericRosterTitle($state.params.questionnaireId, $state.params.itemId, $scope.activeRoster.rosterSizeNumericQuestionId).then(function (result) {
                        $scope.activeRoster.titles = result.data;
                        $scope.selectedTitleQuestion = getSelected($scope.activeRoster.titles, $scope.activeRoster.rosterTitleQuestionId);
                    });
                };

                $scope.selectListQuestion = function (listId) {
                    $scope.activeRoster.rosterSizeListQuestionId = listId;
                    $scope.selectedListQuestion = getSelected($scope.activeRoster.lists, $scope.activeRoster.rosterSizeListQuestionId);
                    $scope.editRosterForm.$setDirty();
                };

                $scope.selectMultiQuestion = function (multiId) {
                    $scope.activeRoster.rosterSizeMultiQuestionId = multiId;
                    $scope.selectedMultiQuestion = getSelected($scope.activeRoster.multiOption, $scope.activeRoster.rosterSizeMultiQuestionId);
                    $scope.editRosterForm.$setDirty();
                };

                $scope.selectTitleQuestion = function (titleQuestionId) {
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

                $scope.loadRoster = function () {
                    questionnaireService.getRosterDetailsById($state.params.questionnaireId, $state.params.itemId)
                        .then(function(result) {
                                var data = result.data;
                                $scope.initialRoster = angular.copy(data);
                                dataBind(data);

                                var focusId = null;
                                switch ($state.params.property) {
                                    case 'Title':
                                        focusId = 'edit-group-title';
                                        break;
                                    case 'VariableName':
                                        focusId = 'edit-group-variableName';
                                        break;
                                    case 'EnablingCondition':
                                        focusId = 'edit-group-condition';
                                        break;
                                    case 'FixedRosterItem':
                                        focusId = 'fixed-item-' + $state.params.indexOfEntityInProperty;
                                        break;
                                    default:
                                        break;
                                }
                                utilityService.setFocusIn(focusId);
                            }
                        );
                };

                $scope.saveRoster = function (callback) {
                    if ($scope.editRosterForm.$valid) {
                        $scope.showRosterTitlesInList();
                        commandService.updateRoster($state.params.questionnaireId, $scope.activeRoster).then(function () {
                            $scope.initialRoster = angular.copy($scope.activeRoster);

                            $rootScope.$emit('rosterUpdated', {
                                itemId: $scope.activeRoster.itemId,
                                variable: $scope.activeRoster.variableName,
                                title: $scope.activeRoster.title,
                                hasCondition: ($scope.activeRoster.enablementCondition !== null && /\S/.test($scope.activeRoster.enablementCondition)),
                                type: 'Roster',
                                hideIfDisabled: $scope.activeRoster.hideIfDisabled
                            });
                            if (_.isFunction(callback)) {
                                callback();
                            }
                        });
                    }
                };

                $scope.deleteRoster = function () {
                    var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup($scope.activeRoster.title || $i18next.t('UntitledRoster')));

                    modalInstance.result.then(function (confirmResult) {
                        if (confirmResult === 'ok') {
                            commandService.deleteGroup($state.params.questionnaireId, $state.params.itemId).then(function () {
                                var itemIdToDelete = $state.params.itemId;
                                questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                                $scope.resetSelection();
                                $rootScope.$emit('rosterDeleted', itemIdToDelete);
                            });
                        }
                    });
                };

                $scope.$on('verifing', function (scope, params) {
                    if ($scope.editRosterForm.$dirty) {
                        $scope.saveRoster(function () {
                            $scope.editRosterForm.$setPristine();
                        });
                    }
                });

                $scope.cancelRoster = function () {
                    var temp = angular.copy($scope.initialRoster);
                    dataBind(temp);
                };

                $scope.changeDisplayMode = function(displayMode) {
                    $scope.activeRoster.displayMode = displayMode;
                    $scope.editRosterForm.$setDirty();
                };

                $rootScope.$on('groupMoved', function (event, data) {
                    if (data === $state.params.itemId && !_.isUndefined($scope.editRosterForm)) {
                        questionnaireService.getRosterDetailsById($state.params.questionnaireId, $state.params.itemId).then(function (result) {
                            var resultData = result.data;
                            $scope.activeRoster.lists = resultData.textListsQuestions;
                            $scope.activeRoster.numerics = resultData.numericIntegerQuestions;
                            $scope.activeRoster.multiOption = resultData.notLinkedMultiOptionQuestions;
                            $scope.activeRoster.titles = resultData.numericIntegerTitles;
                            
                            bindListsWithSelectedElements();

                            if (!_.isNull($scope.activeRoster.rosterSizeNumericQuestionId) && !_.isUndefined($scope.activeRoster.rosterSizeNumericQuestionId)) {
                                $scope.editRosterForm.$setPristine();
                                questionnaireService.getQuestionsEligibleForNumericRosterTitle($state.params.questionnaireId, $state.params.itemId, $scope.activeRoster.rosterSizeNumericQuestionId).then(function (result) {
                                    $scope.activeRoster.titles = result.data;
                                    $scope.selectedTitleQuestion = getSelected($scope.activeRoster.titles, $scope.activeRoster.rosterTitleQuestionId);
                                });
                            }
                        });
                    }
                });

                $scope.loadRoster();
            }
        );
}());
