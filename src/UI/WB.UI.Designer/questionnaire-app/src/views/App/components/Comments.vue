<template>
    <form role="form" id="comments-editor" name="addCommentForm" unsaved-warning-form>
        <div class="form-holder">
            <div v-for="comment in comments" :key="comment.id" class="comment"
                :class="{ isResolved: comment.resolveDate != null }">
                <button type="button" class="btn btn-sm btn-link pull-right" @click="deleteComment(comment.id)"
                    v-t="{ path: 'QuestionnaireEditor.Delete' }"></button>
                <button v-show="comment.resolveDate == null" type="button" class="btn btn-sm btn-link pull-right"
                    @click="resolveComment(comment.id)" v-t="{ path: 'QuestionnaireEditor.CommentEditorResolve' }"></button>
                <span class="author">{{ comment.userEmail }}</span>
                <span class="date">{{ comment.date }}</span>
                <p class="comment-text">{{ comment.comment }}</p>
            </div>

            <div class="row" id="edit-entity-comment-row">
                <div class="form-group col-xs-12">
                    <label class="wb-label" for="edit-entity-comment"> {{ $t('QuestionnaireEditor.EntityComment') }}
                        <help link="newComment" />
                    </label>
                    <textarea name="comment" id="edit-entity-comment" v-model="activeComment.comment" class="form-control"
                        required v-autosize></textarea>
                    <div>
                        <span>{{ activeComment.comment.length }} / {{ maxCommentLength }}</span>
                    </div>
                    <div class="text-danger" v-show="activeComment.serverValidation !== null">
                        <span>{{ activeComment.serverValidation }}</span>
                    </div>
                </div>
            </div>
            <div class="form-group">
                <button type="button" id="edit-post-comment-button" class="btn btn-lg " :class="{ 'btn-primary': dirty }"
                    unsaved-warning-clear @click="postComment()" v-if="!questionnaire.isReadOnlyForUser" :disabled="!valid"
                    v-t="{ path: 'QuestionnaireEditor.EditorAddComment' }"></button>
            </div>
        </div>
    </form>
</template>

<script>

import { useUserStore } from '../../../stores/user';
import { useCommentsStore } from '../../../stores/comments';
import { resolveComment, postComment, deleteComment } from '../../../services/commentsService';
import Help from './Help.vue'

export default {
    name: 'Comments',
    components: { Help },
    inject: ['questionnaire'],
    props: {
        questionnaireId: { type: String, required: true },
        entityId: { type: String, required: true }
    },
    data() {
        return {
            activeComment: {
                comment: '',
                serverValidation: null
            },
            maxCommentLength: 1000,
            dirty: false,
        }
    },
    watch: {
        activeComment: {
            handler(newVal, oldVal) {
                if (oldVal != null) this.dirty = true;
            },
            deep: true
        },
        async entityId(newValue, oldValue) {
            if (newValue != oldValue) {
                this.commentsStore.clear();
                await this.fetch();
            }
        }
    },
    setup() {
        const commentsStore = useCommentsStore();

        return {
            commentsStore
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    computed: {
        comments() {
            return this.commentsStore.getComments;
        },
        valid() {
            return this.activeComment.comment && this.activeComment.comment.length > 0 && this.activeComment.comment.length < this.maxCommentLength
        }
    },
    methods: {
        async fetch() {
            await this.commentsStore.fetchComments(this.questionnaireId, this.entityId);
        },
        async postComment() {

            const userStore = useUserStore();
            const userName = userStore.userName;
            const userEmail = userStore.email;

            await postComment(this.questionnaireId, this.activeComment.comment, this.entityId, userName, userEmail);

            this.activeComment.comment = '';
            this.activeComment.serverValidation = null;

            this.dirty = false;
        },
        deleteComment(commentId) {
            const params = {
                title: this.$t('QuestionnaireEditor.DeleteCommentConfirm'),
                okButtonTitle: this.$t('QuestionnaireEditor.Delete'),
                cancelButtonTitle: this.$t('QuestionnaireEditor.Cancel'),
                isReadOnly: this.questionnaire.isReadOnlyForUser,
                callback: async confirm => {
                    if (confirm) {
                        deleteComment(this.questionnaire.questionnaireId, commentId, this.entityId);
                    }
                }
            };

            this.$confirm(params);
        },
        resolveComment(commentId) {
            resolveComment(this.questionnaire.questionnaireId, commentId, this.entityId);
        }
    }
};
</script>
