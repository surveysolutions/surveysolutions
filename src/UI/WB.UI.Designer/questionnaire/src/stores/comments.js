import { defineStore } from 'pinia';
import _ from 'lodash';
import { getComments, postComment } from '../services/commentsService';
import emitter from '../services/emitter';
import moment from 'moment';

export const useCommentsStore = defineStore('comments', {
    state: () => ({
        comments: [],
        questionnaireId: null,
        entityId: null,
        isCommentsBlockVisible: false,
        entityInfoProvider: null
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
            _.remove(
                this.$state.comments,
                comment => comment.id === payload.id
            );
        },
        commentAdded(payload) {
            this.$state.comments.push(payload);
        },
        async fetchComments(questionnaireId, entityId) {
            const data = await getComments(questionnaireId, entityId);

            this.questionnaireId = questionnaireId;
            this.entityId = entityId;

            _.forEach(data, function(comment) {
                comment.date = moment.utc(comment.date);
                comment.isResolved = !_.isNull(comment.resolveDate || null);
            });

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

        registerEntityInfoProvider(provider) {
            this.entityInfoProvider = provider;
        },

        async postComment(
            questionnaireId,
            comment,
            entityId,
            userName,
            userEmail,
            chapterId
        ) {
            var title = '';
            var variable = '';
            var type = '';

            if (this.entityInfoProvider) {
                var entityInfo = this.entityInfoProvider();
                title = entityInfo.title;
                variable = entityInfo.variable;
                type = entityInfo.type;
            }

            await postComment(
                questionnaireId,
                comment,
                entityId,
                userName,
                userEmail,
                chapterId,
                title,
                variable,
                type
            );
        }
    }
});
