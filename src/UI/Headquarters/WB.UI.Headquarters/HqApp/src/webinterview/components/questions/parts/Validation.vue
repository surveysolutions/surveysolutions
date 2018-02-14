<template>
    <div class="information-block" :class="blockClass" v-if="!$me.validity.isValid">
        <h6 v-if="$me.validity.errorMessage == null">{{ $t("WebInterviewUI.AnswerIsInvalid") }}</h6>
        <h6 v-else>{{ $t("WebInterviewUI.AnswerWasNotSaved") }}</h6>
        <template v-for="message in messages">
            <p v-dateTimeFormatting v-html="message" :key="message"></p>
        </template>
    </div>
</template>
<script lang="js">
    import { entityPartial } from "~/webinterview/components/mixins"

    export default {
        props: {
            messages: {
                type: Array,
                default: []
            },
            blockClass: {
                type: String,
                default: 'text-danger'
            }
        },
        mixins: [entityPartial],
        name: "wb-validation"
    }
</script>
