(function() {
    'use strict';

    angular.module('designerApp')
        .controller('MainCtrl', [
            '$scope', '$routeParams', '$route', 'questionnaireService', 'commandService', 'verificationService', 'utilityService', 'navigationService', '$modal', '$log',
            function($scope, $routeParams, $route, questionnaireService, commandService, verificationService, utilityService, navigationService, $modal, $log) {

                $scope.verificationStatus = {
                    errorsCount: 0,
                    errors: []
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

                $scope.chapters = [];

                $scope.items = [];

                $scope.item = null;

                $scope.questionnaire = null;

                $scope.groupsTree = {
                    dropped: function(event) {
                        var movedItem = event.source.nodeScope.item;
                        var destItem = event.dest.nodesScope.item;
                        var destGroupId = destItem ? destItem.itemId : $scope.questionnaire.chapters[0].itemId;

                        if ($scope.isQuestion(movedItem)) {
                            commandService.execute('MoveQuestion', {
                                    targetGroupId: destGroupId,
                                    targetIndex: event.dest.index,
                                    questionId: movedItem.itemId,
                                    questionnaireId: $routeParams.questionnaireId
                                }
                            );
                        } else {
                            commandService.execute('MoveGroup', {
                                    targetGroupId: destGroupId,
                                    targetIndex: event.dest.index,
                                    groupId: event.source.nodeScope.item.itemId,
                                    questionnaireId: $routeParams.questionnaireId
                                }
                            );
                        }
                    }
                };

                $scope.currentChapter = null;

                $scope.currentChapterId = null;

                $scope.setItem = function(item) {
                    navigationService.openItem($routeParams.questionnaireId, $scope.currentChapterId, item.itemId);
                    $scope.currentItemId = item.itemId;
                    if ($scope.isQuestion(item)) {
                        $scope.activeRoster = undefined;
                        $scope.activeChapter = undefined;
                        $scope.activeQuestion = item;
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

                $scope.addQuestion = function(item) {
                    var newId = utilityService.guid();
                    var emptyQuestion = {
                        "itemId": newId,
                        "title": "New Question",
                        "variable": "",
                        "type": 7,
                        "linkedVariables": [],
                        "brokenLinkedVariables": null
                    };
                    commandService.addQuestion($routeParams.questionnaireId, item, newId).success(function() {
                            item.items.push(emptyQuestion);
                        }
                    );
                };

                $scope.addGroup = function(item) {
                    var newId = utilityService.guid();
                    var emptyGroup = {
                        "itemId": newId,
                        "title": "New group",
                        "items": []
                    };
                    commandService.addGroup($routeParams.questionnaireId, emptyGroup, item.itemId).success(function() {
                        item.items.push(emptyGroup);
                    });
                };

                $scope.addRoster = function(item) {
                    var newId = utilityService.guid();
                    var emptyRoster = {
                        "itemId": newId,
                        "title": "New roster",
                        "items": []
                    };

                    commandService.addRoster($routeParams.questionnaireId, emptyRoster, item.itemId).success(function() {
                        item.items.push(emptyRoster);
                    });
                };

                $scope.showShareInfo = function() {
                    $modal.open({
                        templateUrl: 'app/views/share.html',
                        controller: 'shareCtrl',
                        resolve:
                        {
                            questionnaire: function() {
                                return $scope.questionnaire;
                            }
                        }
                    });
                };

                $scope.collapse = function(item) {
                    item.collapsed = true;
                };

                $scope.expand = function(item) {
                    item.collapsed = false;
                };

                $scope.closePanel = function() {
                    $scope.activeChapter = undefined;
                    $scope.activeRoster = undefined;
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

                //do not reload views, change url only
                var lastRoute = $route.current;
                $scope.$on('$locationChangeSuccess', function() {
                    $route.current = lastRoute;
                });
            }
        ]);
}());