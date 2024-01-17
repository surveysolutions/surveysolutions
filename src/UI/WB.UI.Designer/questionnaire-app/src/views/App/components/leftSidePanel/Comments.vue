<template>
    <div class="comments">
        <perfect-scrollbar class="scroller">
            <h3>
                <span>{{ $t('QuestionnaireEditor.SideBarCommentsCounter', {
                    count: commentThreads.length
                }) }}</span>
            </h3>
            <div class="empty-list" v-if="commentThreads.length == 0">
                <p> {{ $t('QuestionnaireEditor.SideBarCommentsCounter') }}</p>
            </div>
            <ul>
                <li class="comment-thread" v-for="commentThread in commentThreads">

                    <router-link class="reference-item" :id="commentThread.entity.itemId" :to="{
                        name: commentThread.entity.type.toLowerCase(),
                        params: {
                            statictextId: commentThread.entity.itemId,
                            variableId: commentThread.entity.itemId,
                            groupId: commentThread.entity.itemId,
                            rosterId: commentThread.entity.itemId,

                            questionId: commentThread.entity.itemId,
                        }
                    }">
                        <span v-if="commentThread.entity.type == 'Question'" class="icon"
                            :class="commentThread.entity.questionType"></span>
                        <span
                            v-if="commentThread.entity.type !== 'Question' && commentThread.entity.type !== 'Group' && commentThread.entity.type !== 'Roster'"
                            class="icon" :class="'icon-' + commentThread.entity.type.toLowerCase()"></span>
                        <span class="title" v-text="commentThread.entity.title"></span>
                        <span class="variable" v-html="commentThread.entity.variable || '&nbsp;'"></span>
                    </router-link>

                    <div class="comments-in-thread">
                        <ul>
                            <li class="comment" :class="{ resolved: comment.isResolved }"
                                v-for="comment in commentThread.comments">
                                <span class="author">{{ comment.userEmail }}</span>
                                <span class="date">{{ comment.date }}</span>
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
                                    <span class="date">{{ comment.date }}</span>
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
import { useCommentsStore } from '../../../../stores/comments';

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
    setup() {
        const commentsStore = useCommentsStore();

        return {
            commentsStore
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    methods: {
        async fetch() {
            this.commentThreads = await this.commentsStore.getCommentThreads(this.questionnaireId);
        },
        showCommentsAndNavigateTo(entity) { },
    }
}
</script>
  