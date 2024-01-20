import { defineStore } from 'pinia';
import _ from 'lodash';
import { get, post } from '../services/apiService';
import { getComments } from '../services/commentsService';
import emitter from '../services/emitter';

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
        setupListeners() {
            emitter.on('commentResolved', this.commentResolved);
            emitter.on('commentDeleted', this.commentDeleted);
            emitter.on('commentAdded', this.commentAdded);            
        },
        commentResolved(payload) {
            const index = _.findIndex(this.comments, function(i) {
                return i.id === payload.id;
            });
            if (index !== -1) {
                this.comments[index].resolveDate = payload.resolveDate;                
            }
        },
        commentDeleted(payload) {
            _.remove(this.$state.comments, comment => comment.id === payload.id);
        },
        commentAdded(payload) {
            this.$state.comments.push(payload);
        },
        async fetchComments(questionnaireId, entityId) {
            const data = await getComments(questionnaireId, entityId);

            this.questionnaireId = questionnaireId;
            this.entityId = entityId;
            this.setComments(data);
        },

        clear() {
            this.comments = [];
            this.questionnaireId = null;
            this.entityId = null;
        },

        setComments(data) {
            this.comments = data;
            this.isCommentsBlockVisible =
                this.isCommentsBlockVisible == true && data && data.length > 0;
        },

        toggleComments() {
            this.isCommentsBlockVisible = !this.isCommentsBlockVisible;
        },        
    }
});
