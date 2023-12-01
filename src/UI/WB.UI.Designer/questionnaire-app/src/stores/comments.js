import { defineStore } from 'pinia';
import { mande } from 'mande';
import moment from 'moment';
import { remove } from 'lodash';

const api = mande('/questionnaire' /*, globalOptions*/);

export const useCommentsStore = defineStore('comments', {
    state: () => ({
        comments: [],
        questionnaireId: null,
        entityId: null,
        isCommentsBlockVisible: false
    }),
    getters: {
        getComments: state => state.comments,
        getCommentsCount: state => state.comments.length,
        getIsCommentsBlockVisible: state => state.isCommentsBlockVisible
    },
    actions: {
        async fetchComments(questionnaireId, entityId) {
            const data = await api.get(
                questionnaireId + '/entity/' + entityId + '/comments'
            );
            this.questionnaireId = questionnaireId;
            this.entityId = entityId;
            this.setComments(data);
        },

        setComments(data) {
            this.comments = data;
            this.isCommentsBlockVisible = data.length > 0;
        },

        toggleComments() {
            this.isCommentsBlockVisible = !this.isCommentsBlockVisible;
        },

        async postComment(comment) {
            commentsService.postComment(
                $state.params.questionnaireId,
                $state.params.itemId,
                $scope.activeComment
            );

            const data = await api
                .post(questionnaireId + '/entity/addComment')
                .then(function(response) {
                    if (!_.isNull(response.data.error || null)) {
                        $scope.activeComment.serverValidation =
                            response.data.error || null;
                        $scope.addCommentForm.$setPristine();
                    } else {
                        var comment = angular.copy($scope.activeComment);
                        comment.date = moment(new Date()).format('LLL');
                        comment.userName = $scope.currentUserName;
                        comment.userEmail = $scope.currentUserEmail;
                        $scope.comments.push(comment);
                        $rootScope.$broadcast('newCommentPosted', comment);
                        $scope.activeComment = createNewComment();

                        $rootScope.$broadcast(
                            'commentsCount',
                            $scope.comments.length
                        );
                    }
                });
        },

        deleteComment(commentId) {
            return api
                .delete(this.questionnaireId + '/comment/' + commentId)
                .then(res => {
                    remove(
                        this.state.comments,
                        comment => comment.id === commentId
                    );
                });
        },

        resolveComment(comment) {
            return api
                .patch(this.questionnaireId + '/comment/resolve/' + comment.id)
                .then(res => {
                    comment.resolveDate = new Date();
                });
        }
    }
});
