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
                            commentThread.resolvedAreExpanded = false;
                            commentThread.toggleResolvedComments = function() {
                                this.resolvedAreExpanded = !this.resolvedAreExpanded;
                            }

                            _.forEach(commentThread.comments, function(comment) {
                                comment.date = moment.utc(comment.date).local().format("LLL");
                                comment.isResolved = !_.isNull(comment.resolveDate || null);
                            });

                            if (commentThread.indexOfLastUnresolvedComment != null) {
                                var comments = commentThread.comments.slice(0, commentThread.indexOfLastUnresolvedComment);
                                var resolvedComments = commentThread.comments.slice(commentThread.indexOfLastUnresolvedComment);

                                commentThread.comments = comments;
                                commentThread.resolvedComments = resolvedComments;
                                
                            } else {
                                commentThread.resolvedComments = commentThread.comments;
                                commentThread.comments = [];
                            }
                        });
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

            $rootScope.$on('commentResolved', function (comment) {
                $scope.loadCommentThreads();
            });

            $rootScope.$on('questionnaireLoaded', function () {
                $scope.loadCommentThreads();
            });
            
            $rootScope.$on('questionUpdated', function (event, data) {
                var itemToFind = _.find($scope.commentThreads, function(thread) { return thread.entity.itemId === data.itemId });
                if (_.isNull(itemToFind)) return;

                itemToFind.entity.title = data.title;
                itemToFind.entity.variable = data.variable;
                itemToFind.entity.questionType = ("icon-" + data.type).toLowerCase(); //map to type
                
            });

            $rootScope.$on('staticTextUpdated', function (event, data) {
                var itemToFind = _.find($scope.commentThreads, function (thread) { return thread.entity.itemId === data.itemId });
                if (_.isNull(itemToFind)) return;

                itemToFind.entity.title = data.text;
            });

            $rootScope.$on('variableUpdated', function (event, data) {
                var itemToFind = _.find($scope.commentThreads, function (thread) { return thread.entity.itemId === data.itemId });
                if (_.isNull(itemToFind)) return;

                itemToFind.entity.variable = data.name;
                itemToFind.entity.title = data.label;
            });

            $rootScope.$on('groupUpdated', function (event, data) {
                
                var itemToFind = _.find($scope.commentThreads, function (thread) { return thread.entity.itemId === data.itemId });
                if (_.isNull(itemToFind)) return;

                itemToFind.entity.title = data.title;
            });

            $rootScope.$on('rosterUpdated', function (event, data) {
                var itemToFind = _.find($scope.commentThreads, function (thread) { return thread.entity.itemId === data.itemId });
                if (_.isNull(itemToFind)) return;

                itemToFind.entity.title = data.title;
                itemToFind.entity.variable = data.variable;
            });
        });
