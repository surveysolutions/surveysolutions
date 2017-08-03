<template>
    <div class="information-block comments-block">

        <template v-for="comment in $me.comments">
            <h6>{{ getCommentTitle(comment) }}</h6>
            <p>{{ comment.text }}</p>
        </template>

        <div class="comment active" v-if="isShowingAddCommentDialog">
            <div class="form-inline">
                <label>Your comment</label>
                <div class="form-group">
                    <div class="input-group field">
                        <input type="text" class="field-to-fill" v-on:keyup.enter="postComment" v-model="comment" placeholder="Enter your comment" />
                    </div>
                </div>
                <button type="button" class="btn btn-default btn-post-comment" :class="buttonClass" @click="postComment($event)">Post</button>
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
                    return "Your comment"
                }
                if (comment.userRole == 1 /*'Administrator'*/) {
                    return "Admin comment"
                }
                if (comment.userRole == 2/*'Supervisor'*/) {
                    return "Supervisor comment"
                }
                if (comment.userRole == 4/*'Interviewer'*/) {
                    return "Interviewer comment"
                }
                if (comment.userRole == 6/*'Headquarter'*/) {
                    return "Headquarters comment"
                }
                return 'Comment';
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
