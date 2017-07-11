<template>
    <div class="information-block comments-block">

        <template v-for="comment in $me.comments">
            <h6>{{ getCommentTitle(comment) }}</h6>
            <p>{{ comment.text }}</p>
        </template>

        <div class="comment active" v-if="isShowingAddCommentDialog">
            <form class="form-inline">
                <label>Your comment</label>
                <div class="form-group">
                    <div class="input-group">
                        <input type="text" class="form-control" placeholder="Tap to enter your comment" />
                    </div>
                </div>
                <button type="submit" class="btn btn-default  btn-gray">Post</button>
            </form>
        </div>
    </div>
</template>
<script lang="ts">
    import { entityPartial } from "components/mixins"

    export default {
        mixins: [entityPartial],
        name: "wb-remove-answer",
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
                    return "Headquarter comment"
                }
                return 'Comment';
            }
        }
    }

</script>
