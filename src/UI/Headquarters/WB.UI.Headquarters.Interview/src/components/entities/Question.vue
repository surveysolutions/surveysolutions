<template>
    <div class="question" v-if="isEnabled" :class="questionClass" :id="hash">
        <div class="question-editor" :class="questionEditorClass">
            <wb-title v-if="!noTitle" />
            <wb-instructions v-if="!noInstructions" />
            <slot />
            <wb-validation v-if="!noValidation"/>
        </div>
    </div>
</template>
<script lang="ts">
    import { getLocationHash } from "src/store/store.fetch"

    export default {
        name: 'wb-question',
        props: ["question", 'questionCssClassName', 'noTitle', 'noInstructions', 'noValidation'],
        computed: {
            id() {
                return this.question.id
            },
            hash() {
                return getLocationHash(this.question.id)
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
