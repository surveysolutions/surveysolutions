angular.module('designerApp')
    .controller('TreeCtrl', [
        '$scope', '$state', 'questionnaireService', 'commandService', 'verificationService', 'utilityService', 'hotkeys', '$log', '$modal',
        function($scope, $state, questionnaireService, commandService, verificationService, utilityService, hotkeys, $log, $modal) {
            'use strict';
            var me = this;

            var filtersBlockModes = {
                default: 'default',
                search: 'search'
            };

            var itemTypes = {
                question: 'question',
                roster: 'roster',
                group: 'group'
            };

            $scope.search = { searchText: '' };
            $scope.filtersBoxMode = filtersBlockModes.default;

            hotkeys.add({
                combo: 'ctrl+f',
                description: 'Search for groups and questions in chapter',
                callback: function(event) {
                    $scope.showSearch();
                    event.preventDefault();
                }
            });

            hotkeys.add('down', 'Navigate to next sibling', function(event) {
                event.preventDefault();
                $scope.goToNextItem();
            });

            hotkeys.add('up', 'Navigate to previous sibling', function(event) {
                event.preventDefault();
                $scope.goToPrevItem();
            });

            hotkeys.add('left', 'Navigate to parent', function(event) {
                event.preventDefault();
                $scope.goToParent();
            });

            hotkeys.add('right', 'Navigate to child', function(event) {
                event.preventDefault();
                $scope.goToChild();
            });

            $scope.showSearch = function() {
                $scope.filtersBoxMode = filtersBlockModes.search;
                utilityService.focus('focusSearch');
            };

            $scope.hideSearch = function() {
                $scope.filtersBoxMode = filtersBlockModes.default;
                $scope.search.searchText = '';
            };

            $scope.searchItem = function(item) {
                if (!$scope.search.searchText) return true;
                var variableMatches = item.variable && item.variable.toLowerCase().indexOf($scope.search.searchText.toLowerCase()) != -1;
                var titleMatches = item.title && item.title.toLowerCase().indexOf($scope.search.searchText.toLowerCase()) != -1;

                var found = variableMatches || titleMatches;

                if (!found) {
                    angular.forEach(item.items, function(item) {
                        var match = $scope.searchItem(item);
                        if (match) {
                            found = true;
                        }
                    });
                }

                return found;
            };

            var upDownMove = function(updownStepValue) {
                if ($scope.items && $state.params.itemId) {
                    var currentItem = getCurrentItem();
                    var parent = currentItem.getParentItem();
                    var target = null;

                    if (_.isNull(parent)) {
                        var siblingIndex = _.indexOf($scope.items, currentItem) + updownStepValue;
                        if (siblingIndex < $scope.items.length && siblingIndex >= 0) {
                            target = $scope.items[siblingIndex];
                        }
                    } else {
                        var nextItemIndex = _.indexOf(parent.items, currentItem) + updownStepValue;

                        if (nextItemIndex < $scope.items.length && nextItemIndex >= 0) {
                            target = parent.items[nextItemIndex];
                        }
                    }

                    $state.go('questionnaire.chapter.' + getItemType(target), {
                        itemId: target.itemId
                    });
                }
            };

            $scope.goToNextItem = function() {
                upDownMove(1);
            };

            $scope.goToPrevItem = function() {
                upDownMove(-1);
            };

            $scope.goToParent = function() {
                if ($scope.items && $state.params.itemId) {
                    var parent = getCurrentItem().getParentItem();
                    if (parent != null) {
                        $state.go('questionnaire.chapter.' + getItemType(parent), {
                            itemId: parent.itemId
                        });
                    }
                }
            };

            $scope.goToChild = function() {
                if ($scope.items && $state.params.itemId) {
                    var currentItem = getCurrentItem();
                    if (!_.isEmpty(currentItem.items)) {
                        var target = currentItem.items[0];
                        $state.go('questionnaire.chapter.' + getItemType(target), {
                            itemId: target.itemId
                        });
                    }
                }
            };

            $scope.toggle = function(scope) {
                scope.toggle();
            };

            var getCurrentItem = function() {
                return questionnaireService.findItem($scope.items, $state.params.itemId);
            }

            var connectTree = function() {
                var setParent = function(item, parent) {
                    item.getParentItem = function() {
                        return parent;
                    };
                    _.each(item.items, function(child) {
                        setParent(child, item);
                    });
                };

                _.each($scope.items, function(item) {
                    setParent(item, null);
                });
            };

            var getItemType = function(item) {
                if (item.hasOwnProperty('type')) {
                    return itemTypes.question;
                }
                if (item.isRoster) {
                    return itemTypes.roster;
                }
                if (!item.hasOwnProperty('type'))
                    return itemTypes.group;
                throw 'unknown item type: ' + item;
            };

            $scope.isQuestion = function(item) {
                return item.hasOwnProperty('type');
            };

            $scope.isGroup = function(item) {
                return !item.hasOwnProperty('type');
            };

            $scope.groupsTree = {
                accept: function(sourceNodeScope, destNodesScope) {
                    var message = _.isNull(destNodesScope.item) || $scope.isGroup(destNodesScope.item);
                    return message;
                },
                beforeDrop: function(event) {
                    me.draggedFrom = event.source.nodeScope.item.getParentItem();
                },
                dropped: function(event) {

                    var movedItem = event.source.nodeScope.item;
                    var destItem = event.dest.nodesScope.item;
                    var destGroupId = destItem ? destItem.itemId : $scope.questionnaire.chapters[0].itemId;
                    var putItem = function(item, parent, index) {
                        var dropFrom = item.parent || $scope;

                        dropFrom.items.splice(_.indexOf(dropFrom.items, item), 1);
                        var itemsToAddTo = _.isNull(parent) ? $scope.items : parent.items;
                        itemsToAddTo.splice(index, 0, item);

                        connectTree();
                    };

                    if (event.dest.nodesScope !== event.source.nodesScope || event.dest.index !== event.source.index) {
                        if ($scope.isQuestion(movedItem)) {
                            questionnaireService.moveQuestion(movedItem.itemId, event.dest.index, destGroupId, $state.params.questionnaireId)
                                .success(function(data) {
                                    if (!data.IsSuccess) {
                                        putItem(movedItem, me.draggedFrom, event.source.index);
                                    }
                                })
                                .error(function() {
                                    putItem(movedItem, me.draggedFrom, event.source.index);
                                });
                        } else {
                            questionnaireService.moveGroup(movedItem.itemId, event.dest.index, destGroupId, $state.params.questionnaireId)
                                .success(function(data) {
                                    if (!data.IsSuccess) {
                                        putItem(movedItem, me.draggedFrom, event.source.index);

                                    }
                                })
                                .error(function() {
                                    putItem(movedItem, me.draggedFrom, event.source.index);
                                });
                        }
                    }
                }
            };

            $scope.deleteQuestion = function(item) {
                var itemIdToDelete = item.itemId || $state.params.itemId;

                var modalInstance = $modal.open({
                    templateUrl: 'app/views/confirm.html',
                    controller: 'confirmCtrl',
                    windowClass: 'confirm-window',
                    resolve:
                    {
                        item: function() {
                            return item;
                        }
                    }
                });

                modalInstance.result.then(function(confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteQuestion($state.params.questionnaireId, itemIdToDelete)
                            .success(function(result) {
                                if (result.IsSuccess) {
                                    questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                                    $scope.resetSelection();
                                }
                            });
                    }
                });
            };

            $scope.deleteGroup = function(item) {
                var itemIdToDelete = item.itemId || $state.params.itemId;

                var modalInstance = $modal.open({
                    templateUrl: 'app/views/confirm.html',
                    controller: 'confirmCtrl',
                    windowClass: 'confirm-window',
                    resolve:
                    {
                        item: function() {
                            return item;
                        }
                    }
                });

                modalInstance.result.then(function(confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteGroup($state.params.questionnaireId, itemIdToDelete)
                            .success(function(result) {
                                if (result.IsSuccess) {
                                    questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                                    $scope.resetSelection();
                                }
                            });
                    }
                });
            };

            $scope.moveToChapter = function(chapterId) {
                var itemToMoveId = $state.params.itemId;

                questionnaireService.moveQuestion(itemToMoveId, 0, chapterId, $state.params.questionnaireId).success(function(result) {
                    if (result.IsSuccess) {
                        questionnaireService.removeItemWithId($scope.items, itemToMoveId);
                        $scope.resetSelection();
                    }
                });
            };


            $scope.resetSelection = function() {
                $state.go('questionnaire.chapter', { chapterId: $state.params.chapterId });
            }

            questionnaireService.getChapterById($state.params.questionnaireId, $state.params.chapterId)
                .success(function(result) {
                    $scope.items = result.items;
                    $scope.currentChapter = result;
                    connectTree();

                    window.ContextMenuController.get().init();
                });
        }
    ]);