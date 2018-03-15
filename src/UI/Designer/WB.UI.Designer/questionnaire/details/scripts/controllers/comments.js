angular.module('designerApp')
    .controller('CommentsCtrl', 
        function ($rootScope, $scope, $state, $i18next, commandService, utilityService, commentsService, hotkeys, $log, confirmService) {
            'use strict';

            $scope.commentThreads = [];

            $scope.loadCommentThreads = function() {
                if ($scope.questionnaire === null)
                    return;
                commentsService.getCommentThreads($state.params.questionnaireId)
                    .then(function(result) {
                        $scope.commentThreads = result.data;
                        _.forEach($scope.commentThreads, function(commentThread) {
                            commentThread.resolvedComments = [];
                            _.forEach(commentThread.comments, function(comment) {
                                comment.date = moment(comment.date).format("LLL");
                                comment.isResolved = !_.isNull(comment.resolveDate || null);
                            });

                            if (commentThread.indexOfLastUnresolvedComment != null) {
                                var comments = commentThread.comments.slice(0, commentThread.indexOfLastUnresolvedComment);
                                var resolvedComments = commentThread.comments.slice(commentThread.indexOfLastUnresolvedComment);

                                commentThread.comments = comments;
                                commentThread.resolvedComments = resolvedComments;
                                commentThread.resolvedAreExpanded = false;
                                commentThread.toggleResolvedComments = function() {
                                    this.resolvedAreExpanded = !this.resolvedAreExpanded;
                                }
                            }


                        });
                        console.log(result.data);
                    });
            };

            var hideCommentsPane = 'ctrl+alt+c';

            if (hotkeys.get(hideCommentsPane) !== false) {
                hotkeys.del(hideCommentsPane);
            }

            hotkeys.add(hideCommentsPane, $i18next.t('HotkeysCloseLookup'), function (event) {
                event.preventDefault();
                $scope.foldback();
            });
            
            $scope.isFolded = false;

            $scope.unfold = function () {
                $scope.isFolded = true;
            };

            $scope.foldback = function () {
                $scope.isFolded = false;
                $rootScope.$broadcast("closeComments", {});
            };

            $scope.showCommentsAndNavigateTo = function(entity) {
                $rootScope.$broadcast("openCommentEditorRequested", {});
                $scope.navigateTo(entity);
            }

            $scope.$on('openComments', function (scope, params) {
                $scope.unfold();
                if (!_.isUndefined(params) && !_.isUndefined(params.focusOn)) {
                    setTimeout(function () { utilityService.focus("focusComment" + params.focusOn); }, 500);
                }
            });

            $scope.$on('closeCommentsRequested', function () {
                $scope.foldback();
            });
            
            $rootScope.$on('newCommentPosted', function (comment) {
                $scope.loadCommentThreads();
            });

            $rootScope.$on('questionnaireLoaded', function () {
                $scope.loadCommentThreads();
            });
        });
