<template>
    <div class="question" v-if="!question.isLoading && !(question.isDisabled && question.hideIfDisabled)" :class="[{'hidden-question': question.isDisabled}]">
        <div class="question-editor" :class="[{ answered: question.isAnswered, 'has-error': !question.validity.isValid}, questionCssClassName]">
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
            id: function () {
                return this.question.id
            }
        }
    }
</script>
