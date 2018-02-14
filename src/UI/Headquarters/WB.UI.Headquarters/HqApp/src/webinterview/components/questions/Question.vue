<template>
    <div class="question" v-if="isVisible" :class="questionClass" :id="hash">
        <button class="section-blocker" disabled="disabled" v-if="isFetchInProgress"></button>
        <div class="dropdown aside-menu" v-if="showSideMenu">
            <button type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" class="btn btn-link">
                <span></span>
            </button>
            <ul class="dropdown-menu">
                <li v-if="!isShowingAddCommentDialog">
                    <a href="javascript:void(0)" @click="showAddComment" :disabled="!$store.getters.addCommentsAllowed">
                        {{ $t("WebInterviewUI.CommentAdd") }}
                    </a>
                </li>
                <li v-else>
                    <a href="javascript:void(0)" @click="hideAddComment" :disabled="!$store.getters.addCommentsAllowed">
                        {{ $t("WebInterviewUI.CommentHide") }}
                    </a>
                </li>
                <slot name="sideMenu"></slot>
            </ul>
        </div>

        <div class="question-editor" :class="questionEditorClass">
            <wb-flag v-if="$store.getters.isReviewMode === true && !noFlag" />
            <wb-title v-if="!noTitle" />
            <wb-instructions v-if="!noInstructions" />
            <slot />
            <wb-validation v-if="!noValidation"/>
            <wb-warnings v-if="!noValidation" />
            <wb-comments v-if="!noComments" :isShowingAddCommentDialog="isShowingAddCommentDialog" />
        </div>
        <wb-progress :visible="isFetchInProgress" :valuenow="valuenow" :valuemax="valuemax" />
    </div>
</template>

<script lang="js">
    import { getLocationHash } from "~/shared/helpers"
    import { debounce } from "lodash"

    export default {
        name: 'wb-question',
        props: ["question", 'questionCssClassName', 'noTitle', 'noInstructions', 'noValidation', 'noAnswer', 'noComments', 'isDisabled', 'noFlag'],
        data() {
            return {
                isShowingAddCommentDialogFlag: undefined
            }
        },

        watch: {
            ["$store.getters.scrollState"]() {
                 this.scroll();
            },

            "question.updatedAt"() {
                 this.scroll();
            }
        },

        mounted() {
            this.scroll();
        },

        computed: {
            id() {
                return this.question.id
            },

            highlight() {
                if(!this.$store.getters.isReviewMode) return false;
                
                return !this.$store.state.webinterview.sidebar.searchResultsHidden && this.$store.state.route.hash === '#' + this.id
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
            isVisible() {
                return !this.question.isLoading && 
                    !(this.question.isDisabled && this.question.hideIfDisabled && !this.$store.getters.isReviewMode)
            },
            disabled() {
                return this.isDisabled || this.question.isDisabled;
            },
            hasFlag(){
                if(this.$store.getters.isReviewMode === true) 
                    return this.$store.getters.flags[this.id]
                    
                return false
            },
            questionClass() {
                return [{ 
                    'mark': this.highlight,
                    'disabled-question': this.disabled, 
                    'with-flag': this.hasFlag}]
            },
            questionEditorClass() {
                return [{
                    answered: this.question.isAnswered && !this.noAnswer,
                    readonly: !this.question.acceptAnswer,
                    'has-error': !this.question.validity.isValid,
                    'for-supervisor': this.question.isForSupervisor
                }, this.questionCssClassName]
            },
            isShowingAddCommentDialog() {
                if (this.isShowingAddCommentDialogFlag == undefined)
                    return this.question.comments && this.question.comments.length > 0
                else
                    return this.isShowingAddCommentDialogFlag
            },
            showSideMenu() {
                return !this.disabled && !this.noComments;
            }
        },
        methods : {
            doScroll: debounce(function() {
                if(this.$store.getters.scrollState ==  "#" + this.id){
                    window.scroll({ top: this.$el.offsetTop, behavior: "smooth" })
                    this.$store.dispatch("resetScroll")
                }
            }, 200),

            scroll() {
                if(this.$store && this.$store.state.route.hash === "#" + this.id) {
                    this.doScroll(); 
                }
            },

            showAddComment() {
                if (this.$store.getters.addCommentsAllowed)
                    this.isShowingAddCommentDialogFlag = true;
            },
            hideAddComment(){
                this.isShowingAddCommentDialogFlag = false;
            }
        }
    }

</script>
