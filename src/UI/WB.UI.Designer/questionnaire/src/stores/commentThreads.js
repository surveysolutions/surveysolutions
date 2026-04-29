import { defineStore } from 'pinia';
import { getCommentThreads } from '../services/commentsService';
import emitter from '../services/emitter';

export const useCommentThreadsStore = defineStore('commentThreads', {
    state: () => ({
        unresolvedCount: 0,
    }),
    getters: {
        getUnresolvedCount: state => state.unresolvedCount,
    },
    actions: {
        setUnresolvedCount(count) {
            this.unresolvedCount = count;
        },
        setupListeners() {
            emitter.on('commentAdded', this.onCommentAdded);
            emitter.on('commentResolved', this.onCommentResolved);
            emitter.on('commentDeleted', this.onCommentDeleted);
        },
        teardownListeners() {
            emitter.off('commentAdded', this.onCommentAdded);
            emitter.off('commentResolved', this.onCommentResolved);
            emitter.off('commentDeleted', this.onCommentDeleted);
        },
        onCommentAdded() {
            this.unresolvedCount++;
        },
        onCommentResolved() {
            if (this.unresolvedCount > 0) this.unresolvedCount--;
        },
        onCommentDeleted(payload) {
            if (!payload.resolveDate) {
                if (this.unresolvedCount > 0) this.unresolvedCount--;
            }
        },
        async initializeCount(questionnaireId) {
            const data = await getCommentThreads(questionnaireId);
            let count = 0;
            if (data) {
                data.forEach(thread => {
                    thread.comments.forEach(comment => {
                        if (!comment.resolveDate) count++;
                    });
                });
            }
            this.unresolvedCount = count;
        },
    }
});
