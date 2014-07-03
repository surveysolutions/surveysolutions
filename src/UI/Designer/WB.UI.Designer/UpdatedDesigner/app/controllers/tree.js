

angular.module('designerApp')
    .controller('TreeCtrl', [
        '$scope', '$stateParams', 'questionnaireId', 'questionnaireService', 'commandService', 'verificationService', 'utilityService', 'hotkeys', '$state', '$modal', '$log',
        function ($scope, $stateParams, questionnaireId, questionnaireService, commandService, verificationService, utilityService, hotkeys, $state, $modal, $log) {
            'use strict';
            var me = this;

            var filtersBlockModes = {
                default: 'default',
                search: 'search'
            };

            $scope.search = { searchText: '' };
            $scope.filtersBoxMode = filtersBlockModes.default;

            hotkeys.add({
                combo: 'ctrl+f',
                description: 'Search for groups and questions in chapter',
                callback: function (event) {
                    $scope.showSearch();
                    event.preventDefault();
                }
            });

            hotkeys.add('down', 'Navigate to next sibling', function (event) {
                event.preventDefault();
                $scope.goToNextItem();
            });

            hotkeys.add('up', 'Navigate to previous sibling', function (event) {
                event.preventDefault();
                $scope.goToPrevItem();
            });

            hotkeys.add('left', 'Navigate to parent', function (event) {
                event.preventDefault();
                $scope.goToParent();
            });

            hotkeys.add('right', 'Navigate to child', function (event) {
                event.preventDefault();
                $scope.goToChild();
            });

            $scope.showSearch = function () {
                $scope.filtersBoxMode = filtersBlockModes.search;
                utilityService.focus('focusSearch');
            };

            $scope.hideSearch = function () {
                $scope.filtersBoxMode = filtersBlockModes.default;
                $scope.search.searchText = '';
            };

            $scope.searchItem = function (item) {
                if (!$scope.search.searchText) return true;
                var variableMatches = item.variable && item.variable.toLowerCase().indexOf($scope.search.searchText.toLowerCase()) != -1;
                var titleMatches = item.title && item.title.toLowerCase().indexOf($scope.search.searchText.toLowerCase()) != -1;

                var found = variableMatches || titleMatches;

                if (!found) {
                    angular.forEach(item.items, function (item) {
                        var match = $scope.searchItem(item);
                        if (match) {
                            found = true;
                        }
                    });
                }

                return found;
            };


            var upDownMove = function (updownStepValue) {
                if ($scope.items && $scope.currentItem) {
                    var parent = $scope.currentItem.getParentItem();

                    if (_.isNull(parent)) {
                        var siblingIndex = _.indexOf($scope.items, $scope.currentItem) + updownStepValue;
                        if (siblingIndex < $scope.items.length && siblingIndex >= 0) {
                            $scope.nav($stateParams.questionnaireId, $scope.currentChapterId, $scope.items[siblingIndex]);
                        }
                        return;
                    }

                    var nextItemIndex = _.indexOf(parent.items, $scope.currentItem) + updownStepValue;

                    if (nextItemIndex < $scope.items.length && nextItemIndex >= 0) {
                        $scope.nav($stateParams.questionnaireId, $scope.currentChapterId, parent.items[nextItemIndex]);
                    }
                }
            };

            $scope.goToNextItem = function () {
                upDownMove(1);
            };

            $scope.goToPrevItem = function () {
                upDownMove(-1);
            };

            $scope.goToParent = function () {
                if ($scope.items && $scope.currentItem) {
                    var parent = $scope.currentItem.getParentItem();
                    if (parent != null) {
                        $scope.nav($stateParams.questionnaireId, $scope.currentChapterId, parent);
                    }
                }
            };

            $scope.goToChild = function () {
                if ($scope.items && $scope.currentItem) {
                    if (!_.isEmpty($scope.currentItem.items)) {
                        $scope.nav($stateParams.questionnaireId, $scope.currentChapterId, $scope.currentItem.items[0]);
                    }
                }
            };

            var connectTree = function () {
                var setParent = function (item, parent) {
                    item.getParentItem = function () {
                        return parent;
                    };
                    _.each(item.items, function (child) {
                        setParent(child, item);
                    });
                };

                _.each($scope.items, function (item) {
                    setParent(item, null);
                });
            };

            $scope.groupsTree = {
                accept: function (sourceNodeScope, destNodesScope) {
                    var message = _.isNull(destNodesScope.item) || $scope.isGroup(destNodesScope.item);
                    return message;
                },
                beforeDrop: function (event) {
                    me.draggedFrom = event.source.nodeScope.item.getParentItem();
                },
                dropped: function (event) {

                    var movedItem = event.source.nodeScope.item;
                    var destItem = event.dest.nodesScope.item;
                    var destGroupId = destItem ? destItem.itemId : $scope.questionnaire.chapters[0].itemId;
                    var putItem = function (item, parent, index) {
                        var dropFrom = item.parent || $scope;

                        dropFrom.items.splice(_.indexOf(dropFrom.items, item), 1);
                        var itemsToAddTo = _.isNull(parent) ? $scope.items : parent.items;
                        itemsToAddTo.splice(index, 0, item);

                        connectTree();
                    };

                    if (event.dest.nodesScope !== event.source.nodesScope || event.dest.index !== event.source.index) {
                        if ($scope.isQuestion(movedItem)) {
                            questionnaireService.moveQuestion(movedItem.itemId, event.dest.index, destGroupId, $stateParams.questionnaireId)
                                .success(function (data) {
                                    if (!data.IsSuccess) {
                                        putItem(movedItem, me.draggedFrom, event.source.index);
                                    }
                                })
                                .error(function () {
                                    putItem(movedItem, me.draggedFrom, event.source.index);
                                });
                        } else {
                            questionnaireService.moveGroup(movedItem.itemId, event.dest.index, destGroupId, $stateParams.questionnaireId)
                                .success(function (data) {
                                    if (!data.IsSuccess) {
                                        putItem(movedItem, me.draggedFrom, event.source.index);

                                    }
                                })
                                .error(function () {
                                    putItem(movedItem, me.draggedFrom, event.source.index);
                                });
                        }
                    }
                }
            };

            questionnaireService.getChapterById(questionnaireId, $stateParams.chapterId)
                .success(function (result) {
                    $scope.items = result.items;
                    $scope.currentChapter = result;
                    connectTree();

                    window.ContextMenuController.get().init();
                });
        }
    ]);