import { defineStore } from 'pinia';
import { getCommentThreads } from '../services/commentsService';
import emitter from '../services/emitter';

export const useCommentThreadsStore = defineStore('commentThreads', {
    state: () => ({
        unresolvedCommentIds: [],
        questionnaireId: null,
        isInitializing: false,
        needsRefresh: false,
        pendingEvents: [],
    }),
    getters: {
        getUnresolvedCount: state => state.unresolvedCommentIds.length,
    },
    actions: {
        setUnresolvedCommentIds(commentIds) {
            this.unresolvedCommentIds = [...new Set(commentIds)];
        },
        setupListeners(questionnaireId) {
            this.questionnaireId = questionnaireId;

            emitter.on('commentAdded', this.onCommentAdded);
            emitter.on('commentResolved', this.onCommentResolved);
            emitter.on('commentDeleted', this.onCommentDeleted);
            emitter.on('staticTextDeleted', this.onEntityDeleted);
            emitter.on('groupDeleted', this.onEntityDeleted);
            emitter.on('rosterDeleted', this.onEntityDeleted);
            emitter.on('variableDeleted', this.onEntityDeleted);
            emitter.on('questionDeleted', this.onEntityDeleted);
        },
        teardownListeners() {
            emitter.off('commentAdded', this.onCommentAdded);
            emitter.off('commentResolved', this.onCommentResolved);
            emitter.off('commentDeleted', this.onCommentDeleted);
            emitter.off('staticTextDeleted', this.onEntityDeleted);
            emitter.off('groupDeleted', this.onEntityDeleted);
            emitter.off('rosterDeleted', this.onEntityDeleted);
            emitter.off('variableDeleted', this.onEntityDeleted);
            emitter.off('questionDeleted', this.onEntityDeleted);
        },
        queueOrApplyEvent(handlerName, payload) {
            if (this.isInitializing) {
                this.pendingEvents.push({ handlerName, payload });
                return;
            }

            this[handlerName](payload);
        },
        applyQueuedEvent({ handlerName, payload }) {
            this[handlerName](payload);
        },
        addUnresolvedCommentId(commentId) {
            if (commentId && !this.unresolvedCommentIds.includes(commentId)) {
                this.unresolvedCommentIds.push(commentId);
            }
        },
        removeUnresolvedCommentId(commentId) {
            this.unresolvedCommentIds = this.unresolvedCommentIds.filter(id => id !== commentId);
        },
        onCommentAdded(payload) {
            this.queueOrApplyEvent('addUnresolvedCommentId', payload?.id);
        },
        onCommentResolved(payload) {
            this.queueOrApplyEvent('removeUnresolvedCommentId', payload?.id);
        },
        onCommentDeleted(payload) {
            this.queueOrApplyEvent('removeUnresolvedCommentId', payload?.id);
        },
        async onEntityDeleted() {
            if (this.isInitializing) {
                this.needsRefresh = true;
                return;
            }

            await this.initializeCount(this.questionnaireId);
        },
        getUnresolvedCommentIds(data) {
            return (data ?? [])
                .flatMap(thread => thread.comments ?? [])
                .filter(comment => !comment.resolveDate)
                .map(comment => comment.id);
        },
        async initializeCount(questionnaireId) {
            if (!questionnaireId) return;

            this.questionnaireId = questionnaireId;
            this.isInitializing = true;
            this.pendingEvents = [];
            try {
                const data = await getCommentThreads(questionnaireId);
                this.unresolvedCommentIds = this.getUnresolvedCommentIds(data);

                const pendingEvents = this.pendingEvents;
                this.pendingEvents = [];

                pendingEvents.forEach(event => this.applyQueuedEvent(event));
            } finally {
                this.isInitializing = false;
            }

            if (this.needsRefresh) {
                this.needsRefresh = false;
                await this.initializeCount(questionnaireId);
            }
        },
    }
});
