import { defineStore } from 'pinia';
import { mande } from 'mande';
import moment from 'moment';
import { remove } from 'lodash';
import { useUserStore } from './user';
import { newGuid } from '../helpers/guid';
import { post, patch, del } from '../services/apiService';

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
                this.questionnaireId + '/entity/addComment',
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
            await del(this.questionnaireId + '/comment/' + commentId);
            remove(this.$state.comments, comment => comment.id === commentId);
        },

        async resolveComment(comment) {
            await patch(
                this.questionnaireId + '/comment/resolve/' + comment.id
            );

            comment.resolveDate = new Date();
        }
    }
});
