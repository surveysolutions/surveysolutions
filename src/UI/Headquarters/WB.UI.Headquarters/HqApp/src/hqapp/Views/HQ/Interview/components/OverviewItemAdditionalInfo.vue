<template>
    <div ref="additionalInfo" :class="{visible : isAdditionalInfoVisible}" v-if="isAdditionalInfoVisible" class="custom-popover overview-additional-information">
        <div class="popover-header">
            <button @click="close" type="button" class="close close-popover" ><span></span></button>
            <h5>Additional question information</h5>
        </div>
        <div class="popover-content">
            <div v-if="errors.length > 0">
                <div class="information-block text-danger">
                    <h6>Data error</h6>
                    <p v-for="(error, index) in errors" :key="index">{{error}}</p>
                </div>
                <hr />
            </div>
            <div v-if="warnings.length > 0">
                <div class="information-block text-warning">
                    <h6>Data warning</h6>
                    <p v-for="(warning, index) in warnings" :key="index">{{warning}}</p>
                </div>
                <hr />
            </div>
            <div class="information-block comments-block">
                <div class="enumerators-comment">
                    <h6>ENUMERATOR'S comment <span>(dec 12)</span></h6>
                    <p>Please ask about quantity in total for household</p>
                </div>
                <h6>Your comment <span>(dec 12)</span></h6>
                <p>Value of quantity is correct</p>
                <div class="enumerators-comment">
                    <h6>ENUMERATOR'S comment <span>(dec 12)</span></h6>
                    <p>Please ask about quantity in total for household</p>
                </div>
                <h6>Your comment <span>(23 hours ago)</span></h6>
                <p>Ok</p>
                <div class="comment" v-if="isCommentFormIsVisible">
                    <form class="form-inline">
                        <label>Enter Your comment</label>
                        <div class="form-group">
                            <div class="input-group comment-field">
                                <input type="text" class="form-control" aria-label="..." placeholder="Tap to enter your comment">
                                <div class="input-group-btn">
                                    <button @click="postComment" type="button" class="btn btn-default  btn-post-comment">Post</button>
                                </div>
                            </div>
                        </div>
                    </form>
                </div>
            </div>
        </div>
        <div class="popover-footer clearftix">
            <button type="button" @click="showAddCommentForm" class="btn btn-link gray-action-unit pull-left add-comment">Add comment</button>
            <button type="button" @click="close" class="btn btn-link gray-action-unit pull-right close-popover">close</button>
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
        postComment(){
            this.isCommentFormIsVisible = false;
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
    },
}

</script>
