angular.module('designerApp')
    .controller('TreeCtrl',
        function ($rootScope, $scope, $state, $sce, $i18next, questionnaireService, commandService, verificationService, utilityService, confirmService, 
            hotkeys, notificationService, $timeout, $uibModal) {
            'use strict';
            var me = this;
            var emptySectionAddQuestion = "<button class='btn' disabled type='button'>"+ $i18next.t('AddQuestion') +" </button>";
            var emptySectionAddSubsectionHtml = "<button class=\"btn\" disabled type=\"button\">"+ $i18next.t('AddSubsection') +" </button>";
            var emptySectionSettingsHtml = "<button class=\"btn\" type=\"button\" disabled>" + $i18next.t('Settings') + " </button>";
            $scope.emptySectionHtmlLine1 = $sce.trustAsHtml($i18next.t('EmptySectionLine2', {addQuestionBtn: emptySectionAddQuestion, addSubsectionBtn: emptySectionAddSubsectionHtml}));
            $scope.emptySectionHtmlLine2 = $sce.trustAsHtml($i18next.t('EmptySectionLine5', {settingsBtn: emptySectionSettingsHtml}))

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
                staticText: 'statictext',
                variable: 'variable'
            };

            $scope.itemTemplate = function (itemType) {
                return 'views/tree' + itemType.toLowerCase() + '.html';//to use cache casted to lower
            };

            $scope.highlightedId = null;
            $scope.isSearchInFocus = false;
            

            $scope.search = { searchText: '' };
            $scope.filtersBoxMode = filtersBlockModes.default;
            $scope.items = [];

            $rootScope.readyToPaste = !(_.isNull(Cookies.get('itemToCopy')) || _.isUndefined(Cookies.get('itemToCopy')));

            var scrollDown = 'down';
            var scrollUp = 'up';
            var focusSearchField = 'ctrl+f';
            var openTreeItemInEditor = 'enter';
           
            
            $scope.searchForQuestion = function(parent) {
                var showModal = function() {
                    var modalInstance = $uibModal.open({
                        templateUrl: 'views/search-for-question.html',
                        backdrop: false,
                        windowClass: "add-classification-modal search-for-question-modal dragAndDrop",
                        controller: 'searchForQuestionCtrl',
                        resolve: {
                            isReadOnlyForUser: $scope.questionnaire.isReadOnlyForUser || false
                        }
                    });

                    modalInstance.result.then(
                        function(entityToPaste) {
                            var newId = utilityService.guid();

                            commandService.pasteItemInto($state.params.questionnaireId, parent.itemId, entityToPaste.questionnaireId, entityToPaste.itemId, newId)
                                .then(function () {

                                $scope.refreshTree();

                                $rootScope.$emit('itemPasted');
                                $state.go('questionnaire.chapter.' + entityToPaste.itemType, { chapterId: $state.params.chapterId, itemId: newId });
                            });
                        },
                        function() { });
                };
                showModal();
            }

            if (hotkeys.get(scrollDown) !== false) {
                hotkeys.del(scrollDown);
            }
            hotkeys.add(scrollDown, $i18next.t('HotkeysNavigateToSibling'), function (event) {
                event.preventDefault();
                $scope.goToNextItem();
            });

            if (hotkeys.get(scrollUp) !== false) {
                hotkeys.del(scrollUp);
            }
            hotkeys.add(scrollUp,  $i18next.t('HotkeysNavigateToPrevSibling'), function (event) {
                event.preventDefault();
                $scope.goToPrevItem();
            });

            if (hotkeys.get(focusSearchField) !== false) {
                hotkeys.del(focusSearchField);
            }
            hotkeys.add({
                combo: focusSearchField,
                description: $i18next.t('HotkeysSearch'),
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
                description: $i18next.t('HotkeysOpenItem'),
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

            $scope.migrateToNewVersion = function () {
                commandService.migrateToNewVersion($state.params.questionnaireId)
                    .then(function () {
                        document.location.reload();
                    });
            };

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
                        highlightAndScroll(parent);
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
            var getScrollPositionInDivFromTop = function(itemId) {
                var positionOfItemFromPageTop = $(itemId).offset().top;
                var positionOfContainerFromPageTop = $(".question-list").offset().top;
                var scrollOffset = 10;
                return Math.max(positionOfItemFromPageTop - positionOfContainerFromPageTop - scrollOffset, 0);
            };
            var getItemType = function (item) {
                switch (item.itemType) {
                    case 'Question': return itemTypes.question;
                    case 'Group': return (item.isRoster ? itemTypes.roster : itemTypes.group);
                    case 'StaticText': return itemTypes.staticText;
                    case 'Chapter': return itemTypes.group;
                    case 'Variable': return itemTypes.variable;
                }
                throw 'unknown item type: ' + item;
            };

            var highlightAndScroll = function(target) {
                $scope.highlightedId = target.itemId;
                scrollToElement("#" + target.itemId, scrollMode.makeVisible);
            };

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
                return !($scope.isQuestion(item) || $scope.isStaticText(item) || $scope.isVariable(item));
            };

            $scope.isStaticText = function (item) {
                return item.hasOwnProperty('text');
            };

            $scope.isVariable = function(item) {
                return item.hasOwnProperty('variableData');
            }

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
                    var rollbackMoveAction = function (item, parent, index, destEvent) {
                        var dropFrom = item.getParentItem() || $scope.currentChapter;

                        var indexOfItem = _.indexOf(dropFrom.items, item);
                        if (indexOfItem >= 0)
                            dropFrom.items.splice(indexOfItem, 1);
                        var itemsToAddTo = _.isNull(parent) ? $scope.items : parent.items;
                        itemsToAddTo.splice(index, 0, item);

                        var droppedTo = destEvent.nodesScope.item || $scope.currentChapter;
                        droppedTo.items.splice(destEvent.index, 1);

                        connectTree();
                    };

                    if (event.dest.nodesScope !== event.source.nodesScope || event.dest.index !== event.source.index) {
                        if ($scope.isQuestion(movedItem)) {
                            questionnaireService.moveQuestion(movedItem.itemId, event.dest.index, destGroupId, $state.params.questionnaireId)
                                .then(
                                    function () {
                                        $rootScope.$emit('questionMoved', movedItem.itemId);
                                        connectTree();
                                    },
                                    function () {
                                        rollbackMoveAction(movedItem, me.draggedFrom, event.source.index, event.dest);
                                    });
                        } else if ($scope.isStaticText(movedItem)) {
                            questionnaireService.moveStaticText(movedItem.itemId, event.dest.index, destGroupId, $state.params.questionnaireId)
                                .then(function () {
                                    $rootScope.$emit('staticTextMoved', movedItem.itemId);
                                    connectTree();
                                },
                                function () {
                                    rollbackMoveAction(movedItem, me.draggedFrom, event.source.index, event.dest);
                                });
                            
                        }
                        else if ($scope.isVariable(movedItem)) {
                            questionnaireService.moveVariable(movedItem.itemId, event.dest.index, destGroupId, $state.params.questionnaireId)
                                .then(function () {
                                    $rootScope.$emit('variableMoved', movedItem.itemId);
                                    connectTree();
                                },
                                function () {
                                    rollbackMoveAction(movedItem, me.draggedFrom, event.source.index, event.dest);
                                });
                        }
                        else {
                            questionnaireService.moveGroup(movedItem.itemId, event.dest.index, destGroupId, $state.params.questionnaireId)
                                .then(function () {
                                    $rootScope.$emit('groupMoved', movedItem.itemId);
                                    connectTree();
                                },
                                function () {
                                    rollbackMoveAction(movedItem, me.draggedFrom, event.source.index, event.dest);
                                });
                        }
                    }
                }
            };

            var removeSelectionIfHighlighted = function(id) {
                if ($scope.highlightedId == id) {
                    $scope.highlightedId = null;
                }
            };

            $scope.deleteQuestion = function (item) {
                var itemIdToDelete = item.itemId || $state.params.itemId;

                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(item.title || $i18next.t('UntitledQuestion')));

                modalInstance.result.then(function (confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteQuestion($state.params.questionnaireId, itemIdToDelete).then(function () {
                            questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                            $scope.resetSelection();
                            $rootScope.$emit('questionDeleted', itemIdToDelete);
                            removeSelectionIfHighlighted(itemIdToDelete);
                        });
                    }
                });
            };
            
            $scope.deleteVariable = function (item) {
                var itemIdToDelete = item.itemId || $state.params.itemId;

                var label = _.isUndefined(item.variableData) ? item.label : item.variableData.label;

                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(label || $i18next.t('UntitledVariable')));

                modalInstance.result.then(function (confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteVariable($state.params.questionnaireId, itemIdToDelete).then(function () {
                            questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                            $scope.resetSelection();
                            $rootScope.$emit('variableDeleted', itemIdToDelete);
                            removeSelectionIfHighlighted(itemIdToDelete);
                        });
                    }
                });
            };


            var deleteGroupPermanently = function (itemIdToDelete) {
                commandService.deleteGroup($state.params.questionnaireId, itemIdToDelete)
                    .then(function () {
                        var publishDelete = function (deleted) {
                            var children = deleted.items || [];
                            $rootScope.$emit(getItemType(deleted) + 'Deleted', deleted.itemId);
                            _.each(children, function (child) {
                                publishDelete(child);
                            });
                            removeSelectionIfHighlighted(deleted.itemId);
                        };

                        publishDelete(questionnaireService.findItem($scope.items, itemIdToDelete));

                        questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                        $scope.resetSelection();
                    });
            };

            $scope.deleteGroup = function (item) {
                var itemIdToDelete = item.itemId || $state.params.itemId;

                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(item.title || $i18next.t('UntitledGroupOrRoster')));

                modalInstance.result.then(function (confirmResult) {
                    if (confirmResult === 'ok') {

                        questionnaireService.getAllBrokenGroupDependencies($state.params.questionnaireId, itemIdToDelete)
                            .then(function (result) {
                                var data = result.data;
                                if (data.length === 0) {
                                    deleteGroupPermanently(itemIdToDelete);
                                } else {

                                    var links = _.reduce(data, function (result, item) {
                                        return result + '<a href=#' + $state.params.questionnaireId +
                                            "/chapter/" + item.chapterId + "/question/" + item.id + '>' + item.title.substring(0, 30) + '</a>';
                                    }, "");

                                    notificationService.notify({
                                        title: $i18next.t('ConditionMightBeBroken'),
                                        text: '<div class="broken-links"><p>'+ $i18next.t('MultipleDependencies') +'<p>' + links + '</div>',
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
                        removeSelectionIfHighlighted(itemIdToDelete);
                    }
                });
            };

            $scope.deleteStaticText = function (staticTextId) {
                var itemIdToDelete = staticTextId || $state.params.itemId;

                var item = questionnaireService.findItem($scope.items, itemIdToDelete);

                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(item.text || $i18next.t('UntitledStaticText')));

                modalInstance.result.then(function (confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteStaticText($state.params.questionnaireId, itemIdToDelete)
                            .then(function (result) {
                                if (result.status === 200) {
                                    questionnaireService.removeItemWithId($scope.items, itemIdToDelete);
                                    $scope.resetSelection();
                                    $rootScope.$emit('statictextDeleted', itemIdToDelete);
                                    removeSelectionIfHighlighted(itemIdToDelete);
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
                else if ($scope.isVariable(itemToMove)) {
                    moveCommand = questionnaireService.moveVariable;
                }
                else {
                    moveCommand = questionnaireService.moveQuestion;
                }

                moveCommand(itemToMoveId, 0, chapterId, $state.params.questionnaireId).then(function () {
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
                    .then(function () {
                        parent.items.push(emptyQuestion);
                        emitAddedItemState("question", emptyQuestion.itemId);
                    });
            };

            $scope.addGroup = function (parent) {
                var emptyGroup = utilityService.createEmptyGroup(parent);
                commandService.addGroup($state.params.questionnaireId, emptyGroup, parent.itemId)
                    .then(function () {
                        parent.items.push(emptyGroup);
                        emitAddedItemState("group", emptyGroup.itemId);
                    });
            };

            $scope.addRoster = function (parent) {
                var emptyRoster = utilityService.createEmptyRoster(parent);
                commandService.addRoster($state.params.questionnaireId, emptyRoster, parent.itemId)
                    .then(function () {
                        parent.items.push(emptyRoster);
                        emitAddedItemState("roster", emptyRoster.itemId);
                    });
            };

            $scope.addStaticText = function (parent) {
                var emptyStaticText = utilityService.createEmptyStaticText(parent);

                commandService.addStaticText($state.params.questionnaireId, emptyStaticText, parent.itemId)
                    .then(function () {
                        parent.items.push(emptyStaticText);
                        emitAddedItemState("statictext", emptyStaticText.itemId);
                    });
            };

            $scope.addVariable = function(parent) {
                var emptyVariable = utilityService.createEmptyVariable(parent);

                commandService.addVariable($state.params.questionnaireId, emptyVariable, parent.itemId)
                    .then(function() {
                        parent.items.push(emptyVariable);
                        emitAddedItemState("variable", emptyVariable.itemId);
                    });
            };

            $scope.addVariableAfter = function(item) {
                var parent = item.getParentItem() || $scope.currentChapter;
                var index = getItemIndexByIdFromParentItemsList(parent, item.itemId) + 1;

                var emptyVariable = utilityService.createEmptyVariable(parent);

                commandService.addVariable($state.params.questionnaireId, emptyVariable, parent.itemId, index)
                    .then(function() {
                        parent.items.splice(index, 0, emptyVariable);
                        emitAddedItemState("variable", emptyVariable.itemId);
                    });
            };

            $scope.addQuestionAfter = function (item) {
                var parent = item.getParentItem() || $scope.currentChapter;
                var index = getItemIndexByIdFromParentItemsList(parent, item.itemId) + 1;
                var emptyQuestion = utilityService.createEmptyQuestion(parent);

                commandService.addQuestion($state.params.questionnaireId, parent.itemId, emptyQuestion.itemId, index)
                    .then(function (result) {
                        if (result.status !== 200) return;
                        parent.items.splice(index, 0, emptyQuestion);
                        emitAddedItemState("question", emptyQuestion.itemId);
                    });
            };

            $scope.addGroupAfter = function (item) {
                var parent = item.getParentItem() || $scope.currentChapter;

                var index = getItemIndexByIdFromParentItemsList(parent, item.itemId) + 1;

                var emptyGroup = utilityService.createEmptyGroup(parent);

                commandService.addGroup($state.params.questionnaireId, emptyGroup, parent.itemId, index)
                    .then(function () {
                        parent.items.splice(index, 0, emptyGroup);
                        emitAddedItemState("group", emptyGroup.itemId);
                    });
            };

            $scope.addRosterAfter = function (item) {
                var parent = item.getParentItem() || $scope.currentChapter;

                var index = getItemIndexByIdFromParentItemsList(parent, item.itemId) + 1;

                var emptyRoster = utilityService.createEmptyRoster(parent);

                commandService.addRoster($state.params.questionnaireId, emptyRoster, parent.itemId, index)
                    .then(function () {
                        parent.items.splice(index, 0, emptyRoster);
                        emitAddedItemState("roster", emptyRoster.itemId);
                    });
            };

            $scope.addStaticTextAfter = function (item) {
                var parent = item.getParentItem() || $scope.currentChapter;

                var index = getItemIndexByIdFromParentItemsList(parent, item.itemId) + 1;

                var emptyStaticText = utilityService.createEmptyStaticText(parent);

                commandService.addStaticText($state.params.questionnaireId, emptyStaticText, parent.itemId, index)
                    .then(function () {
                        parent.items.splice(index, 0, emptyStaticText);
                        emitAddedItemState("statictext", emptyStaticText.itemId);
                    });
            };

            $scope.pasteItemInto = function (parent) {

                var itemToCopy = Cookies.getJSON('itemToCopy');
                if (_.isNull(itemToCopy) || _.isUndefined(itemToCopy))
                    return;
                
                var newId = utilityService.guid();

                commandService.pasteItemInto($state.params.questionnaireId, parent.itemId, itemToCopy.questionnaireId, itemToCopy.itemId, newId).then(function () {
                    $scope.refreshTree();
                    $rootScope.$emit('itemPasted');
                    if (!parent.isCover)
                        $state.go('questionnaire.chapter.' + itemToCopy.itemType, { chapterId: $state.params.chapterId, itemId: newId });
                });
            };

            $scope.pasteItemAfter = function (item) {

                var itemToCopy = Cookies.getJSON('itemToCopy');
                if (_.isNull(itemToCopy) || _.isUndefined(itemToCopy))
                    return;

                var idToPasteAfter = item.itemId || $state.params.itemId;
                var newId = utilityService.guid();

                commandService.pasteItemAfter($state.params.questionnaireId, idToPasteAfter, itemToCopy.questionnaireId, itemToCopy.itemId, newId).then(function () {
                    $scope.refreshTree();
                    $rootScope.$emit('itemPasted');
                    if (!$scope.currentChapter.isCover)
                        $state.go('questionnaire.chapter.' + itemToCopy.itemType, { chapterId: $state.params.chapterId, itemId: newId });
                });
            };

            $rootScope.copyRef = function (item) {
                var itemIdToCopy = item.itemId || $state.params.itemId;

                var itemToCopy = {
                    questionnaireId: $state.params.questionnaireId,
                    itemId: itemIdToCopy,
                    itemType: getItemType(item)
                };

                Cookies.remove('itemToCopy');
                Cookies.set('itemToCopy', itemToCopy, { expires: 7 });

                $rootScope.readyToPaste = true;
            };

            $scope.refreshTree = function () {
                questionnaireService.getChapterById($state.params.questionnaireId, $state.params.chapterId)
                    .then(function (result) {
                        var data = result.data;
                        $scope.items = data.chapter.items;
                        $scope.currentChapter = data.chapter;
                        $scope.currentChapter.isCover = data.isCover;
                        $scope.currentChapter.isReadOnly = data.isReadOnly;

                        $scope.currentChapter.hideIfDisabled = data.hideIfDisabled;
                        $scope.currentChapter.hasCondition = data.hasCondition;
                        
                        $rootScope.updateVariableNames(data.variableNames);
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
                question.hasValidation = data.hasValidation;
                question.hasCondition = data.hasCondition;
                question.linkedToEntityId = data.linkedToEntityId;
                question.linkedToType = data.linkedToType;
                question.isInteger = data.isInteger;
                question.yesNoView = data.yesNoView;
                question.hideIfDisabled = data.hideIfDisabled;

                $rootScope.updateVariableTypes(question);
            });

            $rootScope.$on('staticTextUpdated', function (event, data) {
                var staticText = questionnaireService.findItem($scope.items, data.itemId);
                if (_.isNull(staticText)) return;
                staticText.text = data.text;
                staticText.attachmentName = data.attachmentName;

                staticText.hasValidation = data.hasValidation;
                staticText.hasCondition = data.hasCondition;
                staticText.hideIfDisabled = data.hideIfDisabled;
            });

            $rootScope.$on('variableUpdated', function (event, data) {
                var variable = questionnaireService.findItem($scope.items, data.itemId);
                if (_.isNull(variable)) return;
                variable.variableData.name = data.name;
                variable.variableData.label = data.label;
                $rootScope.updateSelfVariable(data.type);
                $rootScope.addOrUpdateLocalVariable(data.itemId, data.name, data.type);
            });

            $rootScope.$on('groupUpdated', function (event, data) {
                if ($scope.currentChapter.itemId === data.itemId) {
                    $scope.currentChapter.title = data.title;
                    $scope.currentChapter.hasCondition = data.hasCondition;
                    $scope.currentChapter.hideIfDisabled = data.hideIfDisabled;
                }

                var group = questionnaireService.findItem($scope.items, data.itemId);
                if (_.isNull(group)) return;
                group.title = data.title;
                group.variable = data.variable;
                group.hasCondition = data.hasCondition;
                group.hideIfDisabled = data.hideIfDisabled;

                $rootScope.addOrUpdateLocalVariable(data.itemId, data.variable, data.type);
            });

            $rootScope.$on('rosterUpdated', function (event, data) {
                var roster = questionnaireService.findItem($scope.items, data.itemId);
                if (_.isNull(roster)) return;
                roster.title = data.title;
                roster.variable = data.variable;
                roster.hasCondition = data.hasCondition;
                roster.hideIfDisabled = data.hideIfDisabled;
                $rootScope.updateSelfVariable(data.type);
                $rootScope.addOrUpdateLocalVariable(data.itemId, data.variable, data.type);
            });
        }
    );
