<template>
    <div class="question" v-if="isEnabled" :class="questionClass">
        <div class="question-editor" :class="questionEditorClass">
            <wb-title />
            <wb-instructions />
            <slot />
            <wb-validation />
        </div>
    </div>
</template>
<script lang="ts">
    export default {
        name: 'wb-question',
        props: ["question", 'questionCssClassName'],
        computed: {
            id() {
                return this.question.id
            },
            isEnabled() {
                return !this.question.isLoading && !(this.question.isDisabled && this.question.hideIfDisabled)
            },
            questionClass() {
                return [{ 'hidden-question': this.question.isDisabled }]
            },
            questionEditorClass() {
                return [{
                    answered: this.question.isAnswered,
                    'has-error': !this.question.validity.isValid
                }, this.questionCssClassName]
            }
        }
    }
</script>
