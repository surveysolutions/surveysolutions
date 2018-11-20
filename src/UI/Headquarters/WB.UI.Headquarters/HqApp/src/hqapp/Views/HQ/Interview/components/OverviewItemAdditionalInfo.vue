<template>
    <div ref="additionalInfo" :class="{visible : isAdditionalInfoVisible}" v-if="isAdditionalInfoVisible" class="custom-popover overview-additional-information">
        <div class="popover-header">
            <button @click="close" type="button" class="close close-popover" ><span></span></button>
            <h5>{{$t("WebInterviewUI.Interview_Overview_AdditionalInformation")}}</h5>
        </div>
        <div class="popover-content">
            <div v-if="errors.length > 0">
                <div class="information-block text-danger">
                    <h6>{{ $t("WebInterviewUI.AnswerIsInvalid") }}</h6>
                    <p v-for="(error, index) in errors" :key="index"><span v-html="error"></span></p>
                </div>
                <hr />
            </div>
            <div v-if="warnings.length > 0">
                <div class="information-block text-warning">
                    <h6>{{ $t("WebInterviewUI.WarningsHeader") }}</h6>
                    <p v-for="(warning, index) in warnings" :key="index"><span v-html="warning"></span></p>
                </div>
                <hr />
            </div>
            <div class="information-block comments-block">
                <template v-for="comment in comments">
                    <wb-comment-item :userRole="comment.UserRole" :text="comment.Text" :isOwnComment="comment.IsOwnComment" :key="comment.CommentTimeUtc" />
                </template>
                <div class="comment" v-if="isCommentFormIsVisible">
                    <form class="form-inline" onsubmit="return false;">
                        <label>{{ $t("WebInterviewUI.CommentYours") }}</label>
                        <div class="form-group">
                            <div class="input-group comment-field">
                                <textarea-autosize 
                                    autocomplete="off"
                                    rows="1"
                                    v-on:keyup.enter="postComment" 
                                    v-model="comment"
                                    :placeholder='$t("WebInterviewUI.CommentEnter")' 
                                    :disabled="!$store.getters.addCommentsAllowed"
                                    class="form-control"
                                    :title="inputTitle"/>
                                <div class="input-group-btn">
                                    <button 
                                        @click="postComment($event)" 
                                        :disabled="!$store.getters.addCommentsAllowed" 
                                        type="button" 
                                        class="btn btn-default  btn-post-comment">
                                        {{postBtnText}}</button>
                                </div>
                            </div>
                        </div>
                    </form>
                </div>
            </div>
        </div>
        <div class="popover-footer clearftix">
            <button type="button" v-if="item.SupportsComments" @click="showAddCommentForm" class="btn btn-link gray-action-unit pull-left add-comment">{{$t("WebInterviewUI.CommentAdd")}}</button>
            <button type="button" @click="close" class="btn btn-link gray-action-unit pull-right close-popover">{{$t("Pages.CloseLabel")}}</button>
        </div>
    </div>
</template>

<script>
export default {
    props: {
        item: {
            required: true,
            type: Object
        }
    },
    data() {
        return {
            isAdditionalInfoVisible: false,
            isCommentFormIsVisible: false,
            comment: null,
            postingComment: false
        };
    },
    methods: 
    {
        show(){
            this.$store.dispatch("loadAdditionalInfo", { id: this.item.Id });
            this.isAdditionalInfoVisible = true;
        },
        close(){
            this.isAdditionalInfoVisible = false;
        },
        showAddCommentForm(){
            this.isCommentFormIsVisible = true;
        },
        async postComment(evnt){
            this.postingComment = true;
            const com = this.comment

            if (!com || !com.trim())
                return

            await this.$store.dispatch("sendNewComment", { questionId: this.item.Id, comment: com.trim() })

            this.$store.dispatch("loadAdditionalInfo", { id: this.item.Id });

            this.comment = ''
            if(evnt && evnt.target) {
                evnt.target.blur()
            }

            this.item.HasComment = true;
            this.isCommentFormIsVisible = false;
            this.postingComment = false;
        }
    },
    computed: {
        additionalInfo() {
            return this.$store.state.review.overview.additionalInfo[this.item.Id] || {};
        },
        errors() {
            return this.additionalInfo.Errors || [];
        },
        warnings() {
            return this.additionalInfo.Warnings || [];
        },
        comments() {
            return this.additionalInfo.Comments || [];
        },
        postBtnText() {
            return this.postingComment ? this.$t("WebInterviewUI.CommentPosting") : this.$t("WebInterviewUI.CommentPost")
        },
        inputTitle() {
            if (this.$store.state.webinterview.receivedByInterviewer === true) {
                return this.$t('WebInterviewUI.InterviewReceivedCantModify')
            }
            return "";
        },
    },
}

</script>
