<template>
    <div class="ag-input-text-wrapper">
        <component ref='editQuestionComponent' 
            :key="question.identity" 
            v-bind:is="'MatrixRoster_' + question.entityType" 
            v-bind:id="question.identity" 
            :editorParams="params">
        </component>
        <wb-progress :visible="isFetchInProgress" />
    </div>
</template>

<script lang="js">
    import Vue from 'vue'

    export default {
        name: 'MatrixRoster_QuestionEditor',
        
        data() {
            return {
                question: null
            }
        }, 
        computed: {
            $me() {
                return this.$store.state.webinterview.entityDetails[this.question.identity] 
            },
            isFetchInProgress() {
                const result = this.$store.state.webinterview.fetch.state[this.question.identity]
                return result
            }
        },
        methods: {
            getValue() {
                return this.question
            },
            isCancelBeforeStart() {
                if (this.$me.isDisabled || this.$me.isLocked || !this.$me.acceptAnswer)
                    return true
                return false
            },
            isCancelAfterEnd() {
                if (this.$refs.editQuestionComponent.isCancelBeforeStart) {
                    var isNeedCancel = this.$refs.editQuestionComponent.isCancelBeforeStart()
                    if (isNeedCancel) return true
                }

                if (this.$refs.editQuestionComponent.saveAnswer)
                    this.$refs.editQuestionComponent.saveAnswer()

                return false
            },
            destroy() {
                if (this.$refs.editQuestionComponent.destroy)
                    this.$refs.editQuestionComponent.destroy()
            }
        },
        created() {
            this.question = this.params.value
        }
    }
</script>
