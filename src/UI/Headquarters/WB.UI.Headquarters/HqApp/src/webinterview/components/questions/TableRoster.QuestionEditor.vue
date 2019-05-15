<template>
    <div class="ag-input-text-wrapper">
        <component ref='editQuestionComponent' 
            :key="question.identity" 
            v-bind:is="'TableRoster_' + question.entityType" 
            v-bind:id="question.identity" 
            :editorParams="params" 
            fetchOnMount>
        </component>
    </div>
</template>

<script lang="js">
    import Vue from 'vue'

    export default {
        name: 'TableRoster_QuestionEditor',
        
        data() {
            return {
                question: null
            }
        }, 
        methods: {
            getValue() {
                return this.question;
            },
            isCancelAfterEnd() {
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
            this.question = this.params.value;
        }
    }
</script>











