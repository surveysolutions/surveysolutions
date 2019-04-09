angular.module('designerApp')
    .controller('CommentsEditorCtrl', 
        function ($rootScope, $scope, $state, commentsService, $i18next, commandService, utilityService, moment, confirmService) {
            'use strict';

            $scope.maxCommentLength = 1000;

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
                            comment.date = moment.utc(comment.date).local().format("MMM DD, YYYY HH:mm");
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
            };

            $scope.deleteComment = function(itemId) {
                var modalInstance = confirmService.open({
                    title: $i18next.t('DeleteCommentConfirm'),
                    okButtonTitle: $i18next.t('Delete'),
                    cancelButtonTitle: $i18next.t('Cancel'),
                    isReadOnly: $scope.questionnaire.isReadOnlyForUser
                });
                modalInstance.result.then(function(confirmResult) {
                    if (confirmResult === 'ok') {
                        commentsService.deleteComment($state.params.questionnaireId, itemId).then(function() {
                                $rootScope.$broadcast("commentDeleted", { id: itemId });
                            }
                        );
                    }
                });
            };

            $scope.postComment = function() {
                $scope.activeComment.serverValidation = null;

                commentsService.postComment($state.params.questionnaireId, $state.params.itemId, $scope.activeComment)
                    .then(function(response) {
                        if (!_.isNull(response.data.error || null)) {
                            $scope.activeComment.serverValidation = (response.data.error || null);
                            $scope.addCommentForm.$setPristine();
                        } else {
                            var comment = angular.copy($scope.activeComment);
                            comment.date = moment(new Date()).format("LLL");
                            comment.userName = $scope.currentUserName;
                            comment.userEmail = $scope.currentUserEmail;
                            $scope.comments.push(comment);
                            $rootScope.$broadcast("newCommentPosted", comment);
                            $scope.activeComment = createNewComment();
                        }
                    });
            }

            $scope.$watch('addCommentForm', function (value) {
                $scope.addCommentForm.comment.$setValidity("serverValidation", true);
            }, false);

            $rootScope.$on('commentsOpened', function (event, data) {
                utilityService.setFocusIn("edit-entity-comment");
            });

            $rootScope.$on('commentDeleted', function (event, data) {
                $scope.comments = _.filter($scope.comments, function(comment) {
                    return comment.id !== data.id;
                });
            });

            $scope.currentChapterId = $state.params.chapterId;
            $scope.loadComments();
        });
