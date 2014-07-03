(function () {
    'use strict';

    angular.module('designerApp')
        .controller('MainCtrl', [
            '$scope', '$stateParams', 'questionnaireService', 'commandService', 'verificationService', 'utilityService', 'hotkeys', '$state', '$modal', '$log',
            function ($scope, $stateParams, questionnaireService, commandService, verificationService, utilityService, hotkeys, $state, $modal, $log) {
                var me = this;

                $scope.verificationStatus = {
                    errorsCount: null,
                    errors: []
                };

                $scope.verify = function () {
                    verificationService.verify($stateParams.questionnaireId).success(function (result) {
                        $scope.verificationStatus.errors = result.errors;
                        $scope.verificationStatus.errorsCount = result.errors.length;

                        if ($scope.verificationStatus.errorsCount > 0) {
                            $('#verification-modal').modal({
                                backdrop: false,
                                show: true
                            });
                            // --- dialog and arrow positioning
                            $('#verification-modal .modal-dialog').stop().css({
                                position: 'absolute',
                                top: $('#verification-btn').offset().top + 15,
                                left: 50
                            });
                            $('#verification-modal .modal-dialog .arrow').css({
                                left: $('#verification-btn').offset().left - 45
                            });
                            // ---
                        }
                    });
                };

                $scope.answerTypeClass = {
                    YesNo: 'icon-singleanswer',
                    DropDownList: 'icon-singleanswer',
                    MultyOption: 'icon-multianswer',
                    Numeric: 'icon-intedit',
                    DateTime: 'icon-datetime',
                    GpsCoordinates: 'icon-geoloc',
                    AutoPropagate: 'icon-textedit',
                    TextList: 'icon-textarea',
                    QRBarcode: 'icon-multimedia',
                    Text: 'icon-textedit',
                    SingleOption: 'icon-singleanswer'
                };

                $scope.chapters = [];

                $scope.items = [];

                $scope.item = null;

                $scope.questionnaire = null;

                $scope.chaptersTree = {
                    dropped: function (event) {
                        var rollback = function (item, targetIndex) {
                            $scope.questionnaire.chapters.splice(_.indexOf($scope.chapters, item), 1);
                            $scope.questionnaire.chapters.splice(targetIndex, 0, item);
                        };

                        if (event.dest.index !== event.source.index) {
                            var group = event.source.nodeScope.chapter;
                            questionnaireService.moveGroup(group.itemId, event.dest.index, null, $stateParams.questionnaireId)
                                .success(function (data) {
                                    if (!data.IsSuccess) {
                                        rollback(group, event.source.index);
                                    }
                                })
                                .error(function () {
                                    rollback(group, event.source.index);
                                });
                        }
                    }
                };

                $scope.currentChapter = null;

                $scope.currentChapterId = null;

                $scope.isQuestion = function (item) {
                    return item.hasOwnProperty('type');
                };

                $scope.isGroup = function (item) {
                    return !item.hasOwnProperty('type');
                };

                $scope.addQuestion = function (parent) {
                    var newId = utilityService.guid();
                    var variable = 'q' + newId.substring(0, 5);
                    var emptyQuestion = {
                        "itemId": newId,
                        "title": 'New Question',
                        "variable": variable,
                        "type": 'Text',
                        "linkedVariables": [],
                        "brokenLinkedVariables": null
                    };

                    commandService.addQuestion($stateParams.questionnaireId, parent.itemId, newId, variable).success(function (result) {
                        if (result.IsSuccess) {
                            parent.items.push(emptyQuestion);
                        } else {
                            $log.error(result.Error);
                        }
                    }
                    );
                };

                $scope.addGroup = function (parent) {
                    var newId = utilityService.guid();
                    var emptyGroup = {
                        "itemId": newId,
                        "title": "New group",
                        "items": []
                    };
                    commandService.addGroup($stateParams.questionnaireId, emptyGroup, parent.itemId).success(function (result) {
                        if (result.IsSuccess) {
                            parent.items.push(emptyGroup);
                        } else {
                            $log.error(result.Error);
                        }
                    });
                };

                $scope.addRoster = function (parent) {
                    var newId = utilityService.guid();
                    var emptyRoster = {
                        "itemId": newId,
                        "title": "New roster",
                        "items": [],
                        isRoster: true
                    };

                    commandService.addRoster($stateParams.questionnaireId, emptyRoster, parent.itemId).success(function (result) {
                        if (result.IsSuccess) {
                            parent.items.push(emptyRoster);
                        } else {
                            $log.error(result.Error);
                        }
                    });
                };

                $scope.showShareInfo = function () {
                    $modal.open({
                        templateUrl: 'app/views/share.html',
                        controller: 'shareCtrl',
                        windowClass: 'share-window',
                        resolve:
                        {
                            questionnaire: function () {
                                return $scope.questionnaire;
                            }
                        }
                    });
                };

                questionnaireService.getQuestionnaireById($stateParams.questionnaireId)
                    .success(function (result) {
                        $scope.questionnaire = result;

                        if ($stateParams.chapterId) {
                            //$scope.currentChapterId = $stateParams.chapterId;
                            //$scope.loadChapterDetails($stateParams.questionnaireId, $scope.currentChapterId);
                        } else {
                            if (result.chapters.length > 0) {
                                $state.go('questionnaire.chapter', { chapterId: _.first(result.chapters).itemId });
                                //$scope.currentChapter = result.chapters[0];
                                //$scope.currentChapterId = $scope.currentChapter.itemId;
                                //$scope.loadChapterDetails($stateParams.questionnaireId, $scope.currentChapterId);
                            }
                        }
                        //if ($stateParams.itemId) {
                        //    $scope.currentItemId = $stateParams.itemId;

                        //    if (document.URL.indexOf('question') > 0) {
                        //        $scope.activeRoster = undefined;
                        //        $scope.activeChapter = undefined;
                        //        $scope.activeQuestion = { itemId: $stateParams.itemId };
                        //    }

                        //    if (document.URL.indexOf('group') > 0) {
                        //        $scope.activeRoster = undefined;
                        //        $scope.activeChapter = { itemId: $stateParams.itemId };
                        //        $scope.activeQuestion = undefined;
                        //    }

                        //    if (document.URL.indexOf('roster') > 0) {
                        //        $scope.activeRoster = { itemId: $stateParams.itemId };
                        //        $scope.activeChapter = undefined;
                        //        $scope.activeQuestion = undefined;
                        //    }
                        //}
                    });
            }
        ]);
}());