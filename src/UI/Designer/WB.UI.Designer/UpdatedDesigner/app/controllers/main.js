(function() {
    'use strict';

    angular.module('designerApp')
        .controller('MainCtrl', [
            '$scope', '$routeParams', '$route', 'questionnaireService', 'commandService', 'verificationService', 'utilityService', 'hotkeys', 'navigationService', '$modal', '$log',
            function($scope, $routeParams, $route, questionnaireService, commandService, verificationService, utilityService, hotkeys, navigationService, $modal, $log) {

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

                $scope.verificationStatus = {
                    errorsCount: null,
                    errors: []
                };

                var filtersBlockModes = {
                    default: 'default',
                    search: 'search'
                };

                $scope.verify = function() {
                    verificationService.verify($routeParams.questionnaireId).success(function(result) {
                        $scope.verificationStatus.errors = result.errors;
                        $scope.verificationStatus.errorsCount = result.errors.length;

                        if ($scope.verificationStatus.errorsCount > 0) {
                            $('#verification-modal').modal({
                                backdrop: false,
                                show: true
                            });
                        }
                    });
                };

                $scope.navigateTo = function(itemId, chapterId) {
                    navigationService.openItem($routeParams.questionnaireId, chapterId, itemId);
                };

                $scope.answerTypeClass = {
                    YesNo: 'cat-singleanswer',
                    DropDownList: 'cat-singleanswer',
                    MultyOption: 'cat-multianswer',
                    Numeric: 'cat-intedit',
                    DateTime: 'cat-datetime',
                    GpsCoordinates: 'cat-geoloc',
                    AutoPropagate: 'cat-textedit',
                    TextList: 'cat-textarea',
                    QRBarcode: 'cat-multimedia',
                    Text: 'cat-textedit',
                    SingleOption: 'cat-singleanswer'
                };

                $scope.chapters = [];

                $scope.items = [];

                $scope.item = null;

                $scope.search = { searchText: '' };

                $scope.questionnaire = null;

                $scope.filtersBoxMode = filtersBlockModes.default;

                $scope.groupsTree = {
                    dropped: function(event) {
                        var movedItem = event.source.nodeScope.item;
                        var destItem = event.dest.nodesScope.item;
                        var destGroupId = destItem ? destItem.itemId : $scope.questionnaire.chapters[0].itemId;

                        if ($scope.isQuestion(movedItem)) {
                            questionnaireService.moveQuestion(movedItem.itemId, event.dest.index, destGroupId, $routeParams.questionnaireId);
                        } else {
                            questionnaireService.moveGroup(movedItem.itemId, event.dest.index, destGroupId, $routeParams.questionnaireId);
                        }
                    }
                };

                $scope.currentChapter = null;

                $scope.currentChapterId = null;

                $scope.resetSelection = function() {
                    navigationService.openChapter($routeParams.questionnaireId, $scope.currentChapterId);
                    $scope.currentItemId = null;
                    $scope.activeRoster = null;
                    $scope.activeQuestion = null;
                };

                var upDownMove = function(updownStepValue) {
                    if ($scope.items && $scope.currentItem) {
                        var parent = $scope.currentItem.parent;

                        if (_.isNull(parent)) {
                            var siblingIndex = _.indexOf($scope.items, $scope.currentItem) + updownStepValue;
                            if (siblingIndex < $scope.items.length && siblingIndex >= 0) {
                                $scope.nav($routeParams.questionnaireId, $scope.currentChapterId, $scope.items[siblingIndex]);
                            }
                            return;
                        }

                        var nextItemIndex = _.indexOf(parent.items, $scope.currentItem) + updownStepValue;

                        if (nextItemIndex < $scope.items.length && nextItemIndex >= 0) {
                            $scope.nav($routeParams.questionnaireId, $scope.currentChapterId, parent.items[nextItemIndex].item);
                        }
                    }
                }

                $scope.goToNextItem = function() {
                    upDownMove(1);
                };

                $scope.goToPrevItem = function() {
                    upDownMove(-1);
                };

                $scope.goToParent = function() {
                    if ($scope.items && $scope.currentItem) {
                        if ($scope.currentItem.parent != null) {
                            $scope.nav($routeParams.questionnaireId, $scope.currentChapterId, $scope.currentItem.parent);
                        }
                    }
                }

                $scope.goToChild = function() {
                    if ($scope.items && $scope.currentItem) {
                        if (!_.isEmpty($scope.currentItem.items)) {
                            $scope.nav($routeParams.questionnaireId, $scope.currentChapterId, $scope.currentItem.items[0]);
                        }
                    }
                }

                $scope.nav = function(questionnaireId, currentChapterId, item) {
                    $scope.currentItemId = item.itemId;
                    $scope.currentItem = item;
                    if ($scope.isQuestion(item)) {
                        $scope.activeRoster = undefined;
                        $scope.activeChapter = undefined;
                        $scope.activeQuestion = item;
                        navigationService.openQuestion(questionnaireId, currentChapterId, item.itemId);
                    } else if (item.isRoster) {
                        $scope.activeRoster = item;
                        $scope.activeQuestion = undefined;
                        $scope.activeChapter = undefined;
                        navigationService.openRoster(questionnaireId, currentChapterId, item.itemId);
                    } else {
                        $scope.activeRoster = undefined;
                        $scope.activeQuestion = undefined;
                        $scope.activeChapter = item;
                        navigationService.openGroup(questionnaireId, currentChapterId, item.itemId);
                    }
                };

                $scope.setItem = function(item) {
                    $scope.nav($routeParams.questionnaireId, $scope.currentChapterId, item);
                };

                $scope.changeChapter = function(chapter) {
                    navigationService.openChapter($routeParams.questionnaireId, chapter.itemId);
                    $scope.currentChapterId = chapter.itemId;
                    $scope.loadChapterDetails($routeParams.questionnaireId, $scope.currentChapterId);
                };

                $scope.loadChapterDetails = function(questionnaireId, chapterId) {
                    questionnaireService.getChapterById(questionnaireId, chapterId)
                        .success(function(result) {

                            var setParent = function(item, parent) {
                                item.parent = parent;
                                _.each(item.items, function(child) {
                                    setParent(child, item);
                                });
                            }

                            _.each(result.items, function(item) {
                                setParent(item, null);
                            });

                            $scope.items = result.items;
                            $scope.currentChapter = result;

                            window.ContextMenuController.get().init();
                        });
                };

                $scope.isQuestion = function(item) {
                    return item.hasOwnProperty('type');
                };

                $scope.addQuestion = function(parent) {
                    var newId = utilityService.guid();
                    var variable = "q" + newId.substring(0, 5);
                    var emptyQuestion = {
                        "itemId": newId,
                        "title": "New Question",
                        "variable": variable,
                        "type": 7, // todo: explain parameter
                        "linkedVariables": [],
                        "brokenLinkedVariables": null
                    };

                    commandService.addQuestion($routeParams.questionnaireId, parent.itemId, newId, variable).success(function(result) {
                            if (result.IsSuccess) {
                                parent.items.push(emptyQuestion);
                            } else {
                                $log.error(result.Error);
                            }
                        }
                    );
                };

                $scope.addGroup = function(parent) {
                    var newId = utilityService.guid();
                    var emptyGroup = {
                        "itemId": newId,
                        "title": "New group",
                        "items": []
                    };
                    commandService.addGroup($routeParams.questionnaireId, emptyGroup, parent.itemId).success(function(result) {
                        if (result.IsSuccess) {
                            parent.items.push(emptyGroup);
                        } else {
                            $log.error(result.Error);
                        }
                    });
                };

                $scope.addRoster = function(parent) {
                    var newId = utilityService.guid();
                    var emptyRoster = {
                        "itemId": newId,
                        "title": "New roster",
                        "items": [],
                        isRoster: true
                    };

                    commandService.addRoster($routeParams.questionnaireId, emptyRoster, parent.itemId).success(function(result) {
                        if (result.IsSuccess) {
                            parent.items.push(emptyRoster);
                        } else {
                            $log.error(result.Error);
                        }
                    });
                };

                $scope.showShareInfo = function() {
                    $modal.open({
                        templateUrl: 'app/views/share.html',
                        controller: 'shareCtrl',
                        windowClass: 'share-window',
                        resolve:
                        {
                            questionnaire: function() {
                                return $scope.questionnaire;
                            }
                        }
                    });
                };

                $scope.toggle = function(scope) {
                    scope.toggle();
                };

                $scope.showSearch = function() {
                    $scope.filtersBoxMode = filtersBlockModes.search;
                    utilityService.focus('focusSearch');
                };

                $scope.hideSearch = function() {
                    $scope.filtersBoxMode = filtersBlockModes.default;
                    $scope.search.searchText = '';
                };


                questionnaireService.getQuestionnaireById($routeParams.questionnaireId)
                    .success(function(result) {
                        $scope.questionnaire = result;

                        if ($routeParams.chapterId) {
                            $scope.currentChapterId = $routeParams.chapterId;
                            $scope.loadChapterDetails($routeParams.questionnaireId, $scope.currentChapterId);
                        } else {
                            if (result.chapters.length > 0) {
                                $scope.currentChapter = result.chapters[0];
                                $scope.currentChapterId = $scope.currentChapter.itemId;
                                $scope.loadChapterDetails($routeParams.questionnaireId, $scope.currentChapterId);
                            }
                        }
                        if ($routeParams.itemId) {
                            $scope.currentItemId = $routeParams.itemId;

                            if (document.URL.indexOf('question') > 0) {
                                $scope.activeRoster = undefined;
                                $scope.activeChapter = undefined;
                                $scope.activeQuestion = { itemId: $routeParams.itemId };
                            }

                            if (document.URL.indexOf('group') > 0) {
                                $scope.activeRoster = undefined;
                                $scope.activeChapter = { itemId: $routeParams.itemId };
                                $scope.activeQuestion = undefined;
                            }

                            if (document.URL.indexOf('roster') > 0) {
                                $scope.activeRoster = { itemId: $routeParams.itemId };
                                $scope.activeChapter = undefined;
                                $scope.activeQuestion = undefined;
                            }
                        }
                    });

                //do not reload views, change url only
                var lastRoute = $route.current;
                $scope.$on('$locationChangeSuccess', function() {
                    $route.current = lastRoute;
                });
            }
        ]);
}());