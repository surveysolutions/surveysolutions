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
                            };

                            _.forEach(commentThread.comments, function(comment) {
                                comment.date = moment.utc(comment.date).local().format("MMM DD, YYYY HH:mm");
                                comment.isResolved = !_.isNull(comment.resolveDate || null);
                            });

                            commentThread.resolvedComments = _.where(commentThread.comments, { isResolved: true });
                            commentThread.comments = _.where(commentThread.comments, { isResolved: false });
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

            $rootScope.$on("commentDeleted", function(event, data) {
                _.each($scope.commentThreads, function(thread) {
                    thread.comments = _.filter(thread.comments, function(comment) {
                        return comment.id !== data.id;
                    });
                });
                $scope.commentThreads = _.filter($scope.commentThreads, function(thread) {
                    return thread.comments.length > 0;
                });
            });

            $scope.foldback = function () {
                $scope.isFolded = false;
                $rootScope.$broadcast("closeComments", {});
            };

            $scope.showCommentsAndNavigateTo = function(entity) {
                $rootScope.$broadcast("openCommentEditorRequested", {});
                $scope.navigateTo(entity);
            };

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
                if (_.isNull(itemToFind) || _.isUndefined(itemToFind)) return;

                itemToFind.entity.title = data.title;
                itemToFind.entity.variable = data.variable;
                itemToFind.entity.questionType = ("icon-" + data.type).toLowerCase(); //map to type
            });

            $rootScope.$on('staticTextUpdated', function (event, data) {
                var itemToFind = _.find($scope.commentThreads, function (thread) { return thread.entity.itemId === data.itemId });
                if (_.isNull(itemToFind) || _.isUndefined(itemToFind)) return;

                itemToFind.entity.title = data.text;
            });

            $rootScope.$on('variableUpdated', function (event, data) {
                var itemToFind = _.find($scope.commentThreads, function (thread) { return thread.entity.itemId === data.itemId });
                if (_.isNull(itemToFind) || _.isUndefined(itemToFind)) return;

                itemToFind.entity.variable = data.name;
                itemToFind.entity.title = data.label;
            });

            $rootScope.$on('groupUpdated', function (event, data) {
                
                var itemToFind = _.find($scope.commentThreads, function (thread) { return thread.entity.itemId === data.itemId });
                if (_.isNull(itemToFind) || _.isUndefined(itemToFind)) return;

                itemToFind.entity.title = data.title;
            });

            $rootScope.$on('rosterUpdated', function (event, data) {
                var itemToFind = _.find($scope.commentThreads, function (thread) { return thread.entity.itemId === data.itemId });
                if (_.isNull(itemToFind) || _.isUndefined(itemToFind)) return;

                itemToFind.entity.title = data.title;
                itemToFind.entity.variable = data.variable;
            });

            $rootScope.$on('rosterDeleted', function (event, entityId) {
                $scope.removeCommentsForEntity(entityId);
            });

            $rootScope.$on('groupDeleted', function (event, entityId) {
                $scope.removeCommentsForEntity(entityId);
            });

            $rootScope.$on('questionDeleted', function (event, entityId) {
                $scope.removeCommentsForEntity(entityId);
            });

            $rootScope.$on('statictextDeleted', function (event, entityId) {
                $scope.removeCommentsForEntity(entityId);
            });

            $rootScope.$on('variableDeleted', function (event, entityId) {
                $scope.removeCommentsForEntity(entityId);
            });

            $scope.removeCommentsForEntity = function(entityId) {
                var itemIndex = _.findIndex($scope.commentThreads, function (thread) { return thread.entity.itemId === entityId });
                if (_.isNull(itemIndex) || _.isUndefined(itemIndex) || itemIndex < 0) return;

                $scope.commentThreads.splice(itemIndex, 1);
            }

        });
