angular.module('designerApp')
    .controller('ChaptersCtrl', 
        function ($rootScope, $scope, $state, $i18next, commandService, utilityService, $log, confirmService, questionnaireService, hotkeys) {
            'use strict';

            var hideChaptersPane = 'right';

            if (hotkeys.get(hideChaptersPane) !== false) {
                hotkeys.del(hideChaptersPane);
            }
            hotkeys.add(hideChaptersPane, $i18next.t('HotkeysHideSections'), function (event) {
                event.preventDefault();
                $scope.foldback();
            });
            
            $scope.isFolded = false;

            $scope.unfold = function () {
                $scope.isFolded = true;
            };

            $scope.foldback = function () {
                if ($scope.chaptersTree.isDragging) {
                    return;
                }
                $scope.isFolded = false;
                $rootScope.$broadcast("closeChaptersList", {});
            };

            $scope.editChapter = function (chapter) {
                $state.go('questionnaire.chapter.group', { chapterId: chapter.itemId, itemId: chapter.itemId });
                $scope.foldback();
            };

            $scope.isCurrentChapter = function(chapter) {
                return chapter.itemId === $state.params.chapterId;
            };

            $scope.addNewChapter = function () {
                var newId = utilityService.guid();

                var newChapter = {
                    title: $i18next.t('DefaultNewSection'),
                    itemId: newId,
                    itemType: "Chapter",
                    isCover: false
                };

                commandService.addChapter($state.params.questionnaireId, newChapter).then(function () {
                    $scope.questionnaire.chapters.push(newChapter);
                    $state.go('questionnaire.chapter.group', { chapterId: newChapter.itemId, itemId: newChapter.itemId });
                    $rootScope.$emit('groupAdded');
                });
            };

            $scope.deleteChapter = function (chapter) {
                var itemIdToDelete = chapter.itemId || $state.params.itemId;

                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(chapter.title || $i18next.t('UntitledSection')));

                modalInstance.result.then(function (confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteGroup($state.params.questionnaireId, itemIdToDelete)
                            .then(function () {
                                var chapter = _.find($scope.questionnaire.chapters, { itemId: itemIdToDelete });
                                var index = _.indexOf($scope.questionnaire.chapters, chapter);
                                if (index > -1) {
                                    $scope.questionnaire.chapters.splice(index, 1);
                                    $rootScope.$emit('chapterDeleted');
                                }

                                questionnaireService.getQuestionnaireById($state.params.questionnaireId).then(function (result) {
                                    $scope.questionnaire.chapters = [];
                                    var data = result.data;
                                    _.forEach(data.chapters, function (c) {
                                        $scope.questionnaire.chapters.push(c);
                                    });
                                    if (data.chapters.length > 0) {
                                        var defaultChapter = _.first(data.chapters);
                                        var itemId = defaultChapter.itemId;
                                        $scope.currentChapter = defaultChapter;
                                        $state.go('questionnaire.chapter.group', { chapterId: itemId, itemId: itemId });
                                    }
                                });
                            });
                    }
                });
            };
            
            $scope.pasteAfterChapter = function (chapter) {

                var itemToCopy = Cookies.getJSON('itemToCopy');
                if (_.isNull(itemToCopy) || _.isUndefined(itemToCopy))
                    return;

                var idToPasteAfter = chapter.itemId || $state.params.itemId;
                var newId = utilityService.guid();

                commandService.pasteItemAfter($state.params.questionnaireId, idToPasteAfter, itemToCopy.questionnaireId, itemToCopy.itemId, newId)
                    .then(function () {

                    var newChapter = {
                        title: "pasting...",
                        itemId: newId,
                        itemType: "Chapter"
                    };

                    var chapter = _.find($scope.questionnaire.chapters, { itemId: idToPasteAfter });
                    var targetIndex = _.indexOf($scope.questionnaire.chapters, chapter) + 1;

                    $scope.questionnaire.chapters.splice(targetIndex, 0, newChapter);
                    $rootScope.$emit('chapterPasted');

                    $state.go('questionnaire.chapter.group', { chapterId: newId, itemId: newId });
                });
            };

            $rootScope.$on('$stateChangeSuccess', function () {
                if ($scope.isFolded) {
                    $scope.foldback();
                }
            });

            $rootScope.$on('groupUpdated', function (event, data) {
                var chapter = questionnaireService.findItem($scope.questionnaire.chapters, data.itemId);
                if (chapter !== null) {
                    chapter.title = data.title;
                }
            });

            $rootScope.$on('deleteChapter', function (event, data) {
                $scope.deleteChapter(data.chapter);
            });

            $scope.$on('openChaptersList', function () {
                $scope.unfold();
            });

            $scope.$on('closeChaptersListRequested', function () {
                $scope.foldback();
            });

            $rootScope.$on('copyChapter', function (event, data) {
                $scope.copyChapter(data.chapter);
            });

            $rootScope.$on('pasteAfterChapter', function (event, data) {
                $scope.pasteAfterChapter(data.chapter);
            });
        });
