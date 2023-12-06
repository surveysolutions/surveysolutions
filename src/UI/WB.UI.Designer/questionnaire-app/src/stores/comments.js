import { defineStore } from 'pinia';
import moment from 'moment';
import { remove } from 'lodash';
import { useUserStore } from './user';
import { newGuid } from '../helpers/guid';
import { get, post, patch, del } from '../services/apiService';

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
            const data = await get(
                'questionnaire/' +
                    questionnaireId +
                    '/entity/' +
                    entityId +
                    '/comments'
            );
            this.questionnaireId = questionnaireId;
            this.entityId = entityId;
            this.setComments(data);
        },

        setComments(data) {
            this.comments = data;
            this.isCommentsBlockVisible = data && data.length > 0;
        },

        toggleComments() {
            this.isCommentsBlockVisible = !this.isCommentsBlockVisible;
        },

        async postComment(comment) {
            const userStore = useUserStore();
            const userName = userStore.userName;
            const userEmail = userStore.email;
            const id = newGuid();

            const response = await post(
                'questionnaire/' + this.questionnaireId + '/entity/addComment',
                {
                    comment: comment,
                    entityId: this.entityId,
                    id: id,
                    questionnaireId: this.questionnaireId
                }
            );

            if (response && response.error) return response;

            this.$state.comments.push({
                id: id,
                comment: comment,
                date: moment(new Date()).format('LLL'),
                userName: userName,
                userEmail: userEmail
            });

            return response;
        },

        async deleteComment(commentId) {
            await del(
                'questionnaire/' +
                    this.questionnaireId +
                    '/comment/' +
                    commentId
            );
            remove(this.$state.comments, comment => comment.id === commentId);
        },

        async resolveComment(comment) {
            await patch(
                'questionnaire/' +
                    this.questionnaireId +
                    '/comment/resolve/' +
                    comment.id
            );

            comment.resolveDate = new Date();
        }
    }
});
