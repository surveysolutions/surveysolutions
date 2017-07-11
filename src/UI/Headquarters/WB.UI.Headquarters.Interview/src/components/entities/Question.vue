<template>
    <div class="question" v-if="isEnabled" :class="questionClass" :id="hash">
        <button class="section-blocker" disabled="disabled" v-if="isFetchInProgress"></button>
        <div class="dropdown aside-menu" v-if="!noComments">
            <button type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" class="btn btn-link">
                <span></span>
            </button>
            <ul class="dropdown-menu">
                <li v-if="!isShowingAddCommentDialog"><a href="javascript:void(0)" @click="showAddComment">Write comment</a></li>
                <li v-if="isShowingAddCommentDialog"><a href="javascript:void(0)" @click="hideAddComment">Hide comment</a></li>
            </ul>
        </div>

        <div class="question-editor" :class="questionEditorClass">
            <wb-title v-if="!noTitle" />
            <wb-instructions v-if="!noInstructions" />
            <slot />
            <wb-validation v-if="!noValidation" />
            <wb-comments v-if="!noComments" :isShowingAddCommentDialog="isShowingAddCommentDialog" />
        </div>
        <wb-progress :visible="isFetchInProgress" :valuenow="valuenow" :valuemax="valuemax" />
    </div>
</template>
<script lang="ts">
    import { getLocationHash } from "src/store/store.fetch"

    export default {
        name: 'wb-question',
        props: ["question", 'questionCssClassName', 'noTitle', 'noInstructions', 'noValidation', 'noAnswer', 'noComments'],
        data() {
            return {
                isShowingAddCommentDialog: false
            }
        },
        computed: {
            id() {
                return this.question.id
            },
            valuenow() {
                if (this.question.fetchState) {
                    return this.question.fetchState.uploaded
                }
                return 100
            },
            valuemax() {
                if (this.question.fetchState) {
                    return this.question.fetchState.total
                }
                return 100
            },
            hash() {
                return getLocationHash(this.question.id)
            },
            isFetchInProgress() {
                return this.question.fetching
            },
            isEnabled() {
                return !this.question.isLoading && !(this.question.isDisabled && this.question.hideIfDisabled)
            },
            questionClass() {
                return [{ 'disabled-question': this.question.isDisabled }]
            },
            questionEditorClass() {
                return [{
                    answered: this.question.isAnswered && !this.noAnswer,
                    'has-error': !this.question.validity.isValid
                }, this.questionCssClassName]
            }
        },
        methods : {
            showAddComment(){
                this.isShowingAddCommentDialog = true;
            },
            hideAddComment(){
                this.isShowingAddCommentDialog = false;
            }
        }
    }

</script>
