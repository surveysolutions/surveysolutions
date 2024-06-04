<template>
    <div class="comments">
        <perfect-scrollbar class="scroller">
            <h3>
                <span>{{ $t('QuestionnaireEditor.SideBarCommentsCounter', {
                    count: commentThreads.length
                }) }}</span>
            </h3>
            <div class="empty-list" v-if="commentThreads.length == 0">
                <p> {{ $t('QuestionnaireEditor.SideBarEmptyCommentsLine') }}</p>
            </div>
            <ul>
                <li class="comment-thread" v-for="commentThread in commentThreads">

                    <router-link class="reference-item" :id="commentThread.entity.itemId" :to="{
                    name: commentThread.entity.type.toLowerCase(),
                    params: {
                        entityId: commentThread.entity.itemId,
                        chapterId: commentThread.entity.chapterId,
                    }
                }">
                        <span v-if="commentThread.entity.type == 'Question'" class="icon"
                            :class="commentThread.entity.questionType"></span>
                        <span
                            v-if="commentThread.entity.type !== 'Question' && commentThread.entity.type !== 'Group' && commentThread.entity.type !== 'Roster'"
                            class="icon" :class="'icon-' + commentThread.entity.type.toLowerCase()"></span>
                        <span class="title" v-dompurify-html="sanitizeText(commentThread.entity.title)"></span>
                        <span class="variable" v-dompurify-html="commentThread.entity.variable || '&nbsp;'"></span>
                    </router-link>

                    <div class="comments-in-thread">
                        <ul>
                            <li class="comment" :class="{ resolved: comment.isResolved }"
                                v-for="comment in commentThread.comments">
                                <span class="author">{{ comment.userEmail }}</span>
                                <span class="date" v-dateTime="comment.date"></span>
                                <p class="comment-text">{{ comment.comment }}</p>
                            </li>
                        </ul>
                        <div v-if="commentThread.resolvedComments.length > 0">
                            <a href="javascript:void(0);" class="show-more"
                                @click="commentThread.resolvedAreExpanded = !commentThread.resolvedAreExpanded">
                                <span v-if="!commentThread.resolvedAreExpanded">
                                    {{ $t('QuestionnaireEditor.ViewResolvedCommentsCounter', {
                    count: commentThread.resolvedComments.length
                }) }}
                                </span>
                                <span v-if="commentThread.resolvedAreExpanded">{{
                    $t('QuestionnaireEditor.HideResolvedComments') }}</span>
                            </a>
                            <ul v-if="commentThread.resolvedAreExpanded">
                                <li class="comment" :class="{ resolved: comment.isResolved }"
                                    v-for="comment in commentThread.resolvedComments">
                                    <span class="author">{{ comment.userEmail }}</span>
                                    <span class="date" v-dateTime="comment.date"></span>
                                    <p class="comment-text">{{ comment.comment }}</p>
                                </li>
                            </ul>
                        </div>
                    </div>
                </li>
            </ul>
        </perfect-scrollbar>
    </div>
</template>

<script>
import { getCommentThreads } from '../../../../services/commentsService';
import { sanitize } from '../../../../services/utilityService';
import _ from 'lodash';
import moment from 'moment';

