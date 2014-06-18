(function() {
    'use strict';

    angular.module('designerApp')
        .controller('MainCtrl', [
            '$scope', '$routeParams', '$route', 'questionnaireService', 'commandService', 'verificationService', 'utilityService', 'navigationService', '$modal', '$log',
            function($scope, $routeParams, $route, questionnaireService, commandService, verificationService, utilityService, navigationService, $modal, $log) {

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

                $scope.resetSelection = function () {
                    navigationService.openChapter($routeParams.questionnaireId, $scope.currentChapterId);
                    $scope.currentItemId = null;
                    $scope.activeRoster = null;
                    $scope.activeQuestion = null;
                };

                $scope.setItem = function(item) {
                    navigationService.openItem($routeParams.questionnaireId, $scope.currentChapterId, item.itemId);
                    $scope.currentItemId = item.itemId;
                    if ($scope.isQuestion(item)) {
                        $scope.activeRoster = undefined;
                        $scope.activeChapter = undefined;
                        $scope.activeQuestion = item;
                        console.log(JSON.stringify($scope.activeQuestion));
                        console.log(item);
                    } else if (item.isRoster) {
                        $scope.activeRoster = item;
                        $scope.activeQuestion = undefined;
                        $scope.activeChapter = undefined;
                    } else {
                        $scope.activeRoster = undefined;
                        $scope.activeQuestion = undefined;
                        $scope.activeChapter = item;
                    }
                };

                $scope.changeChapter = function(chapter) {
                    navigationService.openChapter($routeParams.questionnaireId, chapter.itemId);
                    $scope.currentChapterId = chapter.itemId;
                    $scope.loadChapterDetails($routeParams.questionnaireId, $scope.currentChapterId);
                };

                $scope.loadChapterDetails = function(questionnaireId, chapterId) {
                    questionnaireService.getChapterById(questionnaireId, chapterId)
                        .success(function(result) {
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
                    commandService.addGroup($routeParams.questionnaireId, emptyGroup, parent.itemId).success(function (result) {
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
                        isRoster : true
                    };

                    commandService.addRoster($routeParams.questionnaireId, emptyRoster, parent.itemId).success(function (result) {
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
                
                $scope.toggle = function (scope) {
                    scope.toggle();
                };

                questionnaireService.getQuestionnaireById($routeParams.questionnaireId)
                    .success(function(result) {
                        if (result === 'null') {
                            alert('Questionnaire not found');
                        } else {
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
                            }
                        }
                    });

                $scope.showSearch = function() {
                    $scope.filtersBoxMode = filtersBlockModes.search;
                    utilityService.focus('focusSearch');
                };

                $scope.hideSearch = function() {
                    $scope.filtersBoxMode = filtersBlockModes.default;
                    $scope.search.searchText = '';
                };

                //do not reload views, change url only
                var lastRoute = $route.current;
                $scope.$on('$locationChangeSuccess', function() {
                    $route.current = lastRoute;
                });
            }
        ]);
}());