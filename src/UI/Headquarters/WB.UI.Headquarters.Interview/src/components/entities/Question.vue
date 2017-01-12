<template>
    <div class="question" v-if="question && !(question.isDisabled && question.hideIfDisabled)" :class="[{'hidden-question': question.isDisabled}]">
        <div class="question-editor" :class="[{ answered: question.isAnswered, 'has-error': !question.validity.isValid}, questionType]">
            <wb-title :id="question.id" />
            <wb-instructions :id="question.id" />
            <slot />
            <wb-validation :id="question.id" />
        </div>
    </div>
</template>
<script lang="ts">
    export default {
        name: 'wb-question',
        props: ["question", 'questionType']
    }
</script>