export default {
    name: 'Comments',
    props: {
        questionnaireId: { type: String, required: true },
    },
    data() {
        return {
            commentThreads: [],
        }
    },
    async beforeMount() {
        await this.fetch();
    },
    mounted() {
        this.$emitter.on('commentResolved', this.commentResolved);
        this.$emitter.on('commentDeleted', this.commentDeleted);
        this.$emitter.on('commentAdded', this.commentAdded);

        this.$emitter.on('staticTextUpdated', this.staticTextUpdated);
        this.$emitter.on('groupUpdated', this.groupUpdated);
        this.$emitter.on('rosterUpdated', this.rosterUpdated);
        this.$emitter.on('variableUpdated', this.variableUpdated);
        this.$emitter.on('questionUpdated', this.questionUpdated);

        this.$emitter.on('staticTextDeleted', this.entityDeleted);
        this.$emitter.on('groupDeleted', this.compoundEntityDeleted);
        this.$emitter.on('rosterDeleted', this.compoundEntityDeleted);
        this.$emitter.on('variableDeleted', this.entityDeleted);
        this.$emitter.on('questionDeleted', this.entityDeleted);
    },
    unmounted() {
        this.$emitter.off('commentResolved', this.commentResolved);
        this.$emitter.off('commentDeleted', this.commentDeleted);
        this.$emitter.off('commentAdded', this.commentAdded);

        this.$emitter.off('staticTextUpdated', this.staticTextUpdated);
        this.$emitter.off('groupUpdated', this.groupUpdated);
        this.$emitter.off('rosterUpdated', this.rosterUpdated);
        this.$emitter.off('variableUpdated', this.variableUpdated);
        this.$emitter.off('questionUpdated', this.questionUpdated);

        this.$emitter.off('staticTextDeleted', this.entityDeleted);
        this.$emitter.off('groupDeleted', this.compoundEntityDeleted);
        this.$emitter.off('rosterDeleted', this.compoundEntityDeleted);
        this.$emitter.off('variableDeleted', this.entityDeleted);
        this.$emitter.off('questionDeleted', this.entityDeleted);
    },
    methods: {
        async fetch() {
            const data = await getCommentThreads(this.questionnaireId);

            _.forEach(data, function (commentThread) {
                commentThread.resolvedComments = [];
                commentThread.resolvedAreExpanded = false;

                _.forEach(commentThread.comments, function (comment) {
                    comment.date = moment.utc(comment.date)
                    comment.isResolved = !_.isNull(comment.resolveDate || null);
                });

                commentThread.resolvedComments = _.filter(commentThread.comments, {
                    isResolved: true
                });
                commentThread.comments = _.filter(commentThread.comments, {
                    isResolved: false
                });
            });

            this.commentThreads = data;
        },
        commentResolved(payload) {
            const index = _.findIndex(this.commentThreads, function (i) {
                return i.entity.itemId === payload.entityId;
            });
            if (index !== -1) {
                const indexComment = _.findIndex(this.commentThreads[index].comments, function (i) {
                    return i.id === payload.id;
                });
                if (indexComment !== -1) {
                    this.commentThreads[index].comments[indexComment].resolveDate = payload.resolveDate;

                    this.commentThreads[index].resolvedComments.push(this.commentThreads[index].comments[indexComment]);

                    this.commentThreads[index].comments.splice(indexComment, 1);
                }
            }
        },
        commentDeleted(payload) {

            const index = _.findIndex(this.commentThreads, function (i) {
                return i.entity.itemId === payload.entityId;
            });
            if (index !== -1) {
                const indexComment = _.findIndex(this.commentThreads[index].comments, function (i) {
                    return i.id === payload.id;
                });
                if (indexComment !== -1) {
                    this.commentThreads[index].comments.splice(indexComment, 1);
                }
                const indexCommentResolved = _.findIndex(this.commentThreads[index].resolvedComments, function (i) {
                    return i.id === payload.id;
                });
                if (indexCommentResolved !== -1) {
                    this.commentThreads[index].resolvedComments.splice(indexCommentResolved, 1);
                }

                if (this.commentThreads[index].resolvedComments.length == 0 && this.commentThreads[index].comments.length == 0) {
                    this.commentThreads.splice(index, 1)
                }
            }
        },
        commentAdded(payload) {
            const index = _.findIndex(this.commentThreads, function (i) {
                return i.entity.itemId === payload.entityId;
            });

            const comment = Object.assign({}, payload);

            if (index === -1) {
                this.commentThreads.push({
                    entity: {
                        itemId: payload.entityId,
                        title: payload.title,
                        type: payload.type,
                        variable: payload.variable,
                        chapterId: payload.chapterId
                    },
                    comments: [comment],
                    resolvedComments: [],
                    resolvedAreExpanded: false
                });
            }
            else {
                this.commentThreads[index].comments.push(comment);
            }
        },

        staticTextUpdated(payload) {
            const index = _.findIndex(this.commentThreads, function (i) {
                return payload.id && i.entity.itemId === payload.id.split('-').join('');
            });
            if (index !== -1) {
                this.commentThreads[index].entity.title = payload.text;
            }
        },

        groupUpdated(payload) {
            var id = payload.group.id.split('-').join('');
            const index = _.findIndex(this.commentThreads, function (i) {
                return i.entity.itemId === id
            });

            if (index !== -1) {
                this.commentThreads[index].entity.title = payload.group.title;
                this.commentThreads[index].entity.variable = payload.group.variableName;
            }
        },

        rosterUpdated(payload) {
            var id = payload.roster.itemId.split('-').join('');
            const index = _.findIndex(this.commentThreads, function (i) {
                return i.entity.itemId === id;
            });
            if (index !== -1) {
                this.commentThreads[index].entity.title = payload.roster.title;
                this.commentThreads[index].entity.variable = payload.roster.variableName;
            }
        },

        variableUpdated(payload) {
            const index = _.findIndex(this.commentThreads, function (i) {
                return i.entity.itemId === payload.id;
            });
            if (index !== -1) {
                this.commentThreads[index].entity.title = payload.label;
                this.commentThreads[index].entity.variable = payload.variable;
            }
        },
        questionUpdated(payload) {
            const id = payload.id.split('-').join('');
            const index = _.findIndex(this.commentThreads, function (i) {
                return i.entity.itemId === id;
            });
            if (index !== -1) {
                this.commentThreads[index].entity.title = payload.title;
                this.commentThreads[index].entity.variable = payload.variableName;
            }
        },

        entityDeleted(payload) {
            var id = payload.itemId ?? payload.id;
            if (id) {
                id = id.split('-').join('');
                const index = _.findIndex(this.commentThreads, function (i) {
                    return i.entity.itemId === id;
                });
                if (index !== -1) {
                    this.commentThreads.splice(index, 1)
                }
            }
        },

        async compoundEntityDeleted(payload) {
            await this.fetch();
        },

        sanitizeText(value) {
            const sanitizedValue = sanitize(value);
            return sanitizedValue;
        }
    }
}
</script>