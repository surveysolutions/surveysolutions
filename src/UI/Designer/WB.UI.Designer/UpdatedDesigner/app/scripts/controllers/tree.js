angular.module('designerApp')
    .controller('TreeCtrl',
        function ($rootScope, $scope, $state, questionnaireService, commandService, verificationService, utilityService, confirmService, hotkeys, notificationService, $timeout) {
            'use strict';
            var me = this;

            var scrollMode = {
                makeVisible: "makeVisible",
                toTop: "toTop"
            };

            var filtersBlockModes = {
                default: 'default',
                search: 'search'
            };

            var itemTypes = {
                question: 'question',
                roster: 'roster',
                group: 'group',
                staticText: 'statictext'
            };

            $scope.itemTemplate = function (itemType) {
                return 'views/tree' + itemType + '.html';
            };

            $scope.highlightedId = null;
            $scope.isSearchInFocus = false;

            $scope.search = { searchText: '' };
            $scope.filtersBoxMode = filtersBlockModes.default;
            $scope.items = [];

            var scrollDown = 'down';
            var scrollUp = 'up';
            var focusSearchField = 'ctrl+f';
            var openTreeItemInEditor = 'enter';
           
            if (hotkeys.get(scrollDown) !== false) {
                hotkeys.del(scrollDown);
            }
            hotkeys.add(scrollDown, 'Navigate to next sibling', function (event) {
                event.preventDefault();
                $scope.goToNextItem();
            });

            if (hotkeys.get(scrollUp) !== false) {
                hotkeys.del(scrollUp);
            }
            hotkeys.add(scrollUp, 'Navigate to previous sibling', function (event) {
                event.preventDefault();
                $scope.goToPrevItem();
            });

            if (hotkeys.get(focusSearchField) !== false) {
                hotkeys.del(focusSearchField);
            }
            hotkeys.add({
                combo: focusSearchField,
                description: 'Search for sub-sections and questions in section',
                callback: function (event) {
                    event.preventDefault();
                    $scope.showSearch();
                }
            });

            if (hotkeys.get(openTreeItemInEditor) !== false) {
                hotkeys.del(openTreeItemInEditor);
            }
            hotkeys.add({
                combo: openTreeItemInEditor,
                allowIn: ['INPUT', 'SELECT'],
                description: 'Open item in editor',
                callback: function (event) {
                    event.preventDefault();
                    if ($scope.isSearchInFocus) {
                        utilityService.focusout('focusSearch');
                        $scope.isSearchInFocus = false;
                    } else {
                        if (_.isNull($scope.highlightedId)) return;
                        $state.go('questionnaire.chapter.' + getItemType(getCurrentItem()), { chapterId: $state.params.chapterId, itemId: $scope.highlightedId });
                        
                    }
                }
            });

            $scope.showSearch = function () {
                $scope.filtersBoxMode = filtersBlockModes.search;
                utilityService.focus('focusSearch');
                $scope.isSearchInFocus = true;
            };

            $scope.hideSearch = function () {
                $scope.filtersBoxMode = filtersBlockModes.default;
                $scope.search.searchText = '';
                $scope.isSearchInFocus = false;
            };

            $scope.searchItem = function (item) {
                if (!$scope.search.searchText) return true;
                var variableMatches = item.variable && item.variable.toLowerCase().indexOf($scope.search.searchText.toLowerCase()) != -1;
                var titleMatches = item.title && item.title.toLowerCase().indexOf($scope.search.searchText.toLowerCase()) != -1 ||
                                   item.text && item.text.toLowerCase().indexOf($scope.search.searchText.toLowerCase()) != -1;

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
                var currentItem = getCurrentItem();

                if (_.isNull(currentItem)) {
                    return;
                }

                if (_.isNull($scope.highlightedId)) {
                    highlightAndScroll(currentItem);
                    return;
                }

                var ids = _.map($(".question-list .item-body"), function (item) {
                    return ($(item).attr('id') || "");
                });

                var nextIndex = _.indexOf(ids, $scope.highlightedId) + updownStepValue;
                nextIndex = Math.min(Math.max(nextIndex, 0), ids.length);

                var nextId = ids[nextIndex];

                var target = questionnaireService.findItem($scope.items, nextId);

                if (!_.isNull(target)) {
                    highlightAndScroll(target);
                }
            };

            $scope.goToNextItem = function () {
                upDownMove(1);
            };

            $scope.goToPrevItem = function () {
                upDownMove(-1);
            };

            $scope.goToParent = function () {
                var itemToFind = getCurrentItem();
                if (itemToFind) {
                    var parent = itemToFind.getParentItem();
                    if (parent !== null) {
                        highlightAndScroll(target);
                    }
                }
            };

            $scope.goToChild = function () {
                var currentItem = getCurrentItem();
                if (currentItem && !_.isEmpty(currentItem.items)) {
                    var target = currentItem.items[0];
                    highlightAndScroll(target);
                }
            };

            $scope.toggle = function (scope) {
                scope.toggle();
            };


            var scrollToElement = function (itemId, howToScroll) {
                var mode = howToScroll || scrollMode.makeVisible;
                if ($(itemId).length === 0) {
                    return;
                }
                var elementVisibility = utilityService.isTreeItemVisible($(itemId));
                if (elementVisibility.isVisible) {
                    return;
                }

                if (mode === scrollMode.makeVisible) {
                    if (elementVisibility.shouldScrollDown) {
                        mode = scrollMode.toTop;
                    } else {
                        $scope.$broadcast("scrollToPosition", {
                            target: ".question-list",
                            scrollTop: elementVisibility.scrollPositionWhenScrollUp
                        });
                        return;
                    }
                }
                if (mode === scrollMode.toTop) {
                    var scrollPositionInDivFromTop = getScrollPositionInDivFromTop(itemId);
                    $scope.$broadcast("scrollToPosition", {
                        target: ".question-list",
                        scrollTop: scrollPositionInDivFromTop
                    });
                }
            };
            var getScrollPositionInDivFromTop = function (itemId) {
                var positionOfItemFromPageTop = $(itemId).offset().top;
                var positionOfContainerFromPageTop = $(".question-list").offset().top;
                var scrollOffset = 10;
                return Math.max(positionOfItemFromPageTop - positionOfContainerFromPageTop - scrollOffset, 0);
            }
            var getItemType = function (item) {
                switch (item.itemType) {
                    case 'Question': return itemTypes.question;
                    case 'Group': return (item.isRoster ? itemTypes.roster : itemTypes.group);
                    case 'StaticText': return itemTypes.staticText;
                }
                throw 'unknown item type: ' + item;
            };

            var highlightAndScroll = function (target) {
                $scope.highlightedId = target.itemId;
                scrollToElement("#" + target.itemId, scrollMode.makeVisible);
            }

            var getCurrentItem = function () {
                if (_.isNull($scope.items) || _.isUndefined($scope.items)) {
                    return null;
                }
                if (_.isNull($scope.highlightedId)) {
                    var firstItem = _.first($scope.items);
                    if (!_.isUndefined(firstItem)) {
                        $scope.highlightedId = firstItem.itemId;
                    } else {
                        return null;
                    }
                }
                return questionnaireService.findItem($scope.items, $scope.highlightedId);

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



            $scope.isQuestion = function (item) {
                return item.hasOwnProperty('type');
            };

            $scope.isGroup = function (item) {

                return !($scope.isQuestion(item) || $scope.isStaticText(item));
            };

            $scope.isStaticText = function (item) {
                return item.hasOwnProperty('text');
            };

            $scope.showStartScreen = function () {
                return _.isEmpty($scope.items);
            };

            $scope.groupsTree = {
                accept: function (sourceNodeScope, destNodesScope) {
                    var accept = !_.isEmpty(sourceNodeScope.item) && (_.isNull(destNodesScope.item) || destNodesScope.item.itemType === "Group");
                    return accept;
                },
                beforeDrop: function (event) {
                    me.draggedFrom = event.source.nodeScope.item.getParentItem();
                },
                dropped: function (event) {
                    connectTree();
                    var movedItem = event.source.nodeScope.item;
                    var destItem = event.dest.nodesScope.item;
                    var destGroupId = destItem ? destItem.itemId : $state.params.chapterId;
                    var putItem = function (item, parent, index) {
                        var dropFrom = item.getParentItem() || $scope;

                        dropFrom.items.splice(_.indexOf(dropFrom.items, item), 1);
                        var itemsToAddTo = _.isNull(parent) ? $scope.items : parent.items;
                        itemsToAddTo.splice(index, 0, item);

                        connectTree();
                    };

                    if (event.dest.nodesScope !== event.source.nodesScope || event.dest.index !== event.source.index) {
                        if ($scope.isQuestion(movedItem)) {
                            questionnaireService.moveQuestion(movedItem.itemId, event.dest.index, destGroupId, $state.params.questionnaireId)
                                .error(function () {
                                    putItem(movedItem, me.draggedFrom, event.source.index);
                                });
                        } else if ($scope.isStaticText(movedItem)) {
                            questionnaireService.moveStaticText(movedItem.itemId, event.dest.index, destGroupId, $state.params.questionnaireId)
                                .error(function () {
                                    putItem(movedItem, me.draggedFrom, event.source.index);
                                });
                        } else {
                            questionnaireService.moveGroup(movedItem.itemId, event.dest.index, destGroupId, $state.params.questionnaireId)
                                .error(function () {
                                    putItem(movedItem, me.draggedFrom, event.source.index);
                                });
                        }
                    }
                }
            };

            $scope.cloneQuestion = function (questionId) {
                var itemIdToClone = questionId || $state.params.itemId;
                var newId = utilityService.guid();
                commandService.cloneQuestion($state.params.questionnaireId, itemIdToClone, newId).success(function () {
                    var clonnedItem = questionnaireService.findItem($scope.items, itemIdToClone);
                    var parentItem = clonnedItem.getParentItem() || $scope;

                    var cloneDeep = {
                        itemId: newId,
                        variable: '',
                        title: clonnedItem.title,
                        itemType: "Question",
                        type: clonnedItem.type
                    };

                    var indexOf = _.indexOf(parentItem.items, clonnedItem);
                    parentItem.items.splice(indexOf + 1, 0, cloneDeep);
                    connectTree();
                    $state.go('questionnaire.chapter.question', { chapterId: $state.params.chapterId, itemId: newId });
                    $rootScope.$emit('questionAdded');
                });
            };

            $scope.cloneGroup = function (groupId) {
                var itemIdToClone = groupId || $state.params.itemId;
                var clonnedItem = questionnaireService.findItem($scope.items, itemIdToClone);
                var parentItem = clonnedItem.getParentItem() || $scope;
                var indexOf = _.indexOf(parentItem.items, clonnedItem);
                var newId = utilityService.guid();
                commandService.cloneGroup($state.params.questionnaireId, itemIdToClone, indexOf + 1, newId).success(function () {
                    $scope.refreshTree();
                    var publishAdd = function (added) {
                        var children = added.items || [];
                        $rootScope.$emit(getItemType(added) + 'Added');
                        _.each(children, function (child) {
                            publishAdd(child);
                        });
                    };

                    publishAdd(clonnedItem);
                    if (clonnedItem.isRoster) {
                        $state.go('questionnaire.chapter.roster', { chapterId: $state.params.chapterId, itemId: newId });
                    } else {
                        $state.go('questionnaire.chapter.group', { chapterId: $state.params.chapterId, itemId: newId });
                    }
                });
            };

            $scope.cloneStaticText = function (staticTextId) {
                var itemIdToClone = staticTextId || $state.params.itemId;
                var newId = utilityService.guid();
                commandService.cloneStaticText($state.params.questionnaireId, itemIdToClone, newId).success(function () {
                    var clonnedItem = questionnaireService.findItem($scope.items, itemIdToClone);
                    var parentItem = clonnedItem.getParentItem() || $scope;

                    var cloneDeep = {
                        itemId: newId,
                        itemType: "StaticText",
                        text: clonnedItem.text
                    };

                    var indexOf = _.indexOf(parentItem.items, clonnedItem);
                    parentItem.items.splice(indexOf + 1, 0, cloneDeep);
                    connectTree();
                    $state.go('questionnaire.chapter.statictext', { chapterId: $state.params.chapterId, itemId: newId });
                    $rootScope.$emit('staticTextCloned');
                });
            };

            $scope.deleteQuestion = function (item) {
                var itemIdToDelete = item.itemId || $state.params.itemId;

                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(item.title || "Untitled question"));

                modalInstance.result.then(function (confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteQuestion($state.params.questionnaireId, itemIdToDelete).success(function () {
                            questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                            $scope.resetSelection();
                            $rootScope.$emit('questionDeleted');
                        });
                    }
                });
            };

            var deleteGroupPermanently = function (itemIdToDelete) {
                commandService.deleteGroup($state.params.questionnaireId, itemIdToDelete)
                    .success(function () {
                        var publishDelete = function (deleted) {
                            var children = deleted.items || [];
                            $rootScope.$emit(getItemType(deleted) + 'Deleted');
                            _.each(children, function (child) {
                                publishDelete(child);
                            });
                        };

                        publishDelete(questionnaireService.findItem($scope.items, itemIdToDelete));

                        questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                        $scope.resetSelection();
                    });
            };

            $scope.deleteGroup = function (item) {
                var itemIdToDelete = item.itemId || $state.params.itemId;

                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(item.title));

                modalInstance.result.then(function (confirmResult) {
                    if (confirmResult === 'ok') {

                        questionnaireService.getAllBrokenGroupDependencies($state.params.questionnaireId, itemIdToDelete)
                            .success(function (result) {
                                if (result.length === 0) {
                                    deleteGroupPermanently(itemIdToDelete);
                                } else {

                                    var links = _.reduce(result, function (result, item) {
                                        return result + '<a href=#' + $state.params.questionnaireId +
                                            "/chapter/" + item.chapterId + "/question/" + item.id + '>' + _.trunc(item.title, 30) + '</a>';
                                    }, "");

                                    notificationService.notify({
                                        title: 'Depended items might be broken',
                                        text: '<div class="broken-links"><p>One or more questions/sub-sections depend on<p>' + links + '</div>',
                                        hide: false,
                                        confirm: { confirm: true },
                                        history: { history: false },
                                        buttons: {
                                            closer: false,
                                            sticker: false
                                        }
                                    }).get().on('pnotify.confirm', function () {
                                        deleteGroupPermanently(itemIdToDelete);
                                    });
                                }
                            });
                    }
                });
            };

            $scope.deleteStaticText = function (staticTextId) {
                var itemIdToDelete = staticTextId || $state.params.itemId;

                var item = questionnaireService.findItem($scope.items, itemIdToDelete);

                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(item.text));

                modalInstance.result.then(function (confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteStaticText($state.params.questionnaireId, itemIdToDelete)
                            .success(function (result) {
                                if (result.IsSuccess) {
                                    questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                                    $scope.resetSelection();
                                    $rootScope.$emit('staticTextDeleted');
                                }
                            });
                    }
                });
            };

            $scope.moveToChapter = function (chapterId) {
                var itemToMoveId = $state.params.itemId;
                var itemToMove = questionnaireService.findItem($scope.items, itemToMoveId);


                var moveCommand;
                if ($scope.isStaticText(itemToMove)) {
                    moveCommand = questionnaireService.moveStaticText;
                }
                else if ($scope.isGroup(itemToMove)) {
                    moveCommand = questionnaireService.moveGroup;
                }
                else {
                    moveCommand = questionnaireService.moveQuestion;
                }

                moveCommand(itemToMoveId, 0, chapterId, $state.params.questionnaireId).success(function () {
                    questionnaireService.removeItemWithId($scope.items, itemToMoveId);
                    $scope.resetSelection();
                });
            };

            $scope.resetSelection = function () {
                $state.go('questionnaire.chapter.group', { chapterId: $state.params.chapterId, itemId: $state.params.chapterId });
            };

            var getItemIndexByIdFromParentItemsList = function (parent, id) {
                if (_.isNull(parent) || _.isUndefined(parent))
                    return -1;

                var index = _.findIndex(parent.items, function (i) {
                    return i.itemId === id;
                }); //-1 if not found

                return index;
            };

            var emitAddedItemState = function (type, id) {
                $rootScope.$emit(type + "Added");
                $state.go("questionnaire.chapter." + type, { chapterId: $state.params.chapterId, itemId: id });
            };

            $scope.addQuestion = function (parent) {
                var emptyQuestion = utilityService.createEmptyQuestion(parent);

                commandService.addQuestion($state.params.questionnaireId, parent.itemId, emptyQuestion.itemId)
                    .success(function (result) {
                        if (!result.IsSuccess) return;
                        parent.items.push(emptyQuestion);
                        emitAddedItemState("question", emptyQuestion.itemId);
                    });
            };

            $scope.addGroup = function (parent) {
                var emptyGroup = utilityService.createEmptyGroup(parent);
                commandService.addGroup($state.params.questionnaireId, emptyGroup, parent.itemId)
                    .success(function () {
                        parent.items.push(emptyGroup);
                        emitAddedItemState("group", emptyGroup.itemId);
                    });
            };

            $scope.addRoster = function (parent) {
                var emptyRoster = utilityService.createEmptyRoster(parent);
                commandService.addRoster($state.params.questionnaireId, emptyRoster, parent.itemId)
                    .success(function () {
                        parent.items.push(emptyRoster);
                        emitAddedItemState("roster", emptyRoster.itemId);
                    });
            };

            $scope.addStaticText = function (parent) {
                var emptyStaticText = utilityService.createEmptyStaticText(parent);

                commandService.addStaticText($state.params.questionnaireId, emptyStaticText, parent.itemId)
                    .success(function () {
                        parent.items.push(emptyStaticText);
                        emitAddedItemState("statictext", emptyStaticText.itemId);
                    });
            };

            $scope.addQuestionAfter = function (item) {
                var parent = item.getParentItem() || $scope.currentChapter;
                var index = getItemIndexByIdFromParentItemsList(parent, item.itemId) + 1;
                var emptyQuestion = utilityService.createEmptyQuestion(parent);

                commandService.addQuestion($state.params.questionnaireId, parent.itemId, emptyQuestion.itemId, index)
                    .success(function (result) {
                        if (!result.IsSuccess) return;
                        parent.items.splice(index, 0, emptyQuestion);
                        emitAddedItemState("question", emptyQuestion.itemId);
                    });
            };

            $scope.addGroupAfter = function (item) {
                var parent = item.getParentItem() || $scope.currentChapter;

                var index = getItemIndexByIdFromParentItemsList(parent, item.itemId) + 1;

                var emptyGroup = utilityService.createEmptyGroup(parent);

                commandService.addGroup($state.params.questionnaireId, emptyGroup, parent.itemId, index)
                    .success(function () {
                        parent.items.splice(index, 0, emptyGroup);
                        emitAddedItemState("group", emptyGroup.itemId);
                    });
            };

            $scope.addRosterAfter = function (item) {
                var parent = item.getParentItem() || $scope.currentChapter;

                var index = getItemIndexByIdFromParentItemsList(parent, item.itemId) + 1;

                var emptyRoster = utilityService.createEmptyRoster(parent);

                commandService.addRoster($state.params.questionnaireId, emptyRoster, parent.itemId, index)
                    .success(function () {
                        parent.items.splice(index, 0, emptyRoster);
                        emitAddedItemState("roster", emptyRoster.itemId);
                    });
            };

            $scope.addStaticTextAfter = function (item) {
                var parent = item.getParentItem() || $scope.currentChapter;

                var index = getItemIndexByIdFromParentItemsList(parent, item.itemId) + 1;

                var emptyStaticText = utilityService.createEmptyStaticText(parent);

                commandService.addStaticText($state.params.questionnaireId, emptyStaticText, parent.itemId, index)
                    .success(function () {
                        parent.items.splice(index, 0, emptyStaticText);
                        emitAddedItemState("statictext", emptyStaticText.itemId);
                    });
            };

            $scope.refreshTree = function () {
                questionnaireService.getChapterById($state.params.questionnaireId, $state.params.chapterId)
                    .success(function (result) {
                        $scope.items = result.items;
                        $scope.currentChapter = result;
                        connectTree();
                    });
            };
            $scope.refreshTree();

            $scope.$on('scrollToElement', function (event, itemId) {
                if ($(itemId).length === 0) {
                    $timeout(function () {
                        scrollToElement(itemId);
                    }, 1000);
                } else {
                    scrollToElement(itemId);
                }
            });

            $rootScope.$on('questionUpdated', function (event, data) {
                var question = questionnaireService.findItem($scope.items, data.itemId);
                if (_.isNull(question)) return;

                question.title = data.title;
                question.variable = data.variable;
                question.type = data.type;
                question.linkedToQuestionId = data.linkedToQuestionId;
            });

            $rootScope.$on('staticTextUpdated', function (event, data) {
                var staticText = questionnaireService.findItem($scope.items, data.itemId);
                if (_.isNull(staticText)) return;
                staticText.text = data.text;
            });

            $rootScope.$on('groupUpdated', function (event, data) {
                if ($scope.currentChapter.itemId === data.itemId) {
                    $scope.currentChapter.title = data.title;
                }

                var group = questionnaireService.findItem($scope.items, data.itemId);
                if (_.isNull(group)) return;
                group.title = data.title;
            });

            $rootScope.$on('rosterUpdated', function (event, data) {
                var roster = questionnaireService.findItem($scope.items, data.itemId);
                if (_.isNull(roster)) return;
                roster.title = data.title;
                roster.variable = data.variable;
            });
        }
    );