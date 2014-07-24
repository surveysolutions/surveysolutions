(function () {
    'use strict';

    angular.module('designerApp')
        .controller('MainCtrl', [
            '$rootScope', '$scope', '$state', 'questionnaireService', 'commandService', 'verificationService', 'utilityService', 'hotkeys', '$modal', '$log',
            function ($rootScope, $scope, $state, questionnaireService, commandService, verificationService, utilityService, hotkeys, $modal, $log) {
                $scope.verificationStatus = {
                    errorsCount: null,
                    errors: []
                };

                $scope.questionnaire = {
                    questionsCount: 0,
                    groupsCount: 0,
                    rostersCount: 0
                };

                $scope.questionnaireId = $state.params.questionnaireId;

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
                                top: $('#verification-btn').offset().top + 15
                            });
                            $('#verification-modal .modal-dialog .arrow').css({
                                left: $('#verification-btn').offset().left +5
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
                    var emptyQuestion = {
                        "itemId": newId,
                        "title": 'New Question',
                        "type": 'Text',
                        "linkedVariables": [],
                        "brokenLinkedVariables": null,
                        getParentItem: function () { return parent; }
                    };

                    commandService.addQuestion($state.params.questionnaireId, parent.itemId, newId).success(function (result) {
                        if (result.IsSuccess) {
                            parent.items.push(emptyQuestion);
                            $state.go('questionnaire.chapter.question', { chapterId: $state.params.chapterId, itemId: newId });
                            $rootScope.$emit('questionAdded');
                        }
                    });
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
                            $rootScope.$emit('groupAdded');
                            $state.go('questionnaire.chapter.group', { chapterId: $state.params.chapterId, itemId: newId });
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
                            $rootScope.$emit('rosterAdded');
                            $state.go('questionnaire.chapter.roster', { chapterId: $state.params.chapterId, itemId: newId });
                        }
                    });
                };
                
                $scope.addStaticText = function (parent) {
                    var newId = utilityService.guid();
                    var emptyStaticText = {
                        "itemId": newId,
                        "text": "New static text",
                        getParentItem: function () { return parent; }
                    };

                    commandService.addStaticText($state.params.questionnaireId, emptyStaticText, parent.itemId).success(function (result) {
                        if (result.IsSuccess) {
                            parent.items.push(emptyStaticText);
                            $state.go('questionnaire.chapter.staticText', { chapterId: $state.params.chapterId, itemId: newId });
                        }
                    });
                };


                $rootScope.$on('groupDeleted', function() {
                    $scope.questionnaire.groupsCount--;
                });

                $rootScope.$on('questionDeleted', function () {
                    $scope.questionnaire.questionsCount--;
                });

                $rootScope.$on('rosterDeleted', function () {
                    $scope.questionnaire.rostersCount--;
                });

                $rootScope.$on('groupAdded', function () {
                    $scope.questionnaire.groupsCount++;
                });

                $rootScope.$on('questionAdded', function () {
                    $scope.questionnaire.questionsCount++;
                });

                $rootScope.$on('rosterAdded', function () {
                    $scope.questionnaire.rostersCount++;
                });

                $rootScope.$on('chapterCloned', function () {
                    getQuestionnaire();
                });

                $rootScope.$on('chapterDeleted', function () {
                    getQuestionnaire();
                });

                $scope.showShareInfo = function () {
                    $modal.open({
                        templateUrl: 'views/share.html',
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

                var getQuestionnaire = function() {
                    questionnaireService.getQuestionnaireById($state.params.questionnaireId).success(function(result) {
                        $scope.questionnaire = result;
                        if (!$state.params.chapterId && result.chapters.length > 0) {
                            var defaultChapter = _.first(result.chapters);
                            var itemId = defaultChapter.itemId;
                            $scope.currentChapter = defaultChapter;
                            $state.go('questionnaire.chapter.group', { chapterId: itemId, itemId: itemId });
                        }
                    });
                };

                getQuestionnaire();
            }
        ]);
}());