<template>
    <div class="question" v-if="question">
        <div class="question-editor" :class="[{ answered: question.isAnswered, 'has-error': !question.validity.isValid}, questionType]">
            <wb-title :id="question.id" />
            <wb-instructions :id="question.id" />
            <slot />
            <div class="information-block text-danger" v-if="!question.validity.isValid">
                <h6>Answer is invalid</h6>
                <template v-for="message in question.validity.messages">
                    <p>{{ message }}</p>
                </template>
            </div>
        </div>
    </div>
</template>
<script lang="ts">
    export default {
        name: 'wb-question',
        props: ["question", 'questionType']
    }
</script>
