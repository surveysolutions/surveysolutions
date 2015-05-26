angular.module('designerApp')
    .controller('ChaptersCtrl', 
       // '$rootScope', '$scope', '$state', 'commandService', 'utilityService', '$log', 'confirmService', 'questionnaireService',
        function ($rootScope, $scope, $state, commandService, utilityService, $log, confirmService, questionnaireService, hotkeys) {
            'use strict';

            var hideChaptersPane = 'right';

            if (hotkeys.get(hideChaptersPane) !== false) {
                hotkeys.del(hideChaptersPane);
            }
            hotkeys.add(hideChaptersPane, 'Close sections', function (event) {
                event.preventDefault();
                $scope.foldback();
            });
            

            $scope.chapters = [];

            $scope.isFolded = false;

            $scope.unfold = function () {
                $scope.isFolded = true;
            };

            $scope.foldback = function () {
                if ($scope.chaptersTree.isDragging) {
                    return;
                }
                $scope.isFolded = false;
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
                    title: 'New Section',
                    itemId: newId
                };

                commandService.addChapter($state.params.questionnaireId, newChapter).success(function () {
                    $scope.questionnaire.chapters.push(newChapter);
                    $state.go('questionnaire.chapter.group', { chapterId: newChapter.itemId, itemId: newChapter.itemId });
                    $rootScope.$emit('groupAdded');
                });
            };

            $rootScope.cloneChapter = function (chapterId) {
                var idToClone = chapterId || $state.params.itemId;
                var newId = utilityService.guid();
                var chapter = _.find($scope.questionnaire.chapters, { itemId: idToClone });
                var targetIndex = _.indexOf($scope.questionnaire.chapters, chapter) + 1;

                commandService.cloneGroup($state.params.questionnaireId, idToClone, targetIndex, newId).success(function () {
                    var newChapter = {
                        title: chapter.title,
                        itemId: newId
                    };
                    $scope.questionnaire.chapters.splice(targetIndex, 0, newChapter);
                    $rootScope.$emit('chapterCloned');
                    $state.go('questionnaire.chapter.group', { chapterId: newId, itemId: newId });
                });
            };

            $scope.deleteChapter = function (chapter) {
                var itemIdToDelete = chapter.itemId || $state.params.itemId;

                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(chapter.title));

                modalInstance.result.then(function (confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteGroup($state.params.questionnaireId, itemIdToDelete)
                            .success(function () {
                                var index = $scope.questionnaire.chapters.indexOf(chapter);
                                if (index > -1) {
                                    $scope.questionnaire.chapters.splice(index, 1);
                                    $rootScope.$emit('chapterDeleted');
                                }

                                questionnaireService.getQuestionnaireById($state.params.questionnaireId).success(function (questionnaire) {
                                    _.remove($scope.questionnaire.chapters);
                                    _.forEach(questionnaire.chapters, function(c) {
                                         $scope.questionnaire.chapters.push(c);
                                    });
                                    if (questionnaire.chapters.length > 0) {
                                        var defaultChapter = _.first(questionnaire.chapters);
                                        var itemId = defaultChapter.itemId;
                                        $scope.currentChapter = defaultChapter;
                                        $state.go('questionnaire.chapter.group', { chapterId: itemId, itemId: itemId });
                                    }
                                });
                            });
                    }
                });
            };


            $rootScope.$on('$stateChangeSuccess', function () {
                $scope.foldback();
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

            $scope.$on('openChaptersList', function (event, data) {
                $scope.unfold();
            });
        });
