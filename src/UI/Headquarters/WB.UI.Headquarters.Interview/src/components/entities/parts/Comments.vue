<template>
    <div class="information-block comments-block">

        <template v-for="comment in $me.comments">
            <div :class="{'enumerators-comment': comment.userRole == 4 /*'Interviewer'*/} ">
                <h6>{{ getCommentTitle(comment) }}</h6>
                <p>{{ comment.text }}</p>
            </div>
        </template>

        <div class="comment active" v-if="isShowingAddCommentDialog">
            <div class="form-inline">
                <label>{{ $t("CommentYours") }}</label>
                <div class="form-group">
                    <div class="input-group field">
                        <input type="text" class="form-control" v-on:keyup.enter="postComment" v-model="comment"
                            :placeholder='$t("CommentEnter")' />
                    </div>
                </div>
                <button type="button" class="btn btn-default btn-post-comment"
                :class="buttonClass" @click="postComment($event)">{{ $t("CommentPost") }}</button>
            </div>
        </div>
    </div>
</template>

<script lang="js">

    import { entityPartial } from "components/mixins"

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
                    return this.$t("CommentYours")
                }
                if (comment.userRole == 1 /*'Administrator'*/) {
                    return this.$t("CommentAdmin") // "Admin comment"
                }
                if (comment.userRole == 2/*'Supervisor'*/) {
                    return this.$t("CommentSupervisor") // "Supervisor comment"
                }
                if (comment.userRole == 4/*'Interviewer'*/) {
                    return this.$t("CommentInterviewer") // "Interviewer comment"
                }
                if (comment.userRole == 6/*'Headquarter'*/) {
                    return this.$t("CommentHeadquarters") // "Headquarters comment"
                }

                return this.$t("Comment") //'Comment';
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
