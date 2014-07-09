(function () {
    'use strict';

    angular.module('designerApp')
        .controller('MainCtrl', [
            '$scope', '$state', 'questionnaireService', 'commandService', 'verificationService', 'utilityService', 'hotkeys', '$modal', '$log',
            function ($scope, $state, questionnaireService, commandService, verificationService, utilityService, hotkeys, $modal, $log) {
                var me = this;

                $scope.verificationStatus = {
                    errorsCount: null,
                    errors: []
                };

                $scope.verify = function () {
                    verificationService.verify($state.params.questionnaireId).success(function (result) {
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
                            questionnaireService.moveGroup(group.itemId, event.dest.index, null, $state.params.questionnaireId)
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

                $scope.navigateTo = function (reference) {
                    $state.go('questionnaire.chapter.' + reference.type.toLowerCase(), {
                        chapterId: reference.chapterId,
                        itemId: reference.itemId
                    });
                };

                $scope.currentChapter = null;

                $scope.addQuestion = function (parent) {
                    var newId = utilityService.guid();
                    var variable = 'q' + newId.substring(0, 5);
                    var emptyQuestion = {
                        "itemId": newId,
                        "title": 'New Question',
                        "variable": variable,
                        "type": 'Text',
                        "linkedVariables": [],
                        "brokenLinkedVariables": null,
                        getParentItem: function () { return parent; }
                    };

                    commandService.addQuestion($state.params.questionnaireId, parent.itemId, newId, variable).success(function (result) {
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
                        "items": [],
                        getParentItem: function () { return parent; }
                    };
                    commandService.addGroup($state.params.questionnaireId, emptyGroup, parent.itemId).success(function (result) {
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
                        isRoster: true,
                        getParentItem: function () { return parent; }
                    };

                    commandService.addRoster($state.params.questionnaireId, emptyRoster, parent.itemId).success(function (result) {
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
                
                questionnaireService.getQuestionnaireById($state.params.questionnaireId).success(function (result) {
                    $scope.questionnaire = result;
                    if (!$state.params.chapterId && result.chapters.length > 0) {
                        var defaultChapter = _.first(result.chapters);
                        var itemId = defaultChapter.itemId;
                        $scope.currentChapter = defaultChapter;
                        $state.go('questionnaire.chapter.group', { chapterId: itemId, itemId: itemId });
                    }
                });
            }
        ]);
}());