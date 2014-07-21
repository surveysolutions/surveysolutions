angular.module('designerApp')
    .controller('TreeCtrl', [
        '$rootScope', '$scope', '$state', 'questionnaireService', 'commandService', 'verificationService', 'utilityService', 'confirmService', 'hotkeys', '$log',
        function($rootScope, $scope, $state, questionnaireService, commandService, verificationService, utilityService, confirmService, hotkeys, $log) {
            'use strict';
            var me = this;

            var filtersBlockModes = {
                default: 'default',
                search: 'search'
            };

            var itemTypes = {
                question: 'question',
                roster: 'roster',
                group: 'group',
                staticText: 'staticText'
            };

            $scope.search = { searchText: '' };
            $scope.filtersBoxMode = filtersBlockModes.default;
            $scope.items = [];

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
            };

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

                if (!item.hasOwnProperty('text'))
                    return itemTypes.staticText;
                throw 'unknown item type: ' + item;
            };

            $scope.isQuestion = function(item) {
                return item.hasOwnProperty('type');
            };

            $scope.isGroup = function(item) {
                return !($scope.isQuestion(item) || $scope.isStaticText(item));
            };

            $scope.isStaticText = function(item) {
                return item.hasOwnProperty('text');
            };

            $scope.showStartScreen = function() {
                return _.isEmpty($scope.items);
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
                        var dropFrom = item.getParentItem() || $scope;

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
                        } else if ($scope.isStaticText(movedItem)) {
                            questionnaireService.moveStaticText(movedItem.itemId, event.dest.index, destGroupId, $state.params.questionnaireId)
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

            $scope.deleteStaticText = function(staticTextId) {
                var itemIdToDelete = staticTextId || $state.params.itemId;

                var item = questionnaireService.findItem($scope.items, itemIdToDelete);

                var modalInstance = confirmService.open({
                    title: item.text.substring(0, 15) + (item.text.length > 15 ? "..." : "")
                });

                modalInstance.result.then(function(confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteStaticText($state.params.questionnaireId, itemIdToDelete)
                            .success(function(result) {
                                if (result.IsSuccess) {
                                    questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                                    $scope.resetSelection();
                                    $rootScope.$emit('staticTextDeleted');
                                }
                            });
                    }
                });
            };

            $scope.cloneStaticText = function(staticTextId) {
                var itemIdToClone = staticTextId || $state.params.itemId;
                var newId = utilityService.guid();
                commandService.cloneStaticText($state.params.questionnaireId, itemIdToClone, newId).success(function(result) {
                    if (result.IsSuccess) {
                        var clonnedItem = questionnaireService.findItem($scope.items, itemIdToClone);
                        var parentItem = clonnedItem.getParentItem() || $scope;

                        var cloneDeep = {
                            itemId: newId,
                            text: clonnedItem.text
                        };

                        var indexOf = _.indexOf(parentItem.items, clonnedItem);
                        parentItem.items.splice(indexOf + 1, 0, cloneDeep);
                        connectTree();
                        $rootScope.$emit('staticTextCloned');
                    }
                });
            };

            $scope.cloneQuestion = function(questionId) {
                var itemIdToClone = questionId || $state.params.itemId;
                var newId = utilityService.guid();
                commandService.cloneQuestion($state.params.questionnaireId, itemIdToClone, newId).success(function(result) {
                    if (result.IsSuccess) {
                        var clonnedItem = questionnaireService.findItem($scope.items, itemIdToClone);
                        var parentItem = clonnedItem.getParentItem() || $scope;

                        var cloneDeep = {
                            itemId: newId,
                            variable: '',
                            title: 'Copy of - ' + clonnedItem.title,
                            type: clonnedItem.type
                        };

                        var indexOf = _.indexOf(parentItem.items, clonnedItem);
                        parentItem.items.splice(indexOf + 1, 0, cloneDeep);
                        connectTree();
                        $rootScope.$emit('questionAdded');
                    }
                });
            };

            $scope.cloneGroup = function(groupId) {
                var itemIdToClone = groupId || $state.params.itemId;
                var clonnedItem = questionnaireService.findItem($scope.items, itemIdToClone);
                var parentItem = clonnedItem.getParentItem() || $scope;
                var indexOf = _.indexOf(parentItem.items, clonnedItem);
                var newId = utilityService.guid();

                commandService.cloneGroup($state.params.questionnaireId, itemIdToClone, indexOf + 1, newId).success(function(result) {
                    if (result.IsSuccess) {
                        $scope.refreshTree();
                        var publishAdd = function(added) {
                            var children = added.items || [];
                            $rootScope.$emit(getItemType(added) + 'Added');
                            _.each(children, function(child) {
                                publishAdd(child);
                            });
                        }

                        publishAdd(clonnedItem);
                    }
                });
            };

            $scope.deleteQuestion = function(item) {
                var itemIdToDelete = item.itemId || $state.params.itemId;

                var modalInstance = confirmService.open(item);

                modalInstance.result.then(function(confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteQuestion($state.params.questionnaireId, itemIdToDelete)
                            .success(function(result) {
                                if (result.IsSuccess) {
                                    questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                                    $scope.resetSelection();
                                    $rootScope.$emit('questionDeleted');
                                }
                            });
                    }
                });
            };

            $scope.deleteGroup = function(item) {
                var itemIdToDelete = item.itemId || $state.params.itemId;

                var modalInstance = confirmService.open(item);

                modalInstance.result.then(function(confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteGroup($state.params.questionnaireId, itemIdToDelete)
                            .success(function(result) {
                                if (result.IsSuccess) {
                                    var publishDelete = function(deleted) {
                                        var children = deleted.items || [];
                                        $rootScope.$emit(getItemType(deleted) + 'Deleted');
                                        _.each(children, function(child) {
                                            publishDelete(child);
                                        });
                                    }

                                    publishDelete(questionnaireService.findItem($scope.items, itemIdToDelete));

                                    questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                                    $scope.resetSelection();
                                }
                            });
                    }
                });
            };

            $scope.moveToChapter = function(chapterId) {
                var itemToMoveId = $state.params.itemId;
                var itemToMove = questionnaireService.findItem($scope.items, itemToMoveId);


                var moveCommand;
                if ($scope.isStaticText(itemToMove)) {
                    moveCommand = questionnaireService.moveStaticText;
                }
                if ($scope.isGroup(itemToMove)) {
                    moveCommand = questionnaireService.moveGroup;
                } else {
                    moveCommand = questionnaireService.moveQuestion;
                }

                moveCommand(itemToMoveId, 0, chapterId, $state.params.questionnaireId).success(function(result) {
                    if (result.IsSuccess) {
                        questionnaireService.removeItemWithId($scope.items, itemToMoveId);
                        $scope.resetSelection();
                    }
                });
            };

            $scope.resetSelection = function() {
                $state.go('questionnaire.chapter.group', { chapterId: $state.params.chapterId, itemId: $state.params.chapterId });
            };

            $scope.refreshTree = function() {
                questionnaireService.getChapterById($state.params.questionnaireId, $state.params.chapterId)
                    .success(function(result) {
                        $scope.items = result.items;
                        $scope.currentChapter = result;
                        connectTree();

                        window.ContextMenuController.get().init();
                    });
            }
            $scope.refreshTree();

            $rootScope.$on('questionUpdated', function(event, data) {
                var question = questionnaireService.findItem($scope.items, data.itemId);
                if (question != null) {
                    question.title = data.title;
                    question.variable = data.variable;
                    question.type = data.type;
                }
            });

            $rootScope.$on('staticTextUpdated', function(event, data) {
                var staticText = questionnaireService.findItem($scope.items, data.itemId);
                if (staticText != null) {
                    staticText.text = data.text;
                }
            });

            $rootScope.$on('groupUpdated', function(event, data) {
                if ($scope.currentChapter.itemId == data.itemId) {
                    $scope.currentChapter.title = data.title;
                }

                var group = questionnaireService.findItem($scope.items, data.itemId);
                if (group != null) {
                    group.title = data.title;
                }
            });

            $rootScope.$on('rosterUpdated', function(event, data) {
                var roster = questionnaireService.findItem($scope.items, data.itemId);
                if (roster != null) {
                    roster.title = data.title;
                }
            });
        }
    ]);