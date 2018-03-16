angular.module('designerApp')
    .controller('CommentsEditorCtrl', 
        function ($rootScope, $scope, $state, commentsService, $i18next, commandService, utilityService, moment) {
            'use strict';

            var createNewComment = function() {
                return {
                    id: utilityService.guid(),
                    comment: '',
                    questionnaireId: $state.params.questionnaireId,
                    entityId: $state.params.itemId
                }
            };
            $scope.loadComments = function() {
                commentsService.getItemCommentsById($state.params.questionnaireId, $state.params.itemId)
                    .then(function(result) {
                        var data = result.data;
                        $scope.comments = data || [];

                        _.forEach($scope.comments, function(comment) {
                            comment.date = moment(comment.date).format("LLL");
                            comment.isResolved = !_.isNull(comment.resolveDate || null);
                        });
                        
                        $scope.activeComment = createNewComment();

                        utilityService.scrollToElement("#comments-editor .form-holder","#edit-entity-comment-row");
                    });
            };

            $scope.resolveComment = function(comment) {
                commentsService.resolveComment($state.params.questionnaireId, comment.id)
                    .then(function() {
                        comment.resolveDate = new Date();
                        comment.isResolved = true;
                        $rootScope.$broadcast("commentResolved", comment);
                    });
            }

            $scope.postComment = function() {
                commentsService.postComment($state.params.questionnaireId, $state.params.itemId, $scope.activeComment)
                    .then(function() {
                        var comment = angular.copy($scope.activeComment);
                        comment.userName = $scope.currentUserName;
                        comment.userEmail = $scope.currentUserEmail;
                        $scope.comments.push(comment);
                        $rootScope.$broadcast("newCommentPosted", comment);
                        $scope.activeComment = createNewComment();
                    });
            }

            $rootScope.$on('commentsOpened', function (event, data) {
                utilityService.setFocusIn("edit-entity-comment");
            });

            $scope.currentChapterId = $state.params.chapterId;
            $scope.loadComments();
        });
