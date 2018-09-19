<template>
    <div class="information-block comments-block">

        <template v-for="comment in $me.comments">
            <wb-comment-item :userRole="comment.userRole" :text="comment.text" :isOwnComment="comment.isOwnComment" :key="comment.commentTimeUtc" />
        </template>

        <div class="comment active" v-if="isShowingAddCommentDialog">
            <form class="form-inline" onsubmit="return false;">
                <label>{{ $t("WebInterviewUI.CommentYours") }}</label>
                <div class="form-group">
                    <div class="input-group comment-field">
                        <input type="text" class="form-control" v-on:keyup.enter="postComment" v-model="comment"
                            :placeholder='$t("WebInterviewUI.CommentEnter")' 
                            :disabled="!$store.getters.addCommentsAllowed"
                            :title="inputTitle"/>
                        <div class="input-group-btn">
                            <button type="button" class="btn btn-default btn-post-comment"
                                :class="buttonClass" 
                                @click="postComment($event)" 
                                :disabled="!allowPostComment">
                                {{ postBtnText }}
                            </button>
                        </div>
                    </div>
                </div>
            </form>
        </div>
    </div>
</template>

<script lang="js">

    import { entityPartial } from "~/webinterview/components/mixins"

    export default {
        mixins: [entityPartial],
        data() {
            return {
                comment: null
            }
        },
        props: {
            isShowingAddCommentDialog: { type: Boolean, default: false }
        },
        methods: {
            getCommentTitle(comment) {
                if (comment.isOwnComment == true) {
                    return this.$t("WebInterviewUI.CommentYours")
                }
                if (comment.userRole == 1 /*'Administrator'*/) {
                    return this.$t("WebInterviewUI.CommentAdmin") // "Admin comment"
                }
                if (comment.userRole == 2/*'Supervisor'*/) {
                    return this.$t("WebInterviewUI.CommentSupervisor") // "Supervisor comment"
                }
                if (comment.userRole == 4/*'Interviewer'*/) {
                    return this.$t("WebInterviewUI.CommentInterviewer") // "Interviewer comment"
                }
                if (comment.userRole == 6/*'Headquarter'*/) {
                    return this.$t("WebInterviewUI.CommentHeadquarters") // "Headquarters comment"
                }

                return this.$t("WebInterviewUI.Comment") //'Comment';
            },

            async postComment(evnt) {
                const com = this.comment

                if (!com || !com.trim())
                    return

                await this.$store.dispatch("sendNewComment", { questionId: this.$me.id, comment: com.trim() })

                this.comment = ''
                if(evnt && evnt.target) {
                    evnt.target.blur()
                }
            }
        },
        computed: {
            allowPostComment() {
                return this.comment && 
                       this.comment.trim().length > 0 &&
                       !this.$me.postingComment 
                       && this.$store.getters.addCommentsAllowed;
            },
            inputTitle() {
                if (this.$store.state.webinterview.receivedByInterviewer === true) {
                    return this.$t('WebInterviewUI.InterviewReceivedCantModify')
                }
                return "";
            },
            buttonClass() {
                return this.isActive ? 'comment-added' : null
            },
            postBtnText() {
                return this.$me.postingComment ? this.$t("WebInterviewUI.CommentPosting") : this.$t("WebInterviewUI.CommentPost")
            }
        }
    }

</script>
