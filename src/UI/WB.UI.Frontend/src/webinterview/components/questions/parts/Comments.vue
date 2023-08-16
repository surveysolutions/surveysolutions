<template>
    <div class="information-block comments-block">
        <button href="javascript:void(0);"
            class="btn btn-link show-resolved-comments"
            v-if="showResolvedVisible"
            @click="showResolved = !showResolved">
            <span>{{ showResolved ? this.$t('WebInterviewUI.HideResolved') : this.$t('WebInterviewUI.ShowResolved')}}</span>
        </button>

        <template v-for="comment in visibleComments">
            <wb-comment-item :userRole="comment.userRole"
                :text="comment.text"
                :isOwnComment="comment.isOwnComment"
                :key="comment.commentTimeUtc"
                :date="comment.commentTimeUtc"
                :resolved="comment.resolved"
                :commentOnPreviousAnswer="comment.commentOnPreviousAnswer" />
        </template>

        <div class="comment active"
            v-if="isShowingAddCommentDialog">
            <form class="form-inline"
                onsubmit="return false;">
                <label>{{ $t("WebInterviewUI.CommentYours") }}</label>
                <div class="form-group">
                    <div class="input-group comment-field">
                        <input type="text"
                            class="form-control"
                            v-on:keyup.enter="postComment"
                            v-model="comment"
                            :placeholder='$t("WebInterviewUI.CommentEnter")'
                            :disabled="!$store.getters.addCommentsAllowed"
                            :id="inpAddCommentId"
                            :title="inputTitle"
                            maxlength="750"/>
                        <div class="input-group-btn">
                            <button type="button"
                                class="btn btn-default btn-post-comment"
                                :class="buttonClass"
                                @click="postComment($event)"
                                :disabled="!allowPostComment"
                                :id="btnAddCommentId">
                                {{ postBtnText }}
                            </button>
                        </div>
                    </div>
                </div>
            </form>
            <button href="javascript:void(0);"
                class="btn btn-link resolve-comments"
                :disabled="isResolving"
                v-if="resolveAllowed"
                @click="resolve"
                :title="$t('WebInterviewUI.ResolveHint')">
                <span class="text-success">{{this.$t('WebInterviewUI.Resolve')}}</span>
            </button>
        </div>
    </div>
</template>

<script lang="js">

import { entityPartial } from '~/webinterview/components/mixins'
import { filter, find } from 'lodash'

export default {
    mixins: [entityPartial],
    data() {
        return {
            comment: null,
            showResolved: false,
            isResolving: false,
        }
    },
    props: {
        isShowingAddCommentDialog: { type: Boolean, default: false },
    },
    methods: {
        async postComment(evnt) {
            const com = this.comment

            if (!com || !com.trim())
                return

            await this.$store.dispatch('sendNewComment', { identity: this.$me.id, comment: com.trim() })

            this.comment = ''
            if(evnt && evnt.target) {
                evnt.target.blur()
            }
        },
        async resolve() {
            this.isResolving = true
            await this.$store.dispatch('resolveComment', { identity: this.questionId })
            this.isResolving = false
        },
    },
    computed: {
        inpAddCommentId() {
            return `inp_${this.$me.id}_addComment`
        },
        btnAddCommentId() {
            return `btn_${this.$me.id}_addComment`
        },
        visibleComments() {
            const self = this
            return filter(this.$me.comments, c => {
                return self.showResolved || !c.resolved
            })
        },
        showResolvedVisible() {
            return this.$me.comments.length > 0 && (find(this.$me.comments, cmt => cmt.resolved) != null)
        },
        allowPostComment() {
            return this.comment &&
                       this.comment.trim().length > 0 &&
                       !this.$me.postingComment
                       && this.$store.getters.addCommentsAllowed
        },
        resolveAllowed() {
            return this.$me.allowResolveComments
        },
        inputTitle() {
            if (this.$store.state.webinterview.receivedByInterviewer === true) {
                return this.$t('WebInterviewUI.InterviewReceivedCantModify')
            }
            return ''
        },
        buttonClass() {
            return this.isActive ? 'comment-added' : null
        },
        postBtnText() {
            return this.$me.postingComment ? this.$t('WebInterviewUI.CommentPosting') : this.$t('WebInterviewUI.CommentPost')
        },
        questionId() {
            return this.$me.id
        },
    },
}

</script>
