<template>
    <div class="information-block comments-block">

        <template v-for="comment in $me.comments">
            <div :class="{'enumerators-comment': comment.userRole == 4 /*'Interviewer'*/}" :key="comment.commentTimeUtc">
                <h6>{{ getCommentTitle(comment) }}</h6>
                <p>{{ comment.text }}</p>
            </div>
        </template>

        <div class="comment active" v-if="isShowingAddCommentDialog">
            <form class="form-inline" v-on:submit.prevent="onSubmit">
                <label>{{ $t("WebInterviewUI.CommentYours") }}</label>
                <div class="form-group">
                    <div class="input-group comment-field">
                         <input type="text" class="form-control" v-on:keyup.enter="postComment" v-model="comment"
                            :placeholder='$t("WebInterviewUI.CommentEnter")' />
                        <div class="input-group-btn">
                             <button type="button" class="btn btn-default btn-post-comment"
                                :class="buttonClass" @click="postComment($event)">{{ $t("WebInterviewUI.CommentPost") }}</button>
                        </div>
                    </div>
                </div>
            </form>
        </div>
    </div>
</template>

<script lang="js">

    import { entityPartial } from "shared/components/mixins"

    export default {
        mixins: [entityPartial],
        name: "wb-remove-answer",
        data() {
            return {
                comment: null,
            }
        },
        props: {
            isShowingAddCommentDialog: { type: Boolean, default: false },
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
            postComment(evnt) {
                const com = this.comment

                if (!com || !com.trim())
                    return

                this.$store.dispatch("sendNewComment", { questionId: this.$me.id, comment: com.trim() })

                this.comment = ''
                if(evnt && evnt.target) {
                    evnt.target.blur()
                }
            }
        },
        computed: {
            buttonClass() {
                const isActive = this.comment && this.comment.trim().length > 0
                return isActive ? 'comment-added' : null
            }
        }
    }

</script>
